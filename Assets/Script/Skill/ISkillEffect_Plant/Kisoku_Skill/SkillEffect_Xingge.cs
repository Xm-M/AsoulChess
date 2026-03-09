using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///// <summary>
///// 星歌要根据不同的角色给予不同的buff
///// 发钱出去 给buff
///// 没图还是做不下去啊 
///// </summary>
public class SkillEffect_Xingge : ISkillEffect
{
    public GameObject money;
    public string targetTag = "结束乐队";
    /// <summary>
    /// 技能效果是寻找队伍里所有带有结束乐队tag的单位 发射一枚buff子弹
    /// </summary>
    /// <param name="user"></param>
    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        foreach (var chess in ChessTeamManage.Instance.GetTeam(user.tag))
        {
            if (chess.propertyController.creator.plantTags.Contains(targetTag))
            {
                GameObject b = ObjectPool.instance.Create(money);
                Bullet_Aming zidan = b.GetComponent<Bullet_Aming>();
                zidan.InitBullet(user, user.equipWeapon.weaponPos.position, chess, user.transform.right);
                zidan.Dm.damage = user.propertyController.GetAttack();
                zidan.Dm.damageTo = chess;
            }
        }
    }
}
