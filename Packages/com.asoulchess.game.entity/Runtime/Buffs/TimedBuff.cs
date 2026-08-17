using AsoulChess.Game.Core.Timing;

namespace AsoulChess.Game.Entity.Buffs
{
    /// <summary>Expires after duration via Core <see cref="ITimerService"/> (AVZ <c>TimeBuff</c>).</summary>
    public abstract class TimedBuff : Buff
    {
        protected float Duration { get; private set; }

        TimerHandle _handle;

        protected TimedBuff(string id, float durationSeconds)
        {
            Id = id;
            Duration = durationSeconds;
        }

        protected TimedBuff() { }

        public override void OnApply(GameEntity target)
        {
            BindTarget(target);
            OnTimedApply(target);
            ScheduleExpire(target);
        }

        public override void OnRemove(GameEntity target)
        {
            StopTimer(target);
            OnTimedRemove(target);
        }

        public override void OnReset(Buff incoming)
        {
            if (incoming is TimedBuff other)
            {
                Duration = other.Duration;
                if (Target != null)
                {
                    StopTimer(Target);
                    ScheduleExpire(Target);
                }
            }

            OnTimedReset(incoming);
        }

        protected virtual void OnTimedApply(GameEntity target) { }
        protected virtual void OnTimedReset(Buff incoming) { }
        protected virtual void OnTimedRemove(GameEntity target) { }

        void ScheduleExpire(GameEntity target)
        {
            if (Duration <= 0f)
            {
                MarkFinished();
                return;
            }

            if (target.Timers == null)
            {
                MarkFinished();
                return;
            }

            _handle = target.Timers.Delay(MarkFinished, Duration);
        }

        void StopTimer(GameEntity target)
        {
            if (_handle == null) return;
            target?.Timers?.Stop(_handle);
            _handle = null;
        }
    }
}
