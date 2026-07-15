using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 肉鸽通用 HUD：常驻金币、Act；子物体可挂植物仓库选卡 UI（只读）；设置按钮打开暂停面板。
/// 预制体：<c>Resources/UIPrefab/RoguelikeRunInfoPanel</c>，根物体 name 须一致。
/// </summary>
public class RoguelikeRunInfoPanel : View
{
    [Header("显示策略")]
    [Tooltip("Run 进行中时自动 Show 本面板")]
    [SerializeField] bool autoShowWhenRunActive = true;
    [Tooltip("无 active Run 时自动 Hide")]
    [SerializeField] bool hideWhenNoActiveRun = true;
    [Tooltip("无 Run 时显示的占位节点（可选）")]
    [SerializeField] GameObject emptyStateRoot;

    [Header("HUD 文本")]
    [Tooltip("当前 Act 标题，如「前院 (1/3)」")]
    [SerializeField] TMP_Text actTitleText;
    [Tooltip("本 Run 持有金币")]
    [SerializeField] TMP_Text runGoldText;
    [Tooltip("本 Run 拥有小推车数量")]
    [SerializeField] TMP_Text runLawnMowerText;
    [Tooltip("本 Run 携带格数量（loadout 上限）")]
    [SerializeField] TMP_Text runLoadoutSlotText;
    [Tooltip("当前地图层（L1、L2…）")]
    [SerializeField] TMP_Text runMapLayerText;

    [Header("可选调试文本（玩家 HUD 可不绑）")]
    [Tooltip("RunMapConfig 资产名")]
    [SerializeField] TMP_Text runConfigNameText;
    [Tooltip("本局随机种子")]
    [SerializeField] TMP_Text runSeedText;
    [Tooltip("当前地图节点")]
    [SerializeField] TMP_Text currentNodeText;
    [Tooltip("待进入战斗的节点")]
    [SerializeField] TMP_Text pendingNodeText;
    [Tooltip("可选下一层节点摘要")]
    [SerializeField] TMP_Text selectableNextText;
    [Tooltip("地图进度统计")]
    [SerializeField] TMP_Text progressText;

    [Header("植物卡组")]
    [Tooltip("打开卡组弹层")]
    [SerializeField] Button openDeckButton;
    [Tooltip("关闭卡组弹层")]
    [SerializeField] Button closeDeckButton;
    [Tooltip("卡组弹层根节点（子物体整段植物选择界面，默认隐藏）")]
    [SerializeField] GameObject deckPanelRoot;
    [Tooltip("仓库选卡预制体，与 PlantsShop.shopSelectIconPre 相同")]
    [SerializeField] GameObject shopSelectIconPre;
    [Tooltip("选卡列表父节点（Grid/Content）")]
    [SerializeField] Transform selectIconParent;

    [Header("植物详情")]
    [Tooltip("植物名称")]
    [SerializeField] TMP_Text detailName;
    [Tooltip("植物立绘/图标")]
    [SerializeField] Image detailPlantImage;
    [Tooltip("羁绊/标签")]
    [SerializeField] TMP_Text detailTags;
    [Tooltip("属性汇总文本（生命/攻击/护甲等）")]
    [SerializeField] TMP_Text detailAttributes;
    [Tooltip("简介 ScrollRect")]
    [SerializeField] ScrollRect descriptionScroll;
    [Tooltip("简介正文")]
    [SerializeField] TMP_Text descriptionText;

    [Header("按钮")]
    [Tooltip("打开暂停/设置面板（ParsePanel）")]
    [SerializeField] Button settingsButton;
    [Tooltip("关闭本 HUD（可选）")]
    [SerializeField] Button closeHudButton;

    [Header("HUD 图标说明（悬停 2s / 长按）")]
    [SerializeField] RectTransform goldStatIconRoot;
    [SerializeField] RectTransform deckStatIconRoot;
    [SerializeField] RectTransform lawnMowerStatIconRoot;
    [SerializeField] RectTransform loadoutStatIconRoot;
    [SerializeField] RectTransform mapLayerStatIconRoot;
    [SerializeField] RectTransform tooltipRoot;
    [SerializeField] TMP_Text tooltipDescriptionText;
    [SerializeField] Vector2 tooltipScreenOffset = new Vector2(-140f, 0f);
    [SerializeField] float hudTooltipHoverDelaySeconds = 2f;

