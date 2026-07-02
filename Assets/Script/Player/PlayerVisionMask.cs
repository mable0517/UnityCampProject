using UnityEngine;

// 플레이어 주변만 보이고, 멀어질수록 흐릿하게 어두워지다가 완전히 검게 만드는
// 시야 제한(포그 오브 워) 오버레이. 방사형 그라데이션 텍스처를 코드로 생성해
// 플레이어를 따라다니게 한다.
//
// 사용법: 이 컴포넌트를 Player 오브젝트에 Add Component 하면 끝.
// (조명/머티리얼/소팅레이어 설정이 필요 없다. 스프라이트 위에 반투명 검정으로 덮는다.)
public class PlayerVisionMask : MonoBehaviour
{
    [Header("시야 반경 (월드 유닛)")]
    [Tooltip("이 반경 안쪽은 완전히 선명하게 보인다.")]
    public float visibleRadius = 2f;
    [Tooltip("여기까지 점점 어두워지고, 이 바깥은 완전한 검정이 된다.")]
    public float fadeRadius = 5f;

    [Header("연출")]
    [Range(0f, 1f)]
    [Tooltip("바깥 어둠의 최대 진하기 (1 = 완전 검정).")]
    public float maxDarkness = 1f;
    [Tooltip("검은 오버레이가 덮는 전체 크기. 카메라 화면보다 넉넉히 크게.")]
    public float maskWorldSize = 60f;
    [Tooltip("모든 스프라이트 위에 그리기 위한 정렬 순서.")]
    public int sortingOrder = 30000;
    public string sortingLayerName = "Default";
    [Tooltip("그라데이션 텍스처 해상도. 클수록 부드럽지만 무겁다.")]
    public int textureSize = 512;

    private Transform maskTransform;
    private SpriteRenderer maskRenderer;

    void Start()
    {
        CreateMask();
    }

    // 플레이어 이동 후 위치를 따라간다. (부모로 붙이지 않아 플레이어 스케일 변화의 영향을 받지 않음)
    void LateUpdate()
    {
        if (maskTransform == null) return;
        Vector3 p = transform.position;
        maskTransform.position = new Vector3(p.x, p.y, maskTransform.position.z);
    }

    private void CreateMask()
    {
        Texture2D tex = BuildRadialTexture();
        float pixelsPerUnit = textureSize / maskWorldSize;
        Sprite sprite = Sprite.Create(
            tex,
            new Rect(0f, 0f, textureSize, textureSize),
            new Vector2(0.5f, 0.5f),
            pixelsPerUnit,
            0,
            SpriteMeshType.FullRect);

        GameObject go = new GameObject("PlayerVisionMask");
        maskTransform = go.transform;
        maskTransform.position = transform.position;

        maskRenderer = go.AddComponent<SpriteRenderer>();
        maskRenderer.sprite = sprite;
        maskRenderer.sortingLayerName = sortingLayerName;
        maskRenderer.sortingOrder = sortingOrder;
        maskRenderer.color = Color.white; // 색은 텍스처(검정+알파)가 담당
    }

    private Texture2D BuildRadialTexture()
    {
        int T = Mathf.Max(16, textureSize);
        Texture2D tex = new Texture2D(T, T, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        Vector2 center = new Vector2(T / 2f, T / 2f);
        float unitsPerPixel = maskWorldSize / T;
        float inner = Mathf.Max(0f, visibleRadius);
        float outer = Mathf.Max(inner + 0.001f, fadeRadius);

        Color[] px = new Color[T * T];
        for (int y = 0; y < T; y++)
        {
            for (int x = 0; x < T; x++)
            {
                float distPx = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                float distWorld = distPx * unitsPerPixel;

                float a;
                if (distWorld <= inner)
                {
                    a = 0f; // 선명한 중심
                }
                else if (distWorld >= outer)
                {
                    a = maxDarkness; // 완전한 바깥 어둠
                }
                else
                {
                    float t = (distWorld - inner) / (outer - inner);
                    t = t * t * (3f - 2f * t); // smoothstep → 부드럽고 흐릿한 페이드
                    a = t * maxDarkness;
                }

                px[y * T + x] = new Color(0f, 0f, 0f, a);
            }
        }

        tex.SetPixels(px);
        tex.Apply();
        return tex;
    }
}
