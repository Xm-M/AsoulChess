using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
/// <summary>
/// 
/// </summary>
public class SkillEffect_Saki : ISkill
{
    public float skillRange=8.75f;
    [LabelText("×ª»»ÀäÈ´")]
    public float cold=30f;
    [LabelText("´¥·¢ÆµÂÊ")]
    public float frequency=5f;
    [SerializeReference]
    [LabelText("÷È»ó")]
    public Buff_Charm buffCharm;
    [SerializeReference]
    [LabelText("ÍþÑ¹")]
    public Buff_Coercion buffCoercion;
    [LabelText("¿Ö¾å")]
    [SerializeReference]
    public Buff_Fear fearBuff;
    float t;
    float tChange;
    public bool IfSkillReady(Chess user)
    {
        //throw new System.NotImplementedException();
        t += Time.deltaTime;
        if (t > frequency)
        {
            t = 0;
            tChange -= frequency;
            if (tChange == 0)
            {
                //×ª»»ÐÎÌ¬ 
                user.animatorController.ChangeFloat("black",1);
            }
            return true;
        }
        return false;
    }
    public void InitSkill(Chess user)
    {
         
    }
    public void LeaveSkill(Chess user)
    {
         
    }
    public void UseSkill(Chess user)
    {
        Collider2D[] cols = CheckObjectPoolManage.GetColArray(100);
        int n = Physics2D.OverlapCircleNonAlloc(user.transform.position, skillRange, cols,
            ChessTeamManage.Instance.GetEnemyLayer(user.gameObject));
        if (n == 0) return;
        if (tChange>0) {
            int i = Random.Range(0, n);
            cols[i].GetComponent<Chess>().buffController.AddBuff(buffCharm);
            Debug.Log("÷È»ó");
        }
        else
        {
            int i = 0;
            for (; i < n; i++)
            {
                cols[i].GetComponent<Chess>().buffController.AddBuff(buffCoercion);
            }
            i= Random.Range(0, n);
            cols[i].GetComponent<Chess>().buffController.AddBuff(fearBuff);
        }
    }

    public void WhenEnter(Chess user)
    {
        t = 0;
        tChange = cold;
    }
}
