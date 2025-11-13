using System.Collections;
using UnityEngine;

public enum CamStatus
{
    FPSMode,
    CinematicMode
}

public class H_CamController : MonoBehaviour
{
    public CamStatus camStatus;

    [Header("Target Settings")]
    public Transform target; // 플레이어 Transform

    [Header("Camera Settings")]
    public float distance = 5.0f; // 카메라와 플레이어 사이의 거리
    public float height = 2.0f; // 카메라 높이 오프셋

    [Header("Distance Control")]
    public float minDistance = 1.0f; // 최소 거리
    public float maxDistance = 10.0f; // 최대 거리
    public float distanceStep = 4f; // N키 누를 때마다 줄어드는 거리
    public float distanceChangeSpeed = 5.0f; // 거리 변경 속도 (부드러운 전환용)

    [Header("Mouse Settings")]
    public float mouseSensitivity = 100.0f;

    [Header("Vertical Rotation Limits")]
    [Range(-89, 0)]
    public float minVerticalAngle = -60.0f; // 아래쪽 각도 제한 (음수)
    [Range(0, 89)]
    public float maxVerticalAngle = 60.0f; // 위쪽 각도 제한 (양수)

    [Header("Smoothing")]
    public float rotationDamping = 3.0f;
    public float positionDamping = 3.0f;

    [Header("Collision Detection")]
    public bool enableWallAvoidance = true;
    public LayerMask collisionLayers = -1;
    public float collisionOffset = 0.3f;

    private float currentX = 0.0f; // 수평 회전 (Y축 회전)
    private float currentY = 0.0f; // 수직 회전 (X축 회전)
    private float desiredDistance;
    private float targetDistance; // 목표 거리 (부드러운 전환용)

    // 디버그용 변수
    [Header("Debug Info")]
    [SerializeField] private float currentVerticalAngle; // 현재 수직 각도 표시
    [SerializeField] private float currentDistance; // 현재 거리 표시

    void Start()
    {
        camStatus = CamStatus.FPSMode;

        //// 커서 잠금
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;

        // 초기 각도 설정
        Vector3 angles = transform.eulerAngles;
        currentX = angles.y;

        // 초기 수직 각도 설정 (0-360도를 -180~180도로 변환)
        currentY = angles.x;
        if (currentY > 180)
            currentY -= 360;

        // 초기 각도도 제한 범위 내로 클램프
        currentY = ClampVerticalAngle(currentY);

        desiredDistance = distance;
        targetDistance = distance;

        // 플레이어 자동 찾기 및 타겟 설정
        if (target == null)
        {

            H_CharacterMovement player = FindFirstObjectByType<H_CharacterMovement>(FindObjectsInactive.Exclude);

            if (player != null)
            {
                target = player.transform;
                // 플레이어에게도 카메라 참조 설정
                player.SetCameraTransform(this.transform);
            }
        }

        // 제한 값 유효성 검사
        ValidateLimits();
    }

    void ValidateLimits()
    {
        // 제한 값이 올바른 범위에 있는지 확인
        minVerticalAngle = Mathf.Clamp(minVerticalAngle, -89f, 0f);
        maxVerticalAngle = Mathf.Clamp(maxVerticalAngle, 0f, 89f);

        // 최소값이 최대값보다 크지 않도록
        if (minVerticalAngle > maxVerticalAngle)
        {
            Debug.LogWarning("minVerticalAngle이 maxVerticalAngle보다 큽니다. 값을 교환합니다.");
            float temp = minVerticalAngle;
            minVerticalAngle = maxVerticalAngle;
            maxVerticalAngle = temp;
        }

        // 거리 제한 검증
        minDistance = Mathf.Max(0.1f, minDistance);
        maxDistance = Mathf.Max(minDistance + 0.1f, maxDistance);
    }

