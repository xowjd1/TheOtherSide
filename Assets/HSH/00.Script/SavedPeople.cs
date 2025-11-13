using UnityEngine;

public class SavedPeople : MonoBehaviour
{
    // FSM 상태 정의
    public enum State
    {
        WaitingForRescue,  // 손들고 기다리는 상태
        Moving,            // 이동 상태
        Arrived            // 도착 완료 상태
    }

    [Header("State Management")]
    [SerializeField] private State currentState = State.WaitingForRescue;

    [Header("Movement Settings")]
    [SerializeField] private Transform targetDestination; // 이동할 목적지
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float arrivalDistance = 0.5f; // 도착 판정 거리

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string handUpAnimationTrigger = "HandUp";
    [SerializeField] private string walkAnimationBool = "IsWalking";
    [SerializeField] private string idleAnimationTrigger = "Idle";

    [Header("Components")]
    [SerializeField] private CharacterController controller; // CharacterController 사용

    [Header("Gravity Settings")]
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;

    [Header("Mission Reference")]
    public TreeMissionClear tmc;

    // 내부 변수
    private Vector3 startPosition;
    private Vector3 velocity;
    private bool isMoving = false;
    private bool isGrounded;

    void Start()
    {
        // 초기화
        startPosition = transform.position;

        // Animator 컴포넌트 가져오기
        if (animator == null)
            animator = GetComponent<Animator>();

        // CharacterController 설정
        if (controller == null)
            controller = GetComponent<CharacterController>();

        // 초기 상태 설정
        ChangeState(State.WaitingForRescue);
    }

    void Update()
    {
        // 바닥 체크
        CheckGround();

        // FSM 업데이트
        UpdateStateMachine();

        // 상태 전환 체크
        CheckStateTransitions();

        // 중력 적용
        ApplyGravity();
    }

    // 바닥 체크
    void CheckGround()
    {
        // groundCheck가 설정되어 있으면 사용, 아니면 controller.isGrounded 사용
        if (groundCheck != null)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        }
        else
        {
            isGrounded = controller.isGrounded;
        }

        // 바닥에 있고 떨어지는 중이면 속도 리셋
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // 바닥에 붙어있게 하기 위한 작은 음수값
        }
    }

    // 중력 적용
    void ApplyGravity()
    {
        // 중력 가속도 적용
        velocity.y += gravity * Time.deltaTime;

        // Y축 이동만 적용 (수평 이동은 UpdateMovingState에서 처리)
        controller.Move(new Vector3(0, velocity.y, 0) * Time.deltaTime);
    }

    // 상태 머신 업데이트
    void UpdateStateMachine()
    {
        switch (currentState)
        {
            case State.WaitingForRescue:
                UpdateWaitingState();
                break;

            case State.Moving:
                UpdateMovingState();
                break;

            case State.Arrived:
                UpdateArrivedState();
                break;
        }
    }

    // 상태 전환 체크
    void CheckStateTransitions()
    {
        switch (currentState)
        {
            case State.WaitingForRescue:
                // treesCleared가 true가 되면 이동 상태로 전환
                if (tmc != null && tmc.treesCleared)
                {
                    ChangeState(State.Moving);
                }
                break;

            case State.Moving:
                // 목적지 도착 체크
                if (HasArrivedAtDestination())
                {
                    ChangeState(State.Arrived);
                }
                break;
        }
    }

    // ========== 각 상태별 업데이트 함수 ==========

    void UpdateWaitingState()
    {
        // 손든 애니메이션 유지
        // 디버그용 - 일정 시간마다 도움 요청
        if (Time.frameCount % 180 == 0) // 약 3초마다 (60fps 기준)
        {
            Debug.Log($"{gameObject.name}: Help! Please clear the trees!");
        }
    }

    void UpdateMovingState()
    {
        if (targetDestination == null)
        {
            Debug.LogWarning("Target destination is not set!");
            return;
        }

        // 목표 방향 계산
        Vector3 direction = (targetDestination.position - transform.position).normalized;
        direction.y = 0; // Y축 이동 제거 (수평 이동만)

        // CharacterController로 이동
        if (controller != null && direction.magnitude > 0.1f)
        {
            // 수평 이동
            Vector3 moveVector = direction * moveSpeed * Time.deltaTime;
            controller.Move(moveVector);

            // 캐릭터 회전
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            }
        }
    }

    void UpdateArrivedState()
    {
        // 도착 후 대기 상태
        // 필요시 추가 동작 (예: 감사 인사, 다른 애니메이션 등)
    }

    // ========== 상태 전환 함수 ==========

    void ChangeState(State newState)
    {
        // 이전 상태 종료 처리
        OnStateExit(currentState);

        // 상태 변경
        State previousState = currentState;
        currentState = newState;

        // 새 상태 시작 처리
        OnStateEnter(currentState);

        Debug.Log($"{gameObject.name}: State changed from {previousState} to {newState}");
    }

    void OnStateEnter(State state)
    {
        switch (state)
        {
            case State.WaitingForRescue:
                // 손든 애니메이션 시작
                if (animator != null)
                {
                    animator.SetTrigger(handUpAnimationTrigger);
                }
                break;

            case State.Moving:
                // 걷기 애니메이션 시작
                if (animator != null)
                {
                    animator.SetBool(walkAnimationBool, true);
                }
                isMoving = true;
                break;

            case State.Arrived:
                // 대기 애니메이션
                if (animator != null)
                {
                    animator.SetTrigger(idleAnimationTrigger);
                }
                break;
        }
    }

    void OnStateExit(State state)
    {
        switch (state)
        {
            case State.WaitingForRescue:
                // 손든 애니메이션 종료
                break;

            case State.Moving:
                // 걷기 애니메이션 종료
                if (animator != null)
                {
                    animator.SetBool(walkAnimationBool, false);
                }
                isMoving = false;
                break;

            case State.Arrived:
                break;
        }
    }

    // ========== 유틸리티 함수 ==========

    bool HasArrivedAtDestination()
    {
        if (targetDestination == null) return false;

        // Y축 제외한 수평 거리만 체크
        Vector3 flatPosition = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 flatDestination = new Vector3(targetDestination.position.x, 0, targetDestination.position.z);

        float distance = Vector3.Distance(flatPosition, flatDestination);
        return distance <= arrivalDistance;
    }

    // ========== Public 함수 (외부에서 호출 가능) ==========

    // 목적지 설정
    public void SetDestination(Transform destination)
    {
        targetDestination = destination;
    }

    // 상태 강제 변경
    public void ForceChangeState(State newState)
    {
        ChangeState(newState);
    }

    // 현재 상태 반환
    public State GetCurrentState()
    {
        return currentState;
    }

    // 나무가 제거되었을 때 호출 (외부에서 사용 가능)
    public void OnTreesCleared()
    {
        if (tmc != null)
        {
            tmc.treesCleared = true;
        }
    }

    // ========== 디버그용 ==========

    void OnDrawGizmos()
    {
        // 목적지 표시
        if (targetDestination != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(targetDestination.position, 0.5f);

            // 현재 위치에서 목적지까지 선
            if (Application.isPlaying && currentState == State.Moving)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, targetDestination.position);
            }
        }

        // 바닥 체크 영역 표시
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }

#if UNITY_EDITOR
        // 현재 상태 표시 (에디터에서만)
        if (Application.isPlaying)
        {
            Vector3 labelPos = transform.position + Vector3.up * 2f;
            UnityEditor.Handles.Label(labelPos, $"State: {currentState}");
        }
#endif
    }
}