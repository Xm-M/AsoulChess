namespace AsoulChess.Game.Entity.Buffs
{
    /// <summary>Timed flat attack bonus on <see cref="Controllers.PropertyController.AttackBonus"/>.</summary>
    public sealed class TimedAttackBuff : TimedBuff
    {
        readonly float _bonus;

        public TimedAttackBuff(string id, float attackBonus, float durationSeconds)
            : base(id, durationSeconds)
        {
            _bonus = attackBonus;
        }

        TimedAttackBuff() { }

        protected override void OnTimedApply(GameEntity target)
        {
            if (target.Property != null)
                target.Property.AttackBonus += _bonus;
        }

        protected override void OnTimedRemove(GameEntity target)
        {
            if (target?.Property != null)
                target.Property.AttackBonus -= _bonus;
        }

        public override Buff Clone() => new TimedAttackBuff(Id, _bonus, Duration);
    }
}
