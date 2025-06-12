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
    public int num;
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
        if (chess.propertyController.creator.plantTags.Contains("无刺有刺"))
        {
            chess.propertyController.onGetDamage.AddListener(OnGetDamage);
            chess.propertyController.onTakeDamage.AddListener(OnTakeDamage);
        }
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
/// <summary>
/// Mygo羁绊：效果：所有Mygo成员的冷却时间减半
/// 周围有四个队友的Mygo成员获得20%增伤和20%免伤害
/// </summary>
public class Mygo : Fetter
{
    [LabelText("Mygobuff")]
    [SerializeReference]
    public Buff_Mygo buff;
    public override void FetterEffect(int num)
    {
        base.FetterEffect(num);
        EventController.Instance.AddListener<Chess>(EventName.WhenPlantChess.ToString(), AddBuff);
        PlantsShop shop = UIManage.GetView<PlantsShop>();
        for (int i = 0; i < shop.shopIconParent.childCount;i++)
        {
            ShopIcon icon=shop.shopIconParent.GetChild(i).GetComponent<ShopIcon>();
            Debug.Log(icon.name);
            Debug.Log(icon.good);
            if (icon.good.plantTags.Contains("Mygo"))
            {
                icon.coldDown /= 2;
            }
        }
    }
    public void AddBuff(Chess chess)
    {
        if (chess.propertyController.creator.plantTags.Contains("Mygo"))
        {
            Debug.Log("添加MygoBuff");
            chess.buffController.AddBuff(buff);
        }
    }
    public override void ResetFetter()
    {
        base.ResetFetter();
        EventController.Instance.RemoveListener<Chess>(EventName.WhenPlantChess.ToString(), AddBuff);

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
