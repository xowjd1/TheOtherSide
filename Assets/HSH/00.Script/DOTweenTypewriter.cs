using UnityEngine;
using DG.Tweening;
using TMPro;

public class DOTweenTypewriter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textComponent;
    [SerializeField] private float typingDuration = 2f;
    [SerializeField] private Ease easeType = Ease.Linear;

    private string fullText;
    private Tween typingTween;

    private void OnEnable()
    {
        if (textComponent == null)
            textComponent = GetComponent<TextMeshProUGUI>();

        // Rich Text 태그 제거
        fullText = textComponent.text;

        // 특수 문자 확인
        for (int i = 0; i < fullText.Length; i++)
        {
            if (char.IsControl(fullText[i]) && fullText[i] != '\n')
            {
                Debug.Log($"Special character at position {i}: {(int)fullText[i]}");
            }
        }

        textComponent.maxVisibleCharacters = 0;

        typingTween = DOTween.To(
            () => textComponent.maxVisibleCharacters,
            x => textComponent.maxVisibleCharacters = x,
            fullText.Length,
            typingDuration
        ).SetEase(easeType);
    }

    private void OnDisable()
    {
        if (typingTween != null && typingTween.IsActive())
        {
            typingTween.Kill();
            textComponent.maxVisibleCharacters = fullText.Length;
        }
    }
}
