using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 杀戮尖塔式战斗奖励面板：每条奖励是一个按钮，点击领取后消失；可跳过未领条目。
/// 预制体：<c>Resources/UIPrefab/RoguelikeRewardPanel</c>，根物体 name 须一致。
/// </summary>
public class RoguelikeRewardPanel : View
{
    [Header("UI 绑定")]
    [SerializeField] TMP_Text titleText;
    [SerializeField] TMP_Text totalGoldText;
    [SerializeField] Transform entriesRoot;
    [SerializeField] RoguelikeRewardEntryWidget entryPrefab;
    [SerializeField] Button skipButton;
    [SerializeField] TMP_Text skipButtonLabel;
    [SerializeField] RoguelikeRewardPlantPickSubview plantPickSubview;

    [Header("Run 结束结算（通关 / 本局结束）")]
    [Tooltip("结算专用「返回主菜单」；未绑定时结算模式回退用 skipButton")]
    [SerializeField] Button returnToMenuButton;
    [Tooltip("returnToMenuButton 文案；未绑定时改按钮子物体 TMP")]
    [SerializeField] TMP_Text returnToMenuButtonLabel;
    [Tooltip("搜刮区根节点（结算时隐藏）")]
    [SerializeField] GameObject rewardsSectionRoot;
    [Tooltip("结算摘要区（搜刮时隐藏）")]
    [SerializeField] GameObject runSummaryRoot;
    [Tooltip("到达 Act / 层数 / 金币 / 植物数")]
    [SerializeField] TMP_Text summaryText;

    [Header("动画")]
    [SerializeField] Animator panelAnimator;
    [SerializeField] string closeAnimStateName = "close";

    [Header("金币领取视觉（Item_Coin scatter）")]
    [Tooltip("硬币相对按钮中心的散开半径（屏幕像素）")]
    [SerializeField] float rewardCoinScatterRadiusMin = 56f;
    [SerializeField] float rewardCoinScatterRadiusMax = 140f;
    [Tooltip("从按钮中心向外散开的时长（秒）")]
    [SerializeField] float rewardCoinScatterDuration = 0.5f;

    [Header("布局（entryPrefab 为空时 runtime 生成用）")]
    [SerializeField] Vector2 entrySize = new Vector2(420f, 56f);
    [SerializeField] float entrySpacing = 8f;

    readonly List<RoguelikeRewardEntry> _entries = new List<RoguelikeRewardEntry>();
    readonly List<RoguelikeRewardEntryWidget> _widgets = new List<RoguelikeRewardEntryWidget>();
    Action _onComplete;
    bool _closing;
    RoguelikeRewardPanelMode _mode = RoguelikeRewardPanelMode.CombatRewards;

    enum RoguelikeRewardPanelMode
    {
        CombatRewards,
        RunEnd,
    }

    public override void Init()
    {
        if (panelAnimator == null)
            panelAnimator = GetComponent<Animator>();
        WireActionButtons();
        EnsurePlantPickSubview();
        EventController.Instance.AddListener(EventName.WhenLeaveLevel.ToString(), OnLeaveLevel);
    }

    void WireActionButtons()
    {
        if (skipButton != null)
        {
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(OnSkipClicked);
        }

        if (returnToMenuButton != null)
        {
            returnToMenuButton.onClick.RemoveAllListeners();
            returnToMenuButton.onClick.AddListener(OnReturnToMenuClicked);
        }
    }

    void EnsurePlantPickSubview()
    {
        if (plantPickSubview != null)
            return;
        plantPickSubview = GetComponentInChildren<RoguelikeRewardPlantPickSubview>(true);
    }

    void OnLeaveLevel()
    {
        RoguelikeRewardCoinVisual.ClearRecycleTarget();
        if (plantPickSubview != null)
            plantPickSubview.Hide();
        _closing = false;
        _mode = RoguelikeRewardPanelMode.CombatRewards;
        Hide();
        _entries.Clear();
        ClearWidgets();
        _onComplete = null;
    }

    /// <summary>Run 通关或失败后展示结算摘要；点「返回主菜单」后执行 onComplete。</summary>
    public static void ShowRunEnd(RoguelikeRunEndSummary summary, Action onComplete)
    {
        var panel = UIManage.GetView<RoguelikeRewardPanel>();
        if (panel == null)
        {
            Debug.LogError("[RoguelikeRewardPanel] 未找到面板");
            onComplete?.Invoke();
            return;
        }

        panel.OpenRunEnd(summary, onComplete);
    }

