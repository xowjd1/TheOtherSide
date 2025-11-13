using UnityEngine;

public class Billboard : MonoBehaviour
{
    public enum BillboardType
    {
        LookAtCamera,      // 카메라를 완전히 바라봄
        YAxisOnly          // Y축 회전만 (수직 빌보드)
    }

    [Header("빌보드 설정")]
    public BillboardType billboardType = BillboardType.LookAtCamera;

    [Header("카메라 설정")]
    public Camera targetCamera;
    public bool useMainCamera = true;

    [Header("추가 옵션")]
    public bool reverseFace = false;  // 뒷면을 보여줄지 여부
    public Vector3 rotationOffset = Vector3.zero;  // 추가 회전 오프셋

    private void Start()
    {
        // 카메라가 지정되지 않았고 메인 카메라를 사용하도록 설정된 경우
        if (targetCamera == null && useMainCamera)
        {
            targetCamera = Camera.main;

            if (targetCamera == null)
            {
                Debug.LogError("Billboard: 메인 카메라를 찾을 수 없습니다!");
            }
        }
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
            return;

        switch (billboardType)
        {
            case BillboardType.LookAtCamera:
                LookAtCameraFull();
                break;
            case BillboardType.YAxisOnly:
                LookAtCameraYAxis();
                break;
        }
    }

    // 카메라를 완전히 바라보는 빌보드
    private void LookAtCameraFull()
    {
        Vector3 lookDirection = targetCamera.transform.position - transform.position;

        if (reverseFace)
            lookDirection = -lookDirection;

        Quaternion rotation = Quaternion.LookRotation(lookDirection);
        transform.rotation = rotation * Quaternion.Euler(rotationOffset);
    }

    // Y축 회전만 하는 빌보드 (수직 빌보드)
    private void LookAtCameraYAxis()
    {
        Vector3 lookDirection = targetCamera.transform.position - transform.position;
        lookDirection.y = 0;  // Y 성분을 0으로 만들어 수평 방향만 고려

        if (lookDirection != Vector3.zero)
        {
            if (reverseFace)
                lookDirection = -lookDirection;

            Quaternion rotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = rotation * Quaternion.Euler(rotationOffset);
        }
    }

    //// 에디터에서 기즈모 표시 (선택적)
    //private void OnDrawGizmosSelected()
    //{
    //    if (targetCamera != null)
    //    {
    //        Gizmos.color = Color.yellow;
    //        Gizmos.DrawLine(transform.position, targetCamera.transform.position);
    //    }
    //}
}


