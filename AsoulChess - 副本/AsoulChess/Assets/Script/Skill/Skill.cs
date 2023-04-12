using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class Skill : ScriptableObject
{
    [SerializeField]private float interval = 1f;
    [SerializeField]private float manaCost;

    public float Interval { 
        get { return interval; }
    }
    public float ManaCost
    {
        get { return manaCost; }
    }
    public AudioClip clip;
    public virtual void SkillEffect(Chess user,params Chess[] target)
    {
 
    }
    public virtual void OnSkillEnter(Chess user, params Chess[] target) { }
    public virtual void OnSkillExit(Chess user, params Chess[] target)
    {
         
    }
    
}
