using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[CreateAssetMenu(fileName ="NewProperty",menuName ="Message/Property")]
public class PropertyCreator : ScriptableObject
{
    public Property baseProperty;
    public Property GetClone()
    {
        Property newP = new Property(baseProperty);
        return newP;
    }
}
