using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>休息房选项卡 UI（配图 + 标题 + 描述）。</summary>
public class RoguelikeRestOptionWidget : MonoBehaviour
{
    [SerializeField] Button button;
    [SerializeField] Image backgroundImage;
    [SerializeField] Image iconImage;
    [SerializeField] TMP_Text titleText;
    [SerializeField] TMP_Text descriptionText;

    [SerializeField] Color enabledBackground = new Color(0.18f, 0.22f, 0.28f, 0.94f);
    [SerializeField] Color disabledBackground = new Color(0.12f, 0.12f, 0.12f, 0.55f);

    string _optionId;
    Action<string> _onClick;

    public void Bind(RoguelikeRestOption option, Action<string> onClick)
    {
        _optionId = option?.id;
        _onClick = onClick;

        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.interactable = option != null && option.enabled;
            if (option != null && option.enabled)
                button.onClick.AddListener(OnButtonClick);
        }

        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();
        if (backgroundImage != null)
            backgroundImage.color = option != null && option.enabled ? enabledBackground : disabledBackground;

        if (titleText != null)
            titleText.text = option?.title ?? string.Empty;

        if (descriptionText != null)
            descriptionText.text = option?.description ?? string.Empty;

        if (iconImage != null)
        {
            bool hasIcon = option?.icon != null;
            iconImage.sprite = hasIcon ? option.icon : null;
            iconImage.enabled = hasIcon;
        }
    }

    void OnButtonClick()
    {
        if (!string.IsNullOrEmpty(_optionId))
            _onClick?.Invoke(_optionId);
    }

    public static RoguelikeRestOptionWidget CreateRuntime(Transform parent, Vector2 size)
    {
        var root = new GameObject("RestOption", typeof(RectTransform), typeof(Image), typeof(Button));
        root.transform.SetParent(parent, false);

        var rt = root.GetComponent<RectTransform>();
        rt.sizeDelta = size;

        var img = root.GetComponent<Image>();
        img.color = new Color(0.18f, 0.22f, 0.28f, 0.94f);

        var btn = root.GetComponent<Button>();
        btn.targetGraphic = img;

        var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconGo.transform.SetParent(root.transform, false);
        var iconRt = iconGo.GetComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0f, 0.5f);
        iconRt.anchorMax = new Vector2(0f, 0.5f);
        iconRt.pivot = new Vector2(0f, 0.5f);
        iconRt.anchoredPosition = new Vector2(12f, 0f);
        iconRt.sizeDelta = new Vector2(48f, 48f);
        var iconImg = iconGo.GetComponent<Image>();
        iconImg.enabled = false;

        var textRoot = new GameObject("Text", typeof(RectTransform));
        textRoot.transform.SetParent(root.transform, false);
        var textRt = textRoot.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(72f, 8f);
        textRt.offsetMax = new Vector2(-12f, -8f);

        var titleGo = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleGo.transform.SetParent(textRoot.transform, false);
        var titleRt = titleGo.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0f, 0.5f);
        titleRt.anchorMax = new Vector2(1f, 1f);
        titleRt.offsetMin = Vector2.zero;
        titleRt.offsetMax = Vector2.zero;
        var titleTmp = titleGo.GetComponent<TextMeshProUGUI>();
        titleTmp.fontSize = 22f;
        titleTmp.alignment = TextAlignmentOptions.MidlineLeft;
        titleTmp.color = Color.white;

        var descGo = new GameObject("Description", typeof(RectTransform), typeof(TextMeshProUGUI));
        descGo.transform.SetParent(textRoot.transform, false);
        var descRt = descGo.GetComponent<RectTransform>();
        descRt.anchorMin = new Vector2(0f, 0f);
        descRt.anchorMax = new Vector2(1f, 0.5f);
        descRt.offsetMin = Vector2.zero;
        descRt.offsetMax = Vector2.zero;
        var descTmp = descGo.GetComponent<TextMeshProUGUI>();
        descTmp.fontSize = 16f;
        descTmp.alignment = TextAlignmentOptions.MidlineLeft;
        descTmp.color = new Color(0.85f, 0.85f, 0.85f, 1f);

        var widget = root.AddComponent<RoguelikeRestOptionWidget>();
        widget.button = btn;
        widget.backgroundImage = img;
        widget.iconImage = iconImg;
        widget.titleText = titleTmp;
        widget.descriptionText = descTmp;
        return widget;
    }
}
