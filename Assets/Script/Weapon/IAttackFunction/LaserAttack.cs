using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserAttack : IAttackFunction
{
    public ChessEffect effect;
    public void Attack(Chess user, List<Chess> targets)
    {
        for(int i = 0; i < targets.Count; i++)
        {
            DamageMessege dm = new DamageMessege(user, targets[i],
                user.propertyController.GetAttack());
            user.propertyController.TakeDamage(dm);
        }
        effect?.InitChessEffect(user, targets);
    }
}
