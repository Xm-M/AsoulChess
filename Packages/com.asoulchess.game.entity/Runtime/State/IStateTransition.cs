namespace AsoulChess.Game.Entity.State
{
    public interface IStateTransition
    {
        bool Evaluate(GameEntity entity);
        IStateTransition Clone();
    }
}
