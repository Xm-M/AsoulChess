using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using UnityEngine.Events;

/// <summary>
/// 那么tile到底需不需要考虑那么多事情呢？他应该只提供信息，不提供任何其他东西
/// 就chess应该只能访问tile但是不能修改tile
/// </summary>
public class Tile : MonoBehaviour
{
    public Vector2Int mapPos;
    public TileType tileType;
    public Chess stander;//只考虑主植物
    public UnityEvent<Chess> OnPlant;
    public List<Chess> chessesIntile { get;protected set; }
    protected virtual void Awake()
    {
        chessesIntile = new List<Chess>();
    }
    public virtual void ChessEnter(Chess chess )
    {
        chess.moveController.standTile=this;
        chess.transform.position = transform.position;
        chessesIntile.Add(chess);
    }
    public virtual void ChessLeave(Chess chess)
    {
        if(stander==chess)stander=null;
        if(chessesIntile.Contains(chess))chessesIntile.Remove(chess);
        chess.moveController.standTile=null;
    }
    
    //protected virtual void OnMouseDown()
    //{
    //    //EventController.Instance.TriggerEvent<Tile>(EventName.WhenPlantChess.ToString(), this);
    //}
    public void PlantChess(Chess chess)
    {
        //if (chess.propertyController.creator.plantType!=PlantType.SupportPlant) stander = chess;
        if (chess.propertyController.creator.plantType == PlantType.MainPlant)
            stander = chess;
        OnPlant?.Invoke(chess);
    }
}

[Flags]
public enum TileType
{
    Grass=1<<0,
    Water=1<<1,
    Stone=1<<2,
    Occupation=1<<3,
    All,
}
[Flags]
public enum PlantType
{
    MainPlant=1<<0,
    SupportPlant=1<<1,
    PotPlant=1<<2,
}
