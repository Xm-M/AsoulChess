using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 奖励面板内嵌的植物三选一子面板（非独立 View / Prefab）。
/// 卡牌布局在预制体里拼好，将 <see cref="RoguelikePlantPickCardWidget"/> 拖入 <see cref="pickCards"/> 列表即可。
/// </summary>
public class RoguelikeRewardPlantPickSubview : MonoBehaviour
{
    [SerializeField] GameObject rootPanel;
    [SerializeField] TMP_Text titleText;
    [SerializeField] Button skipButton;
    [SerializeField] TMP_Text skipButtonLabel;

    [Header("三选一卡牌（按顺序对应候选 1/2/3）")]
    [SerializeField] List<RoguelikePlantPickCardWidget> pickCards = new List<RoguelikePlantPickCardWidget>();

    RoguelikeRewardEntry _entry;
    Action<RoguelikeRewardEntry, string> _onFinished;

    void Awake()
    {
        if (rootPanel == null)
            rootPanel = gameObject;

        if (skipButton != null)
            skipButton.onClick.AddListener(OnSkipClicked);

        Hide();
    }

    public bool IsVisible => rootPanel != null && rootPanel.activeSelf;

    public void Show(RoguelikeRewardEntry entry, Action<RoguelikeRewardEntry, string> onFinished)
    {
        _entry = entry;
        _onFinished = onFinished;

        if (titleText != null)
            titleText.text = "选择一张牌加入牌组";
        if (skipButtonLabel != null)
            skipButtonLabel.text = "跳过";

        RebindCards();
        if (rootPanel != null)
            rootPanel.SetActive(true);
    }

    public void Hide()
    {
        _entry = null;
        _onFinished = null;
        ClearCards();
        if (rootPanel != null)
            rootPanel.SetActive(false);
    }

    void RebindCards()
    {
        for (int i = 0; i < pickCards.Count; i++)
        {
            var card = pickCards[i];
            if (card == null)
                continue;

            if (_entry?.plantPickOptions != null && i < _entry.plantPickOptions.Count)
            {
                string chessName = _entry.plantPickOptions[i];
                var creator = ResolveCreator(chessName);
                if (creator != null)
                    card.Bind(creator, OnCardPicked);
                else
                    card.Clear();
            }
            else
                card.Clear();
        }
    }

    void OnCardPicked(string chessName)
    {
        if (_entry == null || _onFinished == null)
            return;
        _onFinished(_entry, chessName);
    }

    void OnSkipClicked()
    {
        if (_entry == null || _onFinished == null)
            return;
        _onFinished(_entry, null);
    }

    void ClearCards()
    {
        for (int i = 0; i < pickCards.Count; i++)
        {
            if (pickCards[i] != null)
                pickCards[i].Clear();
        }
    }

    static PropertyCreator ResolveCreator(string chessName)
    {
        if (string.IsNullOrEmpty(chessName) || GameManage.instance?.allChess == null)
            return null;
        for (int i = 0; i < GameManage.instance.allChess.Count; i++)
        {
            var c = GameManage.instance.allChess[i];
            if (c != null && c.chessName == chessName)
                return c;
        }
        Debug.LogWarning($"[RoguelikeRewardPlantPickSubview] 未找到植物 creator: {chessName}");
        return null;
    }
}
