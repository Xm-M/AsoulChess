using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

/// <summary>
/// 我感觉我这个buff系统写的也很垃圾  
/// </summary>
#region 基础框架buff
[Serializable]
public abstract class Buff
{
    [LabelText("Buff名")]
    public string buffName;//这个buff的名字
    [HideInInspector] public Chess target;//buff的作用对象

    public virtual void BuffReset(Buff resetBuff)
    {
    }

    public virtual void BuffEffect(Chess target)
    {
        this.target = target;
    }
    public virtual void BuffOver()
    {
        target.buffController.RemoveBuff(this);
    }
    public virtual Buff Clone()
    {
        return (Buff)this.MemberwiseClone();
    }
}
public class TimeBuff : Buff
{
    public float continueTime;
    protected Timer timer;
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        timer = GameManage.instance.timerManage.AddTimer(BuffOver, continueTime, false);
    }
    public override void BuffReset(Buff resetBuff)
    {
        continueTime = (resetBuff as TimeBuff).continueTime;
        //Debug.Log(continueTime);
        timer.ResetTime(continueTime);
    }
    public override void BuffOver()
    {
        base.BuffOver();
        timer?.Stop();
        timer = null;
    }
}
public class MultyBuff : Buff
{
    [SerializeReference]
    public List<Buff> buffs;
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        foreach(var buff in buffs){
            target.buffController.AddBuff(buff);
        }
        BuffOver();
    }

}
#endregion
#region 元素属性buff
/// <summary>
/// 减速Buff
/// </summary>
public class ColdBuff : TimeBuff
{
    [LabelText("减速效率")]
    public float slowRate = -0.5f;
    public GameObject coldBuff;
    public ColdBuff()
    {
        buffName = "冰冻";
    }
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);

        if (this.coldBuff != null)
        {
            GameObject cold = ObjectPool.instance.Create(coldBuff);
            cold.transform.position = target.transform.position;
        }
        target.propertyController.ChangeAcceleRate(slowRate);
        target.animatorController.ChangeColor(Color.blue);
        
    }
    public override void BuffOver()
    {
        base.BuffOver();
        target.propertyController.ChangeAcceleRate(-slowRate);
        target.animatorController.ChangeColor(Color.white);
    }
}
public class FireBuff : Buff
{
    public float damage = 13;//固定值 而且会被均摊
    public FireBuff()
    {
        buffName = "灼烧";
    }
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        
    }
}
public class LightBuff : Buff
{
    [LabelText("光照范围")]
    public float range;
    public float hideTime;

    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        if(Effect_Smoke.Instance!=null)
        Effect_Smoke.Instance.HideSmoke(target.transform.position, range, hideTime);
        BuffOver();
    }
    public LightBuff()
    {
        buffName = "光照";
    }
}

#endregion 
#region 控制buff
/// <summary>
/// 现在重点是恐惧的回头走两步要怎么实现...
/// </summary>
public class Buff_Fear : TimeBuff {

    [LabelText("恐惧特效")]
    public GameObject FearEffect;
    [LabelText("缓速效率")]
    public float moveRate=0.5f;
    GameObject effect;
    StateName current;
    Tile stand;
    public override void BuffEffect(Chess target)
    {
        //continueTime = target.propertyController.GetDizznessTime();
        base.BuffEffect(target);
        current = target.stateController.currentState.state.stateName;
        
        effect = ObjectPool.instance.Create(FearEffect);
        effect.transform.SetParent(target.transform);
        effect.transform.localPosition = Vector3.zero;
        //target.stateController.ChangeState(StateName.DizzyState);
        target.propertyController.ChangeDizznessTime(continueTime);
        //timer = GameManage.instance.timerManage.AddTimer(BuffOver, , false);
        //stand = target.moveController.nextTile;
        //if (target.moveController.standTile != null&&target.propertyController.GetMoveSpeed()!=0) {
        //    float speed = target.propertyController.GetMoveSpeed()*moveRate;
        //    target.moveController.MoveToTarget(target.moveController.standTile,speed
        //         );
        //    target.animatorController.PlayMove();
        //    target.transform.right=-target.transform.right;
        //}
        //else
        //{
        //    target.animatorController.PlayIdle();
        //}
        target.moveController.Turn();
    }
    public override void BuffReset(Buff resetBuff)
    {
        base.BuffReset(resetBuff);
        target.propertyController.ChangeDizznessTime(continueTime);
    }
    public override void BuffOver()
    {
        base.BuffOver();
        //effect.transform.SetParent(null);
        ObjectPool.instance.Recycle(effect);
        //target.stateController.ChangeState(current);
        //if (target.moveController.standTile != null && target.propertyController.GetMoveSpeed() !=0) {
        //    target.moveController.StopMove();
        //    target.transform.right = -target.transform.right;
        //    target.moveController.nextTile = stand; 
        //}
        target.moveController.Turn();
    }
}




/// <summary>
/// 魅惑buff
/// </summary>
public class Buff_Charm : Buff
{
    public Color color;
    public GameObject charmEffect;//魅惑特效
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        ChessTeamManage.Instance.ChangeTeam(target);
        target.transform.right = -target.transform.right;
        //dm.damageTo.Death();
        GameObject effect = ObjectPool.instance.Create(charmEffect);
        effect.transform.SetParent(target.transform);
        effect.transform.localPosition = Vector3.zero;
        target.animatorController.ChangeColor(color);
        target.StartCoroutine(Wait());
    }
    IEnumerator Wait()
    {
        yield return null;
        target.stateController.ChangeState(StateName.IdleState);
    }
    public override void BuffOver()
    {
        base.BuffOver();
    }
}


