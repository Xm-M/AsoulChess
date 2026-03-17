using System;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// 购买效果基类，购买后调用
/// </summary>
[Serializable]
public abstract class ShopItemPurchaseEffect
{
    public abstract void OnPurchase(PlayerSaveData data);
}

/// <summary>
/// Creator 类型商品的购买效果：加入 ownedCreatorIds
/// </summary>
[Serializable]
public class ShopItemEffect_Creator : ShopItemPurchaseEffect
{
    [LabelText("植物 Creator")]
    public PropertyCreator creator;

    public override void OnPurchase(PlayerSaveData data)
    {
        if (data?.ownedCreatorIds == null || creator == null) return;
        if (!data.ownedCreatorIds.Contains(creator.chessName))
            data.ownedCreatorIds.Add(creator.chessName);
    }
}

/// <summary>
/// 单个商品数据（ScriptableObject）
/// </summary>
[CreateAssetMenu(fileName = "NewShopItem", menuName = "Shop/ShopItemData")]
public class ShopItemData : ScriptableObject
{
    [LabelText("显示名称")]
    public string displayName;

    [LabelText("图标")]
    public Sprite icon;

    [LabelText("价格")]
    public int price;

    [LabelText("售卖数量")]
    [Tooltip("-1=无限，0=售罄仅展示不可购买，>0=限量")]
    public int stock = -1;

    [LabelText("商品ID")]
    [Tooltip("用于存档追踪购买次数，留空则用 displayName")]
    public string itemId;

    [LabelText("购买效果")]
    [SerializeReference]
    public ShopItemPurchaseEffect purchaseEffect;

    /// <summary>商品唯一ID，用于存档</summary>
    public string GetItemId() => !string.IsNullOrEmpty(itemId) ? itemId : name;

    /// <summary>剩余可购买数量，-1 表示无限</summary>
    public int GetRemainingStock(PlayerSaveData data)
    {
        if (stock < 0) return -1;
        if (stock == 0) return 0;
        int bought = data != null ? PlayerSaveSystem.GetExtra(data, "shop_buy_" + GetItemId(), 0) : 0;
        return Mathf.Max(0, stock - bought);
    }

    /// <summary>是否可购买（有库存且未拥有）</summary>
    public bool CanPurchase(PlayerSaveData data)
    {
        int remaining = GetRemainingStock(data);
        return remaining != 0 && !IsOwned(data);
    }

    /// <summary>是否已拥有（Creator 类型时检查 ownedCreatorIds）</summary>
    public bool IsOwned(PlayerSaveData data)
    {
        if (data == null) return false;
        if (purchaseEffect is ShopItemEffect_Creator c)
            return c.creator != null && data.ownedCreatorIds != null && data.ownedCreatorIds.Contains(c.creator.chessName);
        return false;
    }
}
