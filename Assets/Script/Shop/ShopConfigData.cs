using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// 商店配置数据，管理所有可售商品
/// </summary>
[CreateAssetMenu(fileName = "ShopConfig", menuName = "Shop/ShopConfigData")]
public class ShopConfigData : ScriptableObject
{
    [LabelText("商品列表")]
    [ListDrawerSettings(ShowIndexLabels = false, ListElementLabelName = "displayName")]
    public List<ShopItemData> items = new List<ShopItemData>();
}
