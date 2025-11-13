using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class WarningTape : MonoBehaviour
{
    [Header("Tape Physical Properties")]
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private float tapeWidth = 0.08f; // 실제 테이프 너비 (8cm)
    [SerializeField] private float sagAmount = 0.3f; // 중력에 의한 처짐
    [SerializeField] private int segmentCount = 30; // 곡선 부드러움
    [SerializeField] private float windStrength = 0.02f; // 바람 효과
    [SerializeField] private float windSpeed = 2f; // 바람 속도

    [Header("Visual Style")]
    [SerializeField] private bool useStripePattern = true; // 대각선 줄무늬 패턴
    [SerializeField] private Color primaryColor = new Color(1f, 0.9f, 0f, 1f); // 노란색
    [SerializeField] private Color secondaryColor = Color.black; // 검은색
    [SerializeField] private float stripeWidth = 0.5f; // 줄무늬 너비
    [SerializeField] private float stripeAngle = 45f; // 줄무늬 각도

    [Header("Material Settings")]
    [SerializeField] private Material tapeMaterial; // 커스텀 머티리얼
    [SerializeField] private float materialGlossiness = 0.3f; // 광택
    [SerializeField] private bool doubleSided = true; // 양면 렌더링

    [Header("Animation")]
    [SerializeField] private bool enableWaving = true; // 바람에 흔들림
    [SerializeField] private AnimationCurve sagCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f); // 처짐 커브

    private LineRenderer lineRenderer;
    private Material instanceMaterial;
    private Vector3[] basePositions;
    private float windPhase = 0f;

    void Start()
    {
        SetupLineRenderer();
        CreateTapeMaterial();
        InitializeBasePositions();
    }

    void SetupLineRenderer()
    {
        lineRenderer = GetComponent<LineRenderer>();

        // Line Renderer 설정
        lineRenderer.positionCount = segmentCount;
        lineRenderer.startWidth = tapeWidth;
        lineRenderer.endWidth = tapeWidth;
        lineRenderer.useWorldSpace = true;
        lineRenderer.textureMode = LineTextureMode.Tile; // 텍스처 타일링
        lineRenderer.alignment = LineAlignment.View; // 항상 카메라를 향함

        // 끝 부분을 사각형으로 (둥근 캡 제거)
        lineRenderer.numCapVertices = 0; // 0으로 설정하면 사각형 끝
        lineRenderer.numCornerVertices = 0; // 모서리도 각지게

        // 그림자 설정
        lineRenderer.shadowCastingMode = doubleSided ?
            UnityEngine.Rendering.ShadowCastingMode.TwoSided :
            UnityEngine.Rendering.ShadowCastingMode.On;
        lineRenderer.receiveShadows = true;
    }

    void CreateTapeMaterial()
    {
        if (tapeMaterial != null)
        {
            instanceMaterial = new Material(tapeMaterial);
        }
        else
        {
            // 기본 머티리얼 생성 (URP Shader 사용)
            instanceMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (instanceMaterial == null) // URP가 없는 경우 폴백
            {
                instanceMaterial = new Material(Shader.Find("Sprites/Default"));
            }
            instanceMaterial.name = "WarningTapeMaterial";
        }

        // 머티리얼 속성 설정
        ConfigureMaterial();

        lineRenderer.material = instanceMaterial;
    }

    void ConfigureMaterial()
    {
        if (instanceMaterial == null) return;

        // 기본 색상 설정
        instanceMaterial.color = primaryColor;

        // URP Lit 셰이더용 속성 설정
        if (instanceMaterial.shader.name.Contains("Universal Render Pipeline"))
        {
            // URP Metallic/Smoothness 설정
            instanceMaterial.SetFloat("_Smoothness", materialGlossiness);
            instanceMaterial.SetFloat("_Metallic", 0f);
        }
        else
        {
            // Legacy Standard 셰이더용 (폴백)
            instanceMaterial.SetFloat("_Glossiness", materialGlossiness);
            instanceMaterial.SetFloat("_Metallic", 0f);
        }

        // 양면 렌더링
        if (doubleSided)
        {
            instanceMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        }

        // 줄무늬 패턴 텍스처 생성
        if (useStripePattern)
        {
            Texture2D stripeTexture = CreateStripeTexture();
            instanceMaterial.mainTexture = stripeTexture;
            instanceMaterial.SetTextureScale("_MainTex", new Vector2(10f, 1f));
        }

        // 약간의 투명도 (옵션)
        if (primaryColor.a < 1f || secondaryColor.a < 1f)
        {
            // URP Transparent 설정
            if (instanceMaterial.shader.name.Contains("Universal Render Pipeline"))
            {
                // Surface Type을 Transparent로 변경
                instanceMaterial.SetFloat("_Surface", 1); // 0 = Opaque, 1 = Transparent
                instanceMaterial.SetFloat("_Blend", 0); // 0 = Alpha, 1 = Premultiply, 2 = Additive, 3 = Multiply

                // Render Face 설정
                instanceMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);

                // 블렌딩 모드 설정
                instanceMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                instanceMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                instanceMaterial.SetInt("_ZWrite", 0);

                // 렌더 큐 설정
                instanceMaterial.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

                // 키워드 활성화
                instanceMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                instanceMaterial.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            }
            else
            {
                // Legacy Transparent 렌더링 모드 설정
                instanceMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                instanceMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                instanceMaterial.SetInt("_ZWrite", 0);
                instanceMaterial.DisableKeyword("_ALPHATEST_ON");
                instanceMaterial.EnableKeyword("_ALPHABLEND_ON");
                instanceMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                instanceMaterial.renderQueue = 3000;
            }
        }
    }

    Texture2D CreateStripeTexture()
    {
        int textureSize = 256;
        Texture2D texture = new Texture2D(textureSize, textureSize);

        // 대각선 줄무늬 패턴 생성
        for (int x = 0; x < textureSize; x++)
        {
            for (int y = 0; y < textureSize; y++)
            {
                // 대각선 계산
                float diagonal = (x + y * Mathf.Tan(stripeAngle * Mathf.Deg2Rad)) % (textureSize * stripeWidth);
                bool isStripe = diagonal < (textureSize * stripeWidth * 0.5f);

                Color pixelColor = isStripe ? primaryColor : secondaryColor;
                texture.SetPixel(x, y, pixelColor);
            }
        }

        texture.Apply();
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.filterMode = FilterMode.Bilinear;

        return texture;
    }

    void InitializeBasePositions()
    {
        basePositions = new Vector3[segmentCount];
        UpdateBasePositions();
    }

    void UpdateBasePositions()
    {
        if (startPoint == null || endPoint == null) return;

        Vector3 start = startPoint.position;
        Vector3 end = endPoint.position;

        for (int i = 0; i < segmentCount; i++)
        {
            float t = i / (float)(segmentCount - 1);
            Vector3 point = Vector3.Lerp(start, end, t);

            // 카테나리 곡선 (실제 늘어진 테이프 모양)
            float sag = CalculateCatenarySag(t);
            point.y -= sag * sagAmount;

            basePositions[i] = point;
        }
    }

    float CalculateCatenarySag(float t)
    {
        // 카테나리 곡선 근사 (쌍곡코사인 함수)
        // 중앙이 가장 많이 처지도록
        float x = (t - 0.5f) * 2f; // -1 to 1 범위로 변환
        float cosh = (Mathf.Exp(x) + Mathf.Exp(-x)) / 2f;
        float catenary = cosh - 1f;

        // AnimationCurve로 추가 제어
        if (sagCurve != null && sagCurve.length > 0)
        {
            catenary *= sagCurve.Evaluate(t);
        }
        else
        {
            // 기본 포물선 커브
            catenary *= Mathf.Sin(t * Mathf.PI);
        }

        return catenary;
    }

    void Update()
    {
        if (startPoint != null && endPoint != null)
        {
            UpdateTapePosition();

            if (enableWaving)
            {
                ApplyWindEffect();
            }
        }
    }

    void UpdateTapePosition()
    {
        UpdateBasePositions();
    }

    void ApplyWindEffect()
    {
        windPhase += Time.deltaTime * windSpeed;

        for (int i = 0; i < segmentCount; i++)
        {
            Vector3 position = basePositions[i];

            // 바람에 의한 좌우 흔들림
            float t = i / (float)(segmentCount - 1);
            float windEffect = Mathf.Sin(windPhase + t * Mathf.PI * 2f) * windStrength;

            // 중앙 부분이 더 많이 흔들리도록
            float centerWeight = Mathf.Sin(t * Mathf.PI);
            windEffect *= centerWeight;

            // 수평 방향으로만 흔들림
            Vector3 windDirection = Vector3.Cross(Vector3.up,
                (endPoint.position - startPoint.position).normalized);
            position += windDirection * windEffect;

            // 약간의 수직 움직임도 추가
            position.y += Mathf.Sin(windPhase * 1.5f + t * Mathf.PI) * windStrength * 0.3f * centerWeight;

            lineRenderer.SetPosition(i, position);
        }
    }

    // 공개 메서드들
    public void SetPoints(Transform newStartPoint, Transform newEndPoint)
    {
        startPoint = newStartPoint;
        endPoint = newEndPoint;
        InitializeBasePositions();
    }

    public void SetPoints(Vector3 startPosition, Vector3 endPosition)
    {
        if (startPoint == null)
        {
            GameObject startObj = new GameObject("TapeStart");
            startObj.transform.position = startPosition;
            startObj.transform.parent = transform;
            startPoint = startObj.transform;
        }
        else
        {
            startPoint.position = startPosition;
        }

        if (endPoint == null)
        {
            GameObject endObj = new GameObject("TapeEnd");
            endObj.transform.position = endPosition;
            endObj.transform.parent = transform;
            endPoint = endObj.transform;
        }
        else
        {
            endPoint.position = endPosition;
        }

        InitializeBasePositions();
    }

    public void SetTapeStyle(Color primary, Color secondary, float stripeWidthValue)
    {
        primaryColor = primary;
        secondaryColor = secondary;
        stripeWidth = stripeWidthValue;

        if (instanceMaterial != null)
        {
            ConfigureMaterial();
        }
    }

    public void SetPhysicalProperties(float width, float sag, float wind)
    {
        tapeWidth = width;
        sagAmount = sag;
        windStrength = wind;

        if (lineRenderer != null)
        {
            lineRenderer.startWidth = tapeWidth;
            lineRenderer.endWidth = tapeWidth;
        }
    }

    // 테이프 스타일 프리셋
    public enum TapeStyle
    {
        YellowBlack,    // 노란색-검은색 (일반 경고)
        RedWhite,       // 빨간색-흰색 (위험)
        BlueWhite,      // 파란색-흰색 (정보)
        OrangeWhite,    // 주황색-흰색 (주의)
        GreenWhite      // 녹색-흰색 (안전)
    }

    public void ApplyPresetStyle(TapeStyle style)
    {
        switch (style)
        {
            case TapeStyle.YellowBlack:
                SetTapeStyle(new Color(1f, 0.9f, 0f), Color.black, 0.5f);
                break;
            case TapeStyle.RedWhite:
                SetTapeStyle(Color.red, Color.white, 0.5f);
                break;
            case TapeStyle.BlueWhite:
                SetTapeStyle(Color.blue, Color.white, 0.5f);
                break;
            case TapeStyle.OrangeWhite:
                SetTapeStyle(new Color(1f, 0.5f, 0f), Color.white, 0.5f);
                break;
            case TapeStyle.GreenWhite:
                SetTapeStyle(Color.green, Color.white, 0.5f);
                break;
        }
    }

    // 기즈모 (에디터에서 시각화)
    void OnDrawGizmos()
    {
        if (startPoint != null && endPoint != null)
        {
            // 직선 연결선
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawLine(startPoint.position, endPoint.position);

            // 시작점과 끝점
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(startPoint.position, 0.1f);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(endPoint.position, 0.1f);

            // 처짐 표시
            if (Application.isPlaying && basePositions != null && basePositions.Length > 0)
            {
                Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
                for (int i = 0; i < basePositions.Length - 1; i++)
                {
                    Gizmos.DrawLine(basePositions[i], basePositions[i + 1]);
                }
            }
        }
    }
}