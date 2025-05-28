using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 特殊的地面效果
/// </summary>
public abstract class TileEffect : MonoBehaviour
{
    public abstract void EnterTile(Tile tile);

    public abstract  void ResetTileEffect();

    public abstract void LeaveTile(Tile tile);

    protected abstract void OnTriggerEnter2D(Collider2D collision);
    protected abstract void OnTriggerStay2D(Collider2D collision);
     
}
