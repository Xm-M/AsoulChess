using UnityEngine;

/// <summary>
/// 特殊关卡用：不种植，购买后按 <see cref="LevelOutCome_Trophy"/> 同款逻辑生成 <see cref="Item_Reward"/>（收集阳光后点奖杯过关）。
/// </summary>
public class ShopIcon_Reward : ShopIcon
{
    public override bool IfCanBuyCard()
    {
        if (good == null)
        {
            Destroy(gameObject);
            return false;
        }

        return good.IfCanBuyCard() && SunLightPanel.instance.sunLight >= price;
    }

    public override void BuyPlant()
    {
        var shop = UIManage.GetView<PlantsShop>();
        if (!LevelManage.instance.IfGameStart && !shop.SelectOver)
        {
            if (selectIcon != null)
            {
                selectIcon.UnselectCard();
                shop.RemoveSelection(selectIcon);
            }

            shop.shopAudio.PlayAudio("tap");
            shop.RemoveShopIcon(this);
            Destroy(gameObject);
            return;
        }

        if (!ifCanbuy || !IfColdDown() || SunLightPanel.instance.sunLight < price || !good.IfCanBuyCard())
        {
            shop.shopAudio.PlayAudio("CantPlant");
            return;
        }

        SunLightPanel.instance.ChangeSunLight(-price);
        ColdDown();

        Vector3 pos = DefaultTrophyWorldPos();
        Item_Reward reward = UIManage.GetView<ItemPanel>().Create<Item_Reward>() as Item_Reward;
        if (reward != null)
            reward.SetRewardPos(pos);

        shop.RemoveShopIcon(this);
        shop.CancelBuyCard();
        Destroy(gameObject);
        Audio?.PlayAudio("Plant");
    }

    /// <summary>与结算奖杯类似：给一个在地图内的世界坐标；特殊关卡可改预制体上的偏移或后续再扩展。</summary>
    static Vector3 DefaultTrophyWorldPos()
    {
        var map = MapManage.instance;
        if (map == null || map.tiles == null) return Vector3.zero;
        var s = map.mapSize;
        int x = Mathf.Clamp(s.x / 2, 0, s.x - 1);
        int y = Mathf.Clamp(s.y / 2, 0, s.y - 1);
        Tile t = map.tiles[x, y];
        return t != null ? t.transform.position : Vector3.zero;
    }
}
