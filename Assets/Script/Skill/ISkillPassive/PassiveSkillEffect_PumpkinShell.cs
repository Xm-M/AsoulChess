using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 南瓜罩类 Support 被动：同格存在 <see cref="PlantType.MainPlant"/>（<see cref="Tile.stander"/>）时，
/// 在 <see cref="PropertyController.onSetDamage"/> 将本次对 Main 的**可转移伤害**改为由自身结算（仅数值，不带 <see cref="DamageMessege.takeBuff"/>）。
/// 不转移：<see cref="DamageType.Heal"/> / <see cref="DamageType.Real"/> / <see cref="DamageType.Miss"/>。
/// <see cref="ElementType.Grind"/>：Main 与南瓜各自按 <see cref="PropertyController.GetDamage"/> 中碾压逻辑做体型判定。
/// </summary>
[Serializable]
public class PassiveSkillEffect_PumpkinShell : ISkillEffect
{
    public Chess protectChess;
    public DamageMessege dm;
    Chess user;
    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        this.user = user;
        Debug.Log(user.moveController.standTile);
        if (user.moveController.standTile != null)
        {
            Debug.Log("注册");
            Tile t = user.moveController.standTile;
            if (t.stander != null) ProtectChess(t.stander);
            t.OnPlant.AddListener(ProtectChess);
            user.OnRemove.AddListener((c)=>t.OnPlant.RemoveListener(ProtectChess));   
        }
    }
    public void OnGetDamage(DamageMessege damageMes)
    {
        if ((dm.damageElementType & ElementType.Explode) == 0)
        {
            dm.damage = damageMes.damage;
            dm.damageFrom = damageMes.damageFrom;
            dm.damageTo = user;
            dm.damageType = damageMes.damageType;
            dm.damageElementType = damageMes.damageElementType;
            user.propertyController.GetDamage(dm);
            damageMes.damage = 0;
        }
    }
    public void ProtectChess(Chess chess)
    {
        if (chess.propertyController.creator.plantType != PlantType.MainPlant) return;
        protectChess = chess;
        chess.propertyController.onGetDamage.AddListener(OnGetDamage);
    }
}

