using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// 逻辑上 taki要在mygo数量=4的时候才能使用技能
/// 所以技能的触发条件是Mygo触发+是否点击
/// 然后技能效果就是发射一道恐惧波对吧
/// </summary>
public class SkillEffect_TakiFear : ISkillEffect
{
    //[LabelText("恐惧")]
    //[SerializeReference]
    //public Buff_Fear fear;
    public GameObject fearBullet;
    //这个哈气到底要怎么检测呢...是发射波呢 还是直接检测呢
    //直接检测吧

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        RaycastHit2D[] rays = CheckObjectPoolManage.GetHitArray(100);
        Debug.Log("椎名立希使用了哈气！");
        GameObject b = ObjectPool.instance.Create(fearBullet);
        Bullet zidan = b.GetComponent<Bullet>();
        zidan.InitBullet(user, user.equipWeapon.weaponPos.position, user, user.transform.right);
        //zidan.Dm.takeBuff = fear.Clone();
        zidan.rate = config.baseDamage[0];
        CheckObjectPoolManage.ReleaseArray(100, rays);
    }
}