    readonly List<ShopSelectIcon> _deckSelectIcons = new List<ShopSelectIcon>();
    RectTransform _tooltipFollowTarget;

    void Awake()
    {
        if (tooltipRoot != null)
            tooltipRoot.gameObject.SetActive(false);
    }

    public override void Init()
    {
        if (openDeckButton != null)
            openDeckButton.onClick.AddListener(OpenDeckPanel);
        if (closeDeckButton != null)
            closeDeckButton.onClick.AddListener(CloseDeckPanel);
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenPausePanel);
        if (closeHudButton != null)
            closeHudButton.onClick.AddListener(Hide);

        WireStatTooltips();
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
        HideHud();
    }

    /// <summary>进入战斗关卡前隐藏 HUD（含卡组弹层）。</summary>
    public static void HideForCombat()
    {
        HideHud();
    }

    static void HideHud()
    {
        var panel = UIManage.GetView<RoguelikeRunInfoPanel>();
        if (panel != null)
            panel.Hide();
    }

    public override void Show()
    {
        base.Show();
        Refresh();
    }

    public override void Hide()
    {
        HideHudStatTooltip();
        CloseDeckPanel();
        base.Hide();
    }

    void LateUpdate()
    {
        if (tooltipRoot == null || !tooltipRoot.gameObject.activeSelf || _tooltipFollowTarget == null)
            return;
        SyncTooltipPosition();
    }

    public string GetHudStatTooltipText(RoguelikeRunInfoHudStatKind kind) =>
        RoguelikeRunInfoFormatter.FormatHudStatTooltip(kind, RoguelikeRunService.State);

    /// <summary>由 <see cref="RoguelikeRunInfoHudTooltipTrigger"/> 调用。</summary>
    public void ShowHudStatTooltip(RectTransform iconRect, string text)
    {
        if (tooltipRoot == null || tooltipDescriptionText == null || iconRect == null)
            return;

        _tooltipFollowTarget = iconRect;
        tooltipDescriptionText.text = text ?? string.Empty;
        tooltipRoot.gameObject.SetActive(true);
        SyncTooltipPosition();
    }

    /// <summary>由 <see cref="RoguelikeRunInfoHudTooltipTrigger"/> 调用。</summary>
    public void HideHudStatTooltip()
    {
        _tooltipFollowTarget = null;
        if (tooltipRoot != null)
            tooltipRoot.gameObject.SetActive(false);
    }

    void WireStatTooltips()
    {
        WireStatTooltip(goldStatIconRoot, RoguelikeRunInfoHudStatKind.Gold);
        WireStatTooltip(deckStatIconRoot, RoguelikeRunInfoHudStatKind.Deck);
        WireStatTooltip(lawnMowerStatIconRoot, RoguelikeRunInfoHudStatKind.LawnMower);
        WireStatTooltip(loadoutStatIconRoot, RoguelikeRunInfoHudStatKind.LoadoutSlot);
        WireStatTooltip(mapLayerStatIconRoot, RoguelikeRunInfoHudStatKind.MapLayer);
    }

    void WireStatTooltip(RectTransform iconRoot, RoguelikeRunInfoHudStatKind kind)
    {
        if (iconRoot == null)
            return;

        var trigger = iconRoot.GetComponent<RoguelikeRunInfoHudTooltipTrigger>();
        if (trigger == null)
            trigger = iconRoot.gameObject.AddComponent<RoguelikeRunInfoHudTooltipTrigger>();

        trigger.Configure(this, kind, hudTooltipHoverDelaySeconds);

        var image = iconRoot.GetComponent<Image>();
        if (image != null)
            image.raycastTarget = true;
    }

    void SyncTooltipPosition()
    {
        if (_tooltipFollowTarget == null || tooltipRoot == null)
            return;

        Canvas canvas = tooltipRoot.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            tooltipRoot.position = _tooltipFollowTarget.position + (Vector3)tooltipScreenOffset;
            return;
        }

        var canvasRect = canvas.transform as RectTransform;
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(cam, _tooltipFollowTarget.position);
        screen += tooltipScreenOffset;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRect, screen, cam, out Vector3 world))
            tooltipRoot.position = world;
        else
            tooltipRoot.position = _tooltipFollowTarget.position + (Vector3)tooltipScreenOffset;
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

        SetText(actTitleText, RoguelikeRunInfoFormatter.FormatActTitle(state, config));
        SetText(runGoldText, RoguelikeRunInfoFormatter.FormatRunGold(state));
        SetText(runLawnMowerText, RoguelikeRunInfoFormatter.FormatLawnMowerCount(state));
        SetText(runLoadoutSlotText, RoguelikeRunInfoFormatter.FormatLoadoutSlotCount(state));
        SetText(runMapLayerText, RoguelikeRunInfoFormatter.FormatMapLayer(state));
        SetText(runConfigNameText, config != null ? config.name : "—");
        SetText(runSeedText, state.runSeed.ToString());
        SetText(currentNodeText, RoguelikeRunInfoFormatter.FormatCurrentNode(state));
        SetText(pendingNodeText, RoguelikeRunInfoFormatter.FormatPendingNode(state, RoguelikeRunService.PendingNodeId));
        SetText(selectableNextText, RoguelikeRunInfoFormatter.FormatSelectableNext(state));
        SetText(progressText, RoguelikeRunInfoFormatter.FormatProgress(state));

        if (IsDeckPanelVisible())
            RefreshDeckList();
    }

    /// <summary>仓库选卡（<see cref="ShopSelectIcon.viewOnly"/>）点击时展示详情。</summary>
    public void ShowPlantDetail(PropertyCreator creator)
    {
        if (creator == null)
        {
            ClearPlantDetail();
            return;
        }

        if (detailName != null)
            detailName.text = creator.chessName;
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

    void HandleNoActiveRun()
    {
        if (emptyStateRoot != null)
            emptyStateRoot.SetActive(true);

        if (hideWhenNoActiveRun)
            Hide();
    }

    void OpenDeckPanel()
    {
        if (deckPanelRoot != null)
            deckPanelRoot.SetActive(true);
        RefreshDeckList();
    }

    void CloseDeckPanel()
    {
        if (deckPanelRoot != null)
            deckPanelRoot.SetActive(false);
        ClearDeckList();
        ClearPlantDetail();
    }

    bool IsDeckPanelVisible() =>
        deckPanelRoot != null && deckPanelRoot.activeSelf;

    void RefreshDeckList()
    {
        ClearDeckList();
        if (selectIconParent == null || shopSelectIconPre == null)
            return;

        var state = RoguelikeRunService.State;
        var creators = RoguelikeRunPlantPool.ResolveCreators(state?.ownedPlantCreatorIds);
        if (creators == null || creators.Count == 0)
        {
            ClearPlantDetail();
            return;
        }

        for (int i = 0; i < creators.Count; i++)
        {
            var creator = creators[i];
            if (creator == null)
                continue;

            ShopSelectIcon selectIcon;
            if (creator.PlantEntrepotCardPre == null)
                selectIcon = Instantiate(shopSelectIconPre, selectIconParent).GetComponent<ShopSelectIcon>();
            else
                selectIcon = Instantiate(creator.PlantEntrepotCardPre, selectIconParent).GetComponent<ShopSelectIcon>();

            if (selectIcon == null)
                continue;

            selectIcon.viewOnly = true;
            selectIcon.InitSelectIcon(creator);
            _deckSelectIcons.Add(selectIcon);
        }

        if (_deckSelectIcons.Count > 0 && _deckSelectIcons[0].select != null)
            ShowPlantDetail(_deckSelectIcons[0].select);
        else
            ClearPlantDetail();
    }

    void ClearDeckList()
    {
        if (selectIconParent != null)
        {
            for (int i = selectIconParent.childCount - 1; i >= 0; i--)
                Destroy(selectIconParent.GetChild(i).gameObject);
        }
        _deckSelectIcons.Clear();
    }

    void ClearPlantDetail()
    {
        if (detailName != null)
            detailName.text = "";
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
    }

    void OpenPausePanel()
    {
        var parsePanel = UIManage.GetView<ParsePanel>();
        if (parsePanel == null)
        {
            Debug.LogWarning("[RoguelikeRunInfoPanel] 未找到 ParsePanel，无法打开暂停面板");
            return;
        }

        parsePanel.Show();
        parsePanel.ShowMenuPanel();
    }

    static void SetText(TMP_Text text, string value)
    {
        if (text != null)
            text.text = value;
    }

    void OnDestroy()
    {
        ClearDeckList();
    }
}
