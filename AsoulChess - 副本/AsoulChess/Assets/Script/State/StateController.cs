using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateController : MonoBehaviour
{
    public Chess self;
    public StateDate currentState;
    public StateDate preState;
    public StateGraph stateGraph;

    public StateName EnterWarState=StateName.MoveState;
    
    Dictionary<StateName,StateDate> stateDic;

    public void InitStateController()
    {
        stateDic=new Dictionary<StateName, StateDate>();
        self = GetComponent<Chess>();
        foreach(var stateDate in stateGraph.States){
            StateDate newDate=stateDate.Clone();
            newDate.state.chess=self;
            stateDic.Add(newDate.stateName,newDate);

        }
        LeaveWar();
    }
    public void EnterWar(){
        Debug.Log(name+" EnterWar");
        ChangeState(stateDic[EnterWarState]);
    }
    public void LeaveWar(){
        Debug.Log("LeaveWar");
        ChangeState(stateDic[StateName.PrepareState]);
    }
    public void ChangeState(StateDate newState)
    {
        Debug.Log("ChangeTo"+newState.state);
        if(currentState.state!=null)
            currentState.state.Exit(self);
        preState = currentState;
        currentState = newState;
        if(currentState!=null)
            currentState.state.Enter(self);
    }

    public void RevertToPreState()
    {
        ChangeState(preState);
    }

    private void Update()
    {
        if (currentState.state!=null){
            currentState.state.Excute(self);
            foreach(var transition in currentState.transitions){
                if(transition.transition.ifReach(self)){
                    Debug.Log(currentState.state.stateName+" "+transition.transition+" "+transition.targetState);
                    ChangeState(stateDic[transition.targetState]);
                    return;
                }
            }
        }
    }
}
