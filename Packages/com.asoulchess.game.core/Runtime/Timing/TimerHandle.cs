using System;

namespace AsoulChess.Game.Core.Timing
{
    /// <summary>Opaque handle returned by <see cref="ITimerService.Delay"/>.</summary>
    public sealed class TimerHandle
    {
        internal Action OnFinished;
        internal float FinishAt;
        internal float DelaySeconds;
        internal bool Loop;
        internal bool IsFinished;

        internal void Reset(Action onFinished, float finishAt, float delaySeconds, bool loop)
        {
            OnFinished = onFinished;
            FinishAt = finishAt;
            DelaySeconds = delaySeconds;
            Loop = loop;
            IsFinished = false;
        }

        internal void MarkFinished() => IsFinished = true;
    }
}
