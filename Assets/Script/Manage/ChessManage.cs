using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[Serializable]
public class ChessManage : IManager
{
    public string playerTag;
    //public LayerMask layerMask;
    public List<Chess> chesses;
    public void InitManage()
    {
        chesses=new List<Chess>();
        EventController.Instance.AddListener(EventName.GameOver.ToString(), OnGameOver);
    }
    public void OnGameOver()
    {
        Debug.Log("清理队伍" + playerTag);
        List<Chess> list=new List<Chess>(chesses);
        for(int i = 0; i < list.Count; i++)
        {
            list[i].Death();
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
        chess.gameObject.layer=LayerMask.NameToLayer(playerTag);
        chess.gameObject.SetActive(true);
        chess.WhenChessEnterWar();
        return chess;
    }
    public virtual void RecycleChess(Chess chess)
    {
        if (chesses.Contains(chess))
        {
            //Debug.Log("回收" + chess.name);
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

public class ChessTeamManage
{
    public static ChessTeamManage Instance;
    public  ChessManage player;
    public  EnemyManage enemy;
    public ChessTeamManage()
    {
        player = new ChessManage();
        player.playerTag = "Player";
        enemy = new EnemyManage();
        enemy.playerTag = "Enemy";
        player.InitManage();
        enemy.InitManage();
        Instance=this;
    }
    public LayerMask GetEnemyLayer(GameObject gameObject)
    {
        if (gameObject.CompareTag(player.playerTag))
        {
            return LayerMask.GetMask(enemy.playerTag);
        }
        else
        {
            return LayerMask.GetMask(player.playerTag);
        }
    }
    public void RecycleChess(Chess chess)
    {
        if (chess.CompareTag(player.playerTag))
        {
            player.RecycleChess(chess);
        }
        else
        {
            enemy.RecycleChess(chess);
        }
    }
    public List<Chess> GetTeam(string tag)
    {
        if (tag==player.playerTag)
        {
            return player.chesses;
        }
        else
        {
            return enemy.chesses;
        }
    }
    public Chess CreateChess(PropertyCreator creator, Tile tile,string tag  )
    {
        if (tag==enemy.playerTag)
        {
            return enemy.CreateChess(creator, tile);
        }
        else
        {
            return player.CreateChess(creator, tile);
        }
    }
}

