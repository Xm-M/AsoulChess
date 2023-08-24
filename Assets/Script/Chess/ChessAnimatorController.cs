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
    //�ٸ����� ��ͬ�ļ����ֲ�ͬ�Ķ���
    //��ͬ�������в�ͬ�Ĺ�������
    //��ͬ��������ͨ͸����������
    //���ڵ�������Ҫ��ô֪�����ŵ����Ƕζ�����
    //����˵�����ҿ��԰Ѷ������������� ����������
    public void StateAnimatorPlay(StateName stateName)
    {

    }
    //��Ҫһ���ı�״̬��Ӧ�����ĺ���
    //���Ҹо���û����setbool���ַ�ʽ�� ֱ�ӱ��涯������Play���ͺ��� 
    //����ֱ����animator.Play()�ķ������� ���Ҿ�Ҫ���ݲ�ͬ��
    //��Ҫ���� ��ǰ״̬->��Ӧ�Ķ����� Ȼ��Ҫһ���ı䵱ǰ״̬�Ͷ������ķ���
    //�����������ǰ���һ��ʹ�õ� Ҫ�ı�ĳ��������Ĭ��״̬��Ҫ��������һ���
    public StateName CurrentState { get; set; }
    public void ChangeStateAnimation(string animationName)
    {

    }
}