    /// <summary>战斗胜利后展示奖励；全部领完或跳过后调用 onComplete。</summary>
    public static void ShowRewards(IList<RoguelikeRewardEntry> entries, Action onComplete)
    {
        var panel = UIManage.GetView<RoguelikeRewardPanel>();
        if (panel == null)
        {
            Debug.LogError(
                "[RoguelikeRewardPanel] 未找到面板，请在 Resources/UIPrefab 创建 RoguelikeRewardPanel 预制体");
            onComplete?.Invoke();
            return;
        }

        panel.OpenCombatRewards(entries, onComplete);
    }

    void OpenCombatRewards(IList<RoguelikeRewardEntry> entries, Action onComplete)
    {
        _mode = RoguelikeRewardPanelMode.CombatRewards;
        _closing = false;
        _onComplete = onComplete;
        _entries.Clear();
        if (entries != null)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i] != null && !entries[i].claimed)
                    _entries.Add(entries[i]);
            }
        }

        if (titleText != null)
            titleText.text = "搜刮！";
        if (plantPickSubview != null)
            plantPickSubview.Hide();

        SetRewardsSectionVisible(true);
        ApplyRunEndSummary(null);

        RefreshTotalGold();
        RebuildEntries();
        RefreshActionButtonLabel();
        RoguelikeRewardCoinVisual.EnsureRecycleTarget();
        Show();
    }

    void OpenRunEnd(RoguelikeRunEndSummary summary, Action onComplete)
    {
        _mode = RoguelikeRewardPanelMode.RunEnd;
        _closing = false;
        _onComplete = onComplete;
        _entries.Clear();
        ClearWidgets();

        if (titleText != null)
            titleText.text = summary != null && summary.kind == RoguelikeRunEndKind.Victory
                ? "通关！"
                : "本局结束";
        if (plantPickSubview != null)
            plantPickSubview.Hide();

        SetRewardsSectionVisible(false);
        ApplyRunEndSummary(summary);
        RefreshActionButtonLabel();
        Show();
    }

    void SetRewardsSectionVisible(bool rewardsVisible)
    {
        if (runSummaryRoot != null)
            runSummaryRoot.SetActive(!rewardsVisible);

        if (rewardsVisible)
        {
            if (rewardsSectionRoot != null)
                rewardsSectionRoot.SetActive(true);
            SetRewardsPanelContentVisible(true);
            RefreshActionButtonsVisibility(isRunEnd: false);
            return;
        }

        // 结算模式：保留操作按钮，仅隐藏搜刮列表等
        if (rewardsSectionRoot != null)
            rewardsSectionRoot.SetActive(true);
        SetRewardsPanelContentVisible(false);
        RefreshActionButtonsVisibility(isRunEnd: true);
    }

    void SetRewardsPanelContentVisible(bool visible)
    {
        if (entriesRoot != null)
            entriesRoot.gameObject.SetActive(visible);
        if (totalGoldText != null)
            totalGoldText.gameObject.SetActive(visible);

        if (rewardsSectionRoot == null || visible)
            return;

        var root = rewardsSectionRoot.transform;
        for (int i = 0; i < root.childCount; i++)
        {
            var child = root.GetChild(i);
            if (skipButton != null && child == skipButton.transform)
                continue;
            if (returnToMenuButton != null && child == returnToMenuButton.transform)
                continue;
            child.gameObject.SetActive(false);
        }
    }

    void RefreshActionButtonsVisibility(bool isRunEnd)
    {
        bool useDedicatedReturn = returnToMenuButton != null;
        if (skipButton != null)
            skipButton.gameObject.SetActive(!isRunEnd || !useDedicatedReturn);
        if (returnToMenuButton != null)
            returnToMenuButton.gameObject.SetActive(isRunEnd);
    }

    void ApplyRunEndSummary(RoguelikeRunEndSummary summary)
    {
        if (summaryText == null)
            return;
        summaryText.text = summary != null ? summary.FormatSummaryText() : string.Empty;
    }

    void Open(IList<RoguelikeRewardEntry> entries, Action onComplete)
    {
        OpenCombatRewards(entries, onComplete);
    }

    void RebuildEntries()
    {
        ClearWidgets();
        if (entriesRoot == null)
            return;

        float y = 0f;
        for (int i = 0; i < _entries.Count; i++)
        {
            var entry = _entries[i];
            if (entry.claimed)
                continue;

            RoguelikeRewardEntryWidget widget;
            if (entryPrefab != null)
            {
                widget = Instantiate(entryPrefab, entriesRoot);
                var rt = widget.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(0.5f, 1f);
                    rt.anchorMax = new Vector2(0.5f, 1f);
                    rt.pivot = new Vector2(0.5f, 1f);
                    rt.anchoredPosition = new Vector2(0f, -y);
                }
            }
            else
            {
                widget = RoguelikeRewardEntryWidget.CreateRuntime(entriesRoot, entrySize);
                var rt = widget.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 1f);
                rt.anchorMax = new Vector2(0.5f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
                rt.anchoredPosition = new Vector2(0f, -y);
            }

            widget.Bind(entry, e => OnEntryClicked(e, widget));
            _widgets.Add(widget);
            y += entrySize.y + entrySpacing;
        }
    }

    void OnEntryClicked(RoguelikeRewardEntry entry, RoguelikeRewardEntryWidget widget)
    {
        if (entry == null || entry.claimed)
            return;

        if (entry.kind == RoguelikeRewardEntryKind.PlantPick)
        {
            EnsurePlantPickSubview();
            if (plantPickSubview == null)
            {
                Debug.LogError("[RoguelikeRewardPanel] PlantPick 子面板未配置");
                return;
            }
            plantPickSubview.Show(entry, OnPlantPickFinished);
            return;
        }

        if (entry.kind == RoguelikeRewardEntryKind.Gold)
        {
            int amount = entry.goldAmount;
            if (!RoguelikeRewardFlow.TryClaimEntry(entry))
                return;

            RoguelikeRewardCoinVisual.Spawn(
                this, amount, widget,
                rewardCoinScatterRadiusMin, rewardCoinScatterRadiusMax,
                rewardCoinScatterDuration);
            AfterEntryClaimed();
            return;
        }

        if (!RoguelikeRewardFlow.TryClaimEntry(entry))
            return;

        AfterEntryClaimed();
    }

    void OnPlantPickFinished(RoguelikeRewardEntry entry, string chosenChessName)
    {
        if (!RoguelikeRewardFlow.TryClaimPlantPick(entry, chosenChessName))
            return;

        if (plantPickSubview != null)
            plantPickSubview.Hide();
        AfterEntryClaimed();
    }

    void AfterEntryClaimed()
    {
        RefreshTotalGold();
        RebuildEntries();
        RefreshActionButtonLabel();
        TryCompleteIfEmpty();
    }

    void OnSkipClicked()
    {
        if (plantPickSubview != null && plantPickSubview.IsVisible)
            plantPickSubview.Hide();
        Complete();
    }

    void OnReturnToMenuClicked()
    {
        if (_mode != RoguelikeRewardPanelMode.RunEnd)
            return;
        OnSkipClicked();
    }

    void TryCompleteIfEmpty()
    {
        for (int i = 0; i < _entries.Count; i++)
        {
            if (!_entries[i].claimed)
                return;
        }
        Complete();
    }

    void Complete()
    {
        if (_closing)
            return;
        _closing = true;

        if (panelAnimator != null && !string.IsNullOrEmpty(closeAnimStateName))
        {
            panelAnimator.Play(closeAnimStateName, 0, 0f);
            return;
        }

        FinishClose();
    }

    /// <summary>close 动画最后一帧 Animation Event 调用。</summary>
    public void OnCloseAnimationFinished()
    {
        FinishClose();
    }

    void FinishClose()
    {
        RoguelikeRewardCoinVisual.ClearRecycleTarget();
        if (plantPickSubview != null)
            plantPickSubview.Hide();
        _closing = false;
        Hide();
        ClearWidgets();
        _entries.Clear();
        var cb = _onComplete;
        _onComplete = null;
        cb?.Invoke();
    }

    void RefreshTotalGold()
    {
        if (totalGoldText == null)
            return;
        int gold = RoguelikeRunService.State?.runGold ?? 0;
        totalGoldText.text = $"当前金币：{gold}";
    }

    void RefreshActionButtonLabel()
    {
        if (_mode == RoguelikeRewardPanelMode.RunEnd)
        {
            SetButtonLabel(returnToMenuButton, returnToMenuButtonLabel, "返回主菜单");
            if (returnToMenuButton == null)
                SetButtonLabel(skipButton, skipButtonLabel, "返回主菜单");
            return;
        }

        bool anyUnclaimed = false;
        for (int i = 0; i < _entries.Count; i++)
        {
            if (!_entries[i].claimed)
            {
                anyUnclaimed = true;
                break;
            }
        }

        SetButtonLabel(skipButton, skipButtonLabel, anyUnclaimed ? "跳过" : "继续");
    }

    static void SetButtonLabel(Button button, TMP_Text label, string text)
    {
        if (label != null)
            label.text = text;
        else if (button != null)
        {
            var tmp = button.GetComponentInChildren<TMP_Text>(true);
            if (tmp != null)
                tmp.text = text;
        }
    }

    void ClearWidgets()
    {
        for (int i = _widgets.Count - 1; i >= 0; i--)
        {
            if (_widgets[i] != null)
                Destroy(_widgets[i].gameObject);
        }
        _widgets.Clear();
    }
}
