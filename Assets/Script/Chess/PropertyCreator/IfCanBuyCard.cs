using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IfCanBuyCard 
{
    public bool BuyCard(PropertyCreator creator);
}
public class MultyIfCanBuyCard_And : IfCanBuyCard
{
    [SerializeReference]
    public List<IfCanBuyCard> ifCanBuyCards;
    public bool BuyCard(PropertyCreator creator)
    {
        bool ans = true;
        foreach(var ifCanBuyCard in ifCanBuyCards)
        {
            ans=ifCanBuyCard.BuyCard(creator)&ans;
        }
        return ans;
    }
}

/// <summary>
/// 场上同 <see cref="PropertyCreator"/> 数量上限；<see cref="maxCount"/> = 1 时为原「唯一」限制。
/// </summary>
public class OnlyOne_Limit : IfCanBuyCard
{
    [Tooltip("场上允许存在的同种植物数量上限（按 creator 比较）")]
    [Min(1)]
    public int maxCount = 1;

    public bool BuyCard(PropertyCreator creator)
    {
        if (creator == null || ChessTeamManage.Instance == null)
            return false;

        int onField = 0;
        List<Chess> team = ChessTeamManage.Instance.GetTeam("Player");
        foreach (var chess in team)
        {
            if (chess?.propertyController?.creator == creator)
                onField++;
        }
        return onField < maxCount;
    }
}
public class LevelUp_Limit : IfCanBuyCard
{
    public PropertyCreator basePlant;
    public bool BuyCard(PropertyCreator creator)
    {
        foreach (var chess in GameManage.instance.chessTeamManage.GetTeam("Player"))
        {
            if (chess.propertyController.creator == basePlant)
            {
                //chess.animatorController.ChangeFlash(1);
                return true;
            }
        }
        return false;
    }
}
