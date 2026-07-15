using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 棒球投手被动：敌人门控通过且攻击范围内有挥棒手时喂球（<see cref="FindTarget_BaseballPitcherRelay"/> + <see cref="BaseballPitchAttack"/>）。
/// </summary>
[Serializable]
public class PassiveSkillEffect_BaseballPitcher : ISkillEffect
{
    [Tooltip("喂球用 Bullet_PitchBall 预制体；为空则复用 ShootBullet.bullet")]
    public GameObject pitchBullet;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user == null)
            return;

        IFindTarget savedFind = null;
        IAttackFunction savedAttack = null;
        UnityAction<Chess> onEnter = null;
        UnityAction<Chess> onRemove = null;

        void RestoreWeapon()
        {
            var weapon = user?.equipWeapon?.weapon as Weapon_Sample;
            if (weapon == null)
                return;
            if (savedFind != null)
                weapon.findTarget = savedFind;
            if (savedAttack != null)
                weapon.attackFunction = savedAttack;
        }

        void ApplyRelay()
        {
            var weapon = user.equipWeapon?.weapon as Weapon_Sample;
            if (weapon == null)
                return;

            IFindTarget originalFind = weapon.findTarget;
            if (originalFind is FindTarget_BaseballPitcherRelay existingRelay)
                originalFind = existingRelay.enemyFindTarget;

            if (savedFind == null)
                savedFind = originalFind;

            weapon.findTarget = new FindTarget_BaseballPitcherRelay
            {
                enemyFindTarget = savedFind
            };

            if (savedAttack == null)
                savedAttack = weapon.attackFunction;

            GameObject enemyBullet = null;
            if (savedAttack is ShootBullet shoot)
                enemyBullet = shoot.bullet;

            var pitchAttack = new BaseballPitchAttack
            {
                enemyBullet = enemyBullet,
                pitchBullet = pitchBullet
            };
            weapon.attackFunction = pitchAttack;
        }

        onEnter = _ =>
        {
            user.WhenEnterGame.RemoveListener(onEnter);
            ApplyRelay();
        };

        onRemove = _ =>
        {
            RestoreWeapon();
            if (user != null)
                user.OnRemove.RemoveListener(onRemove);
        };

        ApplyRelay();
        user.WhenEnterGame.AddListener(onEnter);
        user.OnRemove.AddListener(onRemove);
    }
}
