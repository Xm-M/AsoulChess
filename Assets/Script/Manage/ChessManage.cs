using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[Serializable]
public class ChessManage : IManager
{
    public LayerMask playerMask;
    public string playerTag;
    public List<Chess> chesses;
    public void InitManage()
    {
         chesses=new List<Chess>();
    }
    public void OnGameOver()
    {
        for(int i = 0; i < chesses.Count; i++)
        {
            chesses[i].Death();
        }
        chesses.Clear();
    }
    public void OnGameStart()
    {
         
    }
    public Chess CreateChess(PropertyCreator creator,Tile tile)
    {
        Chess chess = GameManage.instance.chessFactory.ChessCreate(creator.chessPre,creator.chessName);
        chesses.Add(chess);
        tile.ChessEnter(chess);
        chess.tag=playerTag;
        //chess.gameObject.layer=playerMask;
        chess.gameObject.SetActive(true);
        return chess;
    }
    public void RecycleChess(Chess chess)
    {
        if (chesses.Contains(chess))
        {
            chesses.Remove(chess);
            GameManage.instance.chessFactory.RecycleChess(chess, chess.propertyController.creator.chessName);
        }
    }
}
