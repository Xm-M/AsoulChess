using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 肉鸽局内数据面板（HUD / 侧边栏）。布局由你在预制体里搭，本脚本只负责绑定与刷新。
/// 预制体：<c>Resources/UIPrefab/RoguelikeRunInfoPanel</c>，根物体 name 须一致。
/// Run 进行中自动显示；放弃/通关后隐藏。
/// </summary>
public class RoguelikeRunInfoPanel : View
{
    [Header("显示策略")]
    [SerializeField] bool autoShowWhenRunActive = true;
    [SerializeField] bool hideWhenNoActiveRun = true;
    [Tooltip("无 Run 时显示（可选）")]
    [SerializeField] GameObject emptyStateRoot;

    [Header("文本绑定（按需拖，不用的留空）")]
    [SerializeField] TMP_Text bandNameText;
    [SerializeField] TMP_Text runConfigNameText;
    [SerializeField] TMP_Text actTitleText;
    [SerializeField] TMP_Text runSeedText;
    [SerializeField] TMP_Text currentNodeText;
    [SerializeField] TMP_Text pendingNodeText;
    [SerializeField] TMP_Text selectableNextText;
    [SerializeField] TMP_Text progressText;
    [SerializeField] TMP_Text ownedPlantCountText;
    [SerializeField] TMP_Text runGoldText;
    [SerializeField] TMP_Text summaryText;

    [Header("植物图标")]
    [SerializeField] List<Image> plantIconImages = new List<Image>();
    [Tooltip("若配置了 Root + Prefab，则动态生成图标；否则用上面固定 Image 列表")]
    [SerializeField] Transform plantIconRoot;
    [SerializeField] Image plantIconPrefab;
    [SerializeField] bool hideEmptyPlantSlots = true;
    [SerializeField] Color emptyPlantTint = new Color(1f, 1f, 1f, 0.15f);

    [Header("按钮（可选）")]
    [SerializeField] Button toggleDetailButton;
    [SerializeField] GameObject detailContentRoot;
    [SerializeField] Button openMapButton;
    [SerializeField] Button closeButton;

    readonly List<Image> _spawnedPlantIcons = new List<Image>();
    bool _detailVisible = true;

