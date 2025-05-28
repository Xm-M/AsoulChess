using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
/// <summary>
/// 把所有的Fetter都放在这把 反正也没有
/// </summary>
public class Fetter
{
    //public Chess self;
    public string fetterName;
    public Sprite fetterIcon;
    public int bandMemberNum;
    public virtual void UseFetter()
    {

    }
    /// <summary>
    /// 这个是进入游戏就会调用一次的函数
    /// </summary>
    /// <param name="num"></param>
    public virtual void FetterEffect(int num)
    {

    }
    /// <summary>
    /// 这个是结束的时候会调用的函数
    /// </summary>
    public virtual void ResetFetter()
    {

    }
    public virtual bool FetterLight(int num)
    {
        return (num >= bandMemberNum);
    }
}
/// <summary>
/// 结束乐队 有概率生成暴龙波奇 
/// </summary>
public class KessokuBand:Fetter
{
    [LabelText("卡牌列表")]
    public List<PropertyCreator> cards;
    [LabelText("间隔时间")]
    public float interval=15f;
    [LabelText("下落速度")]
    public float fallSpeed=2f;
    Timer timer;
    public override void FetterEffect(int num)
    {
        base.FetterEffect(num);
        timer = GameManage.instance.timerManage.AddTimer(CreateCard, interval, true);
    }
    public void CreateCard()
    {
        for(int i = 0; i < cards.Count; i++)
        {
            Vector2 endpos = MapManage_PVZ.instance.RandomTile().transform.position;
            Vector2 startpos=new Vector2(endpos.x, endpos.y+15);
            endpos = Camera.main.WorldToScreenPoint(endpos);
            startpos = Camera.main.WorldToScreenPoint(startpos);
            Item_PlantCard card= UIManage.GetView<ItemPanel>().Create<Item_PlantCard>();
            card.InitCard(cards[i], startpos, endpos, fallSpeed);
        }
    }

    public override void ResetFetter()
    {
        base.ResetFetter();
        timer.Stop();
        timer = null;
    }
     
}
/// <summary>
/// 无刺有刺 
/// </summary>
public class TogenashiTogeari : Fetter
{
    float thorn;//伤害
    public float maxThorn = 10000;
    public DamageMessege DM;//有一个最大的问题 造成伤害的是谁？？？？ 没有DamageFrom啊 我日你哥了 或者说就直接takedamage
    public override void FetterEffect(int num)
    {
        base.FetterEffect(num);
        thorn = 0;
        EventController.Instance.AddListener<Chess>(EventName.WhenPlantChess.ToString(), AddBuff);
    }
    public void OnGetDamage(DamageMessege Dm)
    {

    }
    public void OnTakeDamage(DamageMessege Dm)
    {

    }
    public void AddBuff(Chess chess)
    {
        chess.propertyController.onGetDamage.AddListener(OnGetDamage);
        chess.propertyController.onTakeDamage.AddListener(OnTakeDamage);
    }
    public void ChangeThron(float value)
    {
        thorn += value;
        if (value >= maxThorn)
        {
            maxThorn = 0;
            //然后在这里发射尖刺 发射尖刺的过程中不会叠加
            //用Timer也是可以的 
        }
    }
    //IEnumerator CreateThorn()
    //{
    //    yield return null; 
    //}

    public override void ResetFetter()
    {
        base.ResetFetter();
        EventController.Instance.RemoveListener<Chess>(EventName.WhenPlantChess.ToString(), AddBuff);
    }
}
public class Mygo : Fetter
{
    public override void FetterEffect(int num)
    {
        base.FetterEffect(num);
    }
    public override void ResetFetter()
    {
        base.ResetFetter();
    }
}
public class AveMujica : Fetter {
    public override void FetterEffect(int num)
    {
        base.FetterEffect(num);
    }
    public override void ResetFetter()
    {
        base.ResetFetter();
    }
}
