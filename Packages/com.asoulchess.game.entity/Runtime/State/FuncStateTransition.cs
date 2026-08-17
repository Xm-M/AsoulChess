using System;

namespace AsoulChess.Game.Entity.State
{
    /// <summary>Delegate-based transition for samples and game code.</summary>
    public sealed class FuncStateTransition : IStateTransition
    {
        readonly Func<GameEntity, bool> _predicate;

        public FuncStateTransition(Func<GameEntity, bool> predicate) =>
            _predicate = predicate ?? (_ => false);

        public bool Evaluate(GameEntity entity) => _predicate(entity);

        public IStateTransition Clone() => new FuncStateTransition(_predicate);
    }
}
