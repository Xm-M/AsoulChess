using UnityEngine;

/// <summary>
/// 默认胜利结算：在最后一只僵尸位置生成奖杯，点击后进入下一关
/// </summary>
public class LevelOutCome_Trophy : ILevelOutcome
{
    public void HandleOutcome(bool win, Vector3 lastZombiePos)
    {
        if (win)
        {
            Item_Reward reward = UIManage.GetView<ItemPanel>().Create<Item_Reward>();
            reward.SetRewardPos(lastZombiePos);
        }
    }
}
