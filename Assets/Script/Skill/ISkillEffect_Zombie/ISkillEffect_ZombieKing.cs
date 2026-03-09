using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 那我们就好好思考一下吧
/// 状态上 其实不多啊 就站立idle 站立技能n个 俯身idle 俯身技能就一个吐火球
/// 那我们给skillcontext 一个bool stand的属性
/// 再给 Animatorcontroller定制一下动画就行了
/// 然后转换形态技能应该放在哪呢？
/// </summary>
public class SkillEffect_ZombieKing_ZombieSummon : ISkillEffect
{
    //根据释放位置的不同 召唤在不同行
    //列数是固定的在 x+朝向*2
    public List<PropertyCreator> zombies;//估计出怪类型跟血量也有关系
    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        //throw new System.NotImplementedException();
        PropertyCreator creator = zombies[Random.Range(0, zombies.Count)];
        int x = user.moveController.standTile.mapPos.x + (int)user.transform.right.x * 2;
        int y = 4;//这个其实应该是随机的
        Tile tile = MapManage.instance.tiles[x, y];
        GameManage.instance.chessTeamManage.CreateChess(creator,tile, user.tag);
    }
}
public class SkillEffect_ZombieKing_FireBall: ISkillEffect
{
    public GameObject ball;
    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        int x = user.moveController.standTile.mapPos.x + (int)user.transform.right.x * 3;
        int y = 4;//这个其实应该是随机的
        Tile tile = MapManage.instance.tiles[x, y];
        GameObject b=GameObject.Instantiate(ball);
        b.tag = user.tag;
        b.GetComponent<CarArmor>().user = user;
        b.transform.position = tile.transform.position;
    }
}

public class SkillEffect_ZombieKing_RV : ISkillEffect
{
    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        throw new System.NotImplementedException();
    }
}