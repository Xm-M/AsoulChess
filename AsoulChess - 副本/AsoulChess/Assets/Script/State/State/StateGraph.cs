using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StateName{
    AttackState,
    MoveState,
    PrepareState,
    SkillState,
        
}
public enum TransitionName{
    InRangeTransition,
    ManaEnough,
    ManaLacking,
    OutRangeTransition,
    
}
[CreateAssetMenu(fileName = "StateGraph", menuName = "State/StateGraph")]
public class StateGraph : ScriptableObject
{
    public List<StateDate> States;
    public void ShowMessage(){
        foreach(var state in States){
            state.RefreshState();
        }
    }
}
[Serializable]
public class StateDate{
    public StateName stateName;
    [SerializeReference]
    public State state;
    public List<TransitionDate> transitions;

    public void RefreshState(){
        if(state==null||state.stateName!=stateName){
            Type t=Type.GetType(stateName.ToString());
            state=Activator.CreateInstance(t) as State;
            
        }
        foreach(var transition in transitions){
            transition.RefreshState();
        }
    }
    public StateDate Clone(){
        StateDate ans=new StateDate();
        ans.stateName=stateName;
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
    public TransitionName transitionName;
    [SerializeReference]
    public Transition transition;
    public StateName targetState;
    public void RefreshState(){
        if(transition==null||transition.transitionName!=transitionName){
            Type t=Type.GetType(transitionName.ToString());
            transition=Activator.CreateInstance(t) as Transition;
        }
    }
    public TransitionDate Clone(){
        TransitionDate ans = new TransitionDate();
        ans.transitionName=transitionName;
        ans.transition=transition.Clone();
        ans.targetState=targetState;
        return ans;
    }
}