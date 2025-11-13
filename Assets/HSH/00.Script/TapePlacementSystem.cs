using System.Collections.Generic;
using UnityEngine;

public class TapePlacementSystem : MonoBehaviour
{
    [Header("Tape Settings")]
    [SerializeField] private GameObject tapePrefab; // WarningTape 프리팹
    [SerializeField] private float tapeEndOffset = 1.5f; // 플레이어 앞 거리
    [SerializeField] private float tapeHeight = 1f; // 테이프 높이

    [Header("Visual Feedback")]
    [SerializeField] private Color placingTapeColor = new Color(1f, 1f, 0f, 0.7f); // 설치 중 테이프 색상
    [SerializeField] private Color completedTapeColor = Color.yellow; // 완료된 테이프 색상
    [SerializeField] private Color validPlacementColor = Color.green;
    [SerializeField] private Color invalidPlacementColor = Color.red;

    [Header("Anim")]
    public Animator anim;
    public int upperBodyLayer;

    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip placeSound;
    [SerializeField] private AudioClip removeSound;
    [SerializeField] private AudioClip enterSound;
    private AudioSource audioSource;

    // 설치 상태
    private enum PlacementState
    {
        Idle,           // 대기 중
        PlacingTape     // 테이프 설치 중 (시작점에서 끝점 찾는 중)
    }

    private PlacementState currentState = PlacementState.Idle;
    private TapePlacePoint currentStartPoint; // 현재 시작점
    private TapePlacePoint currentEndPoint; // 현재 끝점
    private GameObject currentTape; // 현재 설치 중인 테이프
    private LineRenderer currentTapeRenderer; // 현재 테이프의 LineRenderer
    private List<GameObject> allTapes = new List<GameObject>();

    // 시작점 정보 저장 (StartPoint를 나가도 유지하기 위해)
    private string startPointName;
    private Vector3 startPointPosition;

    private bool canPlaceStart = false; // StartPoint에서 E키 사용 가능
    private bool canPlaceEnd = false; // EndPoint에서 E키 사용 가능
    public bool isPlacing = false;

    void Start()
    {
        // 애니메이터 설정
        anim = GetComponentInChildren<Animator>();
        upperBodyLayer = anim.GetLayerIndex("UpperBody");
        anim.SetLayerWeight(upperBodyLayer, 0.1f);
        // 오디오 소스 설정
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        HandleInput();
        UpdateTapeEndPosition();
        
            

    }

    void HandleInput()
    {
        // E키 처리
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"E키 입력 - State: {currentState}, canPlaceStart: {canPlaceStart}, canPlaceEnd: {canPlaceEnd}");

