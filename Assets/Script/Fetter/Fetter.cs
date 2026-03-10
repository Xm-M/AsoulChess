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
    [HideInInspector]public int num;
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
/// 结束乐队 
/// 凑齐结束乐队所有成员时:结束乐队卡牌的冷却减少50%；所有结束乐队成员在种植后 会生成一个对应缩小卡牌
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
        EventController.Instance.AddListener<Chess>(EventName.WhenPlantChess.ToString(), OnPlantChess);
        foreach(var icon in UIManage.GetView<PlantsShop>().currentShopIcons)
        {
            if (icon.good.plantTags.Contains("结束乐队"))
            {
                icon.coldDown = icon.coldDown * 0.6f;
            }
        }
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

    public void OnPlantChess(Chess chess)
    {
        if (!cards.Contains(chess.propertyController.creator) && chess.propertyController.creator.plantTags.Contains("结束乐队"))
        {
            string chessName = chess.propertyController.creator.chessName;
            foreach(var creator in cards)
            {
                if (creator.chessName.Contains(chessName))
                {
                    Item_PlantCard card = UIManage.GetView<ItemPanel>().Create<Item_PlantCard>();
                    card.InitCard(chess.transform.position,creator);
                }
            }
        }
    }

    public override void ResetFetter()
    {
        base.ResetFetter();
        EventController.Instance.RemoveListener<Chess>(EventName.WhenPlantChess.ToString(), OnPlantChess);
    }
     
}
 


/// <summary>
/// 无刺有刺 
/// </summary>
public class TogenashiTogeari : Fetter
{
    public DamageMessege DM;
    [SerializeReference]
    public Buff GBCBuff;
   
    [LabelText("刺雨冷却")]
    public float coldDown;
    [LabelText("刺雨持续时间")]
    public float continueTime;
    [LabelText("刺雨伤害倍率")]
    public float rate;
    [LabelText("")]
    public float darkPecent = 0.5f;

    public GameObject rainPre;

    [LabelText("描边色")]
    public Color outlineColor;
    [LabelText("描边大小")]
    public float outlineSize;


    [ShowInInspector, ReadOnly]
    [ShowIf("@UnityEngine.Application.isPlaying")]
    public int stressChangeValue;
    ParticleSystem rain;
    Timer rainLoop;
    Timer rainStopTimer;
    Timer rainDamageLoop;

    public override void FetterEffect(int num)
    {
        base.FetterEffect(num);
        EventController.Instance.AddListener<Chess>(EventName.WhenPlantChess.ToString(), AddBuff);
        rainLoop = GameManage.instance.timerManage.AddTimer(Rain, coldDown, true);
        stressChangeValue = 0;
        if (rain == null)
        {
            GameObject raining= ObjectPool.instance.Create(rainPre);
            raining.transform.position = Vector3.zero;
            rain = raining.GetComponent<ParticleSystem>();
            rain.Stop();
        }
    }
    public void AddBuff(Chess chess)
    {
        if (chess.propertyController.creator.plantTags.Contains("无刺有刺")&&chess.CompareTag("Player"))
        {
            chess.buffController.AddBuff(GBCBuff);
        }
    }
    public override void ResetFetter()
    {
        base.ResetFetter();
        rainLoop?.Stop();rainLoop = null;
        rainStopTimer?.Stop();rainStopTimer = null;
        rainDamageLoop?.Stop();rainDamageLoop = null;
        EventController.Instance.RemoveListener<Chess>(EventName.WhenPlantChess.ToString(), AddBuff);
        if (rain != null)
        {
            ObjectPool.instance.Recycle(rain.gameObject);
        }
        (MapManage.instance as MapManage_PVZ).ResumeLight();
    }

    public void Rain()
    {
        rainStopTimer = GameManage.instance.timerManage.AddTimer(Stop, continueTime, false);
        rainDamageLoop = GameManage.instance.timerManage.AddTimer(RainDamage, 1, true);
        (MapManage.instance as MapManage_PVZ).ChangeLight(-darkPecent);
        foreach(var chess in GameManage.instance.chessTeamManage.GetTeam("Player"))
        {
            if (chess.propertyController.creator.plantTags.Contains("无刺有刺") && chess.CompareTag("Player"))
            {
                chess.animatorController.SetOutline(outlineColor,outlineSize);
            }
        }
        
    }
    public void RainDamage()
    {
        rain.Play();
        foreach(var chess in GameManage.instance.chessTeamManage.GetEnemyTeam("Player"))
        {
            DM.damageFrom = null;
            DM.damageTo = chess;
            DM.damage = stressChangeValue*rate ;
            chess.propertyController.GetDamage(DM);
           
        }
    }
    

    public void Stop()
    {
        rainDamageLoop.Stop();
        rainDamageLoop = null;
        rain.Stop();
        stressChangeValue = 0;
        (MapManage.instance as MapManage_PVZ).ChangeLight(darkPecent);
        foreach (var chess in GameManage.instance.chessTeamManage.GetTeam("Player"))
        {
            if (chess.propertyController.creator.plantTags.Contains("无刺有刺") && chess.CompareTag("Player"))
            {
                chess.animatorController.SetOutline(Color.white, 0);
            }
        }
    }
}
public class Buff_Band_GBC : Buff
{
    TogenashiTogeari gbc;
    int stress;
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        gbc = GameManage.instance.fetterManage.GetFetter("无刺有刺") as TogenashiTogeari;
        stress = 0;
        target.skillController.context.OnValueChange.AddListener(OnStressChange);
    }
    public void OnStressChange()
    {
        int current = 0;
        target.skillController.context.TryGet<int>("stress", out current);
        int change = Mathf.Abs(current - stress);
        gbc.stressChangeValue += change;
        stress = current;
    }
    public override void BuffOver()
    {
        base.BuffOver();
        target.skillController.context.OnValueChange.RemoveListener(OnStressChange);
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
/// <summary>
/// 所以说多出来的那点数值其实是mygo buff给的
/// </summary>
public class Buff_Mygo : Buff
{
    [LabelText("额外增伤")]
    public float extraDamage = 0.2f;
    [LabelText("额外减伤")]
    public float extraDefence = 0.2f;
    [LabelText("描边颜色")]
    public Color outlineColor;
    [LabelText("描边大小")]
    public float outlineSize=1;
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        Debug.Log("添加Mygo Buff");
        target.propertyController.ChangeExtraDamage(extraDamage);
        target.propertyController.ChangeExtraDefence(extraDefence);
        target.skillController.context.AddEvent(OnMygoChange);
        OnMygoChange();
    }
    public override void BuffOver()
    {
        base.BuffOver();
        target.propertyController.ChangeExtraDamage(-extraDamage);
        target.propertyController.ChangeExtraDefence(-extraDefence);
        target.skillController.context.RemoveEvent(OnMygoChange);
    }
    public void OnMygoChange()
    {
        int mygo = 0;
        target.skillController.context.TryGet<int>("mygo",out mygo);
        if(mygo == 4)
        {
            target.animatorController.SetOutline(outlineColor, outlineSize);
        }
        else
        {
            target.animatorController.SetOutline(Color.white, 0);
        }
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
