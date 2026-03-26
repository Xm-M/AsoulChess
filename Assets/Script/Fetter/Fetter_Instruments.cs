using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 主唱。配置：detectMode=Tag, tag=主唱, tierThresholds=[2,3,4,5]
/// 每次攻击有概率额外发射子弹：2人20%、3人45%、4人70%、5人100%
/// </summary>
public class Vocal : Fetter
{
    [SerializeReference]
    public Buff_Vocal vocalBuff;
    static readonly float[] ChanceByCount = { 0.2f, 0.45f, 0.7f, 1f };
    public override void FetterEffect(int count, int tier)
    {
        int idx = Mathf.Clamp(count - 2, 0, ChanceByCount.Length - 1);
        vocalBuff.extraBulletChance = ChanceByCount[idx];
        EventController.Instance.AddListener<Chess>(EventName.WhenChessEnterWar.ToString(), AddBuff);
    }
    public void AddBuff(Chess chess)
    {
        if (chess.CompareTag("Player") && chess.propertyController.creator.plantTags != null && chess.propertyController.creator.plantTags.Contains("主唱"))
        {
            chess.buffController.AddBuff(vocalBuff);
        }
    }
    public override void ResetFetter()
    {
        EventController.Instance.RemoveListener<Chess>(EventName.WhenChessEnterWar.ToString(), AddBuff);
    }
}
/// <summary>
/// 贝斯手。配置：detectMode=Tag, tag=贝斯, tierThresholds=[2,3,4,5]
/// 满层：+50%最大生命+600血，+50%偷取，体型+5。2~5人对应20%~100%比例，体型+2~+5。
/// </summary>
public class Bass : Fetter
{
    [SerializeReference]
    public Buff_Bass bassBuff;
    static readonly float[] TierScale = { 0.2f, 0.45f, 0.7f, 1f };
    public override void FetterEffect(int count, int tier)
    {
        base.FetterEffect(count, tier);
        int idx = Mathf.Clamp(count - 2, 0, TierScale.Length - 1);
        bassBuff.tierScale = TierScale[idx];
        bassBuff.sizeAdd = Mathf.Clamp(count, 2, 5);
        EventController.Instance.AddListener<Chess>(EventName.WhenChessEnterWar.ToString(), AddBuff);
    }
    public void AddBuff(Chess chess)
    {
        if (chess.CompareTag("Player") && chess.propertyController.creator.plantTags != null && chess.propertyController.creator.plantTags.Contains("贝斯"))
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
/// 吉他手。配置：detectMode=Tag, tag=吉他, tierThresholds=[2,3,4,5]
/// 双爆：crit=critDamage=√(tierScale)，使等效增伤按20%→100%线性。2人45%/3人67%/4人84%/5人100%
/// </summary>
public class Guitar : Fetter
{
    [SerializeReference]
    public Buff_Guitar guitarBuff;
    static readonly float[] TierScale = { 0.2f, 0.45f, 0.7f, 1f };
    public override void FetterEffect(int count, int tier)
    {
        base.FetterEffect(count, tier);
        int idx = Mathf.Clamp(count - 2, 0, TierScale.Length - 1);
        float c = Mathf.Sqrt(TierScale[idx]);
        if (guitarBuff.critBuff == null) guitarBuff.critBuff = new Buff_BaseValueBuff_Crit();
        if (guitarBuff.critDamageBuff == null) guitarBuff.critDamageBuff = new Buff_BaseValueBuff_CritDamage();
        guitarBuff.critBuff.crit = c;
        guitarBuff.critDamageBuff.critDamage = c;
        EventController.Instance.AddListener<Chess>(EventName.WhenChessEnterWar.ToString(), AddBuff);
    }
    public void AddBuff(Chess chess)
    {
        if (chess.CompareTag("Player") && chess.propertyController.creator.plantTags != null && chess.propertyController.creator.plantTags.Contains("吉他"))
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
/// 鼓手。配置：detectMode=Tag, tag=鼓手, tierThresholds=[2,3,4,5]
/// 周围8格+自身增伤，满层50%。不叠加。
/// </summary>
public class Drummer : Fetter
{
    [SerializeReference] public Buff_DrummerAura drummerAuraBuff;
    static readonly float[] TierScale = { 0.2f, 0.45f, 0.7f, 1f };
    public override void FetterEffect(int count, int tier)
    {
        base.FetterEffect(count, tier);
        if (drummerAuraBuff == null) drummerAuraBuff = new Buff_DrummerAura();
        if (drummerAuraBuff.drummerDamageBuff == null) drummerAuraBuff.drummerDamageBuff = new Buff_DrummerDamage();
        if (drummerAuraBuff.drummerDamageBuff.extraDamageBuff == null)
            drummerAuraBuff.drummerDamageBuff.extraDamageBuff = new Buff_BaseValueBuff_ExtraDamage();
        int idx = Mathf.Clamp(count - 2, 0, TierScale.Length - 1);
        drummerAuraBuff.extraDamage = TierScale[idx] * 0.5f;
        EventController.Instance.AddListener<Chess>(EventName.WhenChessEnterWar.ToString(), AddBuff);
    }
    public void AddBuff(Chess chess)
    {
        if (chess.CompareTag("Player") && chess.propertyController.creator.plantTags != null && chess.propertyController.creator.plantTags.Contains("鼓手"))
        {
            chess.buffController.AddBuff(drummerAuraBuff);
        }
    }
    public override void ResetFetter()
    {
        base.ResetFetter();
        EventController.Instance.RemoveListener<Chess>(EventName.WhenChessEnterWar.ToString(), AddBuff);
    }
}

/// <summary>
/// 键盘手。配置：detectMode=Tag, tag=键盘, tierThresholds=[2,3,4,5]
/// 周围8格+自身护甲。周围0.5倍，自身2倍。满层100护甲。
/// </summary>
public class KeyBoard : Fetter
{
    [SerializeReference] public Buff_KeyBoardAura keyBoardAuraBuff;
    static readonly float[] TierScale = { 0.2f, 0.45f, 0.7f, 1f };
    public override void FetterEffect(int count, int tier)
    {
        base.FetterEffect(count, tier);
        if (keyBoardAuraBuff == null) keyBoardAuraBuff = new Buff_KeyBoardAura();
        if (keyBoardAuraBuff.keyBoardArmorBuff == null) keyBoardAuraBuff.keyBoardArmorBuff = new Buff_KeyBoardArmor();
        if (keyBoardAuraBuff.keyBoardArmorBuff.armorBuff == null)
            keyBoardAuraBuff.keyBoardArmorBuff.armorBuff = new Buff_BaseValueBuff_Armor();
        int idx = Mathf.Clamp(count - 2, 0, TierScale.Length - 1);
        keyBoardAuraBuff.baseArmor = TierScale[idx] * 100f;
        EventController.Instance.AddListener<Chess>(EventName.WhenChessEnterWar.ToString(), AddBuff);
    }
    public void AddBuff(Chess chess)
    {
        if (chess.CompareTag("Player") && chess.propertyController.creator.plantTags != null && chess.propertyController.creator.plantTags.Contains("键盘"))
        {
            chess.buffController.AddBuff(keyBoardAuraBuff);
        }
    }
    public override void ResetFetter()
    {
        base.ResetFetter();
        EventController.Instance.RemoveListener<Chess>(EventName.WhenChessEnterWar.ToString(), AddBuff);
    }
}
