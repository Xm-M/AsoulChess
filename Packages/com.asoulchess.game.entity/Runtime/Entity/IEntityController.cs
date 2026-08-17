namespace AsoulChess.Game.Entity
{
    /// <summary>Pluggable controller attached to a <see cref="GameEntity"/>.</summary>
    public interface IEntityController
    {
        void InitController(GameEntity entity);
        void OnEnterCombat();
        void OnLeaveCombat();
        void Tick(float deltaTime);
    }
}
