using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillEffect_GitaHero : ISkillEffect
{
    public List<GameObject> effects;
    public float checkRange;
    public Vector3 dx;
    [SerializeReference]
    public Buff_Fear Buff_Fear;
    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        Chess c = null;
        PropertyCreator creator = null;
        user.skillController.context.TryGet<Chess>("成员",out c);
        if (c == null) return;
        creator = c.propertyController.creator;
        SearchTarget(c, targets);
        if (creator != null)
        {
            DamageMessege dm = user.skillController.DM;
            GameObject effect = null;
            float damage = 0;
            if (creator.chessName.Contains("虹夏"))
            {
                damage = config.baseDamage[0] * user.propertyController.GetAttack();//1.5f
                dm.damageElementType=ElementType.AOE|ElementType.CloseAttack;
                dm.damageType = DamageType.Physical;
                effect = effects[0];
                foreach(var target in targets)
                {
                    SunLight lignt = UIManage.GetView<ItemPanel>().Create<SunLight>() as SunLight;
                    lignt.InitSunLight(target.moveController.standTile, 25, target.transform.position);
                }
            }else if (creator.chessName.Contains("凉"))
            {
                damage = config.baseDamage[1] * user.propertyController.GetAttack();//4.5
                dm.damageElementType = ElementType.AOE |ElementType.Grind;
                dm.damageType = DamageType.Physical;
                effect = effects[1];
            }
            else if (creator.chessName.Contains("喜多"))
            {
                damage = config.baseDamage[2] * user.propertyController.GetAttack();
                dm.damageElementType = ElementType.AOE | ElementType.Explode;
                dm.damageType = DamageType.Real;
                effect = effects[2];
            }
            else if (creator.chessName.Contains("波奇"))
            {
                damage = config.baseDamage[3] * user.propertyController.GetAttack();
                dm.damageElementType = ElementType.AOE | ElementType.CloseAttack;
                dm.damageType = DamageType.Physical;
                effect = effects[3];
                foreach (var target in targets) target.buffController.AddBuff(Buff_Fear);
            }
            dm.damageFrom = user;
            GameObject e= ObjectPool.instance.Create(effect);
            e.transform.position = c.transform.position;
            foreach (var chess in targets)
            {
                dm.damage = damage;
                dm.damageTo = chess;
                user.propertyController.TakeDamage(dm);
            }
            user.skillController.context.Set<Chess>("成员", null);
        }
    }
    public void SearchTarget(Chess user,List<Chess> targets)
    {
        targets.Clear();
        Collider2D[] cols = CheckObjectPoolManage.GetColArray((int)(1000 * checkRange * checkRange));
        LayerMask layer = ChessTeamManage.Instance.GetEnemyLayer(user.gameObject);
        int i = Physics2D.OverlapCircleNonAlloc(user.transform.position + dx, checkRange, cols, layer);
        //Debug.Log("找到了" + i);
        for (int j = 0; j < i; j++)
        {
            Chess enemy = cols[j].GetComponent<Chess>();
            targets.Add(enemy);
        }
        CheckObjectPoolManage.ReleaseColArray(1000, cols);
    }
}