    void LateUpdate()
    {
        if (GameManager.Instance.status == GameStatus.Ready || GameManager.Instance.status == GameStatus.Ending) return;

        if (target == null)
            return;
        UpdateCamMode();


        // 디버그용 현재 값 업데이트
        currentVerticalAngle = currentY;
        currentDistance = desiredDistance;
    }

    void UpdateCamMode()
    {
        switch(camStatus)
        {
            case CamStatus.FPSMode:

                HandleMouseInput();
                UpdateCameraDistance();
                UpdateCameraPosition();
                break;

            case CamStatus.CinematicMode:

                break;

            default:
                break;


        }


        }

    void HandleMouseInput()
    {
        // 마우스 입력 받기
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // 수평 회전 (Y축, 제한 없음)
        currentX += mouseX;

        // 수직 회전 (X축, 제한 있음)
        currentY -= mouseY;
        currentY = ClampVerticalAngle(currentY);
    }

    float ClampVerticalAngle(float angle)
    {
        // 각도를 -180 ~ 180 범위로 정규화
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;

        // 수직 각도 제한 적용
        return Mathf.Clamp(angle, minVerticalAngle, maxVerticalAngle);
    }

    void UpdateCameraDistance()
    {
        // 부드러운 거리 전환
        desiredDistance = Mathf.Lerp(desiredDistance, targetDistance, distanceChangeSpeed * Time.deltaTime);
    }

    void UpdateCameraPosition()
    {
        // 목표 위치와 회전 계산
        Vector3 targetPosition = target.position + Vector3.up * height;

        // 제한된 각도로 회전 생성
        Quaternion targetRotation = Quaternion.Euler(currentY, currentX, 0);

        // 카메라가 위치할 지점 계산
        Vector3 direction = targetRotation * Vector3.back;
        Vector3 desiredPosition = targetPosition + direction * desiredDistance;

        // 벽 충돌 검사
        if (enableWallAvoidance)
        {
            desiredPosition = CheckWallCollision(targetPosition, desiredPosition);
        }

        // 부드러운 이동과 회전
        transform.position = Vector3.Lerp(transform.position, desiredPosition, positionDamping * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationDamping * Time.deltaTime);
    }

    Vector3 CheckWallCollision(Vector3 targetPos, Vector3 desiredPos)
    {
        RaycastHit hit;
        Vector3 direction = (desiredPos - targetPos).normalized;
        float targetDistance = Vector3.Distance(targetPos, desiredPos);

        // 타겟에서 카메라 위치까지 레이캐스트
        if (Physics.Raycast(targetPos, direction, out hit, targetDistance, collisionLayers))
        {
            // 벽에 충돌하면 충돌 지점 앞으로 카메라 위치 조정
            return hit.point - direction * collisionOffset;
        }

        return desiredPos;
    }

    // 공개 메서드들
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;

