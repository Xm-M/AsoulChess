using System.Collections.Generic;
using UnityEngine;

public class CloseAttack : IAttackFunction
{
    public ChessEffect effect;
    public DamageMessege DM;
    public void Attack(Chess user, List<Chess> targets)
    {
        if (targets != null && targets.Count > 0)
        {
            float damage = user.propertyController.GetAttack();
            for (int i = 0; i < targets.Count; i++)
            {
                if (!targets[i].IfDeath)
                {
                    DM.damageFrom = user;
                    DM.damageTo = targets[i];
                    DM.damage = damage;
                    user.propertyController.TakeDamage(DM);
                }
            }
            effect?.InitChessEffect(user, targets);
        }
    }
}
public class ElementCloseAttack : IAttackFunction
{
    public ChessEffect effect;
    public DamageMessege DM;
    public float skillDamge;
    public void Attack(Chess user, List<Chess> targets)
    {
        if (targets != null && targets.Count > 0)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                if (!targets[i].IfDeath)
                {
                    DM.damageFrom = user;
                    DM.damageTo = targets[i];
                    DM.damage = skillDamge;
                    Debug.Log("Ôì³ÉÉËº¦" + DM.damage);
                    user.propertyController.TakeDamage(DM);
                }
            }
            effect?.InitChessEffect(user, targets);
        }
    }
}