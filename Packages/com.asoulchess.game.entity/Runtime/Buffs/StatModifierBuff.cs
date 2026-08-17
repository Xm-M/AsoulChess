using AsoulChess.Game.Entity.Controllers;

namespace AsoulChess.Game.Entity.Buffs
{
    /// <summary>
    /// Modifies <see cref="PropertyController"/> stats (AVZ <c>Buff_BaseValueBuff_*</c> pattern).
    /// Same-id refresh keeps the higher delta for positive gains.
    /// </summary>
    public sealed class StatModifierBuff : Buff
    {
        public EntityStatKind Stat { get; private set; }
        public float Delta { get; private set; }

        float _appliedDelta;
        bool _applied;

        public StatModifierBuff(string id, EntityStatKind stat, float delta)
        {
            Id = id;
            Stat = stat;
            Delta = delta;
        }

        StatModifierBuff() { }

        public override void OnApply(GameEntity target)
        {
            BindTarget(target);
            ApplyDelta(target, Stat, Delta);
            _appliedDelta = Delta;
            _applied = true;
        }

        public override void OnRemove(GameEntity target)
        {
            if (!_applied) return;
            ApplyDelta(target, Stat, -_appliedDelta);
            _applied = false;
            _appliedDelta = 0f;
        }

        public override void OnReset(Buff incoming)
        {
            if (incoming is not StatModifierBuff other || other.Stat != Stat || Target == null)
                return;

            if (other.Delta <= _appliedDelta)
                return;

            ApplyDelta(Target, Stat, -_appliedDelta);
            Delta = other.Delta;
            _appliedDelta = other.Delta;
            ApplyDelta(Target, Stat, _appliedDelta);
        }

        public override Buff Clone() => new StatModifierBuff(Id, Stat, Delta);

        static void ApplyDelta(GameEntity target, EntityStatKind stat, float delta)
        {
            var property = target?.Property;
            if (property == null || MathfApproxZero(delta))
                return;

            switch (stat)
            {
                case EntityStatKind.AttackRate:
                    property.ChangeAttack(delta);
                    break;
                case EntityStatKind.AccelerateRate:
                    property.ChangeAccelerateRate(delta);
                    break;
                case EntityStatKind.MoveAccelerateRate:
                    property.ChangeMoveAccelerateRate(delta);
                    break;
                case EntityStatKind.Armor:
                    property.ChangeArmor(delta);
                    break;
                case EntityStatKind.Crit:
                    property.ChangeCrit(delta);
                    break;
                case EntityStatKind.CritDamage:
                    property.ChangeCritDamage(delta);
                    break;
                case EntityStatKind.DodgeRate:
                    property.ChangeDodgeRate(delta);
                    break;
                case EntityStatKind.ExtraDamage:
                    property.ChangeExtraDamage(delta);
                    break;
                case EntityStatKind.ExtraDefence:
                    property.ChangeExtraDefence(delta);
                    break;
                case EntityStatKind.HpMax:
                    property.ChangeHpMax(delta);
                    break;
                case EntityStatKind.LifeStealing:
                    property.ChangeLifeStealing(delta);
                    break;
                case EntityStatKind.HealRate:
                    property.ChangeHealRate(delta);
                    break;
                case EntityStatKind.Size:
                    property.ChangeSize((int)delta);
                    break;
            }
        }

        static bool MathfApproxZero(float v) => v > -0.0001f && v < 0.0001f;
    }
}
