using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FloatingRigidbody : MonoBehaviour
{
    public WaterSurfaceBase water;
    public Transform[] floatPoints;
    public float maxDepth = 1.0f;
    public float buoyancyMultiplier = 1.0f;
    public float airDrag = 0.05f;
    public float airAngularDrag = 0.05f;
    public float waterDrag = 3.0f;
    public float waterAngularDrag = 1.5f;
    public float flowStrength = 1.0f;
    public GameObject splashVFX;
    public float splashSpeedThreshold = 3.0f;

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (floatPoints == null || floatPoints.Length == 0)
            floatPoints = new[] { this.transform };
    }

    void FixedUpdate()
    {
        if (!water) return;

        int submergedCount = 0;

        foreach (var p in floatPoints)
        {
            Vector3 pos = p.position;

            float waterHeight = water.GetHeight(pos);
            float depth = waterHeight - pos.y;
            if (depth > 0f)
            {
                submergedCount++;
                float submergence = Mathf.Clamp01(depth / maxDepth);
                float buoyancy = Physics.gravity.magnitude * rb.mass * submergence * buoyancyMultiplier;

                Vector3 normal = water.GetNormal(pos);
                rb.AddForceAtPosition(normal * buoyancy, pos, ForceMode.Force);

                if (flowStrength > 0f)
                {
                    Vector3 flowAccel = water.GetFlow(pos) * flowStrength;
                    rb.AddForceAtPosition(flowAccel * rb.mass, pos, ForceMode.Force);
                }
            }
        }
        float t = (float)submergedCount / floatPoints.Length;
        rb.linearDamping = Mathf.Lerp(airDrag, waterDrag, t);
        rb.angularDamping = Mathf.Lerp(airAngularDrag, waterAngularDrag, t);
    }

    void OnCollisionEnter(Collision col)
    {
        if (!splashVFX || !water) return;

        foreach (var c in col.contacts)
        {
            float y = c.point.y;
            float wy = water.GetHeight(c.point);
            if (Mathf.Abs(y - wy) < 0.15f && rb.linearVelocity.magnitude > splashSpeedThreshold)
            {
                Instantiate(splashVFX, new Vector3(c.point.x, wy, c.point.z), Quaternion.identity);
                break;
            }
        }
    }
}
