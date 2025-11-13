using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(Button))]
public class PipeTileUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private PipePuzzleManager manager;
    private Vector2Int gridPosition;
    private PipeTile tileData;
    private Image image;
    private Button button;

    [Header("Visual Effects")]
    public float hoverScale = 1.1f;
    public float clickScale = 0.95f;

    void Awake()
    {
        image = GetComponent<Image>();
        if (image == null)
        {
            image = gameObject.AddComponent<Image>();
            Debug.Log($"Image 컴포넌트 추가: {gameObject.name}");
        }

        button = GetComponent<Button>();
        if (button == null)
        {
            button = gameObject.AddComponent<Button>();
            Debug.Log($"Button 컴포넌트 추가: {gameObject.name}");
        }

        // 레이캐스트 타겟 설정
        image.raycastTarget = true;
    }

    public void Initialize(PipePuzzleManager mgr, Vector2Int pos, PipeTile tile)
    {
        manager = mgr;
        gridPosition = pos;
        tileData = tile;

        Debug.Log($"타일 초기화: 위치({pos.x}, {pos.y}), 타입: {tile.type}");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (manager == null)
        {
            Debug.LogError("Manager가 설정되지 않았습니다!");
            return;
        }

        if (tileData != null &&
            tileData.type != PipeType.Empty &&
            tileData.type != PipeType.Start &&
            tileData.type != PipeType.End)
        {
            Debug.Log($"타일 클릭: ({gridPosition.x}, {gridPosition.y})");
            manager.RotateTile(gridPosition);

            StartCoroutine(ClickAnimation());
        }
    }

    IEnumerator ClickAnimation()
    {
        transform.localScale = Vector3.one * clickScale;
        yield return new WaitForSeconds(0.1f);
        transform.localScale = Vector3.one;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tileData != null && tileData.type != PipeType.Empty)
        {
            StartCoroutine(ScaleAnimation(Vector3.one * hoverScale, 0.2f));
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StartCoroutine(ScaleAnimation(Vector3.one, 0.2f));
    }

    IEnumerator ScaleAnimation(Vector3 targetScale, float duration)
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, targetScale, elapsed / duration);
            yield return null;
        }

        transform.localScale = targetScale;
    }
}