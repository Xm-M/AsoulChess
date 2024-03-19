using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �ǾͲ�������ô�ิ�ӵ� 
/// �ҾͰ���������������������� 
/// Ȼ����ν�Ĳ�ͬ״̬�����ⲿ���ƣ�����ֻ����ı����
/// Ȼ�������ô���ɶ���״̬������������ͺ�
/// </summary>
public class ChessAnimatorController : MonoBehaviour,Controller
{
    public Animator animator;//����Ƕ����� û��ʲô�����
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
