using System.Collections.Generic;
using UnityEngine;

namespace AsoulChess.Game.Board
{
    public interface IBoard
    {
        int Width { get; }
        int Height { get; }
        float CellSize { get; }
        Vector3 Origin { get; }

        bool InBounds(int x, int y);
        bool TryGetTile(int x, int y, out ITile tile);
        ITile GetTile(int x, int y);
        IEnumerable<ITile> GetNeighbors(int x, int y, bool includeDiagonals = false);

        bool TryOccupy(int x, int y, IBoardOccupant occupant);
        bool Vacate(IBoardOccupant occupant);
        bool TryGetOccupantCell(IBoardOccupant occupant, out int x, out int y);

        static int ToCellId(int x, int y, int width) => y * width + x;
        static void FromCellId(int cellId, int width, out int x, out int y)
        {
            x = cellId % width;
            y = cellId / width;
        }
    }
}