/// <summary>
/// 愤怒Buff
/// </summary>
public class AngryBuff:Buff
{
    [LabelText("生气特效")]
    public GameObject angryEffect;
    [LabelText("额外攻速")]
    public float extraAttackSpeed=0.5f;
    [LabelText("额外承伤")]
    public float extraTake=1f;
    GameObject effect;
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        target.propertyController.ChangeExtraDefence(-extraTake);
        target.propertyController.ChangeAcceleRate(extraAttackSpeed);
        effect = ObjectPool.instance.Create(angryEffect);
        effect.transform.SetParent(target.transform);
        effect.transform.localPosition = Vector3.zero;
    }
    public override void BuffOver()
    {
        base.BuffOver();
        target.propertyController.ChangeExtraDefence(extraTake);
        target.propertyController.ChangeAcceleRate(-extraAttackSpeed);
        ObjectPool.instance.Recycle(effect);
    }
    public override void BuffReset(Buff resetBuff)
    {
        base.BuffReset(resetBuff);
        
    }
}

/// <summary>
/// 缴械buff
/// </summary>
public class DisarmBuff : Buff
{
    public GameObject disaarnEffect;//缴械特效
    public DisarmBuff()
    {
        buffName = "缴械";
    }
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        target.equipWeapon.AttackAble = false;
    }
    public override void BuffOver()
    {
        base.BuffOver();
        target.equipWeapon.AttackAble = true;
    }

}
#endregion
#region 治疗类buff
/// <summary>
/// 恢复buff
/// </summary>
public class ResumeBuff : Buff
{
    protected Timer timer;
    public float healPercent = 0.05f;
    public float healRate = 1;
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        float value = 0.5f + target.propertyController.GetHpPerCent();
        target.animatorController.ChangeColor(new Color(1, 1, 1, value));
        timer = GameManage.instance.timerManage.AddTimer(Heal, healRate, true);
         
    }
    public override void BuffOver()
    {
        base.BuffOver();
        timer.Stop();
        timer = null;
        target.animatorController.ChangeColor(Color.white);
    }
    public void Heal()
    {
        target.propertyController.Heal(target.propertyController.GetMaxHp() * healPercent);
        float value = 0.5f + target.propertyController.GetHpPerCent();
        target.animatorController.ChangeColor(new Color(1, 1, 1, value));
        if (target.propertyController.GetHpPerCent() >= 0.999)
        {
            BuffOver();
        }
    }
}
#endregion
#region 场地Buff
/// <summary>
/// 持续10s 上课buff
/// </summary>
public class Buff_ClassBegin : TimeBuff
{
    [LabelText("额外减伤")]
    public float extradefence=0.3f;
    [LabelText("减少移速")]
    public float extraSpeed=0.25f;
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        target.propertyController.ChangeExtraDefence(extraSpeed);
        target.propertyController.ChangeAcceleRate(-extraSpeed);
        Buff buff = null;
        target.buffController.buffDic.TryGetValue("下课",out buff);
        if(buff != null)
        {
            buff.BuffOver();
        }
         
    }
    public override void BuffReset(Buff resetBuff)
    {
        base.BuffReset(resetBuff);
    }
    public override void BuffOver()
    {
        base.BuffOver();
        target.propertyController.ChangeExtraDefence(-extraSpeed);
        target.propertyController.ChangeAcceleRate(extraSpeed);
    }
}
/// <summary>
/// 下课buff 持续50s
/// </summary>
public class Buff_ClassOver : TimeBuff
{
    [LabelText("额外攻击")]
    public float extraAttack=0.3f;
    [LabelText("额外移速")]
    public float extraSpeed=0.25f;
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        target.propertyController.ChangeAttack(extraAttack);
        target.propertyController.ChangeAcceleRate(extraSpeed);
        Buff buff = null;
        target.buffController.buffDic.TryGetValue("上课", out buff);
        if (buff != null)
        {
            buff.BuffOver();
        }
         
    }
    public override void BuffOver()
    {
        base.BuffOver();
    }
    public override void BuffReset(Buff resetBuff)
    {
        base.BuffReset(resetBuff);
        target.propertyController.ChangeAttack(-extraAttack);
        target.propertyController.ChangeAcceleRate(-extraSpeed);
    }
}

#endregion


/// <summary>
/// 这个buff的作用就是创建一个衍生物
/// </summary>
public class Buff_Create:Buff
{
    public PropertyCreator creator;
    Chess user;
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        user = target;
        PrePlantImage_Data data = new PrePlantImage_Data();
        data.creator = creator;
        data.preSprite = creator.chessSprite;
        data.tag=target.tag;
        PrePlantImage.instance.TryToPlant(CancelPlant,PlantOver,data,HandItemType.Plants);
    }

    public void CancelPlant()
    {
        user.skillController.activeSkill.ReturnCD();
        BuffOver();
    }
    public void PlantOver(Chess target)
    {
        //Debug.Log("创建了 "+target); 为什么第二次用这个技能的时候会秒种呢 而且还种下去了 hyw呢
        //debug
        user.skillController.context.Set<Chess>(buffName, target);
        target.OnRemove.AddListener(OnPlantDeath);
        BuffOver();
    }
    public void OnPlantDeath(Chess target)
    {
        //Debug.Log("移除了" + target.name);
        user.skillController.context.Remove(buffName);
    }
    public override void BuffReset(Buff resetBuff)
    {
        base.BuffReset(resetBuff);
        Debug.LogError("不对，你怎么能同时拥有两个这个buff");
    }
    public override void BuffOver()
    {
        base.BuffOver();

    }
}
