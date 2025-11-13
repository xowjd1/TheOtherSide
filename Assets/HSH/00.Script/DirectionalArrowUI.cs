using UnityEngine;

public class DirectionalArrowUI : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform targetLocation; // 미션 목표 위치
    [SerializeField] private Transform player; // 플레이어 Transform

    [Header("Arrow Settings")]
    [SerializeField] private float heightOffset = 2.5f; // 플레이어 머리 위 높이
    [SerializeField] private float rotationSpeed = 5f; // 회전 속도 (부드러운 회전용)
    [SerializeField] private bool smoothRotation = true; // 부드러운 회전 여부

    [Header("Distance Display")]
    [SerializeField] private bool showDistance = true; // 거리 표시 여부
    [SerializeField] private TMPro.TextMeshProUGUI distanceText; // 거리 표시 텍스트 (선택사항)

    private void Start()
    {
        // 플레이어가 설정되지 않았다면 태그로 찾기
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
    }

    private void LateUpdate()
    {
        if (player == null || targetLocation == null)
            return;

        // 화살표 위치를 플레이어 머리 위로 설정
        UpdateArrowPosition();

        // 화살표 회전 (Y축만)
        UpdateArrowRotation();

        // 거리 업데이트 (선택사항)
        if (showDistance && distanceText != null)
        {
            UpdateDistanceDisplay();
        }
    }

    private void UpdateArrowPosition()
    {
        // 플레이어 위치에서 Y축으로만 오프셋 적용
        Vector3 newPosition = player.position + Vector3.up * heightOffset;
        transform.position = newPosition;
    }

    private void UpdateArrowRotation()
    {
        // 목표까지의 방향 계산 (Y축 무시)
        Vector3 direction = targetLocation.position - transform.position;
        direction.y = 0; // Y축 회전만 하기 위해 Y 차이는 무시

        // 방향이 0이 아닐 때만 회전
        if (direction != Vector3.zero)
        {
            // 목표 회전값 계산
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // Y축 회전만 유지 (X와 Z 회전을 0으로)
            targetRotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);

            // 회전 적용
            if (smoothRotation)
            {
                // 부드러운 회전
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
                                                    rotationSpeed * Time.deltaTime);
            }
            else
            {
                // 즉시 회전
                transform.rotation = targetRotation;
            }
        }
    }

    private void UpdateDistanceDisplay()
    {
        // 목표까지의 거리 계산 (XZ 평면상의 거리)
        Vector3 flatPlayerPos = new Vector3(player.position.x, 0, player.position.z);
        Vector3 flatTargetPos = new Vector3(targetLocation.position.x, 0, targetLocation.position.z);
        float distance = Vector3.Distance(flatPlayerPos, flatTargetPos);

        // 거리 텍스트 업데이트
        distanceText.text = $"{distance:F1}m";
    }

    // 목표 위치 동적 변경
    public void SetTarget(Transform newTarget)
    {
        targetLocation = newTarget;
    }

    // 목표 위치 동적 변경 (Vector3)
    public void SetTarget(Vector3 newTargetPosition)
    {
        // 빈 GameObject를 생성하여 위치로 사용
        GameObject targetObj = new GameObject("Target_Position");
        targetObj.transform.position = newTargetPosition;
        targetLocation = targetObj.transform;
    }

    // 화살표 표시/숨기기
    public void ShowArrow(bool show)
    {
        gameObject.SetActive(show);
    }

    // 목표 도달 체크
    public bool IsNearTarget(float threshold = 2f)
    {
        if (player == null || targetLocation == null)
            return false;

        Vector3 flatPlayerPos = new Vector3(player.position.x, 0, player.position.z);
        Vector3 flatTargetPos = new Vector3(targetLocation.position.x, 0, targetLocation.position.z);
        return Vector3.Distance(flatPlayerPos, flatTargetPos) <= threshold;
    }
}