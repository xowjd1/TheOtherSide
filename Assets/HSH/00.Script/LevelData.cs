using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PipeLevel", menuName = "Puzzle/Pipe Level")]
public class LevelData : ScriptableObject
{
    public int levelNumber;
    public int gridWidth = 7;
    public int gridHeight = 7;
    public Vector2Int startPosition;
    public Vector2Int endPosition;
    public List<PresetTile> presetTiles;
    public int targetMoves;
    public float targetTime;

    [System.Serializable]
    public class PresetTile
    {
        public Vector2Int position;
        public PipeType type;
        public int initialRotation;
        public bool isLocked; // 회전 불가능한 타일
    }
}