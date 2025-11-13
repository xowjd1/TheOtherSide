using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;

public enum GameStatus
{
    Ready,
    ShovelMission,
    TapeMission,
    TreeMission,
    PipeMission,
    Ending
}

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<GameManager>(FindObjectsInactive.Exclude);

                if (_instance == null)
                {
                    GameObject go = new GameObject("GameManager");
                    _instance = go.AddComponent<GameManager>();
                }
            }
            return _instance;
        }
    }
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip bgmClip;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool loop = false;
    [SerializeField][Range(0f, 1f)] private float volume = 0.7f;

    public GameStatus status;
    public H_CamController cc;
    public Transform camPosition;

    [Header("Start UI References")]
    public GameObject Panel_Start;
    public GameObject Panel_OS;
    public Button btn_Next;
    public GameObject Panel_Guide;
    public Button btn_FinishTutorial;

    [Header("Update UI References")]
    public GameObject Panel_Update;
    public GameObject Panel_MissionAlarm;
    public TextMeshProUGUI TMP_MissionAlarmText;
    public GameObject Panel_Warning;
    public GameObject Panel_Complete;
    public GameObject Panel_MissionPopUP;
    public TextMeshProUGUI TMP_MissionNum;
    public TextMeshProUGUI TMP_MissionText;
    public GameObject panel_Ending;
    public Button btn_Quit;

    [Header("Mission System")]
    [SerializeField] private DirectionalArrowUI arrowUI;
    [SerializeField] private Transform[] missionLocations; // 미션 위치들
    [SerializeField] private float completionDistance = 3f; // 도달 판정 거리

    private int currentMissionIndex = 0;

    [Header("Bool references")]
    public bool isUIWorking = false;




    // 코루틴 추적용
    private Coroutine alarmCoroutine;

    private void Awake()
    {
        // 싱글톤 패턴 보장
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        status = GameStatus.Ready;

        // NULL 체크 강화
        if (btn_Next != null)
        {
            btn_Next.onClick.AddListener(NextUI);
            Debug.Log("btn_Next 연결됨");
        }
        else
        {
            Debug.LogError("btn_Next가 할당되지 않았습니다!");
        }

        if (btn_FinishTutorial != null)
        {
            btn_FinishTutorial.onClick.AddListener(FinishTutorial);
            Debug.Log("btn_FinishTutorial 연결됨");
        }
        else
        {
            Debug.LogError("btn_FinishTutorial이 할당되지 않았습니다!");
        }

        btn_Quit.onClick.AddListener(QuitGame);

        // UI 초기 설정
        if (Panel_Start != null) Panel_Start.SetActive(true);
        else Debug.LogError("Panel_Start가 할당되지 않았습니다!");

        if (Panel_OS != null) Panel_OS.SetActive(true);
        else Debug.LogError("Panel_OS가 할당되지 않았습니다!");

        // 첫 번째 미션 설정
        if (missionLocations.Length > 0)
        {
            StartMission(0);
        }

        // AudioSource 설정
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // 설정 적용
        audioSource.clip = bgmClip;
        audioSource.loop = loop;
        audioSource.volume = volume;

        // 자동 재생
        if (playOnStart && bgmClip != null)
        {
            audioSource.Play();
        }
        // 모든 UI 레퍼런스 체크
        //CheckUIReferences();
    }
    public void QuitGame()
    {
#if UNITY_EDITOR
        // 에디터에서 실행 중일 때
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBGL
            // WebGL 빌드에서는 종료 불가능 (브라우저 탭 닫기로만 가능)
            Debug.Log("WebGL builds cannot be quit programmatically.");
#else
            // 빌드된 게임에서
            Application.Quit();
#endif
    }
    public void StartMission(int index)
    {
        if (index < missionLocations.Length)
        {
            currentMissionIndex = index;
            arrowUI.SetTarget(missionLocations[index]);
            arrowUI.ShowArrow(true);

            Debug.Log($"Mission {index + 1} Started: Go to {missionLocations[index].name}");
        }
    }

    public void OnMissionComplete()
    {
        Debug.Log($"Mission {currentMissionIndex + 1} Complete!");

        // 다음 미션으로
        currentMissionIndex++;

        if (currentMissionIndex < missionLocations.Length)
        {
            StartMission(currentMissionIndex);
        }
        else
        {
            // 모든 미션 완료
            Debug.Log("All Missions Complete!");
            arrowUI.ShowArrow(false);
        }
    }
    //void CheckUIReferences()
    //{
    //    Debug.Log("=== UI Reference Check ===");
    //    Debug.Log($"Panel_Update: {Panel_Update != null}");
    //    Debug.Log($"Panel_MissionAlarm: {Panel_MissionAlarm != null}");
    //    Debug.Log($"TMP_MissionAlarmText: {TMP_MissionAlarmText != null}");
    //    Debug.Log($"Panel_MissionPopUP: {Panel_MissionPopUP != null}");
    //    Debug.Log($"TMP_MissionNum: {TMP_MissionNum != null}");
    //    Debug.Log($"TMP_MissionText: {TMP_MissionText != null}");
    //    Debug.Log("========================");

    //    // 할당되지 않은 레퍼런스 경고
    //    if (Panel_MissionAlarm == null)
    //        Debug.LogError("Panel_MissionAlarm이 Inspector에서 할당되지 않았습니다!");
    //    if (TMP_MissionAlarmText == null)
    //        Debug.LogError("TMP_MissionAlarmText가 Inspector에서 할당되지 않았습니다!");
    //    if (TMP_MissionNum == null)
    //        Debug.LogError("TMP_MissionNum이 Inspector에서 할당되지 않았습니다!");
    //    if (TMP_MissionText == null)
    //        Debug.LogError("TMP_MissionText가 Inspector에서 할당되지 않았습니다!");
    //}

    void NextUI()
    {
        //Debug.Log("NextUI 호출됨");

        if (Panel_OS != null) Panel_OS.SetActive(false);
        if (Panel_Guide != null) Panel_Guide.SetActive(true);
    }

    void FinishTutorial()
    {
        //Debug.Log("FinishTutorial 호출됨");

        // 커서 잠금
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (Panel_Start != null) Panel_Start.SetActive(false);
        if (Panel_Update != null)
        {
            Panel_Update.SetActive(true);
            //Debug.Log("Panel_Update 활성화됨");
        }
        else
        {
            Debug.LogError("Panel_Update가 null입니다!");
        }

        status = GameStatus.ShovelMission;
        UpdateGameState(status);
    }

    public void UpdateGameState(GameStatus newStatus)
    {
        Debug.Log($"UpdateGameState 호출됨: {newStatus}");
        status = newStatus;

        switch (status)
        {
            case GameStatus.Ready:
                Debug.Log("게임 준비 상태");
                break;

            case GameStatus.ShovelMission:
                Debug.Log("삽 미션 시작");

                // NULL 체크와 함께 함수 호출
                if (Panel_MissionAlarm != null)
                {
                    SetAlarmText(Panel_MissionAlarm);
                }
                else
                {
                    Debug.LogError("Panel_MissionAlarm이 null입니다!");
                }
                if (TMP_MissionAlarmText != null)
                {
                    SetTextUI(TMP_MissionAlarmText, "마우스 왼쪽 버튼을 눌러 흙을 퍼내세요!");
                }
                else
                {
                    Debug.LogError("TMP_MissionAlarmText가 null입니다!");
                }

                // Panel_MissionPopUP도 활성화해야 하는지 확인
                if (Panel_MissionPopUP != null)
                {
                    Panel_MissionPopUP.SetActive(true);
                    Debug.Log("Panel_MissionPopUP 활성화됨");
                }

                if (TMP_MissionNum != null)
                {
                    SetTextUI(TMP_MissionNum, "[MISSION] 01 / 04");
                }
                else
                {
                    Debug.LogError("TMP_MissionNum이 null입니다!");
                }

                if (TMP_MissionText != null)
                {
                    SetTextUI(TMP_MissionText, "삽을 이용해 흙더미를 뚫고 밖으로 나가기 ");
                }
                else
                {
                    Debug.LogError("TMP_MissionText가 null입니다!");
                }
                break;

            case GameStatus.TapeMission:
                Debug.Log("테이프 미션 시작");

                if (Panel_MissionAlarm != null)
                    SetAlarmText(Panel_MissionAlarm);

                if (TMP_MissionAlarmText != null)
                    SetTextUI(TMP_MissionAlarmText, "통제선을 설치해 다리를 막아야 합니다.");

                if (TMP_MissionNum != null)
                    SetTextUI(TMP_MissionNum, "[MISSION] 02 / 04");

                if (TMP_MissionText != null)
                    SetTextUI(TMP_MissionText, "범람 지역을 통제선 표시해 안전 구역 확보 ");
                break;

            case GameStatus.TreeMission:
                Debug.Log("나무 미션 시작");

                if (Panel_MissionAlarm != null)
                    SetAlarmText(Panel_MissionAlarm);

                if (TMP_MissionAlarmText != null)
                    SetTextUI(TMP_MissionAlarmText, " 마우스 왼쪽 버튼을 눌러 나무를 베세요!");

                if (TMP_MissionNum != null)
                    SetTextUI(TMP_MissionNum, "[MISSION] 03 / 04");

                if (TMP_MissionText != null)
                    SetTextUI(TMP_MissionText, "주민들이 탈출할 수 있도록 도와야 합니다.  ");
                break;

            case GameStatus.PipeMission:
                Debug.Log("파이프 미션 시작");

                if (Panel_MissionAlarm != null)
                    SetAlarmText(Panel_MissionAlarm);

                if (TMP_MissionAlarmText != null)
                    SetTextUI(TMP_MissionAlarmText, "파이프를 연결해 물길을 복구하세요!");

                if (TMP_MissionNum != null)
                    SetTextUI(TMP_MissionNum, "[MISSION] 04 / 04");

                if (TMP_MissionText != null)
                    SetTextUI(TMP_MissionText, "장치가 작동하려면 물이 흘러야 합니다. ");
                break;

            case GameStatus.Ending:
                Debug.Log("엔딩");
                break;

            default:
                Debug.LogWarning($"처리되지 않은 상태: {status}");
                break;
        }
    }

    void SetTextUI(TextMeshProUGUI tmp, string text)
    {
        if (tmp == null)
        {
            Debug.LogError("SetTextUI: TextMeshProUGUI 컴포넌트가 null입니다!");
            return;
        }

        tmp.text = text;
        Debug.Log($"텍스트 설정됨: {text}");

        // 텍스트가 속한 GameObject가 활성화되어 있는지 확인
        if (!tmp.gameObject.activeInHierarchy)
        {
            Debug.LogWarning($"TextMeshProUGUI의 GameObject가 비활성화 상태입니다: {tmp.gameObject.name}");
        }
    }

    public void SetCompleteUI()
    {
        // 이전 코루틴이 있다면 중지
        if (alarmCoroutine != null)
        {
            StopCoroutine(alarmCoroutine);
        }

        alarmCoroutine = StartCoroutine(MakeCompleteUIDuration(3));

    }

    public void SetAlarmText(GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogError("SetAlarmText: GameObject가 null입니다!");
            return;
        }

        obj.SetActive(true);
        Debug.Log($"알람 패널 활성화됨: {obj.name}");

        // 이전 코루틴이 있다면 중지
        if (alarmCoroutine != null)
        {
            StopCoroutine(alarmCoroutine);
        }

        alarmCoroutine = StartCoroutine(MakeDuration(5, obj));
    }

    IEnumerator MakeDuration(float duration, GameObject obj)
    {
        Debug.Log($"{duration}초 대기 시작");
        yield return new WaitForSeconds(duration);

        if (obj != null)
        {
            obj.SetActive(false);
            Debug.Log($"알람 패널 비활성화됨: {obj.name}");
        }

        alarmCoroutine = null;
    }

    IEnumerator MakeCompleteUIDuration(float duration)
    {
        Panel_Complete.SetActive(true);
        isUIWorking = true;
        Debug.Log($"{duration}초 대기 시작");

        yield return new WaitForSeconds(duration);

        if (Panel_Complete != null)
        {
            Panel_Complete.SetActive(false);
            Debug.Log($"알람 패널 비활성화됨: {Panel_Complete.name}");
        }

        isUIWorking = false;
        if(status != GameStatus.TreeMission)
        {
            UpdateGameState(status);
        }

        alarmCoroutine = null;
    }

    //[ContextMenu("Check All References")]
    //void CheckAllReferences()
    //{
    //    CheckUIReferences();
    //}
}