using System;
using AsoulChess.Game.Entity.Controllers;

namespace AsoulChess.Game.Entity.Buffs
{
    /// <summary>
    /// Buff base (framework analogue of AVZ <c>Buff</c>).
    /// Game-specific buffs subclass this; attach VFX/UI via events or subclasses in AVZ.
    /// </summary>
    public abstract class Buff
    {
        /// <summary>Unique key per target (AVZ: <c>buffName</c>).</summary>
        public string Id { get; set; }

        public bool IsFinished { get; private set; }

        public GameEntity Target { get; private set; }

        internal BuffController Owner { get; set; }

        public abstract void OnApply(GameEntity target);
        public abstract void OnRemove(GameEntity target);
        public virtual void Tick(float deltaTime) { }

        /// <summary>Same-id refresh (AVZ: <c>BuffReset</c>).</summary>
        public virtual void OnReset(Buff incoming) { }

        public virtual Buff Clone() => (Buff)MemberwiseClone();

        protected void BindTarget(GameEntity target)
        {
            Target = target;
        }

        protected void MarkFinished()
        {
            if (IsFinished) return;
            IsFinished = true;
            Owner?.NotifyFinished(this);
        }
    }
}
