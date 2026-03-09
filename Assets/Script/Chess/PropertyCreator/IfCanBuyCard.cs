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
public class OnlyOne_Limit : IfCanBuyCard
{
    public bool BuyCard(PropertyCreator creator)
    {
        //throw new NotImplementedException();
        List<Chess> team = ChessTeamManage.Instance.GetTeam("Player");
        foreach (var chess in team)
        {
            if (chess.propertyController.creator == creator)
            {
                //Debug.Log("有相同单位"+chess.name);
                return false;
            }
        }
        //Debug.Log("没有相同单位");
        return true;
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
