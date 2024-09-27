using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitWeapon_ResetAnimator : IInitWeapon
{
    public string id;
    public int baseValue;
    public void InitWeapon(AttackController weapon)
    {
        weapon.master.animator.SetInteger(id,baseValue);
    }
}
