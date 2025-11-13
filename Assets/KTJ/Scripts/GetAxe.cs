using TMPro;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GetAxe : MonoBehaviour
{
    public Outline outlineTarget;
    public string playerTag = "Player";
    public TextMeshProUGUI text;

    [Header("활성/비활성 전환 대상")]
    public GameObject axeInPlayer; // 플레이어 손에 끼워둘 Axe 오브젝트
    public GameObject axeInMap;    // 맵에 놓여있는 Axe(이 스크립트 달린 오브젝트와 동일해도 됨)

    private bool inRange = false;
    private bool hasAxe  = false;

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void Awake()
    {
        if (outlineTarget) outlineTarget.enabled = false;
        if (text)          text.gameObject.SetActive(false);
        if (axeInPlayer)   axeInPlayer.SetActive(false); // 시작엔 손 도끼 비활성
    }

    void Update()
    {
        if (!inRange || hasAxe) return;

        if (Input.GetKeyDown(KeyCode.G))
            Pickup();
    }

    private void Pickup()
    {
        hasAxe = true;

        // 플레이어 손 도끼 활성화 / 맵 도끼 비활성화
        if (axeInPlayer) axeInPlayer.SetActive(true);
        if (axeInMap)    axeInMap.SetActive(false);

        // UI/하이라이트 정리
        if (outlineTarget) outlineTarget.enabled = false;
        if (text)          text.gameObject.SetActive(false);

        // 이 픽업 오브젝트 자체 비활성
        gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other) || hasAxe) return;

        inRange = true;
        if (outlineTarget) outlineTarget.enabled = true;
        if (text)          text.gameObject.SetActive(true); // “G: 도끼 줍기” 같은 안내 텍스트
    }

    void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other)) return;

        inRange = false;
        if (outlineTarget) outlineTarget.enabled = false;
        if (text)          text.gameObject.SetActive(false);
    }

    bool IsPlayer(Collider c)
    {
        // 플레이어 태그 또는 CharacterController 기준으로 판별
        return c.CompareTag(playerTag) || c.GetComponent<CharacterController>() != null;
    }
}
