using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 和乐队相关的buff 都被放在这个文件里了
/// </summary>
 
public class Buff_Vocal : Buff
{
    public int maxCount;
    public GameObject extraBullet;
    //这里用添加可能施加的随机buff 
    //public List<Buff> buffs;
    protected int count;
    public override void WriteExtraToSaveData(BuffSaveData data)
    {
        base.WriteExtraToSaveData(data);
        if (data == null) return;
        data.SetExtra("Count", count);
    }
    public override void RestoreExtraFromSaveData(BuffSaveData data)
    {
        base.RestoreExtraFromSaveData(data);
        if (data == null) return;
        count = data.GetExtraInt("Count", 0);
    }
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        target.equipWeapon.OnAttack.AddListener(WhenTakeDamage);
    }

    public void WhenTakeDamage(Chess chess)
    {
        count++;
        if(count == maxCount)
        {
            count = 0;
            Bullet b=ObjectPool.instance.Create(extraBullet).GetComponent<Bullet>();
            b.InitBullet(chess, chess.equipWeapon.weaponPos.position, null, chess.transform.right);
            ////这里要给takebuff 添加随机buff
            //b.Dm.takeBuff=
            ////如果有多种颜色的话 就用这个吧
            //b.GetComponent<Animator>().SetInteger("type",0);
        }
    }

    public override void BuffOver()
    {
        base.BuffOver();
        target.equipWeapon.OnAttack.RemoveListener(WhenTakeDamage);
    }

    public override void BuffReset(Buff resetBuff)
    {
        base.BuffReset(resetBuff);
    }
}
/// <summary>
/// 贝斯 Buff：HPmax + 生命偷取 + 体型 + 事件（半血触发隐身）
/// </summary>
public class Buff_Bass : Buff
{
    [SerializeReference] public Buff_BaseValueBuff_HPmax hpMaxBuff;
    [SerializeReference] public Buff_BaseValueBuff_LifeSteal lifeStealBuff;
    [SerializeReference] public Buff_BaseValueBuff_Size sizeBuff;
    [SerializeReference] public Buff_BassHide buff;
    public float coldDowm = 30;
    [UnityEngine.Serialization.FormerlySerializedAs("extraHpMax")] public float _extraHpMax;
    [UnityEngine.Serialization.FormerlySerializedAs("extraHpSteal")] public float _extraHpSteal;
    [UnityEngine.Serialization.FormerlySerializedAs("extraSize")] public int _extraSize;
    protected Timer timer;
    protected bool cold;
    void EnsureBuffs()
    {
        if (hpMaxBuff == null) hpMaxBuff = new Buff_BaseValueBuff_HPmax { hpmax = _extraHpMax };
        if (lifeStealBuff == null) lifeStealBuff = new Buff_BaseValueBuff_LifeSteal { lifeSteal = _extraHpSteal };
        if (sizeBuff == null) sizeBuff = new Buff_BaseValueBuff_Size { size = _extraSize };
    }
    protected override void PrepareForRestore() => EnsureBuffs();
    public override void BuffEffect(Chess target)
    {
        EnsureBuffs();
        base.BuffEffect(target);
        hpMaxBuff.target = target; hpMaxBuff.BuffEffect(target);
        lifeStealBuff.target = target; lifeStealBuff.BuffEffect(target);
        sizeBuff.target = target; sizeBuff.BuffEffect(target);
        target.propertyController.onSetDamage.AddListener(OnGetDamage);
        cold = true;
    }
    public override void BuffReset(Buff resetBuff)
    {
        base.BuffReset(resetBuff);
        var other = resetBuff as Buff_Bass;
        if (other?.hpMaxBuff != null && hpMaxBuff != null) hpMaxBuff.BuffReset(other.hpMaxBuff);
        if (other?.lifeStealBuff != null && lifeStealBuff != null) lifeStealBuff.BuffReset(other.lifeStealBuff);
        if (other?.sizeBuff != null && sizeBuff != null) sizeBuff.BuffReset(other.sizeBuff);
    }
    public void OnGetDamage(DamageMessege dm)
    {
        if ((target.propertyController.GetHp()- dm.damage)<target.propertyController.GetMaxHp()/2&& cold)
        {
            cold = false;
            dm.damage = 0;
            target.buffController.AddBuff(buff);
            timer = GameManage.instance.timerManage.AddTimer(() => cold = true,coldDowm);
        }
    }
    public override void BuffOver()
    {
        target.propertyController.onSetDamage.RemoveListener(OnGetDamage);
        if (hpMaxBuff != null) hpMaxBuff.BuffOver();
        if (lifeStealBuff != null) lifeStealBuff.BuffOver();
        if (sizeBuff != null) sizeBuff.BuffOver();
        if (timer != null) { timer.Stop(); timer = null; }
        base.BuffOver();
    }
}
/// <summary>
/// 贝斯隐身buff
/// </summary>
public class Buff_BassHide : TimeBuff
{
    //public float continueTime = 2;
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        target.UnSelectable();
        target.animatorController.ChangeColor(new Color(1, 1, 1, 0.5f));
        
    }

    public override void BuffOver()
    {
        base.BuffOver();
        target.ResumeSelectable();
        target.animatorController.ChangeColor(Color.white);
    }
}

