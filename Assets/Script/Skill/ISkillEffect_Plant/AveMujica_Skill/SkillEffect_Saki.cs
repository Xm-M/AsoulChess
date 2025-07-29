using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;
/// <summary>
/// 
/// </summary>
public class SkillEffect_Saki : ISkill
{
    public float skillRange=8.75f;
    [LabelText("魅惑次数")]
    public int useTimes=4;
    [LabelText("触发频率")]
    public float frequency=5f;
    [SerializeReference]
    [LabelText("魅惑")]
    public Buff_Charm buffCharm;
    [SerializeReference]
    [LabelText("威压")]
    public Buff_Coercion buffCoercion;
    [LabelText("恐惧")]
    [SerializeReference]
    public Buff_Fear fearBuff;
    [LabelText("魅惑子弹")]
    public GameObject bullet;
    [LabelText("saki哭")]
    public AudioPlayer auplayer;
    [LabelText("丰川清告")]
    public PropertyCreator qinggao;
    float t;
    int tChange;
    public bool IfSkillReady(Chess user)
    {
        //throw new System.NotImplementedException();
        t += Time.deltaTime;

        List<Chess> team = ChessTeamManage.Instance.GetTeam(user.tag);
        for(int i = 0; i < team.Count; i++)
        {
            if (team[i].propertyController.creator.chessName == "丰川清告")
            {
                user.animatorController.ChangeFloat("black", 1);
                tChange = -1;
                return true;    
            }
        }

        if (t > frequency&&tChange>0)
        {
            t = 0;
 
            return true;
        }
        else if (tChange == 0)
        {
            user.animatorController.ChangeFloat("black", 1);
            //其实是在这里生成一个丰川清告
            List<Tile> nears = MapManage.instance.NearTile(user.moveController.standTile);
            Debug.Log(nears.Count);
            for (int i = 0; i < nears.Count; i++)
            {
                if (nears[i].stander == null && qinggao.IfCanPlant(nears[i])) {
                    Chess lcat = ChessTeamManage.Instance.CreateChess(qinggao, nears[i], user.tag);
                    tChange -= 1;
                    break;
                }
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
        if (tChange > 0)
        {
            Collider2D[] cols = CheckObjectPoolManage.GetColArray(100);
            int n = Physics2D.OverlapCircleNonAlloc(user.transform.position, skillRange, cols,
                ChessTeamManage.Instance.GetEnemyLayer(user.gameObject));
            if (n == 0) return;
            int i = Random.Range(0, n);
            //把这一部分改成魅惑子弹 舒服 怎么改啊 
            Chess targert = cols[i].GetComponent<Chess>(); 
            GameObject b = ObjectPool.instance.Create(bullet);
            Bullet zidan = b.GetComponent<Bullet>();
            zidan.InitBullet(user, user.equipWeapon.weaponPos.position, targert, user.transform.right);
            zidan.Dm.damageTo = targert;
            zidan.Dm.takeBuff = buffCharm.Clone();
            tChange -= 1;
            CheckObjectPoolManage.ReleaseColArray(100, cols);
        }
        else
        {
            auplayer.Play();
            user.UnSelectable();
            Fear(user);
            user.stateController.ChangeState(StateName.MoveState);
            user.moveController.standTile.ChessLeave(user);
        }
        
    }
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
            c.buffController.AddBuff(buffCoercion);
        }
    }
    

    public void WhenEnter(Chess user)
    {
        t = 0;
        tChange = useTimes;
    }
}
public class Passive_SakiFather : ISkill
{
    public float deathTime;
    Timer timer;
    public bool IfSkillReady(Chess user)
    {
        //throw new System.NotImplementedException();
        return false; 
    }

    public void InitSkill(Chess user)
    {
         
    }

    public void LeaveSkill(Chess user)
    {
        timer.Stop();
        timer = null;
    }

    public void UseSkill(Chess user)
    {
        user.UnSelectable();
        timer = GameManage.instance.timerManage.AddTimer(() => user.Death(), deathTime);
    }

    

    public void WhenEnter(Chess user)
    {
         
    }
}
