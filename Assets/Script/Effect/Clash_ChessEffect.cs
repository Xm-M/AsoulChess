using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clash_ChessEffect : ChessEffect
{
    public GameObject clash;
    public override void InitChessEffect(Chess user, params Chess[] chesses)
    {
        //throw new System.NotImplementedException();
        foreach(var chess in chesses)
        {
            ObjectPool.instance.Create(clash).transform.position = chess.transform.position;
        }
    }

    public override void InitChessEffect(Chess user, List<Chess> chesses)
    {
        //throw new System.NotImplementedException();
        foreach (var chess in chesses)
        {
            ObjectPool.instance.Create(clash).transform.position = chess.transform.position;
        }
    }
}
