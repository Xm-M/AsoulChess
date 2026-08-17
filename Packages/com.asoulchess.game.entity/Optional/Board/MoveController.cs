using AsoulChess.Game.Board;
using AsoulChess.Game.Entity;
using UnityEngine;

namespace AsoulChess.Game.Entity.Board
{
    /// <summary>Grid movement via <see cref="IBoard"/> occupy/vacate. Optional — not part of P1-0 core entity package.</summary>
    public sealed class MoveController : IEntityController
    {
        GameEntity _entity;
        IBoard _board;
        EntityBoardOccupant _occupant;
        public bool SnapTransformToTile { get; set; } = true;

        public IBoard Board => _board;
        public int X { get; private set; } = -1;
        public int Y { get; private set; } = -1;
        public bool IsOnBoard => X >= 0 && Y >= 0;

        public void InitController(GameEntity entity)
        {
            _entity = entity;
            _occupant = new EntityBoardOccupant(this);
        }

        public void BindBoard(IBoard board) => _board = board;

        public void OnEnterCombat() { }
        public void OnLeaveCombat() => LeaveBoard();
        public void Tick(float deltaTime) { }

        public bool TryPlace(int x, int y)
        {
            if (_board == null || _entity == null) return false;
            if (!_board.TryOccupy(x, y, _occupant)) return false;
            X = x;
            Y = y;
            Snap();
            return true;
        }

        public bool TryMoveTo(int x, int y)
        {
            if (!IsOnBoard) return TryPlace(x, y);
            if (_board == null) return false;
            if (!_board.InBounds(x, y)) return false;
            if (!_board.TryGetTile(x, y, out var tile) || (!tile.IsEmpty && !ReferenceEquals(tile.Occupant, _occupant)))
                return false;
            if (!_board.TryOccupy(x, y, _occupant)) return false;
            X = x;
            Y = y;
            Snap();
            return true;
        }

        /// <summary>One orthogonal step toward target cell (greedy, no pathfinding).</summary>
        public bool TryStepToward(int tx, int ty)
        {
            if (!IsOnBoard || _board == null) return false;
            int dx = tx - X;
            int dy = ty - Y;
            if (dx == 0 && dy == 0) return true;

            if (Mathf.Abs(dx) >= Mathf.Abs(dy))
            {
                int nx = X + (dx > 0 ? 1 : -1);
                if (TryMoveTo(nx, Y)) return true;
                if (dy != 0)
                {
                    int ny = Y + (dy > 0 ? 1 : -1);
                    return TryMoveTo(X, ny);
                }
            }
            else
            {
                int ny = Y + (dy > 0 ? 1 : -1);
                if (TryMoveTo(X, ny)) return true;
                if (dx != 0)
                {
                    int nx = X + (dx > 0 ? 1 : -1);
                    return TryMoveTo(nx, Y);
                }
            }
            return false;
        }

        public void LeaveBoard()
        {
            _board?.Vacate(_occupant);
            X = Y = -1;
        }

        void Snap()
        {
            if (!SnapTransformToTile || _board == null || !IsOnBoard) return;
            if (_board.TryGetTile(X, Y, out var tile))
                _entity.transform.position = tile.WorldPosition;
        }
    }
}
