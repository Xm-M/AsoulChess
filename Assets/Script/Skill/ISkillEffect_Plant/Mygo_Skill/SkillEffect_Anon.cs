using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 爱音的技能 因为是吉他手 所以技能效果和暴击率相关
/// </summary>
public class SkillEffect_Anon : ISkillEffect
{
    public Transform sunLightPos;
    public float x0 = 0.5f;
    public float movespeed = -1f;
    public float height = 10;
    public float totalTime = 1;
    public float timeSpeed = 1;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        SunLight lignt = UIManage.GetView<ItemPanel>().Create<SunLight>() as SunLight;
        float n = Random.Range(0, 1);
        int sun = (int)config.baseDamage[0];
        if (n < user.propertyController.GetCrit())
        {
            sun = (int)((int)config.baseDamage[0] * user.propertyController.GetCritDamage());
        }
        lignt.InitSunLight(user.moveController.standTile, sun, sunLightPos.transform.position);
    }
}
/// <summary>
/// 千早爱音的隐藏技能 为一个大范围内的所有友军恢复25%的最大生命值；如果目标是Mygo成员（非爱音），则会在目标位置生成阳光
/// </summary>
public class SkillEffect_AnonHide : ISkillEffect
{
    public float radius=6.25f;
    public float healRate = 0.25f;
    public GameObject healEffect;
    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        Collider2D[] cols = CheckObjectPoolManage.GetColArray(100);
        int n = Physics2D.OverlapCircleNonAlloc(user.transform.position, radius, cols, LayerMask.GetMask(user.tag));
        for (int i = 0; i < n; i++)
        {
            Chess friend = cols[i].gameObject.GetComponent<Chess>();
            friend.propertyController.Heal(friend.propertyController.GetMaxHp() * healRate);
            GameObject heal = ObjectPool.instance.Create(healEffect);
            heal.transform.position = friend.transform.position;
            //这里还要生成一个治疗的特效
            if (friend.propertyController.creator.plantTags.Contains("Mygo") &&
                friend.propertyController.creator != user.propertyController.creator)
            {
                SunLight lignt = UIManage.GetView<ItemPanel>().Create<SunLight>() as SunLight;
                lignt.InitSunLight(friend.moveController.standTile, 15, friend.moveController.standTile.transform.position + Vector3.up);
            }
        }
    }
}