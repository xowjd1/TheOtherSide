using UnityEngine;

public class PipeTile
{
    public PipeType type;
    public int rotation; // 0, 90, 180, 270
    public bool isConnected;
    public bool[] connections; // 상(0), 우(1), 하(2), 좌(3)
    public GameObject tileObject;

    public PipeTile(PipeType _type, int _rotation)
    {
        type = _type;
        rotation = _rotation % 360; // 정규화
        isConnected = false;
        connections = new bool[4];
        UpdateConnections();
    }

    public void Rotate()
    {
        // 시계방향으로 90도 회전
        rotation = (rotation + 90) % 360;
        UpdateConnections();
    }

    public void UpdateConnections()
    {
        // 연결 배열 초기화
        connections = new bool[4] { false, false, false, false };

        // 타입별 기본 연결 설정
        switch (type)
        {
            case PipeType.Empty:
                // 연결 없음
                break;

            case PipeType.Straight:
                // 회전값에 따른 연결
                if (rotation == 0 || rotation == 180)
                {
                    // 수평 연결 (좌-우)
                    connections[1] = true; // 우
                    connections[3] = true; // 좌
                }
                else if (rotation == 90 || rotation == 270)
                {
                    // 수직 연결 (상-하)
                    connections[0] = true; // 상
                    connections[2] = true; // 하
                }
                break;

            case PipeType.Corner:
                // 회전에 따른 연결 설정
                if (rotation == 180)
                {
                    // └ 모양 (상-우)
                    connections[0] = true; // 상
                    connections[1] = true; // 우
                }
                else if (rotation == 270)
                {
                    // ┌ 모양 (우-하)
                    connections[1] = true; // 우
                    connections[2] = true; // 하
                }
                else if (rotation == 0)
                {
                    // ┐ 모양 (하-좌)
                    connections[2] = true; // 하
                    connections[3] = true; // 좌
                }
                else if (rotation == 90)
                {
                    // ┘ 모양 (좌-상)
                    connections[3] = true; // 좌
                    connections[0] = true; // 상
                }
                break;

            case PipeType.TShape:
                // T자 연결 (3방향)
                if (rotation == 0)
                {
                    // ┴ 모양 (상-우-좌)
                    connections[0] = true; // 상
                    connections[1] = true; // 우
                    connections[3] = true; // 좌
                }
                else if (rotation == 90)
                {
                    // ├ 모양 (상-우-하)
                    connections[0] = true; // 상
                    connections[1] = true; // 우
                    connections[2] = true; // 하
                }
                else if (rotation == 180)
                {
                    // ┬ 모양 (하-우-좌)
                    connections[1] = true; // 우
                    connections[2] = true; // 하
                    connections[3] = true; // 좌
                }
                else if (rotation == 270)
                {
                    // ┤ 모양 (상-하-좌)
                    connections[0] = true; // 상
                    connections[2] = true; // 하
                    connections[3] = true; // 좌
                }
                break;

            case PipeType.Cross:
                // 십자 연결 (모든 방향)
                connections[0] = true; // 상
                connections[1] = true; // 우
                connections[2] = true; // 하
                connections[3] = true; // 좌
                break;

            case PipeType.Start:
                // 시작점 - 우측으로만 연결
                connections[1] = true; // 우
                break;

            case PipeType.End:
                // 끝점 - 좌측으로만 연결
                connections[3] = true; // 좌
                break;
        }

        // 디버그 출력
        Debug.Log($"[{type}] 회전: {rotation}도, 연결: 상({connections[0]}) 우({connections[1]}) 하({connections[2]}) 좌({connections[3]})");
    }
}

public enum PipeType
{
    Empty,
    Straight,
    Corner,
    TShape,
    Cross,
    Start,
    End
}