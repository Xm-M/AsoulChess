using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
[CreateAssetMenu(fileName ="NewProperty",menuName ="Message/Property")]
public class PropertyCreator : ScriptableObject
{
    public string chessName;
    public Property baseProperty;
    public Chess chessPre;
    public Sprite chessSprite;
    public List<string> plantTags;
    public TileType chessTileType;
    public PlantType plantType;
    [SerializeReference]
    public IPlantFunction plantFunction;
    public Property GetClone()
    {
        Property newP = new Property(baseProperty);
        return newP;
    }
    public virtual bool IfCanBuyCard(int sunLight){
        if(sunLight>=baseProperty.price){
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
        Debug.Log(tile.name);
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
public class PotPlant : IPlantFunction
{
    public bool ifCanPlant(PropertyCreator creator, Tile tile)
    {
        //如果地形已经种植或者地形不满足则无法种植
        if(tile.stander!=null|| (creator.chessTileType & tile.tileType) == 0)return false;
        //如果种植类型相同且地形类型相同 说明是同种花盆，就不能种植了
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
public class NonePlant : IPlantFunction
{
    public bool ifCanPlant(PropertyCreator creator, Tile tile)
    {
        return true;
    }
}

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
