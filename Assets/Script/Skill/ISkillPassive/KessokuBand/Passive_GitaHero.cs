using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 我想把特效也做在这了
/// </summary>
public class Passive_GitaHero : ISkillEffect
{
    Chess user;
    public float interval=2f;
    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        this.user = user;
        EventController.Instance.AddListener<Chess>(EventName.WhenPlantChess.ToString(), OnPlantChess);
        user.OnRemove.AddListener(OnDeath);
        (MapManage.instance as MapManage_PVZ).ChangeLight(-0.5f);
        Timer timer = GameManage.instance.timerManage.AddTimer(() => (MapManage.instance as MapManage_PVZ).ChangeLight(0.5f),interval); ;
    }
    public void OnDeath(Chess chess) => EventController.Instance.RemoveListener<Chess>(EventName.WhenPlantChess.ToString(), OnPlantChess);
    public void OnPlantChess(Chess c)
    {
        if (c == null) return;
        PropertyCreator creator = c.propertyController.creator;
        if (creator.chessName.Contains("虹夏")|| creator.chessName.Contains("凉")
            || creator.chessName.Contains("喜多")|| creator.chessName.Contains("后藤一里"))
        {
            user.skillController.context.Set<Chess>("成员",c);
        }
    }
}
