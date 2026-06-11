using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 那我们就好好思考一下吧
/// 状态上 其实不多啊 就站立idle 站立技能n个 俯身idle 俯身技能就一个吐火球
/// 那我们给skillcontext 一个bool stand的属性
/// 再给 Animatorcontroller定制一下动画就行了
/// 然后转换形态技能应该放在哪呢？
/// 血量高于80%时 只有
/// </summary>
public class SkillEffect_ZombieKing_ZombieSummon : ISkillEffect
{
    //根据释放位置的不同 召唤在不同行
    //列数是固定的在 x+朝向*2
    public List<PropertyCreator> zombies;//估计出怪类型跟血量也有关系
    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (zombies == null || zombies.Count == 0 || user.moveController?.standTile == null || MapManage.instance == null)
            return;
        PropertyCreator creator;
        var ctx = user.skillController.context;
        if (ctx.TryGet<int>(ZombieKingContextKeys.SpawnPoolIndex, out int poolIdx) && poolIdx >= 0 && poolIdx < zombies.Count)
            creator = zombies[poolIdx];
        else
            creator = zombies[Random.Range(0, zombies.Count)];
        int x = user.moveController.standTile.mapPos.x + (int)user.transform.right.x * 2;
        int tileY = 4;
        if (ctx.TryGet<int>(ZombieKingContextKeys.Row, out int rowAnim))
            tileY = ZombieKingMapAnim.AnimRowToTileY(rowAnim, MapManage.instance.mapSize.y);
        if (!MapManage.instance.IfInMapRange(x, tileY)) return;
        Tile tile = MapManage.instance.tiles[x, tileY];
        GameManage.instance.chessTeamManage.CreateChess(creator, tile, user.tag);
    }
}
public class SkillEffect_ZombieKing_FireBall: ISkillEffect
{
    public GameObject ball;
    [Tooltip("可选；BallVisual=1 且非空时用冰球")]
    public GameObject iceBall;
    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user.moveController?.standTile == null || MapManage.instance == null || ball == null)
            return;
        var ctx = user.skillController.context;
        int x = user.moveController.standTile.mapPos.x + (int)user.transform.right.x * 3;
        int tileY = 4;
        if (ctx.TryGet<int>(ZombieKingContextKeys.Row, out int rowAnim))
            tileY = ZombieKingMapAnim.AnimRowToTileY(rowAnim, MapManage.instance.mapSize.y);
        if (!MapManage.instance.IfInMapRange(x, tileY)) return;
        Tile tile = MapManage.instance.tiles[x, tileY];
        GameObject prefab = ball;
        if (ctx.TryGet<int>(ZombieKingContextKeys.BallVisual, out int vis) && vis == 1 && iceBall != null)
            prefab = iceBall;
        GameObject b = GameObject.Instantiate(prefab);
        b.tag = user.tag;
        var armor = b.GetComponent<CarArmor>();
        if (armor != null) armor.user = user;
        b.transform.position = tile.transform.position;
    }
}

public class SkillEffect_ZombieKing_RV : ISkillEffect
{
    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        if (user.moveController?.standTile == null || MapManage.instance == null) return;
        float dmg = user.propertyController.GetAttack();
        if (config != null && config.baseDamage != null && config.baseDamage.Count > 0)
            dmg *= config.baseDamage[0];
        else
            dmg *= 1000f;
        var map = MapManage.instance;
        int sign = user.CompareTag("Enemy") ? -1 : (user.transform.right.x < -0.01f ? -1 : 1);
        int kingX = user.moveController.standTile.mapPos.x;
        string plantTag = user.CompareTag("Enemy") ? "Player" : "Enemy";
        ZombieKingMapAnim.ForEachRvCrushTile(kingX, sign, (tx, ty) =>
        {
            if (!map.IfInMapRange(tx, ty)) return;
            var t = map.tiles[tx, ty];
            var target = t?.stander;
            if (target == null || !target.CompareTag(plantTag)) return;
            var dm = user.skillController.DM;
            dm.damageFrom = user;
            dm.damageTo = target;
            dm.damage = dmg;
            dm.damageType = DamageType.Real;
            dm.damageElementType = ElementType.Grind;
            target.propertyController.GetDamage(dm);
        });
    }
}
