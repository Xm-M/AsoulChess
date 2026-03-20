using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Passive_GitaHero : ISkillEffect
{
    Chess user;
    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        this.user = user;
        EventController.Instance.AddListener<Chess>(EventName.WhenPlantChess.ToString(), OnPlantChess);
    }
    public void OnDeath() => EventController.Instance.RemoveListener<Chess>(EventName.WhenPlantChess.ToString(), OnPlantChess);
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
