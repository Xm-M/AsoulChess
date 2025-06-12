using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
/// <summary>
/// 
/// </summary>
[CreateAssetMenu(fileName ="NewProperty",menuName ="Message/Property")]
public class PropertyCreator : ScriptableObject
{
    [LabelText("棋子名")]
    public string chessName;
    [LabelText("基础数据")]
    public Property baseProperty;
    [LabelText("棋子的预制体")]
    public Chess chessPre;
    [LabelText("棋子头像")]
    public Sprite chessSprite;
    [LabelText("棋子标签")]
    public List<string> plantTags;
    [LabelText("棋子可种植位置")]
    public TileType chessTileType;
    [LabelText("棋子的类型")]
    public PlantType plantType;
    [SerializeReference]
    [LabelText("种植方式")]
    public IPlantFunction plantFunction;
    [LabelText("购买限制")]
    [SerializeReference]
    public IfCanBuyCard plantIfCanBuyCard;
    public Property GetClone()
    {
        Property newP = new Property(baseProperty);
        return newP;
    }
    public virtual bool IfCanBuyCard(int sunLight){
        if(sunLight>=baseProperty.price){
            if (plantIfCanBuyCard != null)
            {
                if (!plantIfCanBuyCard.BuyCard(this)) return false;
            }
            return true;
        }
        return false;
    }
    public virtual bool IfCanPlant(Tile tile){
        return plantFunction.ifCanPlant(this, tile);
    }
}
public interface IPlantFunction
{
    public bool ifCanPlant(PropertyCreator creator, Tile tile);
}
/// <summary>
/// MainPlant只需要考虑目标Tile是否满足种植条件，以及目标Tile是否被占用
/// </summary>
public class MainPlant : IPlantFunction
{
    public bool ifCanPlant(PropertyCreator creator, Tile tile)
    {
        //Debug.Log(tile.name); 
        return !tile.stander&&CompairTile(creator.chessTileType,tile);
    }
    public static bool CompairTile(TileType chess,Tile tile)
    {
        //地形需要包括目标的可适用地形
        return (chess & tile.tileType) != 0;
    }
}
public class SupportPlant : IPlantFunction
{
    //那么support类型的取消tilestander操作只能由被动进行了
    public bool ifCanPlant(PropertyCreator creator, Tile tile)
    {
        //如果地形不满足就肯定种植不了
        if ((creator.chessTileType & tile.tileType) == 0) return false;
        //遍历地形中的所有chess,如果有辅助类则无法种植
        for (int i = 0; i < tile.chessesIntile.Count; i++)
        {
            if (tile.chessesIntile[i].propertyController.creator.plantType == creator.plantType)
            {
                Debug.Log("一个格子只能有一个辅助类植物");
                return false;
            }
        }
        return true;
    }
}
/// <summary>
/// Pot类需要修改一下
/// </summary>
public class PotPlant : IPlantFunction
{
    public bool ifCanPlant(PropertyCreator creator, Tile tile)
    {
        //如果地形已经种植或者地形不满足则无法种植
        if(tile.stander!=null|| (creator.chessTileType & tile.tileType) == 0)return false;
        //如果种植地形是由地形类植物提供的 则无法种植(莲叶上不能放置花盆) 这里还没改完 记得回来改
        for (int i = 0; i < tile.chessesIntile.Count; i++)
        {
            if (tile.chessesIntile[i].propertyController.creator.plantType == creator.plantType &&
                tile.chessesIntile[i].propertyController.creator.chessTileType == creator.chessTileType)
            {
                Debug.Log("已经有同类型的花盆了");
                return false;
            }
        }
        return true;

    }
}
/// <summary>
/// 消耗品植物 只要目标格子上有植物就能使用 或者说有其他需求
/// </summary>
public class ConsumePlant : IPlantFunction
{
    public bool ifCanPlant(PropertyCreator creator, Tile tile)
    {
        //throw new NotImplementedException();
        if (tile.stander != null)
        {
            return true;
        }
        return false;
    }
}
public class NonePlant : IPlantFunction
{
    public bool ifCanPlant(PropertyCreator creator, Tile tile)
    {
        return true;
    }
}


/// <summary>
/// 唯一植物
/// </summary>
public class ExclusivePlant : IPlantFunction
{
    public string targetTag;
    public bool ifCanPlant(PropertyCreator creator, Tile tile)
    {
        if (tile.stander==null||
            !tile.stander.propertyController.creator.plantTags.Contains(targetTag))
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
public class OnlyOne_Limit : IfCanBuyCard
{
    public bool BuyCard(PropertyCreator creator)
    {
        //throw new NotImplementedException();
        List<Chess> team = ChessTeamManage.Instance.GetTeam("Player");
        foreach(var chess in team)
        {
            if(chess.propertyController.creator == creator)
            {
                //Debug.Log("有相同单位"+chess.name);
                return false;
            }
        }
        //Debug.Log("没有相同单位");
        return true;
    }
}

