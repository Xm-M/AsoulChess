using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class Skill  
{
    [SerializeField]private float interval = 1f;
    public string skillName;
    public float Interval { 
        get { return interval; }
    }
    //public AudioClip clip;
    public virtual void SkillEffect(Chess user)
    {
 
    }
    public virtual void OnSkillEnter(Chess user) { 

    }
    public virtual void OnSkillExit(Chess user)
    {
         
    }
    public virtual bool ifSkilOver(Chess chess)
    {
        if(chess.animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.99)
        {
            return true;
        }return false;
    }
    
}
