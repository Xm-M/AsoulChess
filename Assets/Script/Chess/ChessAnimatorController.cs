using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 那就不考虑那么多复杂的 
/// 我就把这个单纯当个动画播放器 
/// 然后所谓的不同状态就由外部控制，这里只负责改变变量
/// 然后具体怎么变由动画状态机连线来处理就好
/// </summary>
public class ChessAnimatorController : MonoBehaviour,Controller
{
    public Animator animator;//这个是动画器 没有什么问题哈
    [Serializable]
    public class StatePairAnimationName
    {
        public StateName stateName;
        public string animName;
    }
    public List<StatePairAnimationName> stateList;
    public List<string> variateNames;
    AudioController audioController;
   
    public void StateAnimatorPlay(StateName stateName)
    {
        for(int i = 0; i < stateList.Count; i++)
        {
            if (stateList[i].stateName== stateName)
            {
                animator.Play(stateList[i].animName);
                return;
            }
        }
        //PlayAnimation();
    }
     
    public void PlayAudio(string player)
    {
        audioController.PlayAudio(player);
    }

    public void InitController(Chess chess)
    {
         //audioController=chess.audioController;
    }

    public void WhenControllerEnterWar()
    {
         foreach(var v in variateNames)
            animator.SetBool(v, false);
    }

    public void WhenControllerLeaveWar()
    {
        foreach (var v in variateNames)
            animator.SetBool(v, false);
    }
    public void SetVariateTrue(string vname)
    {
        animator.SetBool(vname, true);
    }
    public void SetVariateFalse(string vname)
    {
        animator.SetBool(vname, false);
    }
}
