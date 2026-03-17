using UnityEngine;

/// <summary>
/// 车钥匙奖励：点击后 MoveToCenter + 播放 win 动画，由动画事件调用 Win() 打开商店并设置 shopUnlocked
/// </summary>
public class Item_CarKey : RewardItemBase
{
    LevelData nextLevel;

    /// <summary>设置奖励位置和下一关，开始曲线掉落</summary>
    public void SetRewardPos(Vector3 pos, LevelData nextLevel)
    {
        this.nextLevel = nextLevel;
        base.SetRewardPos(pos);
    }

    /// <summary>
    /// 动画事件调用：设置 shopUnlocked，打开商店，关闭后进入下一关
    /// </summary>
    public void Win()
    {
        var data = PlayerSaveContext.CurrentData;
        if (data != null)
        {
            data.shopUnlocked = true;
            PlayerSaveContext.SaveCurrent();
        }
        CoinShopContext.SetReturnToNextLevel(nextLevel);
        UIManage.Show<CoinShopPanel>();
        UIManage.Close<ItemPanel>();
    }

    public override void Recycle()
    {
        UIManage.GetView<ItemPanel>().Recycle<Item_CarKey>(this);
        click = false;
    }
}
