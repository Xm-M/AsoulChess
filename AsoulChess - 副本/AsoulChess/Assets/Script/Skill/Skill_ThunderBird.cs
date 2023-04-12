using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName ="Skill_ThunderBird",menuName ="Skill/ThunderBird")]
public class Skill_ThunderBird : Skill
{
    [SerializeField]private GameObject thunderBird;
    [SerializeField] private float damage;
    public override void SkillEffect(Chess user, params Chess[] target)
    {
        base.SkillEffect(user, target);
        Debug.Log("TunderBird");
        GameObject bird = ObjectPool.instance.Create(thunderBird);
        Bullet bullet = bird.GetComponent<Bullet>();
        bullet.shooter = user;
        bullet.transform.position = user.transform.position;
        bullet.transform.localScale = user.transform.localScale;
        Vector2 dir = (user.target.transform.position - user.transform.position).normalized;
        bullet.ShootTo(damage, dir, 30, BulletEffectType.AOE);
    }
}
