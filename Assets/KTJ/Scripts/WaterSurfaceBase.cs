using UnityEngine;

public abstract class WaterSurfaceBase : MonoBehaviour
{
    public abstract float  GetHeight(Vector3 worldPos);
    public virtual  Vector3 GetNormal(Vector3 worldPos) => Vector3.up;
    public virtual  Vector3 GetFlow(Vector3 worldPos)   => Vector3.zero;
}
