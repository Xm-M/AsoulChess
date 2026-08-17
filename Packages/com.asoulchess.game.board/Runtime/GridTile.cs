using UnityEngine;

namespace AsoulChess.Game.Board
{
    public sealed class GridTile : ITile
    {
        readonly GridBoard _board;

        public int X { get; }
        public int Y { get; }
        public int CellId => IBoard.ToCellId(X, Y, _board.Width);
        public IBoardOccupant Occupant { get; internal set; }
        public bool IsEmpty => Occupant == null;

        public Vector3 WorldPosition =>
            _board.Origin + new Vector3((X + 0.5f) * _board.CellSize, 0f, (Y + 0.5f) * _board.CellSize);

        public GridTile(GridBoard board, int x, int y)
        {
            _board = board;
            X = x;
            Y = y;
        }
    }
}
