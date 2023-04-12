using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill_ElfHeal", menuName = "Skill/Skill_ElfHeal")]
public class Skill_ElfHeal : Skill
{
    [SerializeField] private float healNum;
    public GameObject healEffect;
    public override void SkillEffect(Chess user, params Chess[] target)
    {
        // foreach(var chess in GameManage.instance.teams[user.tag].team)
        // {
        //     EventController.Instance.TriggerEvent<float>(user.instanceID.ToString()+EventName.WhenHealOther.ToString(),healNum);
        //     chess.property.Heal(healNum);
        //     if (healEffect != null)
        //     {
        //         ObjectPool.instance.Create(healEffect).transform.position = chess.transform.position;
        //     }
        // }
    }
}
