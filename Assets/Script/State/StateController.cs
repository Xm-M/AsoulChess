using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
/// <summary>
/// 控制状态是否要归结为一个状态？
/// 如果归为同一个状态 那么动画要怎么搞？
/// 这个是真的难搞哦
/// 还是先把状态机考虑清楚吧
/// </summary>
[Serializable]
public class StateController:Controller
{
    protected Chess self;
    public StateDate currentState;
    public StateDate preState;
    public StateGraph stateGraph;
    public StateName EnterWarState=StateName.IdleState;
    Dictionary<StateName,StateDate> stateDic;
    //如果有多个异常状态 取最新状态覆盖 其中死亡优先级最高
    State anyState;//任意状态  
    Transition anyTransition;//任意转换条件 
    //
    public void InitController(Chess chess)
    {
        this.self=chess;
        stateDic=new Dictionary<StateName, StateDate>();
        foreach(var stateDate in stateGraph.States){
            StateDate newDate=stateDate.Clone();
            newDate.state.chess=self;
            stateDic.Add(newDate.state.stateName,newDate);
        }
    }//
    public void WhenControllerEnterWar(){
        anyState=null;
        anyTransition=null;
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
    }//
    public void ChangeAnyState(State aState,Transition aTransition)
    {
        //if(anyState)  
        anyState=aState;
        anyTransition=aTransition;
        if (currentState != null)
        {
            currentState.state.Exit(self);
        }
        preState=currentState;
    }
    public void ChangeState(StateName stateName)
    {
        ChangeState(stateDic[stateName]);
    }

    public void RevertToPreState()
    {
        ChangeState(preState);
    }
    
    //
    public void StateUpdate()
    {
        if (anyState != null)
        {
            anyState.Execute(self);
            if(anyTransition != null&&anyTransition.ifReach(self))
            {
                RevertToPreState();
            }
        }
        else if (currentState.state != null)
        {
            currentState.state.Execute(self);
            foreach (var transition in currentState.transitions)
            {
                if (transition.transition.ifReach(self))
                {

                    ChangeState(stateDic[transition.targetState]);
                    return;
                }
            }
        }
    }
}//
