using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 主唱  
/// 羁绊图标是一个话筒
/// </summary>
public class Vocal : Fetter
{
    [SerializeReference]
    public Buff_Vocal vocalBuff;
    public override void FetterEffect(int num)
    {
        //base.FetterEffect(num);
        vocalBuff.maxCount =Mathf.Max(1,(6 - num));
        EventController.Instance.AddListener<Chess>(EventName.WhenChessEnterWar.ToString(), AddBuff);
    }

    public void AddBuff(Chess chess)
    {
        if (chess.CompareTag("Player")&&chess.propertyController.creator.plantTags.Contains("主唱"))
        {
            chess.buffController.AddBuff(vocalBuff);
        }
    }
    public override bool FetterLight(int num)
    {
        if (num >= 2)
        {
            return true;
        }
        return false;
    }
    public override void ResetFetter()
    {
        EventController.Instance.RemoveListener<Chess>(EventName.WhenChessEnterWar.ToString(), AddBuff);
    }
}
/// <summary>
/// 贝斯手
/// </summary>
public class Bass : Fetter
{
    [SerializeReference]
    public Buff_Bass bassBuff;
    public override void FetterEffect(int num)
    {
        base.FetterEffect(num);
        //这里要根据 数量来调整bassBuff的数据 具体数值到时候再定
        EventController.Instance.AddListener<Chess>(EventName.WhenChessEnterWar.ToString(), AddBuff);
    }
    public override bool FetterLight(int num)
    {
        if(num >= 2)
        {
            return true;
        }
        return false;
    }
    public void AddBuff(Chess chess)
    {
        if (chess.CompareTag("Player") && chess.propertyController.creator.plantTags.Contains("贝斯"))
        {
            chess.buffController.AddBuff(bassBuff);
        }
    }
    public override void ResetFetter()
    {
        base.ResetFetter();
        EventController.Instance.RemoveListener<Chess>(EventName.WhenChessEnterWar.ToString(), AddBuff);
    }
}
/// <summary>
/// 这个是吉他手的羁绊 图标就是吉他  
/// </summary> 
public class Guitar : Fetter
{
    [SerializeReference]
    public Buff_Guitar guitarBuff;
    public override void FetterEffect(int num)
    {
        base.FetterEffect(num);
        //这里要根据 数量来调整guitarBuff的数据 具体数值到时候再定
        //先随便写点吧
        if (num==2)
        {
            guitarBuff.extraCrit = 0.2f;
            guitarBuff.extraCritDamage = 0.2f;
        }else if (num==3)
        {
            guitarBuff.extraCrit = 0.4f;
            guitarBuff.extraCritDamage = 0.4f;
        }else if (num == 4)
        {
            guitarBuff.extraCrit = 0.75f;
            guitarBuff.extraCritDamage = 0.75f;
        }
        else
        {
            guitarBuff.extraCrit = 1;
            guitarBuff.extraCritDamage = 1;
        }

        EventController.Instance.AddListener<Chess>(EventName.WhenChessEnterWar.ToString(), AddBuff);
    }
    public override bool FetterLight(int num)
    {
        if (num >= 2)
        {
            return true;
        }
        return false;
    }
    public void AddBuff(Chess chess)
    {
        if (chess.CompareTag("Player") && chess.propertyController.creator.plantTags.Contains("吉他"))
        {
            chess.buffController.AddBuff(guitarBuff);
        }
    }
    public override void ResetFetter()
    {
        base.ResetFetter();
        EventController.Instance.RemoveListener<Chess>(EventName.WhenChessEnterWar.ToString(), AddBuff);
    }
}
/// <summary>
/// 键盘手 额 光环效果怎么做？
/// </summary>
public  class KeyBoard : Fetter
{
    [SerializeReference]
    public Buff_KeyBoard keyBoardBuff;
    public float coldDown;//光环效果的间隔时间
    Timer timer;
    public override void FetterEffect(int num)
    {
        base.FetterEffect(num);
        //这里要根据 数量来调整guitarBuff的数据 具体数值到时候再定
        EventController.Instance.AddListener<Chess>(EventName.WhenChessEnterWar.ToString(), AddBuff);
    }
    public override bool FetterLight(int num)
    {
        if (num >= 2)
        {
            return true;
        }
        return false;
    }
    public void AddBuff(Chess chess)
    {
        if (chess.CompareTag("Player"))
        {
            chess.buffController.AddBuff(keyBoardBuff);
        }
    }
    public override void ResetFetter()
    {
        base.ResetFetter();
        EventController.Instance.RemoveListener<Chess>(EventName.WhenChessEnterWar.ToString(), AddBuff);
    }
}