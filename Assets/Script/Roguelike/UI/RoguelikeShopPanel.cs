using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 肉鸽 Run 地图商店。UI 布局复制 <see cref="CoinShopPanel"/>（玩家商店面板 / 戴夫车），
/// 预制体：<c>Resources/UIPrefab/RoguelikeShopPanel</c>（从玩家商店面板复制后换脚本与根物体名）。
/// </summary>
public class RoguelikeShopPanel : View
{
    [Header("UI 引用（与 CoinShopPanel 字段名一致，便于复制 prefab）")]
    public Transform itemParent;
    public GameObject shopItemPrefab;
    public TMP_Text coinText;
    public Button returnButton;

    [Header("翻页（可选，与戴夫车一致）")]
    [Tooltip("拖入车的 Animator，翻页时播放 next 状态")]
    public Animator carAnimator;
    [Tooltip("每页商品数量")]
    public int itemsPerPage = 8;
    [Tooltip("翻页时播放的 Animator 状态名")]
    public string pageTurnAnimName = "next";

    readonly List<GameObject> _itemInstances = new List<GameObject>();
    readonly List<int> _displaySlotIndices = new List<int>();
    int _currentPageIndex;

    public override void Init()
    {
        if (returnButton != null)
            returnButton.onClick.AddListener(OnLeaveClicked);
    }

    public static void ShowShop()
    {
        var panel = UIManage.GetView<RoguelikeShopPanel>();
        if (panel == null)
        {
            Debug.LogError(
                "[RoguelikeShopPanel] 未找到面板。请复制玩家商店面板为 RoguelikeShopPanel.prefab，" +
                "根物体改名 RoguelikeShopPanel 并挂本脚本");
            RoguelikeRunService.LeaveShopNode();
            return;
        }

        panel.Open();
    }

    void Open()
    {
        _currentPageIndex = 0;
        RefreshCoinDisplay();
        RefreshItemList();
        RoguelikeRunInfoPanel.TryShowAndRefresh();
        Show();
    }

    public override void Show()
    {
        base.Show();
        RefreshCoinDisplay();
        RefreshItemList();
    }

    /// <summary>下一页，供 Button OnClick 绑定</summary>
    public void GoToNextPage()
    {
        if (_currentPageIndex >= GetTotalPages() - 1)
            return;
        _currentPageIndex++;
        PlayPageTurnAnim();
        RefreshItemList();
    }

    /// <summary>上一页，供 Button OnClick 绑定</summary>
    public void GoToPreviousPage()
    {
        if (_currentPageIndex <= 0)
            return;
        _currentPageIndex--;
        PlayPageTurnAnim();
        RefreshItemList();
    }

    void PlayPageTurnAnim()
    {
        if (carAnimator == null || string.IsNullOrEmpty(pageTurnAnimName))
            return;
        carAnimator.Play(pageTurnAnimName, 0, 0f);
    }

    void RefreshCoinDisplay()
    {
        if (coinText == null)
            return;
        int gold = RoguelikeRunService.State?.runGold ?? 0;
        coinText.text = gold.ToString();
    }

    void RefreshItemList()
    {
        ClearItems();
        BuildDisplaySlotIndices();

        if (itemParent == null || shopItemPrefab == null || _displaySlotIndices.Count == 0)
        {
            _currentPageIndex = 0;
            return;
        }

        ClampPageIndex(_displaySlotIndices.Count);
        int pageSize = Mathf.Max(1, itemsPerPage);
        int start = _currentPageIndex * pageSize;
        int end = Mathf.Min(start + pageSize, _displaySlotIndices.Count);

        var offers = RoguelikeRunService.State?.shopOffers;
        if (offers == null)
            return;

        for (int i = start; i < end; i++)
        {
            int slotIndex = _displaySlotIndices[i];
            if (slotIndex < 0 || slotIndex >= offers.Count)
                continue;
            var go = CreateShopItem(slotIndex, offers[slotIndex]);
            if (go != null)
                _itemInstances.Add(go);
        }
    }

    /// <summary>展示全部 offer（含已售），与 CoinShopPanel 已拥有/售罄表现一致。</summary>
    void BuildDisplaySlotIndices()
    {
        _displaySlotIndices.Clear();
        var offers = RoguelikeRunService.State?.shopOffers;
        if (offers == null)
            return;

        for (int i = 0; i < offers.Count; i++)
        {
            if (offers[i] != null && !string.IsNullOrEmpty(offers[i].creatorChessName))
                _displaySlotIndices.Add(i);
        }
    }

    int GetTotalPages()
    {
        int count = _displaySlotIndices.Count;
        if (count == 0)
            return 0;
        int pageSize = Mathf.Max(1, itemsPerPage);
        return (count + pageSize - 1) / pageSize;
    }

    void ClampPageIndex(int itemCount)
    {
        int pageSize = Mathf.Max(1, itemsPerPage);
        int totalPages = itemCount == 0 ? 0 : (itemCount + pageSize - 1) / pageSize;
        if (totalPages == 0)
        {
            _currentPageIndex = 0;
            return;
        }
        if (_currentPageIndex >= totalPages)
            _currentPageIndex = totalPages - 1;
        if (_currentPageIndex < 0)
            _currentPageIndex = 0;
    }

    GameObject CreateShopItem(int slotIndex, RoguelikeShopOffer offer)
    {
        var creator = ResolveCreator(offer.creatorChessName);
        if (creator == null)
            return null;

        var go = Instantiate(shopItemPrefab, itemParent);
        var btn = go.GetComponent<Button>();
        var nameText = go.GetComponentInChildren<TMP_Text>();
        var priceText = go.transform.Find("Price")?.GetComponent<TMP_Text>();
        var iconImg = go.transform.Find("Icon")?.GetComponent<Image>();

        if (nameText != null)
            nameText.text = creator.chessName;
        if (iconImg != null && creator.chessSprite != null)
            iconImg.sprite = creator.chessSprite;

        int runGold = RoguelikeRunService.State?.runGold ?? 0;
        bool testFree = IsTestMode();
        bool canBuy = !offer.sold && (testFree || runGold >= offer.price);

        if (priceText != null)
        {
            if (testFree && !offer.sold)
                priceText.text = "免费";
            else if (!offer.sold)
                priceText.text = offer.price.ToString();
        }

        var soldOut = go.transform.Find("SoldOut");
        if (soldOut != null)
            soldOut.gameObject.SetActive(offer.sold);

        if (btn != null && !offer.sold)
        {
            btn.interactable = canBuy;
            btn.onClick.AddListener(() => OnPurchaseClick(slotIndex));
        }

        return go;
    }

    void OnPurchaseClick(int slotIndex)
    {
        if (!RoguelikeShopFlow.TryPurchase(slotIndex))
            return;

        RefreshCoinDisplay();
        RefreshItemList();
        RoguelikeRunInfoPanel.TryShowAndRefresh();
    }

    void ClearItems()
    {
        for (int i = _itemInstances.Count - 1; i >= 0; i--)
        {
            if (_itemInstances[i] != null)
                Destroy(_itemInstances[i]);
        }
        _itemInstances.Clear();
    }

    void OnLeaveClicked()
    {
        RoguelikeRunService.LeaveShopNode();
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
        return null;
    }

    static bool IsTestMode() =>
        GameManage.instance != null && GameManage.instance.mode == GameMode.Test;
}
