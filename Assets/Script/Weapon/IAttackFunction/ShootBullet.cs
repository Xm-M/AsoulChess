using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
/// <summary>
///  
/// </summary>
public class ShootBullet : IAttackFunction
{
    public GameObject bullet;
    public void Attack(Chess user, List<Chess> targets)
    {
         GameObject b=ObjectPool.instance.Create(bullet);
        Bullet zidan=b.GetComponent<Bullet>();
        if (targets.Count == 0) return;
        zidan.InitBullet(user, user.equipWeapon.weaponPos.position, targets[0] ,user.transform.right);
        zidan.Dm.damageTo = targets[0];
        //Debug.Log(zidan.Dm.damageTo.name);
    }
}
public class ShootBulletByDir : IAttackFunction
{
    public GameObject bullet;
    public Transform shooter;
    public void Attack(Chess user, List<Chess> targets)
    {
        if ((targets.Count == 0)) return;
        GameObject b = ObjectPool.instance.Create(bullet);
        Bullet zidan = b.GetComponent<Bullet>();
        zidan.InitBullet(user, shooter.transform.position, targets[0] , shooter.transform.right);
        zidan.Dm.damageTo = targets[0];
    }
}
public class MultiAttack : IAttackFunction
{
    [SerializeReference]
    public List<IAttackFunction> attacks;
    public void Attack(Chess user, List<Chess> targets)
    {
        for(int i = 0; i < attacks.Count; i++)
        {
            attacks[i].Attack(user, targets);
        }
    }
}
//public class ShootBullet_ËïÉÐÏã : IAttackFunction
//{
//    public GameObject bullet;
//    int index = 0;
//    public void Attack(Chess user, List<Chess> targets)
//    {
//        if (index < 3)
//        {
//            GameObject b = ObjectPool.instance.Create(bullet);
//            Bullet zidan = b.GetComponent<Bullet>();
//            zidan.InitBullet(user, user.equipWeapon.weaponPos.position, targets[0], user.transform.right);
//        }
//        else
//        {
//            index = 0;
//        }
//    }
//}
public class ShootBullet_Tomorin : IAttackFunction
{
    [SerializeReference]
    public List<Buff> buffs;
    public GameObject bullet;
    public List<GameObject> effects;
    //public Transform shooter;
    public void Attack(Chess user, List<Chess> targets)
    {
        if ((targets.Count == 0)) return;
        int n = Random.Range(0, buffs.Count);
        GameObject b = ObjectPool.instance.Create(bullet);
        Bullet zidan = b.GetComponent<Bullet>();
        zidan.InitBullet(user, user.equipWeapon.weaponPos.transform.position, targets[0], user.transform.right);
        zidan.Dm.damageTo = targets[0];
        zidan.Dm.takeBuff = buffs[n].Clone();
        zidan.GetComponent<Animator>().SetInteger("type", n);
        if(n==1)
            zidan.GetComponent<Animator>().Play("greenBullet");
        else if (n == 2)
        {
            zidan.GetComponent<Animator>().Play("blueBullet");
        }
        zidan.GetComponent<EffectMiss>().Effect = effects[n];
    }
}
public class ShootBullet_Taki : IAttackFunction
{
    public Armor_TakiArmor armor;
    [LabelText("ÆÕÍ¨×Óµ¯")]
    public GameObject bullet;
    [LabelText("´óºÅ×Óµ¯")]
    public GameObject bigBullet;
    public Transform shooter;
    
    public void Attack(Chess user, List<Chess> targets)
    {
        if ((targets.Count == 0)) return;
        GameObject bu=user.GetComponent<Animator>().GetFloat("Mygo")== 2 ?bigBullet :bullet;
        GameObject b = ObjectPool.instance.Create(bu);
        Bullet zidan = b.GetComponent<Bullet>();
        int n = armor.animator.GetInteger("tomorin");
        zidan.GetComponent<Animator>().SetInteger("type", n);
        zidan.InitBullet(user, shooter.transform.position, targets[0], shooter.transform.right);
        zidan.Dm.damageTo = targets[0];
        zidan.Dm.takeBuff =armor.currentbuff;
    }
}
public class ShootBullet_Rana : IAttackFunction
{
    public GameObject bullet;

    public void Attack(Chess user, List<Chess> targets)
    {
        GameObject b = ObjectPool.instance.Create(bullet);
        Bullet zidan = b.GetComponent<Bullet>();
        if (targets.Count == 0) return;
        if (user.animatorController.animator.GetFloat("Mygo") > 1)
        {
            zidan.MaxHitNum = 1000;
        }
        else
        {
            zidan.MaxHitNum = 1;
        }
        zidan.InitBullet(user, user.equipWeapon.weaponPos.position, targets[0], user.transform.right);
        zidan.Dm.damageTo = targets[0];
        //Debug.Log(zidan.Dm.damageTo.name);
        
    }
}// 
public class ShootBullet_WaitBullet : IAttackFunction
{
    public GameObject bullet;
    public string pamareName = "wait";
    Bullet zidan;
    Chess user;
    public void Attack(Chess user, List<Chess> targets)
    {
        this.user = user;
        GameObject b = ObjectPool.instance.Create(bullet);
        user.animatorController.ChangeFloat(pamareName, 0);
        zidan = b.GetComponent<Bullet>();
        if (targets.Count == 0) return;
        zidan.InitBullet(user, user.equipWeapon.weaponPos.position, targets[0], user.transform.right);
        //zidan.targetPos =;
        zidan.Dm.damageTo = targets[0];
        user.StartCoroutine(WaitBullet());
    }
    IEnumerator WaitBullet()
    {
        while (zidan == null||zidan.gameObject.activeSelf )
        {
            yield return null;  
        }
        user.animatorController.ChangeFloat(pamareName,1);
    }
}