using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 飞行气球僵尸被动：进场不可选中；受到带 <see cref="ElementType.Puncture"/> 的伤害时，
/// 本次对本体 HP 结算为 0，下一帧先 <see cref="Chess.Death"/> 再在同格生成配置的落地僵尸（普通单位由 <see cref="landedZombie"/> 决定）。
/// </summary>
[Serializable]
public class PassiveSkillEffect_BalloonZombiePop : ISkillEffect
{
    [Tooltip("被扎破后在原格子生成的僵尸（如普通小丑/普通僵尸）；留空则仅死亡不生成")]
    public PropertyCreator landedZombie;
    Tile t;
    bool puncturefall;
    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        puncturefall = false;
        t = user.moveController.standTile;
        user.UnSelectable();
        user.equipWeapon.AttackAble = false;
        user.OnRemove.AddListener(CreateZombie);
        user.moveController.OnReachTile.AddListener((a, b) => t = b);
        user.skillController.context.Set<bool>("fly", true);
        
    }
    public void OnGetDamage(DamageMessege dm)
    {
        if ((dm.damageElementType & ElementType.Puncture) != 0&&((dm.damageElementType & ElementType.Explode) ==0))
        {
            puncturefall = true;
        }
    }
    public void CreateZombie(Chess chess)
    {
        if(landedZombie != null&&puncturefall)
        {
            Chess c= GameManage.instance.chessTeamManage.CreateChess(landedZombie, t, chess.tag);
            c.stateController.ChangeState(StateName.ResumeState);
            c.transform.position = chess.transform.position;
        }
    }
}