        // 타겟이 플레이어라면 카메라 참조도 설정
        H_CharacterMovement player = target.GetComponent<H_CharacterMovement>();
        if (player != null)
        {
            player.SetCameraTransform(this.transform);
        }
    }

    public void SetDistance(float newDistance)
    {
        distance = Mathf.Clamp(newDistance, minDistance, maxDistance);
        targetDistance = distance;
        desiredDistance = distance;
    }

    public void SetSensitivity(float newSensitivity)
    {
        mouseSensitivity = newSensitivity;
    }

    // 거리를 줄이는 메서드
    public void DecreaseDistance()
    {
        targetDistance = Mathf.Max(minDistance, targetDistance - distanceStep);
        distance = targetDistance;
    }

    // 거리를 늘리는 메서드
    public void IncreaseDistance()
    {
        targetDistance = Mathf.Min(maxDistance, targetDistance + distanceStep);
        distance = targetDistance;
    }

    // 거리를 특정 값으로 설정
    public void SetDistanceImmediate(float newDistance)
    {
        distance = Mathf.Clamp(newDistance, minDistance, maxDistance);
        targetDistance = distance;
        desiredDistance = distance;
    }

    // 수직 각도 제한 설정
    public void SetVerticalLimits(float min, float max)
    {
        minVerticalAngle = Mathf.Clamp(min, -89f, 0f);
        maxVerticalAngle = Mathf.Clamp(max, 0f, 89f);
        ValidateLimits();

        // 현재 각도도 새로운 제한에 맞게 조정
        currentY = ClampVerticalAngle(currentY);
    }

    // 현재 수직 각도 가져오기
    public float GetCurrentVerticalAngle()
    {
        return currentY;
    }

    // 현재 거리 가져오기
    public float GetCurrentDistance()
    {
        return desiredDistance;
    }

    // 각도 제한 리셋
    public void ResetVerticalLimits()
    {
        minVerticalAngle = -60f;
        maxVerticalAngle = 60f;
    }

    // 거리 리셋
    public void ResetDistance()
    {
        distance = 5.0f;
        targetDistance = distance;
        desiredDistance = distance;
    }

    // ESC 키로 커서 잠금/해제
    void Update()
    {
        // ESC 키 - 커서 잠금/해제
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        // N 키 - 카메라를 플레이어에게 가까이
        if (Input.GetKeyDown(KeyCode.N))
        {
            DecreaseDistance();
            Debug.Log($"카메라 거리: {targetDistance:F2}");
        }

        // M 키 - 카메라를 플레이어에게서 멀리 (추가 기능)
        if (Input.GetKeyDown(KeyCode.M))
        {
            IncreaseDistance();
            Debug.Log($"카메라 거리: {targetDistance:F2}");
        }

        // 마우스 휠로 거리 조절 (추가 기능)
        float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
        if (scrollWheel != 0)
        {
            targetDistance = Mathf.Clamp(targetDistance - scrollWheel * distanceStep * 10, minDistance, maxDistance);
            distance = targetDistance;
        }

        // 디버그용 - 각도 제한 실시간 조정 (개발 중에만 사용)
#if UNITY_EDITOR
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetVerticalLimits();
                Debug.Log($"수직 각도 제한 리셋: [{minVerticalAngle}, {maxVerticalAngle}]");
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                ResetDistance();
                Debug.Log($"거리 리셋: {distance}");
            }
        }
#endif
    }

    // 기즈모로 각도 제한 시각화 (에디터에서만)
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (target == null) return;

        Vector3 targetPos = target.position + Vector3.up * height;

        // 현재 카메라 방향
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(targetPos, transform.position);

        // 최대 위쪽 각도
        Gizmos.color = Color.red;
        Vector3 maxDir = Quaternion.Euler(maxVerticalAngle, currentX, 0) * Vector3.back * desiredDistance;
        Gizmos.DrawLine(targetPos, targetPos + maxDir);

        // 최대 아래쪽 각도
        Gizmos.color = Color.blue;
        Vector3 minDir = Quaternion.Euler(minVerticalAngle, currentX, 0) * Vector3.back * desiredDistance;
        Gizmos.DrawLine(targetPos, targetPos + minDir);

        // 제한 범위 호 그리기
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        int segments = 20;
        float angleRange = maxVerticalAngle - minVerticalAngle;
        Vector3 prevPoint = targetPos + minDir;

        for (int i = 1; i <= segments; i++)
        {
            float t = (float)i / segments;
            float angle = minVerticalAngle + angleRange * t;
            Vector3 dir = Quaternion.Euler(angle, currentX, 0) * Vector3.back * desiredDistance;
            Vector3 point = targetPos + dir;
            Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }

        // 거리 범위 시각화
        Gizmos.color = new Color(1, 1, 0, 0.3f);
        Gizmos.DrawWireSphere(targetPos, minDistance);
        Gizmos.color = new Color(1, 0.5f, 0, 0.3f);
        Gizmos.DrawWireSphere(targetPos, maxDistance);
    }
#endif
}