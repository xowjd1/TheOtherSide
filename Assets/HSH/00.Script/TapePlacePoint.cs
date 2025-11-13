using UnityEngine;

public class TapePlacePoint : MonoBehaviour
{
    public enum PointType
    {
        StartPoint,
        EndPoint,
        AnyPoint  // 시작점이나 끝점 둘 다 가능
    }
    public PointType pointType = PointType.AnyPoint;


    private GameObject player;
    private TapePlacementSystem ps;
    private Renderer sphereRenderer;
    private bool isPlayerInside = false;

    void Start()
    {
        // 플레이어와 TapePlacementSystem 찾기
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            ps = FindFirstObjectByType<TapePlacementSystem>();
            if (ps != null)
            {
                player = ps.gameObject;
            }
        }
        else
        {
            ps = player.GetComponent<TapePlacementSystem>();
        }

        // 렌더러 가져오기
        sphereRenderer = GetComponent<Renderer>();

        // 콜라이더를 트리거로 설정
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }


    }

    private void OnTriggerEnter(Collider other)
    {
        // 플레이어가 들어왔는지 확인
        if (other.gameObject == player)
        {
            isPlayerInside = true;

            // TapePlacementSystem에 이 포인트 등록
            if (ps != null)
            {
                // 이름으로 구분
                if (gameObject.name == "StartPoint" || pointType == PointType.StartPoint)
                {
                    ps.OnEnterStartPoint(this);
                    Debug.Log($"StartPoint 트리거 진입: {gameObject.name}");
                }
                else if (gameObject.name == "EndPoint" || pointType == PointType.EndPoint)
                {
                    ps.OnEnterEndPoint(this);
                    Debug.Log($"EndPoint 트리거 진입: {gameObject.name}");
                }
                else // AnyPoint인 경우
                {
                    // 현재 상태에 따라 시작점 또는 끝점으로 처리
                    if (!ps.IsPlacingTape())
                    {
                        ps.OnEnterStartPoint(this);
                        Debug.Log($"AnyPoint를 StartPoint로 처리: {gameObject.name}");
                    }
                    else
                    {
                        ps.OnEnterEndPoint(this);
                        Debug.Log($"AnyPoint를 EndPoint로 처리: {gameObject.name}");
                    }
                }
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        // 플레이어가 나갔는지 확인
        if (other.gameObject == player)
        {
            isPlayerInside = false;

            // TapePlacementSystem에서 이 포인트 제거
            if (ps != null)
            {
                if (gameObject.name == "StartPoint" || pointType == PointType.StartPoint)
                {
                    ps.OnExitStartPoint(this);
                }
                else if (gameObject.name == "EndPoint" || pointType == PointType.EndPoint)
                {
                    ps.OnExitEndPoint(this);
                }
                else
                {
                    // AnyPoint인 경우 두 메서드 모두 호출 (시스템이 판단)
                    ps.OnExitStartPoint(this);
                    ps.OnExitEndPoint(this);
                }
            }

            Debug.Log($"플레이어가 {gameObject.name}에서 나감");
        }
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }
}