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
    [LabelText("棋子名称")]
    public string chessName;
    [LabelText("棋子描述")]
    [Multiline]
    public string chessDescription;
    [LabelText("棋子效果")]
    [Multiline ]
    public string chessEffect;

    [LabelText("基础属性")]
    public Property baseProperty;
    [LabelText("棋子预制体")]
    [SerializeField]
    public Chess chessPre;
    [LabelText("棋子图标")]
    public Sprite chessSprite;
    [LabelText("羁绊标签")]
    public List<string> plantTags;
    [LabelText("棋子可种植位置")]
    public TileType chessTileType;
    [LabelText("棋子定位类型")]
    public PlantType plantType;
    [SerializeReference]
    [LabelText("种植方式")]
    public IPlantFunction plantFunction;
    [LabelText("购买限制")]
    [SerializeReference]
    public IfCanBuyCard plantIfCanBuyCard;
    [LabelText("种子包预制体")]
    public GameObject PlantCardPre;
    [LabelText("种子仓库预制体")]
    public GameObject PlantEntrepotCardPre;

    public Property GetClone()
    {
        Property newP = new Property(baseProperty);
        return newP;
    }
    public Chess GetPre()
    {
        return chessPre;
    }
    public virtual bool IfCanBuyCard(){
        if (plantIfCanBuyCard != null)
           {
                if (!plantIfCanBuyCard.BuyCard(this)) return false;
           }
        return true;
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
        //Debug.Log(tile.name); 
        //Debug.Log(tile.stander);
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
/// <summary>
/// Pot����Ҫ�޸�һ��
/// </summary>
public class PotPlant : IPlantFunction
{
    public bool ifCanPlant(PropertyCreator creator, Tile tile)
    {
        //��������Ѿ���ֲ���ߵ��β��������޷���ֲ
        if(tile.stander!=null|| (creator.chessTileType & tile.tileType) == 0)return false;
        //�����ֲ�������ɵ�����ֲ���ṩ�� ���޷���ֲ(��Ҷ�ϲ��ܷ��û���) ���ﻹû���� �ǵû�����
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
/// <summary>
/// ����Ʒֲ�� ֻҪĿ���������ֲ�����ʹ�� ����˵����������
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
public class AimTargetPlant:IPlantFunction{
    public PropertyCreator target;
    public bool ifCanPlant(PropertyCreator creator,Tile tile){
        if (tile.stander != null&& tile.stander.propertyController.creator==target)
        {
            return true;
        }
        return false;
    }
}
public class LevelUpPlant : IPlantFunction
{
    public PropertyCreator basePlant;
    public virtual bool ifCanPlant(PropertyCreator creator, Tile tile)
    {
         
       bool ans= tile.stander&&(tile.stander.propertyController.creator==basePlant);
        if (ans)
        {
            tile.stander.Death();
            return ans;
        }return false;
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
/// Ψһֲ��
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

