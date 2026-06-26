using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>奖励面板单行条目（按钮）。</summary>
public class RoguelikeRewardEntryWidget : MonoBehaviour
{
    [SerializeField] Button button;
    [SerializeField] TMP_Text labelText;
    [SerializeField] Image iconImage;

    RoguelikeRewardEntry _entry;
    Action<RoguelikeRewardEntry> _onClick;

    public void Bind(RoguelikeRewardEntry entry, Action<RoguelikeRewardEntry> onClick)
    {
        _entry = entry;
        _onClick = onClick;

        if (button == null)
            button = GetComponent<Button>();
        if (labelText == null)
            labelText = GetComponentInChildren<TMP_Text>();

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnButtonClick);
        }

        RefreshLabel();
    }

    void RefreshLabel()
    {
        if (labelText == null || _entry == null)
            return;

        if (iconImage != null)
            iconImage.enabled = false;

        switch (_entry.kind)
        {
            case RoguelikeRewardEntryKind.Gold:
                labelText.text = $"{_entry.goldAmount} 金币";
                break;
            case RoguelikeRewardEntryKind.PlantPick:
                labelText.text = "将一张牌加入牌组";
                break;
            case RoguelikeRewardEntryKind.Item:
                var prop = RoguelikeRunPropPool.ResolveProp(_entry.propId);
                labelText.text = prop != null && !string.IsNullOrEmpty(prop.displayName)
                    ? prop.displayName
                    : "获得道具";
                if (iconImage != null && prop?.icon != null)
                {
                    iconImage.sprite = prop.icon;
                    iconImage.enabled = true;
                }
                break;
        }
    }

    void OnButtonClick()
    {
        if (_entry != null)
            _onClick?.Invoke(_entry);
    }

    public static RoguelikeRewardEntryWidget CreateRuntime(Transform parent, Vector2 size)
    {
        var root = new GameObject("RewardEntry", typeof(RectTransform), typeof(Image), typeof(Button));
        root.transform.SetParent(parent, false);

        var rt = root.GetComponent<RectTransform>();
        rt.sizeDelta = size;

        var img = root.GetComponent<Image>();
        img.color = new Color(0.15f, 0.12f, 0.1f, 0.92f);

        var btn = root.GetComponent<Button>();
        btn.targetGraphic = img;

        var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(root.transform, false);
        var labelRt = labelGo.GetComponent<RectTransform>();
        labelRt.anchorMin = Vector2.zero;
        labelRt.anchorMax = Vector2.one;
        labelRt.offsetMin = new Vector2(16f, 4f);
        labelRt.offsetMax = new Vector2(-16f, -4f);

        var tmp = labelGo.GetComponent<TextMeshProUGUI>();
        tmp.fontSize = 22;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.color = Color.white;

        var widget = root.AddComponent<RoguelikeRewardEntryWidget>();
        widget.button = btn;
        widget.labelText = tmp;
        return widget;
    }
}
