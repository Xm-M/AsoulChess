using UnityEngine;
using UnityEngine.UI;

/// <summary>可选连线预制体组件；<see cref="RoguelikeMapPanel"/> 会设置位置与长度。</summary>
[RequireComponent(typeof(RectTransform))]
public class RoguelikeMapLineWidget : MonoBehaviour
{
    [SerializeField] Image lineImage;

    public Image LineImage => lineImage != null ? lineImage : lineImage = GetComponent<Image>();

    public void ApplyBetween(Vector2 a, Vector2 b, RoguelikeMapLineStyle style, bool usePrefabAppearanceOnly)
    {
        var img = LineImage;
        var rt = transform as RectTransform;
        if (rt == null) return;

        if (!usePrefabAppearanceOnly && img != null)
        {
            if (style.sprite != null)
                img.sprite = style.sprite;
            img.color = style.color;
        }

        Vector2 dir = b - a;
        float len = dir.magnitude;
        float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float width = style != null ? style.width : 4f;

        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = (a + b) * 0.5f;
        rt.sizeDelta = new Vector2(Mathf.Max(len, 1f), width);
        rt.localEulerAngles = new Vector3(0f, 0f, ang);

        if (img != null)
            img.raycastTarget = false;
    }
}
