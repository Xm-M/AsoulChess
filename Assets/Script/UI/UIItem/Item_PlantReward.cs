using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 植物卡片奖励：点击后打开 AwardPanel 介绍，点「下一关」进入下一关
/// </summary>
public class Item_PlantReward : RewardItemBase
{
    public Image plantImage;
    public TMP_Text plantPrice;
    public PropertyCreator creator;

    public void SetRewardPos(Vector3 pos, PropertyCreator creator)
    {
        this.creator = creator;
        plantImage.sprite = creator.chessSprite;
        plantPrice.text = creator.baseProperty.price.ToString();
        base.SetRewardPos(pos);
    }

    /// <summary>
    /// 动画事件调用：动画结束时打开植物卡片介绍，点「下一关」进入下一关
    /// </summary>
    public void Win()
    {
        UIManage.Show<AwardPanel>();
        UIManage.Close<ItemPanel>();
        UIManage.GetView<AwardPanel>().ShowReward(creator);
    }

    public override void Recycle()
    {
        UIManage.GetView<ItemPanel>().Recycle<Item_PlantReward>(this);
        click = false;
    }
}
