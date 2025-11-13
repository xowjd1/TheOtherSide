using UnityEngine;

public class FlatWaterSurface : WaterSurfaceBase
{
        public float offset = 0f;

        public override float GetHeight(Vector3 worldPos)
        {
                // Plane의 현재 Y를 그대로 물 높이로 사용
                return transform.position.y + offset;
        }
}
