using UnityEngine;

/// <summary>
/// 车钥匙胜利结算：在最后一只僵尸位置生成车钥匙，点击后打开商店、设置 shopUnlocked，关闭商店后进入下一关
/// </summary>
[System.Serializable]
public class LevelOutCome_CarKey : ILevelOutcome
{
    [Tooltip("下一关，为空时使用当前关卡的 nextLevel")]
    public LevelData nextLevel;

    public void HandleOutcome(bool win, Vector3 lastZombiePos)
    {
        if (win)
        {
            var level = LevelManage.instance?.currentLevel;
            var next = nextLevel != null ? nextLevel : level?.nextLevel;

            var item = UIManage.GetView<ItemPanel>().Create<Item_CarKey>();
            item.SetRewardPos(lastZombiePos, next);
        }
    }
}
