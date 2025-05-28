using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
/// <summary>
/// 9’≈≈∆  
/// </summary>
public class SkillEffect_Explode : ISkill
{

    [SerializeReference]
    [LabelText("—∞µ–∑Ω Ω")]
    public IFindTarget findTarget;
    [LabelText("±¨’®…À∫¶±∂¬ ")]
    public float explodeRate;
    [LabelText(" Õ∑≈—”≥Ÿ")]
    public float coldDown;//CD 
    List<Chess> targets;
    float t;
    public bool IfSkillReady(Chess user)
    {
        t += Time.deltaTime;
        if (t > user.propertyController.GetColdDown(coldDown))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public void InitSkill(Chess user)
    {
        t = 0;
        targets = new List<Chess>();
    }

    public void LeaveSkill(Chess user)
    {
        t = 0;
    }
    public void UseSkill(Chess user)
    {
        findTarget.FindTarget(user, targets);
        for (int i = 0; i < targets.Count; i++)
        {
            user.skillController.DM.damageFrom = user;
            user.skillController.DM.damageTo = targets[i];
            user.skillController.DM.damageElementType = ElementType.Explode;
            user.skillController.DM.damage =
                user.propertyController.GetAttack() * explodeRate;
            user.propertyController.TakeDamage(user.skillController.DM);
        }
    } 
    public void WhenEnter(Chess user)
    {
        t = 0;
        targets.Clear();
    }
}
