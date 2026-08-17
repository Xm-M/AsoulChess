namespace AsoulChess.Game.Entity
{
    /// <summary>Game-side hook when an entity dies (e.g. pool recycle). Optional in P1-0.</summary>
    public interface IEntityRecycleHandler
    {
        void Recycle(GameEntity entity);
    }
}
