using UnityEngine;

namespace AsoulChess.Game.Board
{
    public interface ITile
    {
        int X { get; }
        int Y { get; }
        int CellId { get; }
        Vector3 WorldPosition { get; }
        IBoardOccupant Occupant { get; }
        bool IsEmpty { get; }
    }
}
