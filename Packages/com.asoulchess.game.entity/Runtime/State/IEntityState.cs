using AsoulChess.Game.Entity.Controllers;

namespace AsoulChess.Game.Entity.State
{
    public interface IEntityState
    {
        EntityStateId Id { get; }

        void Enter(GameEntity entity, StateController controller);
        void Execute(GameEntity entity, StateController controller, float deltaTime);
        void Exit(GameEntity entity, StateController controller);

        IEntityState Clone();
    }
}
