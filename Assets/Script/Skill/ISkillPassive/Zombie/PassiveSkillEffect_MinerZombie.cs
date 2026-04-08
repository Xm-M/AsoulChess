using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 矿工僵尸被动：掘进阶段不可选中、不可普攻、<see cref="PropertyController.ChangeMoveAcceleRate"/> 掘进移速；
/// 动画复用鸭子僵尸 <see cref="AnimatorController_DuckZombie.SetForceInWater"/>（土里等同 InWater）。
/// 抵达第 0 列（<see cref="MoveController.OnReachTile"/>）出土：撤销掘进移速、关闭 InWater、转向、可普攻、进入 <see cref="ResumeState"/>（播放 resume 等由状态图与 Animator 配置）。
/// </summary>
[Serializable]
public class PassiveSkillEffect_MinerZombie : ISkillEffect
{
    [Tooltip("掘进阶段仅移速倍率增量（出土时对称扣回）")]
    public float digMoveAcceleBonus = 3f;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user == null) return;

        bool surfaced = false;
        UnityAction<Chess, Tile> onReach = null;
        UnityAction<Chess> onRemove = null;

        void RemoveReach()
        {
            if (user?.moveController != null && onReach != null)
                user.moveController.OnReachTile.RemoveListener(onReach);
        }

        onRemove = _ =>
        {
            if (!surfaced && user != null)
            {
                user.propertyController.ChangeMoveAcceleRate(-digMoveAcceleBonus);
                SetBurrowVisual(user, false);
            }
            RemoveReach();
            if (user != null)
                user.OnRemove.RemoveListener(onRemove);
        };

        onReach = (c, newTile) =>
        {
            if (surfaced || user == null || c != user || newTile == null) return;
            if (newTile.mapPos.x != 0) return;
            var map = MapManage.instance;
            if (map != null && !map.IfInMapRange(newTile.mapPos.x, newTile.mapPos.y)) return;

            surfaced = true;
            RemoveReach();

            user.propertyController.ChangeMoveAcceleRate(-digMoveAcceleBonus);
            SetBurrowVisual(user, false);

             FaceTowardFieldFromColumnZero(user, newTile);
            
            if (user.equipWeapon != null)
                user.equipWeapon.AttackAble = true;

            user.stateController?.ChangeState(StateName.ResumeState);
        };

        UnityAction<Chess> onEnter = null;
        onEnter = _ =>
        {
            user.WhenEnterGame.RemoveListener(onEnter);

            user.UnSelectable();
            if (user.equipWeapon != null)
                user.equipWeapon.AttackAble = false;
            user.propertyController.ChangeMoveAcceleRate(digMoveAcceleBonus);
            SetBurrowVisual(user, true);

            user.moveController.OnReachTile.RemoveListener(onReach);
            user.moveController.OnReachTile.AddListener(onReach);
            user.OnRemove.AddListener(onRemove);
        };

        user.WhenEnterGame.AddListener(onEnter);
    }

    static void SetBurrowVisual(Chess chess, bool inSoil)
    {
        if (chess == null) return;
        if (chess.animatorController is AnimatorController_DuckZombie dz)
        {
            dz.SetForceInWater(inSoil);
            return;
        }
        var anim = chess.animatorController?.animator;
        if (anim != null && AnimatorController.HasParameter(anim, "InWater"))
            anim.SetBool("InWater", inSoil);
    }

    /// <summary>站在 x=0 时朝「场内」：用第 0 列与第 1 列世界 X 关系决定面向 +X 还是 −X，避免 <see cref="Chess.UpdateFacingFromHorizontalMove"/> 因水平差过小或与当前朝向一致而不翻面。</summary>
    static void FaceTowardFieldFromColumnZero(Chess chess, Tile landedOn)
    {
        if (chess == null || landedOn == null) return;
        //var map = MapManage.instance;
        //Vector2Int preTile = landedOn.mapPos - new Vector2Int((int)chess.transform.right.x, 0);
        //Debug.Log("flap");
        //Debug.Log(chess.)
        chess.moveController.Turn();
        //chess.EnsureFacingWorldX(facePositiveX);
    }
}