            if (currentState == PlacementState.Idle && canPlaceStart && currentStartPoint != null)
            {
                // StartPoint에서 테이프 설치 시작
                StartTapePlacement();
            }
            else if (currentState == PlacementState.PlacingTape && canPlaceEnd && currentEndPoint != null)
            {
                // EndPoint에서 테이프 설치 완료
                Debug.Log("EndPoint에서 설치 완료 시도");
                CompleteTapePlacement();
            }
        }

        // R키: 모든 테이프 제거
        if (Input.GetKeyDown(KeyCode.R))
        {
            RemoveAllTapes();
        }

        // ESC키: 설치 취소
        if (Input.GetKeyDown(KeyCode.Escape) && currentState == PlacementState.PlacingTape)
        {
            CancelPlacement();
        }
    }

    void StartTapePlacement()
    {
        isPlacing = true;

        currentState = PlacementState.PlacingTape;

        anim.SetLayerWeight(upperBodyLayer, 1f);
        anim.SetBool("Tape", isPlacing);

        // 시작점 이름 저장 (나중에 사용하기 위해)
        startPointName = currentStartPoint.name;
        startPointPosition = currentStartPoint.GetPosition();

        // 테이프 오브젝트 생성
        if (tapePrefab != null)
        {
            currentTape = Instantiate(tapePrefab);
        }
        else
        {
            // 기본 테이프 생성
            currentTape = new GameObject("WarningTape_Placing");
            currentTapeRenderer = currentTape.AddComponent<LineRenderer>();
            currentTapeRenderer.startWidth = 0.1f;
            currentTapeRenderer.endWidth = 0.1f;
            currentTapeRenderer.material = new Material(Shader.Find("Sprites/Default"));
            currentTapeRenderer.material.color = placingTapeColor;
            currentTapeRenderer.positionCount = 2;
        }

        // LineRenderer 가져오기
        if (currentTapeRenderer == null)
        {
            currentTapeRenderer = currentTape.GetComponent<LineRenderer>();
            if (currentTapeRenderer == null)
            {
                currentTapeRenderer = currentTape.AddComponent<LineRenderer>();
            }
        }

        // 테이프 색상을 설치 중 색상으로 설정
        if (currentTapeRenderer != null)
        {
            currentTapeRenderer.material.color = placingTapeColor;
        }

        // 시작점 설정
        Vector3 startPos = startPointPosition;
        startPos.y = tapeHeight;

        // 초기 끝점 설정 (플레이어 앞)
        Vector3 endPos = transform.position + transform.forward * tapeEndOffset;
        endPos.y = tapeHeight;

        // LineRenderer 위치 설정
        currentTapeRenderer.SetPosition(0, startPos);
        currentTapeRenderer.SetPosition(1, endPos);

        // WarningTape 컴포넌트가 있으면 설정
        WarningTape warningTape = currentTape.GetComponent<WarningTape>();
        if (warningTape != null)
        {
            warningTape.SetPoints(startPos, endPos);
        }

        PlaySound(placeSound);
        Debug.Log($"테이프 설치 시작: {startPointName}에서");
    }

    void UpdateTapeEndPosition()
    {
        // 테이프 설치 중일 때만 업데이트
        if (currentState != PlacementState.PlacingTape || currentTapeRenderer == null) return;

        // 시작점 위치 (저장된 위치 사용)
        Vector3 startPos = startPointPosition;
        startPos.y = tapeHeight;

        // 끝점 위치 계산
        Vector3 endPos;
        if (canPlaceEnd && currentEndPoint != null)
        {
            // EndPoint 안에 있으면 EndPoint 위치 사용
            endPos = currentEndPoint.GetPosition();
            endPos.y = tapeHeight;

            // 유효한 위치임을 표시
            currentTapeRenderer.material.color = validPlacementColor;
        }
        else
        {
            // 플레이어 앞 위치 사용
            endPos = transform.position + transform.forward * tapeEndOffset;
            endPos.y = tapeHeight;

            // 설치 중임을 표시
            currentTapeRenderer.material.color = placingTapeColor;
        }

        // LineRenderer 업데이트
        currentTapeRenderer.SetPosition(0, startPos);
        currentTapeRenderer.SetPosition(1, endPos);

        // WarningTape 컴포넌트 업데이트
        WarningTape warningTape = currentTape?.GetComponent<WarningTape>();
        if (warningTape != null)
        {
            warningTape.SetPoints(startPos, endPos);
        }
    }

    void CompleteTapePlacement()
    {
        if (currentTape == null || currentEndPoint == null) return;

        Debug.Log($"테이프 설치 완료 시작: currentEndPoint = {currentEndPoint.name}");

        // 테이프 최종 위치 설정
        Vector3 startPos = startPointPosition;
        startPos.y = tapeHeight;
        Vector3 endPos = currentEndPoint.GetPosition();
        endPos.y = tapeHeight;

        // LineRenderer 최종 설정
        currentTapeRenderer.SetPosition(0, startPos);
        currentTapeRenderer.SetPosition(1, endPos);
        currentTapeRenderer.material.color = completedTapeColor;

        // WarningTape 컴포넌트 최종 설정
        WarningTape warningTape = currentTape.GetComponent<WarningTape>();
        if (warningTape != null)
        {
            warningTape.SetPoints(startPos, endPos);
        }

        // 테이프 이름 변경
        currentTape.name = $"WarningTape_{startPointName}_to_{currentEndPoint.name}";

        // 완성된 테이프를 리스트에 추가
        allTapes.Add(currentTape);

        // 상태 초기화
        currentState = PlacementState.Idle;
        currentTape = null;
        currentTapeRenderer = null;
        currentStartPoint = null;
        canPlaceStart = false;

        StopSound();
        Debug.Log($"테이프 설치 완료: {startPointName} → {currentEndPoint.name}");
        isPlacing = false;

        if(GameManager.Instance.status == GameStatus.TapeMission)
        {
            GameManager.Instance.status = GameStatus.TreeMission;
            GameManager.Instance.OnMissionComplete();
            GameManager.Instance.SetCompleteUI();
        }    

        GameManager.Instance.cc.camStatus = CamStatus.FPSMode;

        anim.SetLayerWeight(upperBodyLayer, 0.1f);
        anim.SetBool("Tape", isPlacing);
    }

    void CancelPlacement()
    {
        if (currentTape != null)
        {
            Destroy(currentTape);
            currentTape = null;
            currentTapeRenderer = null;
        }
        currentState = PlacementState.Idle;
        Debug.Log("테이프 설치가 취소되었습니다.");
    }

    void RemoveAllTapes()
    {
        if (allTapes.Count == 0)
        {
            Debug.Log("제거할 테이프가 없습니다.");
            return;
        }

        foreach (GameObject tape in allTapes)
        {
            if (tape != null) Destroy(tape);
        }

        allTapes.Clear();

        // 현재 설치 중인 테이프도 취소
        if (currentState == PlacementState.PlacingTape)
        {
            CancelPlacement();
        }

        PlaySound(removeSound);
        Debug.Log("모든 테이프가 제거되었습니다.");
    }

    // TapePlacePoint에서 호출하는 메서드들
    public void OnEnterStartPoint(TapePlacePoint point)
    {
        if (currentState == PlacementState.Idle)
        {
            currentStartPoint = point;
            canPlaceStart = true;
            PlaySound(enterSound);
        }
    }

    public void OnExitStartPoint(TapePlacePoint point)
    {
        if (currentStartPoint == point && currentState == PlacementState.Idle)
        {
            // Idle 상태일 때만 초기화 (테이프 설치 중에는 유지)
            currentStartPoint = null;
            canPlaceStart = false;
        }
    }

    public void OnEnterEndPoint(TapePlacePoint point)
    {
        // PlacingTape 상태이고 시작점과 다른 포인트일 때만
        if (currentState == PlacementState.PlacingTape && point != currentStartPoint)
        {
            currentEndPoint = point;
            canPlaceEnd = true;
            PlaySound(enterSound);
            Debug.Log($"EndPoint 진입: {point.name}, canPlaceEnd = {canPlaceEnd}");
        }
    }

    public void OnExitEndPoint(TapePlacePoint point)
    {
        if (currentEndPoint == point)
        {
            currentEndPoint = null;
            canPlaceEnd = false;
            Debug.Log($"EndPoint 나감: {point.name}");
        }
    }

    public bool IsPlacingTape()
    {
        return currentState == PlacementState.PlacingTape;
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void StopSound()
    {
        audioSource.Stop();  // 완전히 중단
    }
}
