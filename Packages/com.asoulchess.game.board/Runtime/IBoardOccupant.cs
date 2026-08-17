namespace AsoulChess.Game.Board
{
    /// <summary>Something that can stand on a tile. Board does not reference Entity.</summary>
    public interface IBoardOccupant
    {
        int CellId { get; }
    }
}
