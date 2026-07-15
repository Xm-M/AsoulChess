using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 棒球挥棒手被动：默认近战；接到投手喂球后仅本次攻击切远程，打完回近战（D1：无队列）。
/// </summary>
[Serializable]
public class PassiveSkillEffect_BaseballBatter : ISkillEffect
{
    [Tooltip("击球远程子弹预制体")]
    public GameObject battedBullet;

    [SerializeField, Min(0f)]
    float battedDamageRate = 1f;

    [SerializeField, Min(0f)]
    [Tooltip("接力远程攻击距离；0 表示使用喂球投手的攻击距离")]
    float relayAttackRange;

    [SerializeReference]
    [Tooltip("临时远程索敌；为空则用 StraightFindTarget")]
    public IFindTarget rangedFindTarget;

    const float RelayShotTimeout = 3f;
    const float RelayAnimTail = 0.5f;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user == null)
            return;

        bool relayArmed = true;
        IFindTarget savedFindTarget = null;
        IAttackFunction savedAttackFunction = null;
        float savedAttackRange = 0f;
        bool savedAttackRangeValid = false;
        Coroutine waitRestoreRoutine = null;
        UnityAction<Chess> onEnter = null;
        UnityAction<Chess> onRemove = null;

        void RestoreMelee()
        {
            var weapon = user?.equipWeapon?.weapon as Weapon_Sample;
            if (weapon == null)
                return;

            if (savedFindTarget != null)
                weapon.findTarget = savedFindTarget;
            if (savedAttackFunction != null)
                weapon.attackFunction = savedAttackFunction;

            if (savedAttackRangeValid && user?.propertyController != null)
                user.propertyController.SetAtttackRange(savedAttackRange);

            relayArmed = true;
        }

        float ResolveRelayAttackRange(Chess pitcher)
        {
            if (relayAttackRange > 0f)
                return relayAttackRange;
            if (pitcher?.propertyController != null)
                return pitcher.propertyController.GetAttackRange();
            return 30f;
        }

        void ApplyRelayRangedSetup(Chess pitcher)
        {
            var weapon = user.equipWeapon?.weapon as Weapon_Sample;
            if (weapon == null || user?.propertyController == null)
                return;

            if (!savedAttackRangeValid)
            {
                savedAttackRange = user.propertyController.GetAttackRange();
                savedAttackRangeValid = true;
            }

            user.propertyController.SetAtttackRange(ResolveRelayAttackRange(pitcher));

            if (savedFindTarget == null)
                savedFindTarget = weapon.findTarget;
            if (savedAttackFunction == null)
                savedAttackFunction = weapon.attackFunction;

            weapon.findTarget = rangedFindTarget ?? new StraightFindTarget();
            weapon.attackFunction = new BaseballBattedAttack
            {
                bullet = battedBullet,
                damageRate = battedDamageRate
            };
        }

        IEnumerator RelaySwingRoutine()
        {
            var weapon = user?.equipWeapon?.weapon as Weapon_Sample;
            if (weapon == null || user?.equipWeapon == null || user.stateController == null)
            {
                RestoreMelee();
                yield break;
            }

            user.equipWeapon.attackOver = false;
            weapon.FindEnemy(user);
            user.stateController.ChangeState(StateName.AttackState);

            bool shotFired = false;
            UnityAction<Chess> onAttack = _ => shotFired = true;
            user.equipWeapon.OnAttack.AddListener(onAttack);

            float timeout = RelayShotTimeout;
            while (!shotFired && timeout > 0f)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            user.equipWeapon.OnAttack.RemoveListener(onAttack);

            yield return new WaitForSeconds(RelayAnimTail);

            user.equipWeapon.attackOver = true;
            if (user.stateController.currentState?.state != null
                && user.stateController.currentState.state.stateName == StateName.AttackState)
            {
                user.stateController.ChangeState(StateName.IdleState);
            }

            RestoreMelee();
            waitRestoreRoutine = null;
        }

        void OnReceivePitch(Chess pitcher)
        {
            if (user == null || user.IfDeath || pitcher == null || pitcher.IfDeath)
                return;
            if (!relayArmed)
                return;

            if (user.stateController?.currentState?.state != null
                && user.stateController.currentState.state.stateName == StateName.AttackState)
                return;

            var weapon = user.equipWeapon?.weapon as Weapon_Sample;
            if (weapon == null || battedBullet == null)
                return;

            relayArmed = false;
            ApplyRelayRangedSetup(pitcher);

            if (waitRestoreRoutine != null)
                user.StopCoroutine(waitRestoreRoutine);
            waitRestoreRoutine = user.StartCoroutine(RelaySwingRoutine());
        }

        void Cleanup()
        {
            if (waitRestoreRoutine != null && user != null)
            {
                user.StopCoroutine(waitRestoreRoutine);
                waitRestoreRoutine = null;
            }

            BaseballBatterReceiveRegistry.Unregister(user);
            RestoreMelee();

            if (user != null)
            {
                user.WhenEnterGame.RemoveListener(onEnter);
                user.OnRemove.RemoveListener(onRemove);
            }
        }

        BaseballBatterReceiveRegistry.Register(user, OnReceivePitch);

        onEnter = _ =>
        {
            user.WhenEnterGame.RemoveListener(onEnter);
            BaseballBatterReceiveRegistry.Register(user, OnReceivePitch);
        };

        onRemove = _ => Cleanup();

        user.WhenEnterGame.AddListener(onEnter);
        user.OnRemove.AddListener(onRemove);
    }
}
