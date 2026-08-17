using System;
using AsoulChess.Game.Entity.Buffs;

namespace AsoulChess.Game.Entity.Combat
{
    /// <summary>Damage payload (framework analogue of AVZ <c>DamageMessege</c>).</summary>
    [Serializable]
    public class DamageInfo
    {
        public GameEntity Source;
        public GameEntity Target;
        public float Amount;
        public DamageType Type = DamageType.Physical;
        public ElementType Element = ElementType.None;
        public bool IsCritical;

        /// <summary>Game-side floating combat text (AVZ: DamagePanel).</summary>
        public bool SuppressFloatingText;

        /// <summary>Applied on hit unless suppressed (AVZ: <c>takeBuff</c> template).</summary>
        public Buff AttachedBuff;

        /// <summary>Game-side attached effect e.g. hit buff (AVZ: <c>takeBuff</c>).</summary>
        public bool SuppressAttachedEffect;

        public DamageInfo() { }

        public DamageInfo(
            GameEntity source,
            GameEntity target,
            float amount,
            DamageType type = DamageType.Physical,
            ElementType element = ElementType.None)
        {
            Source = source;
            Target = target;
            Amount = amount;
            Type = type;
            Element = element;
        }
    }
}
