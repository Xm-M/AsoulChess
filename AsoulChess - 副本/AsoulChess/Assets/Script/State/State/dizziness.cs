using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="DizzinessState",menuName ="State/DizzinessState")]
public class dizziness : State
{
    public static dizziness instance;
    public Dictionary<Chess,float> dic=new Dictionary<Chess, float>();
    private void OnEnable()
    {
        if (instance == null)
            instance = this;
    }
    public override void Enter(Chess chess)
    {
        base.Enter(chess);
    }
    public override void Excute(Chess chess)
    {
        dic[chess]-=Time.deltaTime;
        if (dic[chess] < 0)
        {
            //chess.stateController.ChangeState(MoveState.instance);
        }
        base.Excute(chess);
    }
    public override void Exit(Chess chess)
    {
        base.Exit(chess);
    }
    public void AddChess(Chess chess,float time)
    {
        if(!dic.ContainsKey(chess))dic.Add(chess, time);
        dic[chess] = time;
    }
    public override State Clone()
    {
        return new dizziness();
    }
}
