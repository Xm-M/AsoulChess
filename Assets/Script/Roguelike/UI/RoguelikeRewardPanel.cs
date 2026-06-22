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

    [Header("动画")]
    [SerializeField] Animator panelAnimator;
    [SerializeField] string closeAnimStateName = "close";

    [Header("布局（entryPrefab 为空时 runtime 生成用）")]
    [SerializeField] Vector2 entrySize = new Vector2(420f, 56f);
    [SerializeField] float entrySpacing = 8f;

    readonly List<RoguelikeRewardEntry> _entries = new List<RoguelikeRewardEntry>();
    readonly List<RoguelikeRewardEntryWidget> _widgets = new List<RoguelikeRewardEntryWidget>();
    Action _onComplete;
    bool _closing;

    public override void Init()
    {
        if (panelAnimator == null)
            panelAnimator = GetComponent<Animator>();
        if (skipButton != null)
            skipButton.onClick.AddListener(OnSkipClicked);
        EventController.Instance.AddListener(EventName.WhenLeaveLevel.ToString(), OnLeaveLevel);
    }

    void OnLeaveLevel()
    {
        _closing = false;
        Hide();
        _entries.Clear();
        ClearWidgets();
        _onComplete = null;
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

        panel.Open(entries, onComplete);
    }

    void Open(IList<RoguelikeRewardEntry> entries, Action onComplete)
    {
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
        RefreshTotalGold();
        RebuildEntries();
        RefreshSkipLabel();
        Show();
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

            widget.Bind(entry, OnEntryClicked);
            _widgets.Add(widget);
            y += entrySize.y + entrySpacing;
        }
    }

    void OnEntryClicked(RoguelikeRewardEntry entry)
    {
        if (entry == null || entry.claimed)
            return;

        if (!RoguelikeRewardFlow.TryClaimEntry(entry))
            return;

        RefreshTotalGold();
        RebuildEntries();
        RefreshSkipLabel();
        TryCompleteIfEmpty();
    }

    void OnSkipClicked()
    {
        Complete();
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

    void RefreshSkipLabel()
    {
        if (skipButtonLabel == null)
            return;

        bool anyUnclaimed = false;
        for (int i = 0; i < _entries.Count; i++)
        {
            if (!_entries[i].claimed)
            {
                anyUnclaimed = true;
                break;
            }
        }

        skipButtonLabel.text = anyUnclaimed ? "跳过" : "继续";
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
