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
public class Buff_Zombie_BullyBuff : Buff
{
    public GameObject Effect;
    public float extraDamageRate;
    //public Buff_BaseValueBuff Buff_BaseValueBuff;
    GameObject effect;

    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        if (Effect != null)
        {
            effect= ObjectPool.instance.Create(Effect);
            effect.transform.SetParent(target.transform);
            effect.transform.localPosition = Vector3.zero;
        }
        target.propertyController.ChangeExtraDefence(-extraDamageRate);
    }
    public override void BuffOver()
    {
        base.BuffOver();
        if (effect != null)
        {
            ObjectPool.instance.Recycle(effect);
        }
        target.propertyController.ChangeExtraDefence(+extraDamageRate);
    }
}
