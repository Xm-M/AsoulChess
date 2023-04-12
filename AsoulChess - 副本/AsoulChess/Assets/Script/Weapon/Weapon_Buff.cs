using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon_Buff : Weapon
{
    public Buff buff;
    //public string name;
    public override void TakeDamage ()
    {
        base.TakeDamage ();
        master.target.buffController.AddBuff(master,buff);
    }
}
