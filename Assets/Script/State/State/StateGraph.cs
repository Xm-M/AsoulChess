using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
public enum StateName{
    AttackState,
    MoveState,
    PrepareState,
    SkillState,
    IdleState,
    DeathState,
    DizzyState,
}
[CreateAssetMenu(fileName = "StateGraph", menuName = "State/StateGraph")]
public class StateGraph : ScriptableObject
{
    public List<StateDate> States;
    public State s;
}
[Serializable]
public class StateDate{
    [SerializeReference]
    public State state;
    public List<TransitionDate> transitions;
    public StateDate Clone(){
        StateDate ans=new StateDate();
        ans.state=state.Clone();
        ans.transitions=new List<TransitionDate>();
        foreach(var transisionDate in transitions){
            ans.transitions.Add(transisionDate.Clone());
        }
        return ans;
    }
}
[Serializable]
public class TransitionDate{
    [SerializeReference]
    [HideLabel]
    [HorizontalGroup(group:"hor",width:0.8f)]
    public Transition transition;
    [HorizontalGroup(group: "hor", width: 0.2f)]
    [HideLabel]
    public StateName targetState;
    public TransitionDate Clone(){
        TransitionDate ans = new TransitionDate();
        ans.transition=transition.Clone();
        ans.targetState=targetState;
        return ans;
    }
}