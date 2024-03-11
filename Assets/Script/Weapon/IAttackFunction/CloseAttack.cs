using System.Collections.Generic;

public class CloseAttack : IAttackFunction
{
    public void Attack(Chess user, List<Chess> targets)
    {
        if (targets != null && targets.Count > 0)
        {
            DamageMessege DM;
            float damage = user.propertyController.GetAttack();
            for (int i = 0; i < targets.Count; i++)
            {
                if (!targets[i].IfDeath)
                {
                    DM = new DamageMessege();
                    DM.damageFrom = user;
                    DM.damageTo = targets[i];
                    DM.damage = damage;
                    user.propertyController.TakeDamage(DM);
                }
            }

        }
    }
}
