using UnityEngine;
using TMPro; // ← 추가

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class TwoHitChopZone : MonoBehaviour
{
    public Rigidbody targetPart;
    public int hitsToDrop = 2;
    public bool requireExitForNextHit = true;
    public float minHitInterval = 0.15f;
    public GameObject hitVfxPrefab;
    public float vfxSurfaceOffset = 0.02f;
    public AudioSource audioSource;
    public AudioClip hitSfx;
    public TextMeshProUGUI hintText;

    int _hits = 0;
    bool _axeInside = false;
    float _lastHitTime = -999f;
    int _lastAxeRootId = -1;

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity  = false;
    }

    void Start()
    {
        if (targetPart)
        {
            targetPart.isKinematic = true;
            targetPart.useGravity  = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Axe")) return;
        _axeInside = true;

        if (Time.time - _lastHitTime < minHitInterval) return;

        int rootId = other.transform.root.GetInstanceID();
        if (rootId == _lastAxeRootId && Time.time - _lastHitTime < 0.5f) return;

        if (requireExitForNextHit && _hits > 0 && _axeInside) return;

        _lastHitTime   = Time.time;
        _lastAxeRootId = rootId;

        _hits++;
        SpawnHitFeedback(other);
        if (_hits >= hitsToDrop) DropNow();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Axe")) return;
        _axeInside = false;
    }

    void DropNow()
    {
        if (!targetPart) return;
        targetPart.isKinematic = false;
        targetPart.useGravity  = true;
        
        if (hintText)     hintText.gameObject.SetActive(false);

        GetComponent<Collider>().enabled = false;
        Debug.Log($"[{name}] DROPPED!");
    }

    void SpawnHitFeedback(Collider axeCol)
    {
        var myCol = GetComponent<Collider>();
        Vector3 pos = myCol.ClosestPoint(axeCol.bounds.center);

        Vector3 normal;
        if (myCol is SphereCollider sc)
        {
            Vector3 worldCenter = sc.transform.TransformPoint(sc.center);
            normal = (pos - worldCenter).normalized;
        }
        else
        {
            normal = (pos - myCol.bounds.center).normalized;
        }
        pos += normal * vfxSurfaceOffset;

        if (hitVfxPrefab)
        {
            var inst = Instantiate(hitVfxPrefab, pos, Quaternion.LookRotation(normal));
            var ps = inst.GetComponent<ParticleSystem>();
            Destroy(inst, ps ? ps.main.duration + ps.main.startLifetime.constantMax + 0.1f : 1f);
        }

        if (audioSource && hitSfx)
            audioSource.PlayOneShot(hitSfx);
    }
}
