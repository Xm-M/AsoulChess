using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Passive_ZZZLight : ISkillEffect
{
    [SerializeReference]
    public Buff_ZZZLight_CreateSword buff;
    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        user.buffController.AddBuff(buff);
    }
}
public class Buff_ZZZLight_CreateSword : Buff
{
    public GameObject swordPrefab;//剑的预制体
    List<SwordController> swordControllers;
    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        ;
        target.skillController.context.TryGet<List<SwordController>>("sword", out swordControllers);
        if (swordControllers == null)
        {
            swordControllers = new List<SwordController>();
            target.skillController.context.Set<List<SwordController>>("sword", swordControllers);
        }
        target.equipWeapon.OnAttack.AddListener(CreateSword);
    }
    public override void BuffOver()
    {
        base.BuffOver();
        if(swordControllers != null)
        {
            foreach(var  sword in swordControllers)
            {
                sword.Recycle();
            }
        }
        swordControllers.Clear();
    }
    public void CreateSword(Chess chess)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector2 min = MapManage.instance.tiles[0, 0].transform.position;
            Vector2Int size = MapManage.instance.mapSize;
            Vector2 max = MapManage.instance.tiles[size.x - 1, size.y - 1].transform.position;
            Vector2 RandomPos = new Vector2(Random.Range(min.x, max.x), Random.Range(min.y, max.y));
            GameObject sword = ObjectPool.instance.Create(swordPrefab);
            sword.transform.position = RandomPos;
            List<SwordController> swordControllers;
            target.skillController.context.TryGet<List<SwordController>>("sword", out swordControllers);
            if (swordControllers == null) Debug.LogError("这怎么是空的");
            else
            {
                SwordController swordController = sword.GetComponent<SwordController>();
                swordControllers.Add(swordController);

            }
        }
    }
}