/// <summary>
/// 吉他 Buff：双爆（Crit + CritDamage）组合
/// </summary>
public class Buff_Guitar : Buff
{
    [SerializeReference] public Buff_BaseValueBuff_Crit critBuff;
    [SerializeReference] public Buff_BaseValueBuff_CritDamage critDamageBuff;
    [UnityEngine.Serialization.FormerlySerializedAs("extraCrit")] public float _extraCrit;
    [UnityEngine.Serialization.FormerlySerializedAs("extraCritDamage")] public float _extraCritDamage;
    void EnsureBuffs()
    {
        if (critBuff == null) critBuff = new Buff_BaseValueBuff_Crit { crit = _extraCrit };
        if (critDamageBuff == null) critDamageBuff = new Buff_BaseValueBuff_CritDamage { critDamage = _extraCritDamage };
    }
    protected override void PrepareForRestore() => EnsureBuffs();
    public override void BuffEffect(Chess target)
    {
        EnsureBuffs();
        base.BuffEffect(target);
        critBuff.target = target; critBuff.BuffEffect(target);
        critDamageBuff.target = target; critDamageBuff.BuffEffect(target);
    }
    public override void BuffOver()
    {
        if (critBuff != null) critBuff.BuffOver();
        if (critDamageBuff != null) critDamageBuff.BuffOver();
        base.BuffOver();
    }
    public override void BuffReset(Buff resetBuff)
    {
        base.BuffReset(resetBuff);
        var other = resetBuff as Buff_Guitar;
        if (other?.critBuff != null && critBuff != null) critBuff.BuffReset(other.critBuff);
        if (other?.critDamageBuff != null && critDamageBuff != null) critDamageBuff.BuffReset(other.critDamageBuff);
    }
}
/// <summary>
/// 键盘 Buff：护甲增益，键盘手获得三倍效果（具体效果）
/// </summary>
public class Buff_KeyBoard : Buff
{
    [SerializeReference] public Buff_BaseValueBuff_Armor armorBuff;
    [UnityEngine.Serialization.FormerlySerializedAs("extraArmor")] public float _extraArmor;
    void EnsureBuffs() { if (armorBuff == null) armorBuff = new Buff_BaseValueBuff_Armor { armor = _extraArmor }; }
    protected override void PrepareForRestore() => EnsureBuffs();
    float GetArmorMultiplier(Chess target) => target.propertyController.creator.plantTags.Contains("键盘") ? 3f : 1f;
    public override void BuffEffect(Chess target)
    {
        EnsureBuffs();
        base.BuffEffect(target);
        float mult = GetArmorMultiplier(target);
        target.propertyController.ChangeAR(armorBuff.armor * mult);
    }
    public override void BuffOver()
    {
        if (target != null && armorBuff != null)
        {
            float mult = GetArmorMultiplier(target);
            target.propertyController.ChangeAR(-armorBuff.armor * mult);
        }
        base.BuffOver();
    }
    public override void BuffReset(Buff resetBuff)
    {
        base.BuffReset(resetBuff);
        var other = resetBuff as Buff_KeyBoard;
        if (other?.armorBuff != null && armorBuff != null) armorBuff.BuffReset(other.armorBuff);
    }
}// 