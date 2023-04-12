using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Skill_WildAngry", menuName = "Skill/Skill_WildAngry")]

public class Skill_WildAngry : Skill
{
    [SerializeField] private float extraAttackSpeed=0.1f;
    public GameObject PowerUp;
    public override void SkillEffect(Chess user, params Chess[] target)
    {
        Debug.Log("攻速增加了");
        //user.property.SetCurrentValue(ValueType.AttackSpeed, extraAttackSpeed);
        if(PowerUp != null)
            ObjectPool.instance.Create(PowerUp).transform.position=user.transform.position;
    }
}
