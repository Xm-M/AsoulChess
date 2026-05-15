using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 自爆防具：与敌方 <see cref="Chess"/> 触发碰撞后，按 <see cref="IFindTarget"/> 索敌，
/// 对命中目标各造成一次 <see cref="PropertyController.GetAttack"/>（攻击力），并在 <see cref="MoveController.standTile"/> 处生成特效；随后 <see cref="BrokenArmor"/>。
/// 防具本身不扣耐久，受击伤害直接转交本体（见 <see cref="GetDamage"/>）。
/// </summary>
public class Armor_SelfDetonate : ArmorBase
{
    [SerializeReference]
    [LabelText("索敌（与 Weapon 配置方式相同）")]
    public IFindTarget findTarget;

    [Tooltip("注册到 ObjectPool 的特效预制；在 user.standTile 位置生成")]
    public GameObject tileExplosionEffect;

    [Tooltip("造成伤害的元素类型；与 SkillEffect_RandomExplode 等爆炸类一致时可填 Explode")]
    public ElementType damageElementType = ElementType.Explode;

    public Collider2D triggerCollider;

    readonly List<Chess> _targets = new List<Chess>(32);
    bool _detonated;

    public override void InitArmor()
    {
        if (triggerCollider == null)
            triggerCollider = GetComponent<Collider2D>();
        user.WhenEnterGame.AddListener(ResetArmor);
    }

    public override void ResetArmor(Chess chess)
    {
        _detonated = false;
        if (triggerCollider != null)
            triggerCollider.enabled = true;
    }

    /// <summary>不替本体承伤，传入伤害直接交给本体，避免自爆甲额外吞伤害。</summary>
    public override void GetDamage(DamageMessege dm)
    {
        if (user == null || dm == null)
            return;
        user.propertyController.GetDamage(dm);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (_detonated || user == null || user.IfDeath)
            return;
        if (collision.CompareTag(user.tag))
            return;
        if (!collision.TryGetComponent<Chess>(out Chess other) || other.IfDeath)
            return;
        Debug.Log("撞到了");
        TryDetonate(collision.transform);
    }

    void TryDetonate(Transform target)
    {
        if (_detonated || user == null || user.IfDeath)
            return;
        _detonated = true;
        if (triggerCollider != null)
            triggerCollider.enabled = false;

        //if (findTarget != null)
        //{
        //    findTarget.FindTarget(user, _targets);
        //    for (int i = 0; i < _targets.Count; i++)
        //    {
        //        Chess t = _targets[i];
        //        if (t == null || t.IfDeath)
        //            continue;

        //        user.skillController.DM.damageFrom = user;
        //        user.skillController.DM.damageTo = t;
        //        user.skillController.DM.damageElementType = damageElementType;
        //        user.skillController.DM.damage = user.propertyController.GetAttack();
        //        user.propertyController.TakeDamage(user.skillController.DM);
        //    }
        //}
        Chess t=target.GetComponent<Chess>();
        user.skillController.DM.damageFrom = user;
        user.skillController.DM.damageTo = t;
        user.skillController.DM.damageElementType = damageElementType;
        user.skillController.DM.damage = user.propertyController.GetAttack();
        user.propertyController.TakeDamage(user.skillController.DM);
        //Tile stand = user.moveController != null ? user.moveController.standTile : null;
        if (tileExplosionEffect != null && target != null)
        {
            Vector3 pos = target.transform.position;
            GameObject fx = ObjectPool.instance != null
                ? ObjectPool.instance.Create(tileExplosionEffect)
                : Object.Instantiate(tileExplosionEffect, pos, Quaternion.identity);
            if (fx != null)
            {
                
                //fx.transform.SetParent(target.transform, false);
                fx.transform.position=new Vector3(pos.x,pos.y,pos.z-0.1f);
            }
        }
        user.Death();
        BrokenArmor();
    }
}
