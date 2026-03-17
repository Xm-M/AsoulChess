using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 金币商店面板。Test 模式下金币显示 999999，购买不扣钱。
/// </summary>
public class CoinShopPanel : View
{
    [Header("商店配置")]
    public ShopConfigData shopConfig;

    [Header("UI 引用")]
    public Transform itemParent;
    public GameObject shopItemPrefab;
    public TMP_Text coinText;
    public Button returnButton;

    List<GameObject> itemInstances = new List<GameObject>();

    const int TestModeCoins = 999999;

    public override void Init()
    {
        if (returnButton != null)
            returnButton.onClick.AddListener(OnReturnClick);
    }

    public override void Show()
    {
        base.Show();
        RefreshCoinDisplay();
        RefreshItemList();
    }

    void RefreshCoinDisplay()
    {
        if (coinText == null) return;
        int coins = GetDisplayCoins();
        coinText.text = coins.ToString();
    }

    /// <summary>获取显示的金币数，Test 模式为 999999</summary>
    int GetDisplayCoins()
    {
        if (IsTestMode()) return TestModeCoins;
        return PlayerSaveContext.CurrentData?.coins ?? 0;
    }

    /// <summary>获取实际可用于购买的金币，Test 模式为 999999</summary>
    int GetPurchasableCoins()
    {
        if (IsTestMode()) return TestModeCoins;
        return PlayerSaveContext.CurrentData?.coins ?? 0;
    }

    void RefreshItemList()
    {
        ClearItems();
        if (shopConfig == null || shopConfig.items == null || itemParent == null || shopItemPrefab == null) return;

        foreach (var item in shopConfig.items)
        {
            if (item == null) continue;
            var go = CreateShopItem(item);
            if (go != null) itemInstances.Add(go);
        }
    }

    GameObject CreateShopItem(ShopItemData item)
    {
        var go = Instantiate(shopItemPrefab, itemParent);
        var btn = go.GetComponent<Button>();
        var nameText = go.GetComponentInChildren<TMP_Text>();
        var priceText = go.transform.Find("Price")?.GetComponent<TMP_Text>();
        var iconImg = go.transform.Find("Icon")?.GetComponent<Image>();

        if (nameText != null) nameText.text = item.displayName;
        if (iconImg != null && item.icon != null) iconImg.sprite = item.icon;

        var data = PlayerSaveContext.CurrentData;
        bool owned = item.IsOwned(data);
        int remaining = item.GetRemainingStock(data);
        bool canBuy = item.CanPurchase(data);

        if (priceText != null)
        {
            if (owned) priceText.text = "已拥有";
            else if (remaining == 0) priceText.text = "已售罄";
            else if (remaining > 0) priceText.text = $"{item.price} (剩{remaining})";
            else priceText.text = item.price.ToString();
        }
        if (btn != null)
        {
            btn.interactable = canBuy;
            btn.onClick.AddListener(() => OnPurchaseClick(item));
        }

        return go;
    }

    void ClearItems()
    {
        foreach (var go in itemInstances)
        {
            if (go != null) Destroy(go);
        }
        itemInstances.Clear();
    }

    void OnPurchaseClick(ShopItemData item)
    {
        if (item?.purchaseEffect == null) return;

        var data = PlayerSaveContext.CurrentData;
        if (!IsTestMode() && data == null)
        {
            Debug.LogWarning("[CoinShopPanel] 未登录，无法购买");
            return;
        }

        if (!item.CanPurchase(data))
        {
            Debug.Log("[CoinShopPanel] 该商品已售罄或已拥有");
            return;
        }

        int coins = GetPurchasableCoins();
        if (coins < item.price)
        {
            Debug.Log("[CoinShopPanel] 金币不足");
            return;
        }

        if (!IsTestMode())
        {
            data.coins -= item.price;
            string key = "shop_buy_" + item.GetItemId();
            int bought = PlayerSaveSystem.GetExtra(data, key, 0);
            PlayerSaveSystem.SetExtra(data, key, bought + 1);
            PlayerSaveContext.SaveCurrent();
        }

        if (data != null)
            item.purchaseEffect.OnPurchase(data);

        UpdatePlayerOwnedCreators();
        RefreshCoinDisplay();
        RefreshItemList();
    }

    void UpdatePlayerOwnedCreators()
    {
        if (GameManage.instance == null || PlayerSaveContext.CurrentData?.ownedCreatorIds == null) return;

        GameManage.instance.playerOwnedCreators = new List<PropertyCreator>();
        foreach (var id in PlayerSaveContext.CurrentData.ownedCreatorIds)
        {
            var c = GetCreatorByChessName(id);
            if (c != null) GameManage.instance.playerOwnedCreators.Add(c);
        }
    }

    static PropertyCreator GetCreatorByChessName(string chessName)
    {
        if (string.IsNullOrEmpty(chessName) || GameManage.instance?.allChess == null) return null;
        foreach (var c in GameManage.instance.allChess)
        {
            if (c != null && c.chessName == chessName) return c;
        }
        return null;
    }

    void OnReturnClick()
    {
        Hide();
        var returnToNext = CoinShopContext.ReturnToNextLevelOnClose;
        var nextLevel = CoinShopContext.NextLevelToReturnTo;
        CoinShopContext.ClearReturnToNextLevel();
        if (returnToNext && nextLevel != null)
        {
            LevelManage.instance.ChangeLevel(nextLevel);
        }
        // 从主菜单进入时：ReturnToNextLevelOnClose 为 false，仅 Hide 即可，无需 LoadScene
    }

    static bool IsTestMode()
    {
        return GameManage.instance != null && GameManage.instance.mode == GameMode.Test;
    }
}
