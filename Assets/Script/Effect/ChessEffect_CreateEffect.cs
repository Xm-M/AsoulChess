using Language.Lua;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessEffect_CreateEffect : ChessEffect
{
    public GameObject effect;
    public override void InitChessEffect(Chess user, params Chess[] chesses)
    {
        //throw new System.NotImplementedException();
        foreach(var chess in chesses)
        {
            GameObject eff= ObjectPool.instance.Create(effect);
            eff.transform.position = chess.transform.position;
        }
    }

    public override void InitChessEffect(Chess user, List<Chess> chesses)
    {
        //throw new System.NotImplementedException();
        foreach (var chess in chesses)
        {
            GameObject eff = ObjectPool.instance.Create(effect);
            eff.transform.position = chess.transform.position;
        }
    }
    public void CreateEffect()
    {
        GameObject eff = ObjectPool.instance.Create(effect);
        eff.transform.position =  transform.position;
    }
}
