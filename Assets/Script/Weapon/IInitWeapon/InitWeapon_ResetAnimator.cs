using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 这几把又是个啥
/// </summary>
public class InitWeapon_ResetAnimator : IInitWeapon
{
    public string id;
    public int baseValue;
    public void InitWeapon(AttackController weapon)
    {
        //weapon.master.animator.SetInteger(id,baseValue); 
    }
}
