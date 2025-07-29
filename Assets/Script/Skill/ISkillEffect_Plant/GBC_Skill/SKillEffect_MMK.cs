using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
/// <summary>
/// MMK每5次攻击会使用一次技能
/// 只要attack—>skilllState的时候记得带上是否播放完毕的就好
/// </summary>
public class SKillEffect_MMK : ISkill
{
    [LabelText("攻击间隔")]
    public int attackIndex;
    [LabelText("坚毅buff")]
    public string buffName;
    int current;//当前攻击


    public bool IfSkillReady(Chess user)
    {
        //throw new System.NotImplementedException();
        if (current > attackIndex)
        {
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
        RaycastHit2D[] hits = CheckObjectPoolManage.GetHitArray(100);
        int enemyLayer = ChessTeamManage.Instance.GetEnemyLayer(user.gameObject);
        int friendLayer = 1 << user.gameObject.layer; // 将 Layer Index 转换成 LayerMask

        int combinedLayerMask = enemyLayer | friendLayer; // 同时检测敌我

        int n = Physics2D.RaycastNonAlloc(
            user.transform.position,
            user.transform.right,
            hits,
            user.propertyController.GetAttackRange(),
            combinedLayerMask
        );

        for (int i = 0; i < n; i++)
        {
            Chess enemy = hits[i].collider.GetComponent<Chess>();
            if (enemy != null && enemy.buffController.buffDic.TryGetValue(buffName, out Buff baseBuff))
            {
                Buff_MMKFirm buff = baseBuff as Buff_MMKFirm;
                if (buff != null)
                {
                    buff.TriggerBuffEffect();
                }
            }
        }

        CheckObjectPoolManage.ReleaseArray(100, hits);
    }

    public void WhenEnter(Chess user)
    {
        //throw new System.NotImplementedException();
        user.equipWeapon.OnAttack.AddListener(OnAttack);
    }
    public void OnAttack(Chess chess)
    {
        current++;
    }
}
