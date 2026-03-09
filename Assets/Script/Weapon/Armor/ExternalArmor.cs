using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 暂时没有理清这些东西
/// 这个是铁门的逻辑啊  
/// </summary>
public class ExternalArmor : ArmorBase
{
    public float armorMax;
    public Animator animator;
    public string animName;
    public Collider2D col;
    float armorCurrent;
    float brokenPercent;
    /// <summary>
    /// 这个是初始化，只会被调用一次
    /// </summary>
    public override void InitArmor()
    {
        armorCurrent = armorMax;
        user.WhenEnterGame.AddListener(ResetArmor);
    }
    public override void GetDamage(DamageMessege dm)
    {
       
        if ((dm.damageElementType & ElementType.AOE) != 0)
        {
            armorCurrent -= dm.damage;
            brokenPercent = armorCurrent / armorMax;
            animator?.SetFloat(animName, brokenPercent);
        }
        else if((dm.damageElementType&ElementType.CloseAttack)!=0)
        {
            armorCurrent -= dm.damage;
            dm.damage = 0;
            if(armorCurrent<=0)
            {                 
                BrokenArmor();
            }
            brokenPercent = armorCurrent / armorMax;
            animator?.SetFloat(animName, brokenPercent);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log("hit "+collision.name);
        Bullet bullet = collision.GetComponent<Bullet>();
        if (bullet != null)
        {
            DamageMessege dm=bullet.Dm;
            armorCurrent -= dm.damage;
            dm.damage = 0;
            if (armorCurrent <= 0)
            {
                BrokenArmor();
            }
            brokenPercent = armorCurrent / armorMax;
            animator?.SetFloat(animName, brokenPercent);
            if((bullet.Dm.damageElementType&ElementType.AOE)==0)
                bullet.RecycleBullet();
        }
    }

    /// <summary>
    ///  就是重置函数
    /// </summary>
    /// <exception cref="System.NotImplementedException"></exception>
    public override void ResetArmor(Chess chess)
    {
        tag = user.tag;
        col.enabled = true;
        user.propertyController.onSetDamage.AddListener(GetDamage);
        armorCurrent = armorMax;
    }
    /// <summary>
    ///   是防具损坏函数
    /// </summary>
    /// <exception cref="System.NotImplementedException"></exception>
    public override void BrokenArmor()
    {
        col.enabled=false;
        user.propertyController.onSetDamage.RemoveListener(GetDamage);
    }
}
