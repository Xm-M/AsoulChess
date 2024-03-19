using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[Serializable]
public class StateController:Controller
{
    protected Chess self;
    public StateDate currentState;
    public StateDate preState;
    public StateGraph stateGraph;
    public StateName EnterWarState=StateName.IdleState;
    
    Dictionary<StateName,StateDate> stateDic;
    bool frozen;
    float t, frozenTime;
    public void InitController(Chess chess)
    {
        this.self=chess;
        stateDic=new Dictionary<StateName, StateDate>();
        foreach(var stateDate in stateGraph.States){
            StateDate newDate=stateDate.Clone();
            newDate.state.chess=self;
            stateDic.Add(newDate.state.stateName,newDate);
        }
    }
    public void WhenControllerEnterWar(){
        frozen = false;
        ChangeState(stateDic[EnterWarState]);
    }
    public void WhenControllerLeaveWar(){
        //ChangeState(stateDic[StateName.PrepareState]);
    }
    public void ChangeState(StateDate newState)
    {
        if(currentState.state!=null)
            currentState.state.Exit(self);
        preState = currentState;
        currentState = newState;
        if(currentState!=null)
            currentState.state.Enter(self);
    }
    public void ChangeState(StateName stateName)
    {
        ChangeState(stateDic[stateName]);
    }

    public void RevertToPreState()
    {
        ChangeState(preState);
    }
    public void FrozenState(float forzent)
    {
        frozen = true;
        t = 0;
        if (currentState.state.stateName == StateName.DizzyState)
        {
            frozenTime = Mathf.Max(forzent, frozenTime);
            //self.animator.speed = 1;
        }
        else
        {
            frozenTime= forzent;
            //self.animator.speed = 0;
            ChangeState(StateName.DizzyState);
            
        }
    }
    public void ThawState()
    {
        t = 0;
        frozen = false;
        frozenTime=0;
        //self.animator.speed = 1;
        RevertToPreState();
        
    }

    public void StateUpdate()
    {
        if (frozen)
        {
            t+=Time.deltaTime;

        }
        else if (currentState.state!=null){
            currentState.state.Execute(self);
            foreach(var transition in currentState.transitions){
                if(transition.transition.ifReach(self)){
                     
                    ChangeState(stateDic[transition.targetState]);
                    return;
                }
            }
        }
    }
}
