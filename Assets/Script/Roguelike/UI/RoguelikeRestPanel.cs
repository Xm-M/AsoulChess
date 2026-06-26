using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 肉鸽地图休息房面板。预制体：<c>Resources/UIPrefab/RoguelikeRestPanel</c>，根物体 name 须一致。
/// 未配置子物体时会在 <see cref="Init"/> 中自动生成简易布局。
/// </summary>
public class RoguelikeRestPanel : View
{
    [SerializeField] TMP_Text titleText;
    [SerializeField] TMP_Text hintText;
    [SerializeField] Transform optionRoot;
    [SerializeField] Button leaveButton;
    [Tooltip("二期锻造位，一期隐藏")]
    [SerializeField] GameObject secondPermanentSlot;

    [SerializeField] Vector2 optionButtonSize = new Vector2(420f, 56f);
    [SerializeField] float optionSpacing = 10f;

    int _nodeId = -1;
    readonly List<GameObject> _optionButtons = new List<GameObject>();

    public override void Init()
    {
        EnsureRuntimeUi();
        if (leaveButton != null)
            leaveButton.onClick.AddListener(OnLeaveClicked);
        if (secondPermanentSlot != null)
            secondPermanentSlot.SetActive(false);
    }

    public static void ShowRest(int nodeId)
    {
        var panel = UIManage.GetView<RoguelikeRestPanel>();
        if (panel == null)
        {
            Debug.LogError(
                "[RoguelikeRestPanel] 未找到面板。请在 Resources/UIPrefab 创建 RoguelikeRestPanel.prefab 并挂本脚本");
            RoguelikeRunService.LeaveRestNode();
            return;
        }

        panel.Open(nodeId);
    }

    void Open(int nodeId)
    {
        _nodeId = nodeId;
        if (titleText != null)
            titleText.text = "休息";
        if (hintText != null)
            hintText.text = "选择一项后离开，或直接离开";
        RebuildOptions();
        RoguelikeRunInfoPanel.TryShowAndRefresh();
        Show();
    }

    void RebuildOptions()
    {
        ClearOptionButtons();
        if (optionRoot == null)
            return;

        var options = RoguelikeRestFlow.BuildOptions(_nodeId);
        float y = 0f;
        for (int i = 0; i < options.Count; i++)
        {
            var option = options[i];
            if (option == null)
                continue;

            var go = CreateOptionButton(optionRoot, option, y);
            _optionButtons.Add(go);
            y += optionButtonSize.y + optionSpacing;
        }
    }

    GameObject CreateOptionButton(Transform parent, RoguelikeRestOption option, float yOffset)
    {
        var root = new GameObject($"RestOption_{option.id}", typeof(RectTransform), typeof(Image), typeof(Button));
        root.transform.SetParent(parent, false);

        var rt = root.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = optionButtonSize;
        rt.anchoredPosition = new Vector2(0f, -yOffset);

        var img = root.GetComponent<Image>();
        img.color = option.enabled
            ? new Color(0.18f, 0.22f, 0.28f, 0.94f)
            : new Color(0.12f, 0.12f, 0.12f, 0.55f);

        var btn = root.GetComponent<Button>();
        btn.targetGraphic = img;
        if (option.enabled)
            btn.onClick.AddListener(() => OnOptionClicked(option.id));

        var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(root.transform, false);
        var labelRt = labelGo.GetComponent<RectTransform>();
        labelRt.anchorMin = Vector2.zero;
        labelRt.anchorMax = Vector2.one;
        labelRt.offsetMin = new Vector2(12f, 4f);
        labelRt.offsetMax = new Vector2(-12f, -4f);

        var label = labelGo.GetComponent<TextMeshProUGUI>();
        label.alignment = TextAlignmentOptions.MidlineLeft;
        label.fontSize = 22f;
        label.color = option.enabled ? Color.white : new Color(0.75f, 0.75f, 0.75f, 0.7f);
        label.text = string.IsNullOrEmpty(option.description)
            ? option.title
            : $"{option.title} — {option.description}";

        return root;
    }

    void OnOptionClicked(string optionId)
    {
        if (_nodeId < 0)
            return;
        if (!RoguelikeRestFlow.TryChooseOption(_nodeId, optionId))
            return;

        RoguelikeRunInfoPanel.TryShowAndRefresh();
        RebuildOptions();
    }

    void OnLeaveClicked()
    {
        RoguelikeRunService.LeaveRestNode();
    }

    void ClearOptionButtons()
    {
        for (int i = 0; i < _optionButtons.Count; i++)
        {
            if (_optionButtons[i] != null)
                Destroy(_optionButtons[i]);
        }
        _optionButtons.Clear();
    }

