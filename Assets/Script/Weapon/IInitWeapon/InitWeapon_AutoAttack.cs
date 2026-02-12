using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitWeapon_AutoAttack : IInitWeapon
{
    public float attackInterval=1.5f;
    Timer timer;
    AttackController weapon;
    Chess user;
    public void InitWeapon(AttackController weapon)
    {
        this.weapon = weapon;
        user = weapon.master;
        timer = GameManage.instance.timerManage.AddTimer(AutoAttack, attackInterval, true);
        user.OnRemove.AddListener(OnDeath);
    }
    public void AutoAttack()
    {
        if (user.stateController.currentState.state.stateName != StateName.SkillState) {
            if ( weapon.weapon.FindEnemy(user)==-1) {
                weapon.TakeDamages();
            }
            else if (weapon.weapon.FindEnemy(user)>0)
            {
                weapon.TakeDamages();
            }
        }
    }
    public void OnDeath(Chess chess)
    {
        timer.Stop();
        timer = null;
    }
}
