using System;
using UnityEngine;

/// <summary>
/// sumimi 甜甜圈命中：目标为 <see cref="Sumimi.SkillOnAttackMemberId"/>（三角初华）时在目标格生成阳光。
/// </summary>
[Serializable]
public class BulletEffect_SumimiDonutHit : IBulletEffect
{
    public int sunAmount = 25;

    public void OnBulletHit(Bullet bullet)
    {
        Chess target = bullet?.Dm?.damageTo;
        if (target == null || target.IfDeath || !Sumimi.IsSkillOnAttackMember(target))
            return;

        var stand = target.moveController?.standTile;
        if (stand == null)
            return;

        var panel = UIManage.GetView<ItemPanel>();
        if (panel == null || panel.Create<SunLight>() is not SunLight sun)
            return;

        sun.InitSunLight(stand, sunAmount, target.transform.position);
    }
}
