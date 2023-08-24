 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Eat_Weapon : Weapon
{
    public AudioSource au;
    public void Eat(Chess chess)
    {
        if (chess.propertyController.GetSize() <= this.master.propertyController.GetSize())
        {
            chess.Death();
            au.Play();
        }
    }
}
