using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 通用技能效果：从 <see cref="AttackController.weaponPos"/> 发射一枚子弹。
/// 伤害倍率默认取 <see cref="SkillConfig.baseDamage"/>[0] 作为 <see cref="Bullet.rate"/>（与 <see cref="SkillEffect_TakiFear"/> 一致）。
/// MyGO 四人包围、点击等释放条件请用 <see cref="ISkillReady"/>（如 <see cref="SkillReady_Multy"/> + <see cref="SkillReady_Mygo"/> + <see cref="SkillReady_MouseDown"/>）配置在技能外。
/// </summary>
[Serializable]
public class SkillEffect_ShootBullet : ISkillEffect
{
    [LabelText("子弹预制体")]
    public GameObject bulletPrefab;

    [LabelText("无索敌时仍发射")]
    [Tooltip("为 true 时无 targets 则沿 shooter.transform.right 直线发射")]
    public bool shootWithoutTarget = true;

    [LabelText("伤害倍率（覆盖 config）")]
    [Tooltip("≤0 时使用 config.baseDamage[0]；config 为空则为 1")]
    public float damageRateOverride;

    [LabelText("命中 Buff（可选）")]
    [Tooltip("非空时写入 DamageMessege.takeBuff，覆盖 prefab 默认")]
    [SerializeReference]
    public Buff takeBuffOnHit;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user?.equipWeapon?.weaponPos == null || bulletPrefab == null || ObjectPool.instance == null)
            return;

        Chess target = targets != null && targets.Count > 0 ? targets[0] : null;
        if (target == null && !shootWithoutTarget)
            return;

        float rate = ResolveDamageRate(config);
        Vector3 spawnPos = user.equipWeapon.weaponPos.position;
        Vector2 moveDir = user.transform.right;

        GameObject go = ObjectPool.instance.Create(bulletPrefab);
        if (go == null)
            return;

        Bullet bullet = go.GetComponent<Bullet>();
        if (bullet == null)
        {
            ObjectPool.instance.Recycle(go);
            return;
        }

        if (bullet.Dm == null)
            bullet.Dm = new DamageMessege();

        bullet.InitBullet(user, spawnPos, target, moveDir, -1f, rate);
        if (target != null)
            bullet.Dm.damageTo = target;
        if (takeBuffOnHit != null)
            bullet.Dm.takeBuff = takeBuffOnHit;
    }

    float ResolveDamageRate(SkillConfig config)
    {
        if (damageRateOverride > 0f)
            return damageRateOverride;
        if (config?.baseDamage != null && config.baseDamage.Count > 0)
            return config.baseDamage[0];
        return 1f;
    }
}