    void EnsureRuntimeUi()
    {
        var rt = GetComponent<RectTransform>();
        if (rt == null)
            rt = gameObject.AddComponent<RectTransform>();

        if (transform.Find("DimBackground") == null)
        {
            var dimGo = new GameObject("DimBackground", typeof(RectTransform), typeof(Image));
            dimGo.transform.SetParent(transform, false);
            dimGo.transform.SetAsFirstSibling();
            var dimRt = dimGo.GetComponent<RectTransform>();
            dimRt.anchorMin = Vector2.zero;
            dimRt.anchorMax = Vector2.one;
            dimRt.offsetMin = Vector2.zero;
            dimRt.offsetMax = Vector2.zero;
            var dimImg = dimGo.GetComponent<Image>();
            dimImg.color = new Color(0f, 0f, 0f, 0.65f);
            dimImg.raycastTarget = true;
        }

        if (titleText == null)
        {
            var titleGo = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleGo.transform.SetParent(transform, false);
            var titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.5f, 0.5f);
            titleRt.anchorMax = new Vector2(0.5f, 0.5f);
            titleRt.pivot = new Vector2(0.5f, 0.5f);
            titleRt.anchoredPosition = new Vector2(0f, 120f);
            titleRt.sizeDelta = new Vector2(600f, 48f);
            titleText = titleGo.GetComponent<TextMeshProUGUI>();
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontSize = 36f;
        }

        if (hintText == null)
        {
            var hintGo = new GameObject("Hint", typeof(RectTransform), typeof(TextMeshProUGUI));
            hintGo.transform.SetParent(transform, false);
            var hintRt = hintGo.GetComponent<RectTransform>();
            hintRt.anchorMin = new Vector2(0.5f, 0.5f);
            hintRt.anchorMax = new Vector2(0.5f, 0.5f);
            hintRt.pivot = new Vector2(0.5f, 0.5f);
            hintRt.anchoredPosition = new Vector2(0f, 80f);
            hintRt.sizeDelta = new Vector2(600f, 36f);
            hintText = hintGo.GetComponent<TextMeshProUGUI>();
            hintText.alignment = TextAlignmentOptions.Center;
            hintText.fontSize = 20f;
            hintText.color = new Color(0.85f, 0.85f, 0.85f, 1f);
        }

        if (optionRoot == null)
        {
            var rootGo = new GameObject("OptionRoot", typeof(RectTransform));
            rootGo.transform.SetParent(transform, false);
            var rootRt = rootGo.GetComponent<RectTransform>();
            rootRt.anchorMin = new Vector2(0.5f, 0.5f);
            rootRt.anchorMax = new Vector2(0.5f, 0.5f);
            rootRt.pivot = new Vector2(0.5f, 1f);
            rootRt.anchoredPosition = new Vector2(0f, 60f);
            rootRt.sizeDelta = new Vector2(460f, 200f);
            optionRoot = rootRt;
        }

        if (leaveButton == null)
        {
            var leaveGo = new GameObject("LeaveButton", typeof(RectTransform), typeof(Image), typeof(Button));
            leaveGo.transform.SetParent(transform, false);
            var leaveRt = leaveGo.GetComponent<RectTransform>();
            leaveRt.anchorMin = new Vector2(0.5f, 0.5f);
            leaveRt.anchorMax = new Vector2(0.5f, 0.5f);
            leaveRt.pivot = new Vector2(0.5f, 0.5f);
            leaveRt.anchoredPosition = new Vector2(0f, -140f);
            leaveRt.sizeDelta = new Vector2(200f, 48f);

            var leaveImg = leaveGo.GetComponent<Image>();
            leaveImg.color = new Color(0.25f, 0.45f, 0.3f, 1f);
            leaveButton = leaveGo.GetComponent<Button>();
            leaveButton.targetGraphic = leaveImg;

            var leaveLabelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            leaveLabelGo.transform.SetParent(leaveGo.transform, false);
            var leaveLabelRt = leaveLabelGo.GetComponent<RectTransform>();
            leaveLabelRt.anchorMin = Vector2.zero;
            leaveLabelRt.anchorMax = Vector2.one;
            leaveLabelRt.offsetMin = Vector2.zero;
            leaveLabelRt.offsetMax = Vector2.zero;
            var leaveLabel = leaveLabelGo.GetComponent<TextMeshProUGUI>();
            leaveLabel.alignment = TextAlignmentOptions.Center;
            leaveLabel.fontSize = 24f;
            leaveLabel.text = "离开";
        }
    }
}
