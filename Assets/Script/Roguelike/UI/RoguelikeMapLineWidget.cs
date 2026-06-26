using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 肉鸽地图连线：优先 <see cref="LineRenderer"/>（Canvas 为 Camera / World 时）；
/// Screen Space Overlay 下自动改用 UI Image（Overlay 会盖住 LineRenderer，调 sortingOrder 无效）。
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class RoguelikeMapLineWidget : MonoBehaviour
{
    [SerializeField] LineRenderer lineRenderer;

    static Material _defaultLineMaterial;
    static bool _overlayWarningLogged;

    public LineRenderer LineRendererComponent =>
        lineRenderer != null ? lineRenderer : lineRenderer = GetComponent<LineRenderer>();

    public void ApplyBetween(
        Vector2 localA,
        Vector2 localB,
        Transform coordinateRoot,
        RoguelikeMapLineStyle style,
        bool usePrefabAppearanceOnly)
    {
        var lr = LineRendererComponent;
        if (lr == null || coordinateRoot == null)
            return;

        style ??= new RoguelikeMapLineStyle();

        if (!usePrefabAppearanceOnly)
            ApplyStyle(lr, style, coordinateRoot);
        else
            ApplyCanvasSorting(lr, coordinateRoot);

        lr.useWorldSpace = true;
        lr.alignment = LineAlignment.View;
        lr.positionCount = 2;
        lr.SetPosition(0, LocalToLineWorld(coordinateRoot, localA));
        lr.SetPosition(1, LocalToLineWorld(coordinateRoot, localB));
    }

    /// <summary>Overlay Canvas 下用 UI Image 画线（与节点同一套 Canvas 渲染）。</summary>
    public static GameObject CreateUiImageLine(
        Transform lineParent,
        Vector2 localA,
        Vector2 localB,
        RoguelikeMapLineStyle style)
    {
        style ??= new RoguelikeMapLineStyle();

        var go = new GameObject("MapLine_UI", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(lineParent, false);

        var rt = go.GetComponent<RectTransform>();
        var img = go.GetComponent<Image>();
        img.raycastTarget = false;
        if (style.sprite != null)
        {
            img.sprite = style.sprite;
            img.color = style.color;
        }
        else
        {
            img.sprite = null;
            img.color = style.color;
        }

        Vector2 dir = localB - localA;
        float len = dir.magnitude;
        if (len < 0.001f)
            len = 1f;
        float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float width = style.width;

        rt.anchorMin = rt.anchorMax = new Vector2(0f, 0.5f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.anchoredPosition = localA;
        rt.sizeDelta = new Vector2(len, width);
        rt.localEulerAngles = new Vector3(0f, 0f, ang);

        return go;
    }

    public static bool ShouldUseUiImageLine(Transform coordinateRoot)
    {
        var canvas = coordinateRoot != null ? coordinateRoot.GetComponentInParent<Canvas>() : null;
        return canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay;
    }

    public static void LogOverlayLineRendererWarningOnce()
    {
        if (_overlayWarningLogged)
            return;
        _overlayWarningLogged = true;
        Debug.LogWarning(
            "[RoguelikeMapLine] 当前 UI Canvas 为 Screen Space Overlay，LineRenderer 会被 UI 盖住（调高 sortingOrder 无效）。" +
            "已自动改用 UI Image 画线。若坚持用 LineRenderer + BloodLine 材质，请把 UIRoot Canvas 改为 Screen Space - Camera 并指定 Main Camera。");
    }

    static Vector3 LocalToLineWorld(Transform coordinateRoot, Vector2 local)
    {
        var canvas = coordinateRoot.GetComponentInParent<Canvas>();
        Vector3 world = coordinateRoot.TransformPoint(new Vector3(local.x, local.y, 0f));

        if (canvas == null)
            return world;

        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            return world;

        Camera cam = canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera && cam != null)
        {
            Vector3 screen = RectTransformUtility.WorldToScreenPoint(cam, world);
            float depth = canvas.planeDistance;
            return cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, depth));
        }

        return world;
    }

    static void ApplyStyle(LineRenderer lr, RoguelikeMapLineStyle style, Transform coordinateRoot)
    {
        if (lr.sharedMaterial == null)
            lr.sharedMaterial = GetDefaultLineMaterial();

        lr.startColor = style.color;
        lr.endColor = style.color;

        float width = PixelWidthToWorld(style.width, coordinateRoot);
        lr.startWidth = width;
        lr.endWidth = width;

        if (style.sprite != null)
        {
            lr.textureMode = LineTextureMode.Tile;
            if (lr.sharedMaterial != null)
                lr.sharedMaterial.mainTexture = style.sprite.texture;
        }

        lr.numCornerVertices = 4;
        lr.numCapVertices = 4;
        ApplyCanvasSorting(lr, coordinateRoot);
    }

    static float PixelWidthToWorld(float pixelWidth, Transform coordinateRoot)
    {
        var canvas = coordinateRoot.GetComponentInParent<Canvas>();
        if (canvas != null && canvas.scaleFactor > 0f)
            return pixelWidth / canvas.scaleFactor;
        return pixelWidth;
    }

    static void ApplyCanvasSorting(LineRenderer lr, Transform coordinateRoot)
    {
        var canvas = coordinateRoot.GetComponentInParent<Canvas>();
        if (canvas == null)
            return;

        lr.sortingLayerID = canvas.sortingLayerID;
        lr.sortingOrder = canvas.sortingOrder - 1;
    }

    static Material GetDefaultLineMaterial()
    {
        if (_defaultLineMaterial != null)
            return _defaultLineMaterial;

        string[] shaderNames =
        {
            "Universal Render Pipeline/Unlit",
            "Universal Render Pipeline/2D/Sprite-Unlit-Default",
            "Sprites/Default",
            "Unlit/Color",
        };

        Shader shader = null;
        for (int i = 0; i < shaderNames.Length; i++)
        {
            shader = Shader.Find(shaderNames[i]);
            if (shader != null)
                break;
        }

        if (shader == null)
        {
            Debug.LogError("[RoguelikeMapLine] 未找到可用线条 Shader（URP Unlit / Sprites/Default）");
            return null;
        }

        _defaultLineMaterial = new Material(shader);
        return _defaultLineMaterial;
    }

    /// <summary>无 linePrefab 时由面板创建默认 LineRenderer。</summary>
    public static RoguelikeMapLineWidget CreateRuntime(Transform parent)
    {
        var go = new GameObject("MapLine", typeof(LineRenderer));
        go.transform.SetParent(parent, false);

        var lr = go.GetComponent<LineRenderer>();
        lr.sharedMaterial = GetDefaultLineMaterial();
        lr.useWorldSpace = true;
        lr.alignment = LineAlignment.View;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        lr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        lr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;

        var widget = go.AddComponent<RoguelikeMapLineWidget>();
        widget.lineRenderer = lr;
        return widget;
    }
}
