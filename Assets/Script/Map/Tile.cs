using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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
        if (chess.propertyController.creator.chessTileType != TileType.None)
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
            if (chess.propertyController.creator.chessTileType != TileType.None)
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
public enum TileType
{
    Grass,
    Water,
    Stone,
    All,
    None,
    Occupation,
}