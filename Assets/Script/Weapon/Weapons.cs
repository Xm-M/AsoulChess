using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon_Sample : Weapon
{
    [SerializeReference]
    public IInitWeapon initWeapon;
    [SerializeReference]
    public IFindTarget  findTarget;
    [SerializeReference]
    public IAttackFunction attackFunction;
    /// <summary>仅运行时缓存目标，勿序列化；否则 Odin 在绘制 SerializeReference 嵌套武器时会与 List&lt;Chess&gt; 序列化冲突。</summary>
    [System.NonSerialized]
    public List<Chess> enemys;
    public GameObject targetEffect;
    public GameObject selfEffect;
    public float interval;
    public int FindEnemy(Chess user)
    {
        //throw new System.NotImplementedException();
        enemys.Clear();
        if (findTarget != null)
        {
            findTarget?.FindTarget(user, enemys);
            return enemys.Count;
        }
        else
        {
            return -1;
        }
    }

    public float GetInterval()
    {
        //throw new System.NotImplementedException();
        return interval;
    }

    public void InitWeapon(AttackController attackController)
    {
         enemys= new List<Chess>();
         initWeapon?.InitWeapon(attackController);
    }

    public void TakeDamage(Chess user)
    {
        //throw new System.NotImplementedException();
        attackFunction.Attack(user, enemys);
        if (targetEffect != null)
        {
            foreach(var target in enemys)
            {
               GameObject effect=ObjectPool.instance.Create(targetEffect);
               effect.transform.position = target.transform.position;
            }
        }
        if (selfEffect != null)
        {
            GameObject effect = ObjectPool.instance.Create(selfEffect);
            effect.transform.position = user.transform.position;
        }
    }
}//

