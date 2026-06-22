using System;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

/// <summary>
/// sumimi 乐队羁绊：选卡含 <see cref="SumimiMemberIds"/> 全员时激活；
/// <see cref="DonutProducerMemberId"/> 释放主动技能时向友军发射甜甜圈子弹（优先最近的 <see cref="SkillOnAttackMemberId"/>），
/// 命中初华时在目标格生成阳光；<see cref="SkillOnAttackMemberId"/> 释放主动技能时触发 <see cref="AttackController.OnAttack"/>。
/// </summary>
public class Sumimi : Fetter
{
    public const string SumimiPlantTag = "sumimi";
    public const string FetterNameKey = "sumimi";
    public const string DonutProducerMemberId = "纯田真奈";
    public const string SkillOnAttackMemberId = "三角初华";

    public static readonly string[] SumimiMemberIds = { "三角初华", "纯田真奈" };

    [LabelText("甜甜圈子弹预制体")]
    [Tooltip("需含 Bullet_SumimiDonut + BulletMove_Follow，并在对象池可用")]
    public GameObject donutBulletPrefab;

    [SerializeReference]
    [Tooltip("种植 sumimi 成员时施加，用于监听 onUseSkill")]
    public Buff_Sumimi buffTemplate;

    public static Sumimi Instance { get; private set; }

    public override void FetterEffect(int count, int tier)
    {
        base.FetterEffect(count, tier);
        Instance = this;
        EventController.Instance.AddListener<Chess>(EventName.WhenPlantChess.ToString(), OnPlantChess);
    }

    public override void ResetFetter()
    {
        base.ResetFetter();
        EventController.Instance.RemoveListener<Chess>(EventName.WhenPlantChess.ToString(), OnPlantChess);
        if (Instance == this)
            Instance = null;
    }

    void OnPlantChess(Chess chess)
    {
        if (chess == null || buffTemplate == null || !IsSumimiMember(chess))
            return;
        chess.buffController.AddBuff(buffTemplate);
    }

    /// <summary>由 <see cref="Buff_Sumimi"/> 在成员释放主动技能时调用。</summary>
    public void OnSumimiMemberUsedSkill(Chess user)
    {
        if (user == null)
            return;
        if (IsDonutProducer(user))
            ShootDonutBullet(user);
        if (IsSkillOnAttackMember(user))
            TriggerSkillOnAttack(user);
    }

    static void TriggerSkillOnAttack(Chess user)
    {
        user.equipWeapon?.OnAttack?.Invoke(user);
    }

    void ShootDonutBullet(Chess user)
    {
        if (donutBulletPrefab == null || ObjectPool.instance == null)
            return;

        Chess target = PickDonutAllyTarget(user);
        if (target == null)
            return;

        Vector3 spawnPos = user.equipWeapon?.weaponPos != null
            ? user.equipWeapon.weaponPos.position
            : user.transform.position;

        GameObject go = ObjectPool.instance.Create(donutBulletPrefab);
        if (go == null)
            return;

        var bullet = go.GetComponent<Bullet_SumimiDonut>();
        if (bullet == null)
        {
            ObjectPool.instance.Recycle(go);
            return;
        }

        if (bullet.Dm == null)
            bullet.Dm = new DamageMessege();

        Vector2 dir = (Vector2)(target.transform.position - spawnPos);
        if (dir.sqrMagnitude < 0.0001f)
            dir = user.transform.right;

        bullet.InitBullet(user, spawnPos, target, dir.normalized, 0f, 1f);
        bullet.Dm.damageTo = target;
        bullet.Dm.damageType = DamageType.Miss;
        bullet.Dm.damage = 0;
    }

    /// <summary>优先最近的三角初华，否则最近的任意友军（不含自身）。</summary>
    Chess PickDonutAllyTarget(Chess user)
    {
        var team = ChessTeamManage.Instance?.GetTeam(user.tag);
        if (team == null)
            return null;

        Chess nearestChihaya = null;
        float nearestChihayaDist = float.MaxValue;
        Chess nearestAlly = null;
        float nearestAllyDist = float.MaxValue;
        Vector3 from = user.transform.position;

        for (int i = 0; i < team.Count; i++)
        {
            Chess c = team[i];
            if (c == null || c.IfDeath || c == user)
                continue;

            float d = (c.transform.position - from).sqrMagnitude;
            if (IsSkillOnAttackMember(c) && d < nearestChihayaDist)
            {
                nearestChihayaDist = d;
                nearestChihaya = c;
            }

            if (d < nearestAllyDist)
            {
                nearestAllyDist = d;
                nearestAlly = c;
            }
        }

        return nearestChihaya ?? nearestAlly;
    }

    public static bool IsSumimiMember(Chess chess)
    {
        if (chess == null || !chess.CompareTag("Player"))
            return false;
        return IsSumimiMember(chess.propertyController?.creator);
    }

    public static bool IsSumimiMember(PropertyCreator creator)
    {
        if (creator == null)
            return false;
        if (creator.plantTags != null && creator.plantTags.Contains(SumimiPlantTag))
            return true;
        string mid = string.IsNullOrEmpty(creator.fetterMemberId) ? creator.chessName : creator.fetterMemberId;
        if (string.IsNullOrEmpty(mid))
            return false;
        foreach (string id in SumimiMemberIds)
        {
            if (mid == id)
                return true;
        }
        return false;
    }

    public static bool IsDonutProducer(Chess chess)
    {
        if (chess == null || !chess.CompareTag("Player"))
            return false;
        return IsDonutProducer(chess.propertyController?.creator);
    }

    public static bool IsDonutProducer(PropertyCreator creator)
    {
        if (creator == null)
            return false;
        string mid = string.IsNullOrEmpty(creator.fetterMemberId) ? creator.chessName : creator.fetterMemberId;
        return mid == DonutProducerMemberId;
    }

    public static bool IsSkillOnAttackMember(Chess chess)
    {
        if (chess == null || !chess.CompareTag("Player"))
            return false;
        return IsSkillOnAttackMember(chess.propertyController?.creator);
    }

    public static bool IsSkillOnAttackMember(PropertyCreator creator)
    {
        if (creator == null)
            return false;
        string mid = string.IsNullOrEmpty(creator.fetterMemberId) ? creator.chessName : creator.fetterMemberId;
        return mid == SkillOnAttackMemberId;
    }
}

/// <summary>sumimi 成员在场时：真奈放技能发射甜甜圈，初华放技能触发 OnAttack。</summary>
[Serializable]
public class Buff_Sumimi : Buff
{
    UnityAction<Chess> _onUseSkill;

    public Buff_Sumimi()
    {
        buffName = "Buff_Sumimi";
    }

    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        if (target?.skillController == null)
            return;
        _onUseSkill = OnUseSkill;
        target.skillController.onUseSkill.AddListener(_onUseSkill);
    }

    void OnUseSkill(Chess user)
    {
        if (user != target)
            return;
        Sumimi.Instance?.OnSumimiMemberUsedSkill(user);
    }

    public override void BuffOver()
    {
        if (target?.skillController != null && _onUseSkill != null)
            target.skillController.onUseSkill.RemoveListener(_onUseSkill);
        _onUseSkill = null;
        base.BuffOver();
    }
}
