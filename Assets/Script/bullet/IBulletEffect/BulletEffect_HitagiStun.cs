using System;
using UnityEngine;

/// <summary>订书机：命中施加 0.5s DizznessBuff。</summary>
[Serializable]
public class BulletEffect_HitagiStun : IBulletEffect
{
    [SerializeReference]
    public DizznessBuff stunBuff;

    public float stunDuration = 0.5f;

    public void OnBulletHit(Bullet bullet)
    {
        Chess target = bullet?.Dm?.damageTo;
        if (target == null || target.IfDeath || target.buffController == null)
            return;

        DizznessBuff buff = stunBuff != null
            ? (DizznessBuff)stunBuff.Clone()
            : new DizznessBuff { buffName = "眩晕" };
        if (string.IsNullOrEmpty(buff.buffName))
            buff.buffName = "眩晕";
        buff.continueTime = stunDuration > 0f ? stunDuration : 0.5f;
        target.buffController.AddBuff(buff);
    }
}
