using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[CreateAssetMenu(fileName ="NewProperty",menuName ="Message/Property")]
public class PropertyCreator : ScriptableObject
{
    public Property baseProperty;
    public Chess chessPre;
    public Sprite chessSprite;
    public byte chessLayer;
    public List<TileType> plantType;
    public Property GetClone()
    {
        Property newP = new Property(baseProperty);
        return newP;
    }
    public virtual bool IfCanBuyCard(int sunLight){
        if(sunLight>=baseProperty.price){
            return true;
        }
        return false;
    }
    public virtual bool IfCanPlant(Tile tile){         
        return (!tile.IfContainsLayer(chessLayer))&&plantType.Contains(tile.GetTileType());
    }
    public virtual void WhenChessLeaveWar(Chess chess) { 
    
    }
    public virtual void WhenChessEnterWar(Chess chess)
    {

    }
}
