using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>灯主动技：一次性进入棋子模式（停攻、Blend=1、商店灯卡→棋子卡、快照 ATK）。</summary>
[Serializable]
public class SkillEffect_TomoriChessMode : ISkillEffect
{
    [Tooltip("开技后商店槽切换为该棋子 Creator")]
    public PropertyCreator chessPieceCreator;

    [Tooltip("Animator Blend 目标值（棋子形态）")]
    public float blendValue = 1f;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user == null || chessPieceCreator == null)
            return;
        if (user.skillController?.context != null
            && user.skillController.context.TryGet(TomoriChessKeys.ChessMode, out bool on)
            && on)
            return;

        float atk = user.propertyController != null ? user.propertyController.GetAttack() : 0f;
        user.skillController.context.Set(TomoriChessKeys.AtkSnapshot, atk);
        user.skillController.context.Set(TomoriChessKeys.ChessMode, true);
        TomoriChessKeys.GetOrCreatePieceList(user);

        if (user.equipWeapon != null)
        {
            user.equipWeapon.AttackAble = false;
            user.equipWeapon.StopAttack();
        }

        user.animatorController?.ChangeFloat(blendValue);

        if (user.skillController.skillColdFx != null)
        {
            UnityEngine.Object.Destroy(user.skillController.skillColdFx.gameObject);
            user.skillController.skillColdFx = null;
        }

        PropertyCreator lampCreator = user.propertyController?.creator;
        ShopIcon icon = TomoriChessKeys.FindShopIconByGood(lampCreator);
        icon?.RefreshGood(chessPieceCreator);
    }
}

/// <summary>棋子模式已开启则不可再放主动技（一次性变身）。</summary>
[Serializable]
public class SkillReady_TomoriNotChessMode : ISkillReady
{
    public bool IfSkillReady(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user?.skillController?.context == null)
            return true;
        if (user.skillController.context.TryGet(TomoriChessKeys.ChessMode, out bool on) && on)
            return false;
        return true;
    }

    public void InitSkillReady(Chess user, SkillConfig config, List<Chess> targets) { }
}
