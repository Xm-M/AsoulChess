using System;
using UnityEngine;

/// <summary>橡皮擦：命中叠「擦除」Buff。</summary>
[Serializable]
public class BulletEffect_HitagiErase : IBulletEffect
{
    [SerializeReference]
    public Buff_HitagiErase eraseBuff;

    public void OnBulletHit(Bullet bullet)
    {
        Chess target = bullet?.Dm?.damageTo;
        if (target == null || target.IfDeath || target.buffController == null)
            return;

        Buff_HitagiErase buff = eraseBuff != null
            ? (Buff_HitagiErase)eraseBuff.Clone()
            : new Buff_HitagiErase();
        target.buffController.AddBuff(buff);
    }
}
