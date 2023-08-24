using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessAnimatorController : MonoBehaviour,Controller
{
    public Animator animator;

    [Serializable]
    public class HpAnimatorLevel
    {
        public string animatorName;
        public int level;
        [Range(0, 1)] public float hp; 
    }
    [Serializable]
    public class StatePairAnimationName
    {
        public StateName stateName;
        public string animationName;
    }
    public List<StatePairAnimationName> stateList;
    //public Dictionary<StateName>
    public List<HpAnimatorLevel> levelList;
    List<HpAnimatorLevel> currentLevelList;
    public void InitController(Chess chess)
    {
          currentLevelList = new List<HpAnimatorLevel>();
    }

    public void WhenControllerEnterWar()
    {
         for(int i = 0; i < levelList.Count; i++)
            currentLevelList.Add(levelList[i]);
    }

    public void WhenControllerLeaveWar()
    {
         currentLevelList.Clear();
    }    
    public void OnTakeDamage()
    {

    }
    //举个例子 不同的技能又不同的动画
    //不同的武器有不同的攻击动画
    //不同的死亡有通透的死亡动画
    //现在的问题是要怎么知道播放的是那段动画呢
    //比如说死亡我可以把动画绑定在死亡上 那其他的呢
    public void StateAnimatorPlay(StateName stateName)
    {

    }
    //需要一个改变状态对应动画的函数
    //那我感觉就没有用setbool这种方式了 直接保存动画名再Play不就好了 
    //答案是直接用animator.Play()的方法更好 那我就要根据不同的
    //需要的是 当前状态->对应的动画名 然后还要一个改变当前状态和动画名的方法
    //就下面两个是绑定在一起使用的 要改变某个动画的默认状态就要下面两个一起改
    public StateName CurrentState { get; set; }
    public void ChangeStateAnimation(string animationName)
    {

    }
}
