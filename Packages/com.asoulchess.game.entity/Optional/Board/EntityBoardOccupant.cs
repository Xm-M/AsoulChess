using AsoulChess.Game.Board;

namespace AsoulChess.Game.Entity.Board
{
    sealed class EntityBoardOccupant : IBoardOccupant
    {
        readonly MoveController _move;

        public EntityBoardOccupant(MoveController move) => _move = move;

        public int CellId
        {
            get
            {
                if (_move == null || !_move.IsOnBoard || _move.Board == null)
                    return -1;
                return IBoard.ToCellId(_move.X, _move.Y, _move.Board.Width);
            }
        }
    }
}
