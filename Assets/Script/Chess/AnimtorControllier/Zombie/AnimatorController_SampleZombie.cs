using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class AnimatorController_SampleZombie : AnimatorController
{
    public bool deathfire;

    /// <summary>与 <see cref="SyncVisualTierToAnimator"/> 一致：&gt;0.6 → 0；&gt;0.25 且 ≤0.6 → 1；≤0.25 → 2。供 IceCarArmor 按护甲比例复用。</summary>
    public static float TierFromDamagePhaseRatio(float ratio01)
    {
        if (ratio01 <= 0.25f) return 2f;
        if (ratio01 <= 0.6f) return 1f;
        return 0f;
    }
    [Tooltip("断手阶段生成一次的手臂掉落/特效预制体")]
    public GameObject leftarm;
    [Tooltip("断头阶段生成一次的头颅掉落/特效预制体")]
    public GameObject leftHead;

    [Tooltip("Animator Float，与 idle/run/attack 等状态内 Blend Tree 一致：0 完整 1 断手 2 断头。未配置参数则忽略。")]
    [SerializeField]
    string visualTierParameterName = DefaultVisualTierParam;

    [Tooltip("Animator Float，与单一 death 状态内 Blend Tree 一致：0 普通死亡 1 火烧死亡。未配置参数则忽略。")]
    [SerializeField]
    string deathVariantParameterName = DefaultDeathVariantParam;

    [LabelText("受伤播放器")]
    public AudioPlayer player;
    [SerializeReference]
    public BloodBuff bloodBuff;
    public float randomSpeed = 0.2f;

    bool _spawnedArmStageVfx;
    bool _spawnedHeadStageVfx;

    public override void WhenControllerEnterWar()
    {
        base.WhenControllerEnterWar();
        if (sprite != null)
            sprite.gameObject.SetActive(true);
        ChangeColor(Color.white);
        deathfire = false;
        _spawnedArmStageVfx = false;
        _spawnedHeadStageVfx = false;
        float n = UnityEngine.Random.Range(0, randomSpeed);
        chess.propertyController.ChangeAcceleRate(n);
        if (!ShouldSkipHpVisualTierSync())
            SyncVisualTierToAnimator();
    }

    public override void PlayIdle()
    {
        base.PlayIdle();
    }

    public override void OnGetDamage(DamageMessege dm)
    {
        bool skipHpVisual = ShouldSkipHpVisualTierSync();
        if (!skipHpVisual && (dm.damageElementType & ElementType.Explode) != 0 && chess.propertyController.GetHpPerCent() <= 0)
        {
            deathfire = true;
            SyncVisualTierToAnimator();
        }
        else
        {
            if ((dm.damageElementType & ElementType.Bullet) != 0)
                player?.RandomPlay();

            float hp = chess.propertyController.GetHpPerCent();
            if (hp > 0.6f)
            {
                base.OnGetDamage(dm);
            }
            else
            {
                if (hp <= 0.6f && !_spawnedArmStageVfx)
                {
                    _spawnedArmStageVfx = true;
                    if (leftarm != null)
                        ObjectPool.instance.Create(leftarm).transform.position = transform.position;
                }
                if (hp > 0.1f && hp <= 0.6f && sprite != null)
                    sprite.material.SetFloat("_FlashAmount", Time.time);

                if (hp <= 0.25f && !_spawnedHeadStageVfx)
                {
                    _spawnedHeadStageVfx = true;
                    chess.buffController.AddBuff(bloodBuff);
                    if (leftHead != null)
                    {
                        GameObject lhead = ObjectPool.instance.Create(leftHead);
                        lhead.transform.position = transform.position;
                    }
                }
            }
        }

        if (!skipHpVisual)
            SyncVisualTierToAnimator();
    }

    /// <summary>
    /// 与血量档一致：&gt;0.6 完整；&lt;=0.6 且 &gt;0.25 断手；&lt;=0.25 断头。外观由 Animator Blend Tree + <see cref="visualTierParameterName"/> 驱动。
    /// 带 <see cref="IceCarArmor"/> 或未破的 <see cref="HeadArmor_VisualTier"/> 时由护甲驱动，此处不写入。
    /// </summary>
    void SyncVisualTierToAnimator()
    {
        if (chess == null || chess.propertyController == null) return;
        if (ShouldSkipHpVisualTierSync()) return;
        float hp = chess.propertyController.GetHpPerCent();
        float tier = TierFromDamagePhaseRatio(hp);
        SetVisualTier(tier, visualTierParameterName);
    }

    /// <summary>IceCar 全程接管；头盔 VisualTier 护甲在未破甲前接管（含爆炸仍打本体的情况）。</summary>
    bool ShouldSkipHpVisualTierSync()
    {
        if (chess == null) return false;
        if (chess.GetComponentInChildren<IceCarArmor>() != null) return true;
        var helmet = chess.GetComponentInChildren<HeadArmor_VisualTier>();
        return helmet != null && !helmet.IsBroken;
    }

    /// <summary>由 IceCarArmor 按护甲比例写入，使用本类配置的 <see cref="visualTierParameterName"/>。</summary>
    public void ApplyVisualTierFromRatio(float ratio01)
    {
        SetVisualTier(TierFromDamagePhaseRatio(ratio01), visualTierParameterName);
    }

    public override void PlayDeath()
    {
        if (animator == null) return;
        bool explodeVariant = deathfire;
        var ice = chess != null ? chess.GetComponentInChildren<IceCarArmor>() : null;
        if (ice != null && ice.ExplodeDeathVariant)
            explodeVariant = true;
        SetDeathVariant(explodeVariant ? 1f : 0f, deathVariantParameterName);
        animator.Play("death");
    }

    public override void ChangeColor(Color color)
    {
        base.ChangeColor(color);
    }
}

/// <summary>
/// 僵尸的 自扣血buff
/// </summary>
public class BloodBuff : Buff
{
    public DamageMessege dm;
    float speed;
    Timer timer;
    float leftHp;
    public override void WriteToSaveData(BuffSaveData data)
    {
        base.WriteToSaveData(data);
        if (data != null && timer != null) data.remainingTime = timer.LeftTime();
    }
    public override void WriteExtraToSaveData(BuffSaveData data)
    {
        base.WriteExtraToSaveData(data);
        if (data == null) return;
        data.SetExtra("Speed", speed);
        data.SetExtra("LeftHp", leftHp);
    }
    public override void RestoreExtraFromSaveData(BuffSaveData data)
    {
        base.RestoreExtraFromSaveData(data);
        if (data == null) return;
        speed = data.GetExtraFloat("Speed", 0);
        leftHp = data.GetExtraFloat("LeftHp", 0);
    }
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        dm.damageTo = target;
        dm.damageFrom = target;
        this.target = target;
        target.propertyController.ChangeAttack(-target.propertyController.GetAttack());
        if (speed <= 0 || leftHp <= 0)
        {
            speed = UnityEngine.Random.Range(1, 1.5f);
            leftHp = target.propertyController.GetHp();
        }
        float delay = _restoreRemainingTime >= 0 ? _restoreRemainingTime : 0.03f;
        _restoreRemainingTime = -1f;
        timer = GameManage.instance.timerManage.AddTimer(BloodDamage, delay, true);
    }
    public void BloodDamage()
    {
        dm.damage = Mathf.Max(leftHp * 0.03f / speed, 0.99f);
        if (!target.IfDeath)
            target.propertyController.GetDamage(dm);
    }
    public override void BuffOver()
    {
        base.BuffOver();
        timer.Stop();
        timer = null;
    }
}
