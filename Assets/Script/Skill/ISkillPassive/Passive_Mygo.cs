using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 所有Mygo的被动
/// 思考一下要怎么写好吧
/// 怎么检测四个呢？
/// </summary>
public class Passive_Mygo : ISkill
{
    protected Chess user;
    //List<string> members =new List<string>{ "长崎素世", "高松灯", "椎名立希", "要乐奈","千早爱音" };
    public EggController eggcontroller;
    protected List<string> nearChess;
    //int[] dx =[]
    public bool IfSkillReady(Chess user)
    {
        return false;
    }

    public void InitSkill(Chess user)
    {
        this.user = user;
        nearChess = new List<string>();
        eggcontroller?.InitEggs(user);
    }

    public virtual void LeaveSkill(Chess user)
    {
        eggcontroller?.WhenLeaveWar();
        EventController.Instance.RemoveListener<Chess>(EventName.WhenPlantChess.ToString(), Check);
        EventController.Instance.RemoveListener<Chess>(EventName.WhenDeath.ToString(), Check);

    }

    public virtual void UseSkill(Chess user)
    {
        eggcontroller?.WhenEnterWar();
        user.propertyController.SetAtttackRange(0);
        EventController.Instance.AddListener<Chess>(EventName.WhenPlantChess.ToString(), Check);
        EventController.Instance.AddListener<Chess>(EventName.WhenDeath.ToString(), Check);
        Check(user);
    }

    public void WhenEnter(Chess user)
    {
         
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="chess"></param>
    public virtual void CheckFriend(Chess chess)
    {
        int[] dx = new int[4] { 0, 0, 1, -1 };
        Tile tile = user.moveController.standTile;
        nearChess.Clear();
        bool triggerEgg=false;
        for(int i=0;i<dx.Length; i++)
        {
            //Debug.Log(tile.mapPos.x + dx[i]+"  "+tile.mapPos.y + dx[3 - i]);
            if(!MapManage.instance.IfInMapRange(tile.mapPos.x + dx[i], tile.mapPos.y + dx[3 - i]))
            {
                continue;
            }
            Tile near = MapManage_PVZ.instance.tiles[tile.mapPos.x + dx[i],tile.mapPos.y+dx[3-i]];
            if (near.stander != null && near.stander.propertyController.creator.plantTags.Contains("Mygo")
                &&near.stander.propertyController.creator.chessName!=user.propertyController.creator.chessName
                &&!nearChess.Contains(near.stander.propertyController.creator.chessName))
            {
                nearChess.Add(near.stander.propertyController.creator.chessName);
                if (!triggerEgg&&eggcontroller!=null &&eggcontroller.CheckEgg(near.stander))
                {
                    triggerEgg=true;
                }
            }
        }
        
        if (nearChess.Count > 0)
        {
            //Debug.Log("队友+1");
            user.animatorController.ChangeFloat("Mygo", 1);
            //user.animatorController.animator.Play("change1");
            user.animatorController.ChangeFlash(1);
            user.propertyController.SetAtttackRange(user.propertyController.creator.baseProperty.attackRange);
            if (GameManage.instance.fetterManage.ContainFetter("Mygo")&&nearChess.Count == 4)
            {
                user.animatorController.ChangeFloat("Mygo", 2);
            }
             
        }
        else
        {
            user.animatorController.ChangeFloat("Mygo", 0);
            user.propertyController.SetAtttackRange(-1);
            user.animatorController.ChangeFlash(1);
        }
    }
    public void Check(Chess chess)
    {
        user.StartCoroutine(WaitCheck(chess));
    }
    IEnumerator WaitCheck(Chess chess)
    {
        yield return null;
        CheckFriend(chess);
    }
}
/// <summary>
/// anon的被动 主要是要多一个抱头
/// </summary>
public class Passive_Anon : Passive_Mygo
{
    [SerializeReference]
    public Buff ResumeBuff;
    public override void UseSkill(Chess user)
    {
        base.UseSkill(user);
        user.propertyController.onGetDamage.AddListener(OnGetDamage);
    }
    public override void LeaveSkill(Chess user)
    {
        base.LeaveSkill(user);
        user.propertyController.onGetDamage.RemoveListener(OnGetDamage);
    }
    public  void OnGetDamage(DamageMessege dm)
    {
        if (user.propertyController.GetHp() <= 0&& user.animatorController.animator.GetFloat("Mygo") > 0 )
        {
            user.propertyController.ChangeHp(1);
            //调用无法选中代码
            //Debug.Log(user.propertyController.GetHp());
            //调用转换哭泣状态代码
            user.stateController.ChangeState(StateName.ResumeState);
            user.GetComponent<AudioPlayer>().PlaySubN(1);
            //添加回血代码
            user.buffController.AddBuff(ResumeBuff);
        }
    }
}
/// <summary>
/// 这个主要是改变射程 
/// </summary>
public class Passive_Taki : Passive_Mygo
{
    public override void CheckFriend(Chess chess)
    {
        base.CheckFriend(chess);
        if (nearChess.Count == 4)
        {
            //Debug.Log("增加攻击距离");
            user.propertyController.SetAtttackRange(1000);
        }
    }
}

