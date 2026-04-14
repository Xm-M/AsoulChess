using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;
/// <summary>
/// 大祥的普攻变成魅惑 攻击三次后使用技能
/// </summary>
public class SkillEffect_Saki : ISkillEffect
{ 
    [LabelText("恐惧")]
    [SerializeReference]
    public Buff_Fear fearBuff;
 
    [LabelText("saki哭")]
    public AudioPlayer auplayer;
    [LabelText("丰川清告")]
    public PropertyCreator qinggao;
 
    public void Fear(Chess user)
    {
        int remain = 4;
        var priorityNames = new HashSet<string> { "高松灯", "长崎素世", "若叶睦", "椎名立希" };

        // 拷贝队友列表，避免直接改原列表
        var friends = new List<Chess>(ChessTeamManage.Instance.GetTeam(user.gameObject.tag));

        // ---- 1. 先按名字优先挑选 ----
        for (int i = friends.Count - 1; i >= 0 && remain > 0; --i)
        {
            var friend = friends[i];
            if (friend == null) { friends.RemoveAt(i); continue; }
            if (friend.propertyController.creator.chessName == user.propertyController.creator.chessName)
            {
                friends.RemoveAt(i);
                continue;
            }
            // 逐个优先名匹配
            foreach (var name in priorityNames.ToList())   // ToList 避免枚举时修改集合
            {
                if (friend.propertyController?.creator?.chessName?.Contains(name) == true)
                {
                    ApplyFear(friend);
                    priorityNames.Remove(name);   // 该名字已选
                    friends.RemoveAt(i);          // 从待选池移除
                    --remain;
                    break;                        // 不再检查其它优先名
                }
            }
        }
        if (remain == 0 || friends.Count == 0) return;
        // ---- 2. 从剩余队友里随机挑选 ----
        var shuffled = friends.OrderBy(_ => Random.value).Take(remain);
        foreach (var friend in shuffled)
        {
            if (friend == null) continue;
            ApplyFear(friend);
        }

        // 内部函数：给棋子上两个 Buff 
        void ApplyFear(Chess c)
        {
            c.buffController.AddBuff(fearBuff);
        }
    }

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        auplayer.Play();
        user.UnSelectable();
        Fear(user);
        user.stateController.ChangeState(StateName.MoveState);
        user.moveController.standTile.ChessLeave(user);
    }
}

