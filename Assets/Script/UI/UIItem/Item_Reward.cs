using UnityEngine;

/// <summary>
/// 奖杯奖励：点击后 MoveToCenter + 播放 win 动画，由动画事件调用 Win() 进入下一关
/// </summary>
public class Item_Reward : RewardItemBase
{
    /// <summary>
    /// 动画事件调用：动画结束时进入下一关
    /// </summary>
    public void Win()
    {
        LevelManage.instance.GameOver(true);
    }

    public override void Recycle()
    {
        UIManage.GetView<ItemPanel>().Recycle<Item_Reward>(this);
        click = false;
    }
}
