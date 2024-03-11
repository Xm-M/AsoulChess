using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[Serializable]
public class ChessManage : IManager
{
    public string playerMask;
    public string playerTag;
    public LayerMask layerMask;
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
    public virtual Chess CreateChess(PropertyCreator creator,Tile tile)
    {
        Chess chess = GameManage.instance.chessFactory.ChessCreate(creator.chessPre,creator.chessName);
        chesses.Add(chess);
        tile.ChessEnter(chess);
        chess.tag=playerTag;
        chess.gameObject.layer=LayerMask.NameToLayer( playerMask);
        chess.gameObject.SetActive(true);
        return chess;
    }
    public virtual void RecycleChess(Chess chess)
    {
        if (chesses.Contains(chess))
        {
            //Debug.Log("»ØÊÕ" + chess.name);
            chesses.Remove(chess);
            GameManage.instance.chessFactory.RecycleChess(chess, chess.propertyController.creator.chessName);
        }
    }
}
[Serializable]
public class EnemyManage:ChessManage
{
    public override Chess CreateChess(PropertyCreator creator, Tile tile)
    {
        Chess c= base.CreateChess(creator, tile);
        c.transform.right = Vector2.left;
        //Debug.Log("Create Enemy" + c.name);
        return c;
    }
}
