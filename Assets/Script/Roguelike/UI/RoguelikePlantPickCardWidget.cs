using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 肉鸽 PlantPick 单张详情卡。布局在预制体里拼好，脚本只负责填详情与点击选卡。
/// 字段对齐图鉴 / 商店详情（<see cref="PlantCreatorDetailHelper"/>）。
/// </summary>
public class RoguelikePlantPickCardWidget : MonoBehaviour
{
    [SerializeField] GameObject root;
    [SerializeField] Button pickButton;
    [SerializeField] TMP_Text detailName;
    [SerializeField] TMP_Text detailPlantType;
    [SerializeField] Image detailPlantImage;
    [SerializeField] TMP_Text detailTags;
    [SerializeField] TMP_Text detailAttributes;
    [SerializeField] ScrollRect descriptionScroll;
    [SerializeField] TMP_Text descriptionText;

    string _chessName;
    Action<string> _onPick;

    void Awake()
    {
        if (root == null)
            root = gameObject;
        if (pickButton == null)
            pickButton = GetComponent<Button>();
        if (pickButton != null)
            pickButton.onClick.AddListener(OnPickClicked);
    }

    public void Bind(PropertyCreator creator, Action<string> onPick)
    {
        _onPick = onPick;
        if (creator == null)
        {
            Clear();
            return;
        }

        _chessName = creator.chessName;
        SetVisible(true);

        if (detailName != null)
            detailName.text = creator.chessName;
        if (detailPlantType != null)
            detailPlantType.text = PlantCreatorDetailHelper.GetPlantTypeName(creator.plantType);
        if (detailPlantImage != null)
        {
            detailPlantImage.sprite = creator.chessSprite;
            detailPlantImage.enabled = creator.chessSprite != null;
        }
        if (detailTags != null)
            detailTags.text = PlantCreatorDetailHelper.BuildTagsText(creator);
        if (detailAttributes != null)
            detailAttributes.text = PlantCreatorDetailHelper.BuildAttributeText(creator.baseProperty);
        if (descriptionText != null)
            descriptionText.text = PlantCreatorDetailHelper.BuildDescriptionText(creator);
        if (descriptionScroll != null)
            descriptionScroll.normalizedPosition = Vector2.up;
    }

    public void Clear()
    {
        _chessName = null;
        _onPick = null;
        if (detailName != null)
            detailName.text = "";
        if (detailPlantType != null)
            detailPlantType.text = "";
        if (detailPlantImage != null)
        {
            detailPlantImage.sprite = null;
            detailPlantImage.enabled = false;
        }
        if (detailTags != null)
            detailTags.text = "";
        if (detailAttributes != null)
            detailAttributes.text = "";
        if (descriptionText != null)
            descriptionText.text = "";
        SetVisible(false);
    }

    public void SetVisible(bool visible)
    {
        if (root != null)
            root.SetActive(visible);
    }

    void OnPickClicked()
    {
        if (!string.IsNullOrEmpty(_chessName))
            _onPick?.Invoke(_chessName);
    }
}
