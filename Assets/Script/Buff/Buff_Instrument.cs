using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 和乐队相关的buff 都被放在这个文件里了
/// </summary>
 
/// <summary>
/// 主唱 Buff：每次攻击有 extraBulletChance 概率额外发射一发子弹。子弹的命中数量与羁绊人数有关
/// 概率由羁绊人数决定：2人20%、3人45%、4人70%、5人100%
/// 子弹类型为：火焰（双倍伤害，寒冷(减速效果，光（清除迷雾，
/// </summary>
public class Buff_Vocal : Buff
{
    [Tooltip("每次攻击触发额外子弹的概率 0~1")]
    public float extraBulletChance;
    public GameObject extraBullet;

    public override void WriteExtraToSaveData(BuffSaveData data)
    {
        base.WriteExtraToSaveData(data);
        if (data == null) return;
        data.SetExtra("ExtraBulletChance", extraBulletChance);
    }
    public override void RestoreExtraFromSaveData(BuffSaveData data)
    {
        base.RestoreExtraFromSaveData(data);
        if (data == null) return;
        extraBulletChance = data.GetExtraFloat("ExtraBulletChance", extraBulletChance);
    }
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        target.equipWeapon.OnAttack.AddListener(OnAttack);
    }

    void OnAttack(Chess chess)
    {
        if (Random.value < extraBulletChance && extraBullet != null)
        {
            Bullet b = ObjectPool.instance.Create(extraBullet).GetComponent<Bullet>();
            b.InitBullet(chess, chess.equipWeapon.weaponPos.position, null, chess.transform.right);
        }
    }

    public override void BuffOver()
    {
        base.BuffOver();
        target.equipWeapon.OnAttack.RemoveListener(OnAttack);
    }

    public override void BuffReset(Buff resetBuff)
    {
        base.BuffReset(resetBuff);
        var other = resetBuff as Buff_Vocal;
        if (other != null) extraBulletChance = other.extraBulletChance;
    }
}
/// <summary>
/// 贝斯 Buff：HP（百分比+数值复合）+ 生命偷取 + 体型 + 半血隐身
/// 满层：+50%最大生命+600血，+50%偷取，体型+5。由 Fetter 按人数设置 tierScale/size。
/// </summary>
public class Buff_Bass : Buff
{
    [Tooltip("档位比例 0.2~1，由羁绊人数决定")]
    public float tierScale = 1f;
    [Tooltip("满层时百分比生命，0.5=50%")]
    public float baseHpPercent = 0.5f;
    [Tooltip("满层时固定生命")]
    public float baseHpFlat = 600f;
    [Tooltip("满层时生命偷取，0.5=50%")]
    public float maxLifeSteal = 0.5f;
    [Tooltip("体型增量，2~5人对应2~5")]
    public int sizeAdd = 5;
    [SerializeReference] public Buff_BaseValueBuff_LifeSteal lifeStealBuff;
    [SerializeReference] public Buff_BaseValueBuff_Size sizeBuff;
    [SerializeReference] public Buff_BassHide buff;
    public float coldDowm = 30;
    public override Buff Clone()
    {
        var c = (Buff_Bass)base.Clone();
        c.lifeStealBuff = lifeStealBuff != null ? (Buff_BaseValueBuff_LifeSteal)lifeStealBuff.Clone() : null;
        c.sizeBuff = sizeBuff != null ? (Buff_BaseValueBuff_Size)sizeBuff.Clone() : null;
        c.buff = buff != null ? (Buff_BassHide)buff.Clone() : null;
        return c;
    }
    protected Timer timer;
    protected bool cold;
    float hpAddStored;
    void EnsureBuffs()
    {
        if (lifeStealBuff == null) lifeStealBuff = new Buff_BaseValueBuff_LifeSteal();
        if (sizeBuff == null) sizeBuff = new Buff_BaseValueBuff_Size();
    }
    protected override void PrepareForRestore() => EnsureBuffs();
    public override void WriteExtraToSaveData(BuffSaveData data)
    {
        base.WriteExtraToSaveData(data);
        if (data == null) return;
        data.SetExtra("TierScale", tierScale);
        data.SetExtra("SizeAdd", sizeAdd);
        data.SetExtra("HpAddStored", hpAddStored);
    }
    public override void RestoreExtraFromSaveData(BuffSaveData data)
    {
        base.RestoreExtraFromSaveData(data);
        if (data == null) return;
        tierScale = data.GetExtraFloat("TierScale", tierScale);
        sizeAdd = data.GetExtraInt("SizeAdd", sizeAdd);
        hpAddStored = data.GetExtraFloat("HpAddStored", hpAddStored);
    }
    public override void BuffEffect(Chess target)
    {
        EnsureBuffs();
        base.BuffEffect(target);
        float baseHp = target.propertyController.GetMaxHp();
        hpAddStored = baseHp * baseHpPercent * tierScale + baseHpFlat * tierScale;
        target.propertyController.ChangeHPMax(hpAddStored);
        lifeStealBuff.lifeSteal = maxLifeSteal * tierScale;
        lifeStealBuff.target = target;
        lifeStealBuff.BuffEffect(target);
        sizeBuff.size = sizeAdd;
        sizeBuff.target = target;
        sizeBuff.BuffEffect(target);
        target.propertyController.onSetDamage.AddListener(OnGetDamage);
        cold = true;
    }
    public override void BuffReset(Buff resetBuff)
    {
        base.BuffReset(resetBuff);
        var other = resetBuff as Buff_Bass;
        if (other != null)
        {
            tierScale = other.tierScale;
            sizeAdd = other.sizeAdd;
            if (lifeStealBuff != null && other.lifeStealBuff != null) lifeStealBuff.BuffReset(other.lifeStealBuff);
            if (sizeBuff != null && other.sizeBuff != null) sizeBuff.BuffReset(other.sizeBuff);
        }
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
        target.propertyController.ChangeHPMax(-hpAddStored);
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
    public override Buff Clone()
    {
        var c = (Buff_Guitar)base.Clone();
        c.critBuff = critBuff != null ? (Buff_BaseValueBuff_Crit)critBuff.Clone() : null;
        c.critDamageBuff = critDamageBuff != null ? (Buff_BaseValueBuff_CritDamage)critDamageBuff.Clone() : null;
        return c;
    }
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
/// 鼓手增益 Buff：外壳，内装 Buff_BaseValueBuff_ExtraDamage。BuffReset 不叠加，多鼓手时取先到者。
/// </summary>
public class Buff_DrummerDamage : Buff
{
    [SerializeReference] public Buff_BaseValueBuff_ExtraDamage extraDamageBuff;
    [Tooltip("获得鼓手增益时生成的光环特效")]
    public GameObject auraEffect;
    GameObject _spawnedAura;
    public override Buff Clone()
    {
        var c = (Buff_DrummerDamage)base.Clone();
        c.extraDamageBuff = extraDamageBuff != null ? (Buff_BaseValueBuff_ExtraDamage)extraDamageBuff.Clone() : null;
        return c;
    }
    void EnsureBuff()
    {
        if (extraDamageBuff == null) extraDamageBuff = new Buff_BaseValueBuff_ExtraDamage();
    }
    protected override void PrepareForRestore() => EnsureBuff();
    public override void BuffEffect(Chess target)
    {
        EnsureBuff();
        base.BuffEffect(target);
        extraDamageBuff.target = target;
        extraDamageBuff.BuffEffect(target);
        if (auraEffect != null)
        {
            _spawnedAura = ObjectPool.instance.Create(auraEffect);
            _spawnedAura.transform.SetParent(target.transform);
            _spawnedAura.transform.localPosition = Vector3.zero;
        }
    }
    public override void BuffOver()
    {
        if (_spawnedAura != null)
        {
            ObjectPool.instance.Recycle(_spawnedAura);
            _spawnedAura = null;
        }
        if (extraDamageBuff != null && target != null)
            target.propertyController.ChangeExtraDamage(-extraDamageBuff.extraDamage);
        base.BuffOver();
    }
    public override void BuffReset(Buff resetBuff)
    {
        // 不叠加，保持现有数值
    }
}

/// <summary>
/// 鼓手光环 Buff：挂在鼓手身上，周期性给周围8格+自己施加 Buff_DrummerDamage。满层50%增伤。
/// </summary>
public class Buff_DrummerAura : Buff
{
    public float extraDamage; // 由羁绊设置，满层0.5
    [SerializeReference] public Buff_DrummerDamage drummerDamageBuff;
    [Tooltip("获得鼓手增益时生成的光环特效")]
    public GameObject auraEffect;
    public float checkInterval = 0.5f;
    Timer timer;
    HashSet<Chess> buffedUnits = new HashSet<Chess>();
    void EnsureBuff()
    {
        if (drummerDamageBuff == null) drummerDamageBuff = new Buff_DrummerDamage();
    }
    protected override void PrepareForRestore() => EnsureBuff();
    public override void BuffEffect(Chess target)
    {
        EnsureBuff();
        base.BuffEffect(target);
        drummerDamageBuff.extraDamageBuff.extraDamage = extraDamage;
        drummerDamageBuff.buffName = "鼓手增益";
        drummerDamageBuff.auraEffect = auraEffect;
        ApplyAura();
        timer = GameManage.instance.timerManage.AddTimer(ApplyAura, checkInterval, true);
    }
    void ApplyAura()
    {
        if (target == null || target.IfDeath) return;
        var standTile = target.moveController?.standTile;
        if (standTile == null || MapManage.instance == null) return;
        var toBuff = new List<Chess>();
        toBuff.Add(target); // 自己也能吃到
        var neighbors = MapManage.instance.GetEightNeighborTiles(standTile);
        foreach (var t in neighbors)
        {
            if (t.stander != null && t.stander.CompareTag("Player") && !t.stander.IfDeath)
                toBuff.Add(t.stander);
        }
        var toRemove = new List<Chess>();
        foreach (var c in buffedUnits)
        {
            if (!toBuff.Contains(c) && c != null && !c.IfDeath)
            {
                c.buffController.TryOverBuff(drummerDamageBuff);
                toRemove.Add(c);
            }
        }
        foreach (var c in toRemove) buffedUnits.Remove(c);
        foreach (var c in toBuff)
        {
            if (c == null || c.IfDeath) continue;
            drummerDamageBuff.extraDamageBuff.extraDamage = extraDamage;
            c.buffController.AddBuff(drummerDamageBuff);
            buffedUnits.Add(c);
        }
    }
    public override void BuffOver()
    {
        timer?.Stop();
        timer = null;
        foreach (var c in buffedUnits)
        {
            if (c != null && !c.IfDeath)
                c.buffController.TryOverBuff(drummerDamageBuff);
        }
        buffedUnits.Clear();
        base.BuffOver();
    }

    public override void WriteExtraToSaveData(BuffSaveData data)
    {
        base.WriteExtraToSaveData(data);
        if (data == null) return;
        data.SetExtra("ExtraDamage", extraDamage);
    }
    public override void RestoreExtraFromSaveData(BuffSaveData data)
    {
        base.RestoreExtraFromSaveData(data);
        if (data == null) return;
        extraDamage = data.GetExtraFloat("ExtraDamage", extraDamage);
    }
}

/// <summary>
/// 键盘增益 Buff：外壳，内装 Buff_BaseValueBuff_Armor。BuffReset 不叠加。
/// </summary>
public class Buff_KeyBoardArmor : Buff
{
    [SerializeReference] public Buff_BaseValueBuff_Armor armorBuff;
    [Tooltip("获得键盘增益时生成的光环特效")]
    public GameObject auraEffect;
    GameObject _spawnedAura;
    public override Buff Clone()
    {
        var c = (Buff_KeyBoardArmor)base.Clone();
        c.armorBuff = armorBuff != null ? (Buff_BaseValueBuff_Armor)armorBuff.Clone() : null;
        return c;
    }
    void EnsureBuff() { if (armorBuff == null) armorBuff = new Buff_BaseValueBuff_Armor(); }
    protected override void PrepareForRestore() => EnsureBuff();
    public override void BuffEffect(Chess target)
    {
        EnsureBuff();
        base.BuffEffect(target);
        armorBuff.target = target;
        armorBuff.BuffEffect(target);
        if (auraEffect != null)
        {
            _spawnedAura = ObjectPool.instance.Create(auraEffect);
            _spawnedAura.transform.SetParent(target.transform);
            _spawnedAura.transform.localPosition = Vector3.zero;
        }
    }
    public override void BuffOver()
    {
        if (_spawnedAura != null)
        {
            ObjectPool.instance.Recycle(_spawnedAura);
            _spawnedAura = null;
        }
        if (armorBuff != null && target != null)
            target.propertyController.ChangeAR(-armorBuff.armor);
        base.BuffOver();
    }
    public override void BuffReset(Buff resetBuff) { /* 不叠加 */ }
}

/// <summary>
/// 键盘光环 Buff：挂在键盘身上，周围8格+自身获得护甲。周围0.5倍，键盘自身2倍。满层100护甲。
/// </summary>
public class Buff_KeyBoardAura : Buff
{
    public float baseArmor; // 由羁绊设置，满层100
    [SerializeReference] public Buff_KeyBoardArmor keyBoardArmorBuff;
    [Tooltip("获得键盘增益时生成的光环特效")]
    public GameObject auraEffect;
    public float checkInterval = 0.5f;
    Timer timer;
    HashSet<Chess> buffedUnits = new HashSet<Chess>();
    void EnsureBuff() { if (keyBoardArmorBuff == null) keyBoardArmorBuff = new Buff_KeyBoardArmor(); }
    protected override void PrepareForRestore() => EnsureBuff();
    public override void BuffEffect(Chess target)
    {
        EnsureBuff();
        base.BuffEffect(target);
        keyBoardArmorBuff.buffName = "键盘增益";
        keyBoardArmorBuff.auraEffect = auraEffect;
        ApplyAura();
        timer = GameManage.instance.timerManage.AddTimer(ApplyAura, checkInterval, true);
    }
    void ApplyAura()
    {
        if (target == null || target.IfDeath) return;
        var standTile = target.moveController?.standTile;
        if (standTile == null || MapManage.instance == null) return;
        var toBuff = new List<Chess>();
        toBuff.Add(target);
        var neighbors = MapManage.instance.GetEightNeighborTiles(standTile);
        foreach (var t in neighbors)
        {
            if (t.stander != null && t.stander.CompareTag("Player") && !t.stander.IfDeath)
                toBuff.Add(t.stander);
        }
        var toRemove = new List<Chess>();
        foreach (var c in buffedUnits)
        {
            if (!toBuff.Contains(c) && c != null && !c.IfDeath)
            {
                c.buffController.TryOverBuff(keyBoardArmorBuff);
                toRemove.Add(c);
            }
        }
        foreach (var c in toRemove) buffedUnits.Remove(c);
        foreach (var c in toBuff)
        {
            if (c == null || c.IfDeath) continue;
            float mult = (c == target) ? 2f : 0.5f; // 自身2倍，周围0.5倍
            keyBoardArmorBuff.armorBuff.armor = baseArmor * mult;
            c.buffController.AddBuff(keyBoardArmorBuff);
            buffedUnits.Add(c);
        }
    }
    public override void BuffOver()
    {
        timer?.Stop();
        timer = null;
        foreach (var c in buffedUnits)
        {
            if (c != null && !c.IfDeath)
                c.buffController.TryOverBuff(keyBoardArmorBuff);
        }
        buffedUnits.Clear();
        base.BuffOver();
    }
    public override void WriteExtraToSaveData(BuffSaveData data)
    {
        base.WriteExtraToSaveData(data);
        if (data != null) data.SetExtra("BaseArmor", baseArmor);
    }
    public override void RestoreExtraFromSaveData(BuffSaveData data)
    {
        base.RestoreExtraFromSaveData(data);
        if (data != null) baseArmor = data.GetExtraFloat("BaseArmor", baseArmor);
    }
}// 
