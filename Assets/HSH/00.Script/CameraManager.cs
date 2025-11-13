using DG.Tweening;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private static CameraManager _instance;

    public static CameraManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<CameraManager>(FindObjectsInactive.Exclude);

                if (_instance == null)
                {
                    GameObject go = new GameObject("CameraManager");
                    _instance = go.AddComponent<CameraManager>();
                }
            }
            return _instance;
        }
    }


    [Header("Settings")]
    public float defaultDuration = 2f;
    public Ease defaultEase = Ease.InOutQuad;

    private Camera mainCamera;
    private Tween currentTween;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    public void MoveToPosition(Vector3 position, float duration = -1)
    {
        if (duration < 0) duration = defaultDuration;

        currentTween?.Kill();
        currentTween = mainCamera.transform.DOMove(position, duration)
            .SetEase(defaultEase);
    }

    public void MoveAndRotate(Vector3 position, Vector3 rotation, float duration = -1)
    {
        if (duration < 0) duration = defaultDuration;

        currentTween?.Kill();

        Sequence sequence = DOTween.Sequence();
        sequence.Append(mainCamera.transform.DOMove(position, duration));
        sequence.Join(mainCamera.transform.DORotate(rotation, duration));
        sequence.SetEase(defaultEase);

        currentTween = sequence;
    }

    public void ShakeCamera(float duration = 0.5f, float strength = 1f)
    {
        mainCamera.DOShakePosition(duration, strength);
    }
}
