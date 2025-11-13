using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using static PipeTile;

public class PipePuzzleManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject tilePrefab;
    public Transform gridContainer;
    public GridLayoutGroup gridLayout;

    [Header("Pipe Sprites")]
    public Sprite emptySprite;
    public Sprite straightSprite;
    public Sprite cornerSprite;
    public Sprite tShapeSprite;
    public Sprite crossSprite;
    public Sprite startSprite;
    public Sprite endSprite;

    [Header("Game Settings")]
    public int gridWidth = 7;
    public int gridHeight = 7;
    public Color normalColor = Color.white;
    public Color connectedColor = Color.green;
    public Color startColor = new Color(0.2f, 0.5f, 1f);
    public Color endColor = new Color(1f, 0.5f, 0.2f);

    [Header("UI Elements")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI movesText;
    public TextMeshProUGUI timerText;
    public Button resetButton;
    public GameObject gamePanel;
    public GameObject successPanel;
    public TextMeshProUGUI successMessage;
    public Button nextLevelButton;
    public PipeGameInteraction gameInteraction;


    private PipeTile[,] grid;
    private Vector2Int startPos;
    private Vector2Int endPos;
    private int currentLevel = 1;
    private int moveCount = 0;
    private float gameTime = 0;
    private bool isPlaying = true;
    private List<Vector2Int> solutionPath;
    private bool isAnimating = false;

    private void OnEnable()
    {
        if (!ValidateComponents())
        {
            Debug.LogError("필수 컴포넌트가 없습니다. Inspector를 확인해주세요!");
            return;
        }

        InitializeGrid();
        GenerateLevel();

        if (resetButton != null) resetButton.onClick.AddListener(ResetLevel);
        if (nextLevelButton != null) nextLevelButton.onClick.AddListener(NextLevel);
    }

    bool ValidateComponents()
    {
        bool isValid = true;

        if (tilePrefab == null)
        {
            Debug.LogError("Tile Prefab이 할당되지 않았습니다!");
            isValid = false;
        }

        if (gridContainer == null)
        {
            Debug.LogError("Grid Container가 할당되지 않았습니다!");
            isValid = false;
        }

        if (gridLayout == null)
        {
            gridLayout = gridContainer?.GetComponent<GridLayoutGroup>();
            if (gridLayout == null)
            {
                Debug.LogError("Grid Layout Group을 찾을 수 없습니다!");
                isValid = false;
            }
        }

        return isValid;
    }


    
    void Update()
    {
        if (isPlaying)
        {
            gameTime += Time.deltaTime;
            UpdateTimer();
        }
    }

    void InitializeGrid()
    {
        grid = new PipeTile[gridHeight, gridWidth];

        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = gridWidth;
        gridLayout.cellSize = new Vector2(80, 80);
        gridLayout.spacing = new Vector2(5, 5);
    }

    void GenerateLevel()
    {
        ClearGrid();

        if (tilePrefab == null || gridContainer == null)
        {
            Debug.LogError("필수 컴포넌트가 없습니다!");
            return;
        }

        List<PipeType> availablePipes = new List<PipeType> {
            PipeType.Straight, PipeType.Corner, PipeType.TShape, PipeType.Cross
        };

        // 시작점과 끝점 설정
        startPos = new Vector2Int(0, Random.Range(1, gridHeight - 1));
        endPos = new Vector2Int(gridWidth - 1, Random.Range(1, gridHeight - 1));

        // 솔루션 경로 생성
        GenerateSolutionPath();

        // 그리드 생성
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                GameObject tileObj = Instantiate(tilePrefab, gridContainer);
                tileObj.name = $"Tile_{x}_{y}";

                PipeTileUI tileUI = tileObj.GetComponent<PipeTileUI>();
                if (tileUI == null)
                {
                    tileUI = tileObj.AddComponent<PipeTileUI>();
                }

                PipeType pipeType;
                int rotation = 0;

                if (x == startPos.x && y == startPos.y)
                {
                    pipeType = PipeType.Start;
                    rotation = 0;
                }
                else if (x == endPos.x && y == endPos.y)
                {
                    pipeType = PipeType.End;
                    rotation = 0;
                }
                else if (IsOnSolutionPath(new Vector2Int(x, y)))
                {
                    pipeType = GetPipeTypeForSolution(new Vector2Int(x, y));
                    // 올바른 회전값 설정 후 랜덤하게 섞기
                    rotation = GetCorrectRotationForSolution(new Vector2Int(x, y));
                    // 퍼즐을 위해 랜덤하게 회전 추가
                    rotation = (rotation + Random.Range(1, 4) * 90) % 360;
                }
                else
                {
                    float randomValue = Random.Range(0f, 1f);
                    if (randomValue < 0.3f)
                    {
                        pipeType = PipeType.Empty;
                        rotation = 0;
                    }
                    else
                    {
                        pipeType = availablePipes[Random.Range(0, availablePipes.Count)];
                        rotation = Random.Range(0, 4) * 90;
                    }
                }

                grid[y, x] = new PipeTile(pipeType, rotation);
                grid[y, x].tileObject = tileObj;
                grid[y, x].UpdateConnections();

                tileObj.transform.rotation = Quaternion.Euler(0, 0, rotation);

                tileUI.Initialize(this, new Vector2Int(x, y), grid[y, x]);
                UpdateTileVisual(new Vector2Int(x, y));
            }
        }

        UpdateUI();
        CheckSolution(); // 초기 상태 체크
    }

    void GenerateSolutionPath()
    {
        solutionPath = FindPath(startPos, endPos);

        if (solutionPath == null || solutionPath.Count == 0)
        {
            Debug.LogError("경로를 생성할 수 없습니다!");
            solutionPath = new List<Vector2Int>();
            solutionPath.Add(startPos);
            solutionPath.Add(endPos);
        }

        Debug.Log($"솔루션 경로 생성 완료: {solutionPath.Count}개 타일");
    }

    List<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
    {
        Dictionary<Vector2Int, Vector2Int?> cameFrom = new Dictionary<Vector2Int, Vector2Int?>();
        Dictionary<Vector2Int, float> gScore = new Dictionary<Vector2Int, float>();
        Dictionary<Vector2Int, float> fScore = new Dictionary<Vector2Int, float>();
        List<Vector2Int> openSet = new List<Vector2Int>();

        openSet.Add(start);
        gScore[start] = 0;
        fScore[start] = Vector2Int.Distance(start, end);

        while (openSet.Count > 0)
        {
            Vector2Int current = openSet[0];
            float lowestFScore = fScore[current];
            foreach (var node in openSet)
            {
                if (fScore.ContainsKey(node) && fScore[node] < lowestFScore)
                {
                    current = node;
                    lowestFScore = fScore[node];
                }
            }

            if (current == end)
            {
                List<Vector2Int> path = new List<Vector2Int>();
                Vector2Int? node = current;
                while (node.HasValue)
                {
                    path.Add(node.Value);
                    node = cameFrom.ContainsKey(node.Value) ? cameFrom[node.Value] : null;
                }
                path.Reverse();
                return path;
            }

            openSet.Remove(current);

            Vector2Int[] neighbors = {
                current + Vector2Int.up,
                current + Vector2Int.down,
                current + Vector2Int.left,
                current + Vector2Int.right
            };

            foreach (var neighbor in neighbors)
            {
                if (neighbor.x < 0 || neighbor.x >= gridWidth ||
                    neighbor.y < 0 || neighbor.y >= gridHeight)
                    continue;

                float tentativeGScore = gScore[current] + 1;

                if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = tentativeGScore + Vector2Int.Distance(neighbor, end);

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return CreateDirectPath(start, end);
    }

    List<Vector2Int> CreateDirectPath(Vector2Int start, Vector2Int end)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int current = start;
        path.Add(current);

        while (current.x != end.x)
        {
            current.x += (end.x > current.x) ? 1 : -1;
            path.Add(current);
        }

        while (current.y != end.y)
        {
            current.y += (end.y > current.y) ? 1 : -1;
            path.Add(current);
        }

        return path;
    }

    bool IsOnSolutionPath(Vector2Int pos)
    {
        return solutionPath != null && solutionPath.Contains(pos);
    }

    PipeType GetPipeTypeForSolution(Vector2Int pos)
    {
        int index = solutionPath.IndexOf(pos);
        if (index == -1) return PipeType.Empty;

        Vector2Int? prev = index > 0 ? solutionPath[index - 1] : (Vector2Int?)null;
        Vector2Int? next = index < solutionPath.Count - 1 ? solutionPath[index + 1] : (Vector2Int?)null;

        bool connectUp = false, connectDown = false, connectLeft = false, connectRight = false;

        if (prev.HasValue)
        {
            Vector2Int diff = prev.Value - pos;
            if (diff == Vector2Int.up) connectUp = true;
            else if (diff == Vector2Int.down) connectDown = true;
            else if (diff == Vector2Int.left) connectLeft = true;
            else if (diff == Vector2Int.right) connectRight = true;
        }

        if (next.HasValue)
        {
            Vector2Int diff = next.Value - pos;
            if (diff == Vector2Int.up) connectUp = true;
            else if (diff == Vector2Int.down) connectDown = true;
            else if (diff == Vector2Int.left) connectLeft = true;
            else if (diff == Vector2Int.right) connectRight = true;
        }

        if ((connectLeft && connectRight) || (connectUp && connectDown))
        {
            return PipeType.Straight;
        }
        else if ((connectUp && connectRight) || (connectUp && connectLeft) ||
                 (connectDown && connectRight) || (connectDown && connectLeft))
        {
            return PipeType.Corner;
        }

        return PipeType.Straight;
    }

    int GetCorrectRotationForSolution(Vector2Int pos)
    {
        int index = solutionPath.IndexOf(pos);
        if (index == -1) return 0;

        Vector2Int? prev = index > 0 ? solutionPath[index - 1] : (Vector2Int?)null;
        Vector2Int? next = index < solutionPath.Count - 1 ? solutionPath[index + 1] : (Vector2Int?)null;

        PipeType type = GetPipeTypeForSolution(pos);

        if (type == PipeType.Straight)
        {
            if ((prev.HasValue && prev.Value.x != pos.x) ||
                (next.HasValue && next.Value.x != pos.x))
            {
                return 0; // 수평
            }
            else
            {
                return 90; // 수직
            }
        }
        else if (type == PipeType.Corner)
        {
            bool connectUp = false, connectDown = false, connectLeft = false, connectRight = false;

            if (prev.HasValue)
            {
                Vector2Int diff = prev.Value - pos;
                if (diff == Vector2Int.up) connectUp = true;
                else if (diff == Vector2Int.down) connectDown = true;
                else if (diff == Vector2Int.left) connectLeft = true;
                else if (diff == Vector2Int.right) connectRight = true;
            }

            if (next.HasValue)
            {
                Vector2Int diff = next.Value - pos;
                if (diff == Vector2Int.up) connectUp = true;
                else if (diff == Vector2Int.down) connectDown = true;
                else if (diff == Vector2Int.left) connectLeft = true;
                else if (diff == Vector2Int.right) connectRight = true;
            }

            if (connectUp && connectRight) return 0;
            if (connectRight && connectDown) return 90;
            if (connectDown && connectLeft) return 180;
            if (connectLeft && connectUp) return 270;
        }

        return 0;
    }

    public void RotateTile(Vector2Int pos)
    {
        if (isAnimating) return;

        if (grid[pos.y, pos.x].type == PipeType.Empty ||
            grid[pos.y, pos.x].type == PipeType.Start ||
            grid[pos.y, pos.x].type == PipeType.End)
            return;

        grid[pos.y, pos.x].Rotate();
        grid[pos.y, pos.x].UpdateConnections();

        moveCount++;
        UpdateUI();

        StartCoroutine(RotateAnimation(grid[pos.y, pos.x].tileObject.transform, pos));
    }

    IEnumerator RotateAnimation(Transform tile, Vector2Int pos)
    {
        isAnimating = true;
        float duration = 0.2f;
        float elapsed = 0;

        float startRotation = tile.eulerAngles.z;
        float endRotation = grid[pos.y, pos.x].rotation;

        float diff = Mathf.DeltaAngle(startRotation, endRotation);
        endRotation = startRotation + diff;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t);

            float currentRotation = Mathf.Lerp(startRotation, endRotation, t);
            tile.rotation = Quaternion.Euler(0, 0, currentRotation);
            yield return null;
        }

        tile.rotation = Quaternion.Euler(0, 0, grid[pos.y, pos.x].rotation);

        UpdateTileVisual(pos);
        CheckSolution();

        isAnimating = false;
    }

    void UpdateTileVisual(Vector2Int pos)
    {
        PipeTile tile = grid[pos.y, pos.x];
        if (tile.tileObject == null) return;

        Image img = tile.tileObject.GetComponent<Image>();

        switch (tile.type)
        {
            case PipeType.Empty:
                img.sprite = emptySprite;
                img.color = normalColor;
                break;
            case PipeType.Straight:
                img.sprite = straightSprite;
                img.color = tile.isConnected ? connectedColor : normalColor;
                break;
            case PipeType.Corner:
                img.sprite = cornerSprite;
                img.color = tile.isConnected ? connectedColor : normalColor;
                break;
            case PipeType.TShape:
                img.sprite = tShapeSprite;
                img.color = tile.isConnected ? connectedColor : normalColor;
                break;
            case PipeType.Cross:
                img.sprite = crossSprite;
                img.color = tile.isConnected ? connectedColor : normalColor;
                break;
            case PipeType.Start:
                img.sprite = startSprite;
                img.color = startColor;
                break;
            case PipeType.End:
                img.sprite = endSprite;
                img.color = endColor;
                break;
        }
    }

    void CheckSolution()
    {
        // 모든 타일의 연결 상태 초기화
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                grid[y, x].isConnected = false;
            }
        }

        // BFS로 경로 찾기
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        List<Vector2Int> connectedPath = new List<Vector2Int>();

        queue.Enqueue(startPos);
        visited.Add(startPos);
        connectedPath.Add(startPos);

        bool foundPath = false;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (current == endPos)
            {
                foundPath = true;
            }

            Vector2Int[] directions = {
                Vector2Int.up,    // 상 (0)
                Vector2Int.right, // 우 (1)
                Vector2Int.down,  // 하 (2)
                Vector2Int.left   // 좌 (3)
            };

            for (int i = 0; i < 4; i++)
            {
                Vector2Int next = current + directions[i];

                if (next.x < 0 || next.x >= gridWidth || next.y < 0 || next.y >= gridHeight)
                    continue;

                if (visited.Contains(next) || grid[next.y, next.x].type == PipeType.Empty)
                    continue;

                if (CanConnect(current, next, i))
                {
                    visited.Add(next);
                    queue.Enqueue(next);
                    connectedPath.Add(next);
                }
            }
        }

        // 게임 클리어 시에만 연결된 타일들의 색상 변경
        if (foundPath && visited.Contains(endPos))
        {
            // 연결된 경로의 타일들만 isConnected = true
            foreach (var pos in connectedPath)
            {
                grid[pos.y, pos.x].isConnected = true;
            }

            // 모든 타일 시각적 업데이트
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    UpdateTileVisual(new Vector2Int(x, y));
                }
            }

            OnLevelComplete();
        }
        else
        {
            // 클리어하지 못한 경우 모든 타일을 기본 색상으로
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    UpdateTileVisual(new Vector2Int(x, y));
                }
            }
        }

        Debug.Log($"경로 탐색 완료 - 연결됨: {foundPath}, 방문: {visited.Count}");
    }

    bool CanConnect(Vector2Int from, Vector2Int to, int direction)
    {
        PipeTile fromTile = grid[from.y, from.x];
        PipeTile toTile = grid[to.y, to.x];

        // 연결 배열 확인
        if (fromTile.connections == null || toTile.connections == null ||
            fromTile.connections.Length != 4 || toTile.connections.Length != 4)
        {
            Debug.LogError($"연결 배열 문제: from({from}), to({to})");
            return false;
        }

        // from 타일에서 direction 방향으로 나갈 수 있는지
        bool canExitFrom = fromTile.connections[direction];

        // to 타일에서 반대 방향으로 들어올 수 있는지
        int oppositeDir = (direction + 2) % 4;
        bool canEnterTo = toTile.connections[oppositeDir];

        // 디버그 로그 추가
        if (canExitFrom && canEnterTo)
        {
            Debug.Log($"✓ 연결: [{fromTile.type}]({from.x},{from.y}) → [{toTile.type}]({to.x},{to.y}) | 방향:{direction}");
        }

        return canExitFrom && canEnterTo;
    }

    void OnLevelComplete()
    {
        isPlaying = false;
        ShowMessage($"레벨 {currentLevel} 완료!\n이동 횟수: {moveCount}\n시간: {FormatTime(gameTime)}", true);

        if (successPanel != null)
        {
            successPanel.SetActive(true);
            if (successMessage != null)
                successMessage.text = $"축하합니다!\n레벨 {currentLevel} 클리어!";
        }
    }

    void ShowMessage(string message, bool isSuccess)
    {
        Debug.Log(message);
    }

    void ResetLevel()
    {
        moveCount = 0;
        gameTime = 0;
        isPlaying = true;

        if (successPanel != null)
            successPanel.SetActive(false);

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (grid[y, x].type != PipeType.Empty &&
                    grid[y, x].type != PipeType.Start &&
                    grid[y, x].type != PipeType.End)
                {
                    // 랜덤 회전
                    int newRotation = Random.Range(0, 4) * 90;
                    grid[y, x].rotation = newRotation;
                    grid[y, x].UpdateConnections();
                    grid[y, x].isConnected = false;

                    grid[y, x].tileObject.transform.rotation = Quaternion.Euler(0, 0, newRotation);
                    UpdateTileVisual(new Vector2Int(x, y));
                }
            }
        }

        CheckSolution();
        UpdateUI();
    }

    void NextLevel()
    {
        gameInteraction.OnPuzzleComplete();
        Debug.Log("퍼즐 끝!");

        //currentLevel++;
        //moveCount = 0;
        //gameTime = 0;
        //isPlaying = true;

        //if (successPanel != null)
        //    successPanel.SetActive(false);

        //GenerateLevel();
    }

    void ClearGrid()
    {
        foreach (Transform child in gridContainer)
        {
            Destroy(child.gameObject);
        }
    }

    void UpdateUI()
    {
        if (levelText != null)
            levelText.text = "연결해주세요!";

        if (movesText != null)
            movesText.text = $"Moves: {moveCount}";
    }

    void UpdateTimer()
    {
        if (timerText != null)
            timerText.text = $"Time: {FormatTime(gameTime)}";
    }

    string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}