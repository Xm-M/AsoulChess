using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Armor_TakiArmor : ArmorBase
{
    public List<GameObject> bullets;//Taki 会根据tomorin的子弹更换子弹
    public Animator animator;
    //ShootBullet taki;
    //int current;
    public Buff currentbuff;//怎么把buff加进去呢？
    public override void BrokenArmor()
    {
    }

    public override void GetDamage(DamageMessege dm)
    {
    }

    public override void InitArmor()
    {
        animator = GetComponent<Animator>();
 
        currentbuff = null;
        animator.SetInteger("tomorin", -1);
        user.WhenEnterGame.AddListener(ResetArmor);
    }
    public override void ResetArmor(Chess chess)
    {
 
        animator.SetInteger("tomorin", -1);
        currentbuff = null;
    }
    /// <summary>
    /// 要在这里检测来自tomorin的子弹 并且更换taki的子弹
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Bullet bullet = collision.GetComponent<Bullet>();
        //Debug.Log("受到灯影响");
        if (bullet != null && bullet.Dm.damageFrom != null&&bullet.Dm.damageFrom.propertyController.creator.chessName=="高松灯")
        {
            currentbuff = bullet.Dm.takeBuff;
            //int n = bullet.GetComponent<Animator>().GetInteger("type");
            //0红 1绿 2蓝
            int n = 0;
            if (bullet.name.Contains("黄")) n = 1;
            else if (bullet.name.Contains("蓝")) n = 2;
            animator.SetInteger("tomorin", n);
        }
    }
}
