using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelOutCome_PlantReward : ILevelOutcome
{
    public PropertyCreator rewardPlant;
    public void HandleOutcome(bool win, Vector3 lastZombiePos)
    {
        if (win)
        {
            Item_PlantReward reward = UIManage.GetView<ItemPanel>().Create<Item_PlantReward>();
            reward.SetRewardPos(lastZombiePos, rewardPlant);
        }
    }
}
