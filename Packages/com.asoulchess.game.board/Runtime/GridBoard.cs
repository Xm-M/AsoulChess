using System.Collections.Generic;
using UnityEngine;

namespace AsoulChess.Game.Board
{
    /// <summary>Simple rectangular grid. No AVZ terrain rules.</summary>
    public sealed class GridBoard : IBoard
    {
        readonly GridTile[,] _tiles;
        readonly Dictionary<IBoardOccupant, Vector2Int> _occupantCells = new Dictionary<IBoardOccupant, Vector2Int>();

        public int Width { get; }
        public int Height { get; }
        public float CellSize { get; }
        public Vector3 Origin { get; }

        public GridBoard(int width, int height, float cellSize = 1f, Vector3 origin = default)
        {
            Width = Mathf.Max(1, width);
            Height = Mathf.Max(1, height);
            CellSize = Mathf.Max(0.01f, cellSize);
            Origin = origin;
            _tiles = new GridTile[Width, Height];
            for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                _tiles[x, y] = new GridTile(this, x, y);
        }

        public bool InBounds(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height;

        public bool TryGetTile(int x, int y, out ITile tile)
        {
            if (!InBounds(x, y))
            {
                tile = null;
                return false;
            }
            tile = _tiles[x, y];
            return true;
        }

        public ITile GetTile(int x, int y)
        {
            if (!InBounds(x, y))
                throw new System.ArgumentOutOfRangeException($"({x},{y}) out of board {Width}x{Height}");
            return _tiles[x, y];
        }

        public IEnumerable<ITile> GetNeighbors(int x, int y, bool includeDiagonals = false)
        {
            for (int dy = -1; dy <= 1; dy++)
            for (int dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0) continue;
                if (!includeDiagonals && dx != 0 && dy != 0) continue;
                if (TryGetTile(x + dx, y + dy, out var tile))
                    yield return tile;
            }
        }

        public bool TryOccupy(int x, int y, IBoardOccupant occupant)
        {
            if (occupant == null || !InBounds(x, y)) return false;
            var tile = _tiles[x, y];
            if (tile.Occupant != null && !ReferenceEquals(tile.Occupant, occupant))
                return false;

            if (_occupantCells.TryGetValue(occupant, out var old))
            {
                if (old.x == x && old.y == y) return true;
                _tiles[old.x, old.y].Occupant = null;
            }

            tile.Occupant = occupant;
            _occupantCells[occupant] = new Vector2Int(x, y);
            return true;
        }

        public bool Vacate(IBoardOccupant occupant)
        {
            if (occupant == null || !_occupantCells.TryGetValue(occupant, out var cell))
                return false;
            _tiles[cell.x, cell.y].Occupant = null;
            _occupantCells.Remove(occupant);
            return true;
        }

        public bool TryGetOccupantCell(IBoardOccupant occupant, out int x, out int y)
        {
            if (occupant != null && _occupantCells.TryGetValue(occupant, out var cell))
            {
                x = cell.x;
                y = cell.y;
                return true;
            }
            x = y = -1;
            return false;
        }

        public static int Manhattan(int x0, int y0, int x1, int y1) =>
            Mathf.Abs(x0 - x1) + Mathf.Abs(y0 - y1);
    }
}
