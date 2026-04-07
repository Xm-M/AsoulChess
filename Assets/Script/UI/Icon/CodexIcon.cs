using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// 图鉴中单个棋子选项卡。显示棋子图标和名称，点击后回调通知面板。
/// </summary>
public class CodexIcon : MonoBehaviour
{
    [SerializeField] Image bgImage;
    [SerializeField] Image iconImage;
    [SerializeField] TMP_Text nameText;
    [SerializeField] Color selectedColor = new Color(1f, 0.92f, 0.5f);
    [SerializeField] Color normalColor = Color.white;

    private bool isSelected;

    public void Init(PropertyCreator creator, Action onClick)
    {
        if (iconImage != null) iconImage.sprite = creator.chessSprite;
        if (nameText != null) nameText.text = creator.chessName;

        var btn = GetComponent<Button>();
        if (btn != null) btn.onClick.AddListener(() => onClick?.Invoke());
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        Color c = selected ? selectedColor : normalColor;
        if (bgImage != null) bgImage.color = c;
        if (iconImage != null) iconImage.color = c;
    }
}
