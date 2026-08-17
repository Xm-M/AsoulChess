namespace AsoulChess.Game.Entity.Buffs
{
    /// <summary>Applies a <see cref="StatModifierBuff"/> for a duration then removes it.</summary>
    public sealed class TimedStatModifierBuff : TimedBuff
    {
        readonly StatModifierBuff _statTemplate;

        public TimedStatModifierBuff(string id, EntityStatKind stat, float delta, float durationSeconds)
            : base(id, durationSeconds)
        {
            _statTemplate = new StatModifierBuff(id + ".stat", stat, delta);
        }

        TimedStatModifierBuff() { }

        protected override void OnTimedApply(GameEntity target) => target.Buffs?.Add(_statTemplate);

        protected override void OnTimedRemove(GameEntity target) => target.Buffs?.TryFinish(_statTemplate.Id);

        public override Buff Clone() =>
            new TimedStatModifierBuff(Id, _statTemplate.Stat, _statTemplate.Delta, Duration);
    }
}
