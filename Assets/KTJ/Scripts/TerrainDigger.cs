using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TerrainDigger : MonoBehaviour
{
    [Header("General")]
    public Camera cam;
    public LayerMask terrainMask = ~0;
    public float rayDistance = 8f;
    public bool hasShovel = false;

    [Header("Brush")]
    public float brushRadiusMeters = 1.2f;
    public float lowerAmountMeters = 0.12f;
    public bool softFalloff = true;

    private readonly Dictionary<Terrain, TerrainData> _original = new();
    private readonly Dictionary<Terrain, TerrainData> _runtime  = new();
    private bool _restored = false;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        SetupRuntimeClones();

#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
        Application.quitting += RestoreTerrains; 
    }

    void OnDestroy()
    {
        RestoreTerrains();
        #if UNITY_EDITOR
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        #endif
        Application.quitting -= RestoreTerrains;
    }

#if UNITY_EDITOR
    void OnPlayModeStateChanged(PlayModeStateChange change)
    {
        if (change == PlayModeStateChange.ExitingPlayMode)
            RestoreTerrains();
    }
#endif

    void SetupRuntimeClones()
    {
        foreach (var t in Terrain.activeTerrains)
        {
            if (!t || !t.terrainData) continue;
            if (_original.ContainsKey(t)) continue;

            var orig = t.terrainData;
            _original[t] = orig;

            var clone = Instantiate(orig);
            clone.name = orig.name + " (Runtime)";
            clone.hideFlags = HideFlags.DontSave | HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;

            _runtime[t] = clone;
            
            t.terrainData = clone;

            var tc = t.GetComponent<TerrainCollider>();
            if (tc) tc.terrainData = clone;
        }
    }


    void RestoreTerrains()
    {
        if (_restored) return;

        foreach (var kv in _original)
        {
            var t = kv.Key;
            var orig = kv.Value;
            if (!t) continue;
            
            if (t.terrainData != orig) t.terrainData = orig;
            var tc = t.GetComponent<TerrainCollider>();
            if (tc && tc.terrainData != orig) tc.terrainData = orig;
        }
        _original.Clear();
        _runtime.Clear();
        _restored = true;
    }


    public void SetHasShovel(bool v) => hasShovel = v;

    public void DigOnce()
    {
        if (!hasShovel || !cam) return;

        if (Physics.Raycast(cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)),
                            out var hit, rayDistance, terrainMask, QueryTriggerInteraction.Ignore))
        {
            var terrain = hit.collider.GetComponent<Terrain>();
            if (!terrain) return;
            ApplyDig(terrain, hit.point);
        }
    }

    void ApplyDig(Terrain terrain, Vector3 worldPoint)
    {
        var td = terrain.terrainData;
        Vector3 tPos = terrain.transform.position;

        float normX = Mathf.InverseLerp(tPos.x, tPos.x + td.size.x, worldPoint.x);
        float normZ = Mathf.InverseLerp(tPos.z, tPos.z + td.size.z, worldPoint.z);
        int hmRes = td.heightmapResolution;

        int cx = Mathf.RoundToInt(normX * (hmRes - 1));
        int cz = Mathf.RoundToInt(normZ * (hmRes - 1));

        int r = Mathf.RoundToInt((brushRadiusMeters / td.size.x) * hmRes);
        r = Mathf.Max(1, r);
        int sx = Mathf.Clamp(cx - r, 0, hmRes - 1);
        int sz = Mathf.Clamp(cz - r, 0, hmRes - 1);
        int ex = Mathf.Clamp(cx + r, 0, hmRes - 1);
        int ez = Mathf.Clamp(cz + r, 0, hmRes - 1);
        int w = ex - sx + 1;
        int h = ez - sz + 1;

        var heights = td.GetHeights(sx, sz, w, h);

        float lowerN = Mathf.Abs(lowerAmountMeters) / td.size.y;
        float rr = r;

        for (int z = 0; z < h; z++)
        for (int x = 0; x < w; x++)
        {
            int hx = sx + x;
            int hz = sz + z;
            float dx = hx - cx;
            float dz = hz - cz;
            float dist = Mathf.Sqrt(dx * dx + dz * dz);
            if (dist > rr) continue;

            float falloff = softFalloff ? 0.5f * (1f + Mathf.Cos(Mathf.Clamp01(dist / rr) * Mathf.PI)) : 1f;
            heights[z, x] = Mathf.Clamp01(heights[z, x] - lowerN * falloff);
        }

        td.SetHeightsDelayLOD(sx, sz, heights);
        terrain.ApplyDelayedHeightmapModification();
    }
}
