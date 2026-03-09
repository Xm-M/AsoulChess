using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 蛋包饭的buff
/// </summary>
public class Buff_OmuRice : Buff
{
    [SerializeReference]
    public Buff_BaseValueBuff_Crit critBuff;
    public PropertyCreator boqi;
    Buff_BoqiOmuRice omuRiceBuff;
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        target.buffController.AddBuff(critBuff);
        //Debug.Log("暴击buff");
        if (target.propertyController.creator == boqi)
        {
            omuRiceBuff = new Buff_BoqiOmuRice();
            target.buffController.AddBuff(omuRiceBuff);
            //Debug.Log("变成波奇大蒜");
        }
    }
    public override void BuffOver()
    {
        base.BuffOver();
        target.buffController.TryOverBuff(critBuff);
    }
    public class Buff_BoqiOmuRice :Buff
    {
        Buff_GarlicRedirect buff_GarlicRedirect;
        public Buff_BoqiOmuRice()
        {
            buff_GarlicRedirect = new Buff_GarlicRedirect();
            buffName = "波奇蛋包饭";
        }
        public override void BuffEffect(Chess target)
        {
            base.BuffEffect(target);
            target.propertyController.onGetDamage.AddListener(ChangeDiretion);
        }
        public override void BuffOver()
        {
            base.BuffOver();
            target.propertyController.onGetDamage.RemoveListener(ChangeDiretion);
        }
        public void ChangeDiretion(DamageMessege DM)
        {
            //Debug.Log("受到攻击");
            if (DM.damageFrom != null && (DM.damageElementType & ElementType.CloseAttack) != 0)
            {
                DM.damageFrom.buffController.AddBuff(buff_GarlicRedirect);
                //Debug.Log("换行");
            }
        }
        public override void BuffReset(Buff resetBuff)
        {
            base.BuffReset(resetBuff);
        }
    }
    /// <summary>
    /// 换行buff
    /// </summary>
    public class Buff_GarlicRedirect : Buff
    {
        public DisarmBuff disarmBuff;
        Timer check;
        public Buff_GarlicRedirect()
        {
            disarmBuff = new DisarmBuff();
            buffName = "波奇大蒜";
        }
        public override void BuffEffect(Chess target)
        {
            base.BuffEffect(target);
            target.buffController.AddBuff(disarmBuff);
            ChangeDiretion(target);
            check = GameManage.instance.timerManage.AddTimer(OnReach, 0.1f, true);
            //target.moveController.OnReachTile.AddListener(OnReach);
        }
        void OnReach()
        {
            if (Mathf.Abs(target.moveController.nextTile.transform.position.y - target.transform.position.y) < 0.1f)
            {
                Debug.Log("到达目标");
                BuffOver();
            }
        }
        public override void BuffOver()
        {
            base.BuffOver();
            target.buffController.TryOverBuff(disarmBuff);
            check.Stop();
            check = null;
            //target.moveController.OnReachTile.RemoveListener(OnReach);
        }
        /// <summary>
        /// 换行 现在的问题就是怎么让竖方向移速更快一点 以及只判断y
        /// </summary>
        /// <param name="enemy"></param>
        public void ChangeDiretion(Chess enemy)
        {
            var tiles = MapManage.instance.tiles;
            Vector2Int mapSize = MapManage.instance.mapSize;

            //Chess enemy = DM.damageFrom;
            var standTile = enemy.moveController.standTile;

            // 假设 standTile 里有坐标（按你的项目结构改字段名）
            // 常见命名：standTile.pos / standTile.gridPos / standTile.tileIndex ...
            Vector2Int p = standTile.mapPos;

            int x = p.x;
            int y = p.y;

            int maxY = mapSize.y - 1;
            int newY;

            if (y <= 0)
            {
                newY = 1;
            }
            else if (y >= maxY)
            {
                newY = maxY - 1;
            }
            else
            {
                newY = (UnityEngine.Random.value < 0.5f) ? (y - 1) : (y + 1);
            }
            //Debug.Log("当前StandTile" + enemy.moveController.standTile.mapPos);
            //Debug.Log("当前nextTile" + enemy.moveController.nextTile.mapPos);
            // 同列另一行
            enemy.moveController.standTile = tiles[x, newY];
            //enemy.moveController.standTile = tiles[x-(int)enemy.transform.right.x , newY];
            //Debug.Log("换行后nextTile" + enemy.moveController.nextTile.mapPos);
        }
    }
}
