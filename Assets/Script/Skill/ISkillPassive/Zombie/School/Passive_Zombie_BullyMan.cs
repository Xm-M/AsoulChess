using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Passive_Zombie_BullyMan : ISkillEffect
{
    [SerializeReference]
    public Buff_Zombie_BullyBuff bullyBuff;
    public float checkRange=25;
    Timer Check;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        Check = GameManage.instance.timerManage.AddTimer(() => FindTarget(user ), 0.1f, true);
        user.OnRemove.AddListener((chess) => Check.Stop());
    }
    public void FindTarget(Chess user )
    {
        Chess target = null;
        user.skillController.context.TryGet<Chess>("霸凌目标", out target);
        //Debug.Log("寻找目标");
        if (target == null || target.IfDeath)
        {
            //findTarget.FindTarget(user,targets);
            RaycastHit2D[] hits = CheckObjectPoolManage.GetHitArray(100 );

            int num = Physics2D.RaycastNonAlloc(user.transform.position, user.transform.right,
                hits, checkRange, GameManage.instance.chessTeamManage.GetEnemyLayer(user.gameObject));

            if (num > 0)
            {
                int n = UnityEngine.Random.Range(0, num);
                target = hits[n].collider.GetComponent<Chess>();
               target.buffController.AddBuff(bullyBuff);
                user.skillController.context.Set<Chess>("霸凌目标", target);
                //Debug.Log("霸凌目标" + target.name);
            }

            CheckObjectPoolManage.ReleaseArray(100, hits);
           
        }
    }
}
/// <summary>
/// 霸凌 Buff：ExtraDefence(负值=额外承伤) + 特效
/// </summary>
public class Buff_Zombie_BullyBuff : Buff
{
    public GameObject Effect;
    [SerializeReference] public Buff_BaseValueBuff_ExtraDefence extraDefenceBuff;
    [UnityEngine.Serialization.FormerlySerializedAs("extraDamageRate")] public float _extraDamageRate;
    GameObject effect;
    void EnsureBuffs()
    {
        if (extraDefenceBuff == null) extraDefenceBuff = new Buff_BaseValueBuff_ExtraDefence { extraDefence = -_extraDamageRate };
    }
    protected override void PrepareForRestore() => EnsureBuffs();
    public override void BuffEffect(Chess target)
    {
        EnsureBuffs();
        base.BuffEffect(target);
        if (Effect != null)
        {
            effect = ObjectPool.instance.Create(Effect);
            effect.transform.SetParent(target.transform);
            effect.transform.localPosition = Vector3.zero;
        }
        extraDefenceBuff.target = target;
        extraDefenceBuff.BuffEffect(target);
    }
    public override void BuffOver()
    {
        if (extraDefenceBuff != null) extraDefenceBuff.BuffOver();
        if (effect != null) { ObjectPool.instance.Recycle(effect); effect = null; }
        base.BuffOver();
    }
}
