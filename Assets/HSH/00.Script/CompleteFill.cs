using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CompleteFill : MonoBehaviour
{
    [Header("Fill Settings")]
    [SerializeField] private Image targetImage;
    [SerializeField] private float fillDuration = 2f;
    [SerializeField] private bool loop = true;
    [SerializeField] private float delayBeforeLoop = 0.5f;
    [SerializeField] private Ease easeType = Ease.InOutQuad;

    private Tween fillTween;

    private void Awake()
    {
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }

        if (targetImage != null)
        {
            targetImage.type = Image.Type.Filled;
            targetImage.fillMethod = Image.FillMethod.Horizontal;
            targetImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            targetImage.fillAmount = 0f;
        }
    }

    private void OnEnable()
    {
        StartFillAnimation();
    }

    private void OnDisable()
    {
        StopFillAnimation();
    }

    public void StartFillAnimation()
    {
        // 기존 트윈 제거
        StopFillAnimation();

        if (targetImage != null)
        {
            targetImage.fillAmount = 0f;

            // DOTween 애니메이션 생성
            fillTween = targetImage.DOFillAmount(1f, fillDuration)
                .SetEase(easeType)
                .OnComplete(() =>
                {
                    if (loop)
                    {
                        // 반복 설정
                        fillTween = targetImage.DOFillAmount(0f, 0f)
                            .SetDelay(delayBeforeLoop)
                            .OnComplete(() => StartFillAnimation());
                    }
                });
        }
    }

    public void StopFillAnimation()
    {
        if (fillTween != null && fillTween.IsActive())
        {
            fillTween.Kill();
            fillTween = null;
        }
    }
}
