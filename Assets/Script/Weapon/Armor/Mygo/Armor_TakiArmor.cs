using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Armor_TakiArmor : ArmorBase
{
    public List<GameObject> bullets;//Taki 会根据tomorin的子弹更换子弹
    public Animator animator;
    ShootBullet taki;
    int current;

    public override void BrokenArmor()
    {
    }

    public override void GetDamage(DamageMessege dm)
    {
    }

    public override void InitArmor()
    {
        animator = GetComponent<Animator>();
        taki= (user.equipWeapon.weapon as Weapon_Sample).attackFunction as ShootBullet;
        user.WhenEnterGame.AddListener(ResetArmor);
    }
    public override void ResetArmor(Chess chess)
    {
        taki.bullet = bullets[2];
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
            if (bullet.name == "红音符(Clone)")
            {
                current = 0;
                //animator.Play("red");
                taki.bullet = bullets[0];
            }else if (bullet.name== "蓝音符(Clone)")
            {
                current = 1;
                //animator.Play("blue");
                taki.bullet = bullets[1];
            }else if (bullet.name=="绿音符(Clone)")
            {
                current = 2;
                //animator.Play("green");
                taki.bullet = bullets[2];
            }
        }
    }
}
