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
    [LabelText("简短描述")]
    [Multiline]
    public string chessShortDescription;
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
    [LabelText("羁绊成员ID")]
    [Tooltip("Member 模式用：此卡在羁绊中算作哪个成员。空则用 chessName。如吉他英雄填「波奇」与波奇算同一人")]
    public string fetterMemberId;
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

    /// <summary>
    /// 「一类/二类有限制」出怪或食物池筛选：默认当 <c>wave ≥ waveLimit</c> 时进池；
    /// <see cref="PlantType.LimitType"/> 时仅当当前波次（1 起）为 <c>waveLimit</c> 的正整数倍且 <c>waveLimit &gt; 0</c> 时进池。
    /// </summary>
    public bool PassesWavePoolFilter(int wave1Based)
    {
        if (baseProperty == null) return false;
        int limit = baseProperty.waveLimit;
        if ((plantType & PlantType.LimitType)!=0)
        {
            if (limit <= 0) return false;
            return wave1Based >= limit && wave1Based % limit == 0;
        }
        return limit <= wave1Based;
    }
}
public interface IPlantFunction
{
    public bool ifCanPlant(PropertyCreator creator, Tile tile);
}
/// <summary>
/// MainPlant：无 MainPlant（stander）、且 <see cref="CompairTile"/> 地形有交集即可（由 chessTileType / tile.tileType 数据表达水陆屋顶等）。
/// </summary>
public class MainPlant : IPlantFunction
{
    public bool ifCanPlant(PropertyCreator creator, Tile tile)
    {
        return !tile.stander && CompairTile(creator.chessTileType, tile);
    }
    public static bool CompairTile(TileType chess, Tile tile)
    {
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
/// 地形类植物（睡莲、花盆等）：格上不能有 MainPlant；须满足 chessTileType 与当前格 tileType；
/// 同一格至多一株 PotPlant。
/// </summary>
public class PotPlant : IPlantFunction
{
    public bool ifCanPlant(PropertyCreator creator, Tile tile)
    {
        if (tile.stander != null) return false;
        if ((creator.chessTileType & tile.tileType) == 0) return false;
        if (tile.chessesIntile == null) return true;
        for (int i = 0; i < tile.chessesIntile.Count; i++)
        {
            Chess c = tile.chessesIntile[i];
            if (c == null || c.propertyController?.creator == null) continue;
            if (c.propertyController.creator.plantType == PlantType.PotPlant)
                return false;
        }
        return true;
    }
}
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
        //if (ans)
        //{
        //    tile.stander.Death();
        //    return ans;
        //}return false;
        return ans;
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