    public override void Init()
    {
        if (toggleDetailButton != null)
            toggleDetailButton.onClick.AddListener(ToggleDetail);
        if (openMapButton != null)
            openMapButton.onClick.AddListener(OnOpenMap);
        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);
    }

    void OnEnable()
    {
        RoguelikeRunService.OnRunStarted += OnRunLifecycleChanged;
        RoguelikeRunService.OnActMapGenerated += OnRunLifecycleChanged;
        RoguelikeRunService.OnNodeEntered += OnNodeEntered;
        RoguelikeRunService.OnNodeResolved += OnNodeResolved;
        RoguelikeRunService.OnActCompleted += OnActCompletedHandler;
        RoguelikeRunService.OnRunCompleted += OnRunEnded;
        Refresh();
    }

    void OnDisable()
    {
        RoguelikeRunService.OnRunStarted -= OnRunLifecycleChanged;
        RoguelikeRunService.OnActMapGenerated -= OnRunLifecycleChanged;
        RoguelikeRunService.OnNodeEntered -= OnNodeEntered;
        RoguelikeRunService.OnNodeResolved -= OnNodeResolved;
        RoguelikeRunService.OnActCompleted -= OnActCompletedHandler;
        RoguelikeRunService.OnRunCompleted -= OnRunEnded;
    }

    void OnRunLifecycleChanged(RoguelikeRunState _) => Refresh();
    void OnActCompletedHandler() => Refresh();
    void OnNodeEntered(RoguelikeMapNode _) => Refresh();
    void OnNodeResolved(RoguelikeMapNode _, bool __) => Refresh();
    void OnRunEnded() => HideForRunEnded();

    public static void TryShowAndRefresh()
    {
        var panel = UIManage.GetView<RoguelikeRunInfoPanel>();
        if (panel == null)
            return;

        if (!RoguelikeRunService.HasActiveRun)
        {
            panel.HandleNoActiveRun();
            return;
        }

        if (panel.autoShowWhenRunActive)
            panel.Show();
        panel.Refresh();
    }

    public static void HideForRunEnded()
    {
        UIManage.GetView<RoguelikeRunInfoPanel>()?.Hide();
    }

    public override void Show()
    {
        base.Show();
        Refresh();
    }

    public void Refresh()
    {
        if (!RoguelikeRunService.HasActiveRun)
        {
            HandleNoActiveRun();
            return;
        }

        if (emptyStateRoot != null)
            emptyStateRoot.SetActive(false);

        var state = RoguelikeRunService.State;
        var config = RoguelikeRunService.ActiveRunConfig;

        SetText(bandNameText, string.IsNullOrEmpty(state.selectedBandName) ? "—" : state.selectedBandName);
        SetText(runConfigNameText, config != null ? config.name : "—");
        SetText(actTitleText, RoguelikeRunInfoFormatter.FormatActTitle(state, config));
        SetText(runSeedText, state.runSeed.ToString());
        SetText(currentNodeText, RoguelikeRunInfoFormatter.FormatCurrentNode(state));
        SetText(pendingNodeText, RoguelikeRunInfoFormatter.FormatPendingNode(state, RoguelikeRunService.PendingNodeId));
        SetText(selectableNextText, RoguelikeRunInfoFormatter.FormatSelectableNext(state));
        SetText(progressText, RoguelikeRunInfoFormatter.FormatProgress(state));

        int plantCount = state.ownedPlantCreatorIds?.Count ?? 0;
        SetText(ownedPlantCountText, plantCount.ToString());
        SetText(runGoldText, RoguelikeRunInfoFormatter.FormatRunGold(state));
        SetText(summaryText, BuildSummary(state, config, plantCount));

        RefreshPlantIcons(state);
    }

    void HandleNoActiveRun()
    {
        if (emptyStateRoot != null)
            emptyStateRoot.SetActive(true);

        if (hideWhenNoActiveRun)
            Hide();
    }

    static string BuildSummary(RoguelikeRunState state, RunMapConfig config, int plantCount)
    {
        string band = string.IsNullOrEmpty(state.selectedBandName) ? "未知乐队" : state.selectedBandName;
        string act = RoguelikeRunInfoFormatter.FormatActTitle(state, config);
        string node = RoguelikeRunInfoFormatter.FormatCurrentNode(state);
        return $"{band}\n{act}\n{node}\n金币 {state.runGold} · 植物 {plantCount}";
    }

    void RefreshPlantIcons(RoguelikeRunState state)
    {
        var creators = RoguelikeRunPlantPool.ResolveCreators(state?.ownedPlantCreatorIds);
        if (plantIconRoot != null && plantIconPrefab != null)
        {
            RefreshDynamicPlantIcons(creators);
            return;
        }

        RefreshFixedPlantIcons(creators);
    }

    void RefreshFixedPlantIcons(List<PropertyCreator> creators)
    {
        if (plantIconImages == null || plantIconImages.Count == 0)
            return;

        for (int i = 0; i < plantIconImages.Count; i++)
        {
            var img = plantIconImages[i];
            if (img == null)
                continue;

            bool has = creators != null && i < creators.Count && creators[i] != null;
            if (has)
            {
                img.gameObject.SetActive(true);
                img.sprite = creators[i].chessSprite;
                img.color = creators[i].chessSprite != null ? Color.white : emptyPlantTint;
                img.preserveAspect = true;
            }
            else if (hideEmptyPlantSlots)
            {
                img.gameObject.SetActive(false);
            }
            else
            {
                img.gameObject.SetActive(true);
                img.sprite = null;
                img.color = emptyPlantTint;
            }
        }
    }

    void RefreshDynamicPlantIcons(List<PropertyCreator> creators)
    {
        ClearSpawnedPlantIcons();

        int count = creators != null ? creators.Count : 0;
        for (int i = 0; i < count; i++)
        {
            var creator = creators[i];
            if (creator == null)
                continue;

            var img = Instantiate(plantIconPrefab, plantIconRoot);
            img.gameObject.SetActive(true);
            img.sprite = creator.chessSprite;
            img.color = creator.chessSprite != null ? Color.white : emptyPlantTint;
            img.preserveAspect = true;
            _spawnedPlantIcons.Add(img);
        }
    }

    void ClearSpawnedPlantIcons()
    {
        for (int i = _spawnedPlantIcons.Count - 1; i >= 0; i--)
        {
            if (_spawnedPlantIcons[i] != null)
                Destroy(_spawnedPlantIcons[i].gameObject);
        }
        _spawnedPlantIcons.Clear();
    }

    void ToggleDetail()
    {
        _detailVisible = !_detailVisible;
        if (detailContentRoot != null)
            detailContentRoot.SetActive(_detailVisible);
    }

    void OnOpenMap()
    {
        UIManage.GetView<RoguelikeMapPanel>()?.ShowAndRefresh();
    }

    static void SetText(TMP_Text text, string value)
    {
        if (text != null)
            text.text = value;
    }

    void OnDestroy()
    {
        ClearSpawnedPlantIcons();
    }
}
