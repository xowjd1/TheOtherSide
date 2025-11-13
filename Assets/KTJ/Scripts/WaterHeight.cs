using UnityEngine;

public class WaterHeight : MonoBehaviour
{
    public Transform waterPlane;
    public Transform riverPlane;
    public float targetY = 1f;
    public float slowRiseSpeed = 0.05f;
    public float fastRiseSpeed = 0.2f;
    public float riverTargetY = 0.6f;
    public float riverSlowRiseSpeed = 0.1f;
    public float riverFastRiseSpeed = 0.24f;
    public WarningTriggerCol wtc;
    public bool latchTrigger = true;
    public ParticleSystem[] rainSystems;
    public float rainRateNormal  = 600f;
    public float rainRateBoosted = 4000f;

    private bool goFast = false;
    private bool rainBoosted = false;

    void Awake()
    {
        if (!waterPlane) waterPlane = transform;
        SetRainRate(rainRateNormal);
    }

    void Update()
    {
        if (wtc && wtc.isTriggerd) goFast = true;

        bool fastNow = (goFast || (!latchTrigger && wtc && wtc.isTriggerd));

        if (waterPlane)
        {
            float speed = fastNow ? fastRiseSpeed : slowRiseSpeed;
            Vector3 pos = waterPlane.position;
            float newY = Mathf.MoveTowards(pos.y, targetY, speed * Time.deltaTime);
            waterPlane.position = new Vector3(pos.x, newY, pos.z);
        }

        if (riverPlane)
        {
            float rSpeed = fastNow ? riverFastRiseSpeed : riverSlowRiseSpeed;
            Vector3 rpos = riverPlane.position;
            float rNewY = Mathf.MoveTowards(rpos.y, riverTargetY, rSpeed * Time.deltaTime);
            riverPlane.position = new Vector3(rpos.x, rNewY, rpos.z);
        }

        if (!rainBoosted && fastNow)
        {
            SetRainRate(rainRateBoosted);
            rainBoosted = true;
        }
    }

    void SetRainRate(float rate)
    {
        if (rainSystems == null) return;
        foreach (var ps in rainSystems)
        {
            if (!ps) continue;
            var em = ps.emission;
            em.rateOverTime = new ParticleSystem.MinMaxCurve(rate);
        }
    }
}
