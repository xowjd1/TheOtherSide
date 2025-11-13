using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class UIVideoPlayer : MonoBehaviour
{
    public RawImage rawImage;
    public VideoPlayer videoPlayer;
    public RenderTexture renderTexture;

    void Start()
    {
        // Render Texture 생성 (코드로)
        renderTexture = new RenderTexture(1920, 1080, 16);

        // 연결
        videoPlayer.targetTexture = renderTexture;
        rawImage.texture = renderTexture;

        // 재생
        videoPlayer.Play();
    }
}
