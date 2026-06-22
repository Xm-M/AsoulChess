using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Oblivionis 普攻/技能弹道：在两种可配置子弹间<strong>交替</strong>发射。
/// 上次发射索引存于 <see cref="SkillContext"/>（<c>OblivionisLastBulletIndex</c>，0=A，1=B），按棋子区分，并参与存档。
/// </summary>
public class SkillEffect_Oblivionis : ISkillEffect
{
    public const string ContextKeyLastBulletIndex = "OblivionisLastBulletIndex";

    [FormerlySerializedAs("bullet")]
    public GameObject bulletA;
    public GameObject bulletB;
    public Color outLineColor;
    public float outlineSize;
    public float continueTime = 25f;
    Timer t;
    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user == null || ObjectPool.instance == null)
            return;
        user.animatorController.SetOutline(outLineColor,outlineSize);
        t= GameManage.instance.timerManage.AddTimer(() => { if (user!=null) user.animatorController.SetOutline(Color.white, 0); }, continueTime);
        SkillContext ctx = user.skillController?.context;
        int nextIndex = 0;
        if (ctx != null && ctx.TryGet(ContextKeyLastBulletIndex, out int last) && (last == 0 || last == 1))
            nextIndex = 1 - last;

        GameObject prefab = BulletPrefab(nextIndex);
        int firedIndex = nextIndex;
        if (prefab == null)
        {
            nextIndex = 1 - nextIndex;
            prefab = BulletPrefab(nextIndex);
            firedIndex = nextIndex;
        }

        if (prefab == null)
            return;

        GameObject b = ObjectPool.instance.Create(prefab); 
        if (b == null)
            return;

        Bullet zidan = b.GetComponent<Bullet>();
        if (zidan == null)
            return;

        if (targets != null && targets.Count != 0)
        {
            zidan.InitBullet(user, user.equipWeapon.weaponPos.position, targets[0], user.transform.right);
            zidan.Dm.damageTo = targets[0];
            zidan.rate = config.baseDamage[0];
        }
        else
        {
            zidan.InitBullet(user, user.equipWeapon.weaponPos.position, null, user.transform.right);
            zidan.rate = config.baseDamage[0];
        }

        if (ctx != null)
            ctx.Set(ContextKeyLastBulletIndex, firedIndex);
    }

    GameObject BulletPrefab(int index) => index == 0 ? bulletA : bulletB;
}
