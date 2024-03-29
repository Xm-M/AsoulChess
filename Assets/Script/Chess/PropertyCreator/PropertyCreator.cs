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
/// MainPlantֻ��Ҫ����Ŀ��Tile�Ƿ�������ֲ�������Լ�Ŀ��Tile�Ƿ�ռ��
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
        //������Ҫ����Ŀ��Ŀ����õ���
        return (chess & tile.tileType) != 0;
    }
    
}
public class SupportPlant : IPlantFunction
{
    //��ôsupport���͵�ȡ��tilestander����ֻ���ɱ���������
    public bool ifCanPlant(PropertyCreator creator, Tile tile)
    {
        //������β�����Ϳ϶���ֲ����
        if ((creator.chessTileType & tile.tileType) == 0) return false;
        //���������е�����chess,����и��������޷���ֲ
        for (int i = 0; i < tile.chessesIntile.Count; i++)
        {
            if (tile.chessesIntile[i].propertyController.creator.plantType == creator.plantType)
            {
                Debug.Log("һ������ֻ����һ��������ֲ��");
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
        //��������Ѿ���ֲ���ߵ��β��������޷���ֲ
        if(tile.stander!=null|| (creator.chessTileType & tile.tileType) == 0)return false;
        //�����ֲ������ͬ�ҵ���������ͬ ˵����ͬ�ֻ��裬�Ͳ�����ֲ��
        for (int i = 0; i < tile.chessesIntile.Count; i++)
        {
            if (tile.chessesIntile[i].propertyController.creator.plantType == creator.plantType &&
                tile.chessesIntile[i].propertyController.creator.chessTileType == creator.chessTileType)
            {
                Debug.Log("�Ѿ���ͬ���͵Ļ�����");
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
