using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.EventSystems;
using System;

public class Tile : MonoBehaviour
{
    public Vector3Int cubePos;
    public Vector2Int mapPos;
    public AudioSource au ;//音乐的问题我们之后再考虑
    public bool ifPrePareTile;
    public TileType baseTileType;
    public Stack<TileType> typeStack;
    
    List<Chess> standers;
    List<byte> chessLayers;

    private void Awake()
    {
        standers = new List<Chess>();
        chessLayers = new List<byte>();
        typeStack = new Stack<TileType>();
        typeStack.Push(baseTileType);
    }
    public void ChessMoveIn(Chess chess)
    {
        chess.moveController.standTile = this;
        chess.transform.position = transform.position;
    }
    public void ChessEnter(Chess chess )
    {
        standers.Add(chess);
        chessLayers.Add(chess.propertyController.creator.chessLayer);
        chess.moveController.standTile=this;
        chess.transform.position = transform.position;
        if (chess.propertyController.creator.chessTileType != 0)
        {
            typeStack.Push(chess.propertyController.creator.chessTileType);
        }
        au.Play();
    }
    public void ChessLeave(Chess chess)
    {
        if (standers.Contains(chess))
        {
            standers.Remove(chess);
            chessLayers.Remove(chess.propertyController.creator.chessLayer);
            if (chess.propertyController.creator.chessTileType != 0)
            {
                typeStack.Pop();
            }
        }
    }
    public TileType GetTileType()
    {
        return typeStack.Peek();
    }
    public bool IfContainsLayer(byte layer)
    {
        return chessLayers.Contains(layer);
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

public enum PlantType
{

}
public class TileManage
{
    public bool ifCanPlant(TileType plant,TileType tile)
    {
        return (plant & tile)!=0;
    }
    public bool ifCanPut()
    {
        return false;
    }
}
