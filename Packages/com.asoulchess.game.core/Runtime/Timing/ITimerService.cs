using System;

namespace AsoulChess.Game.Core.Timing
{
    public interface ITimerService
    {
        float TimeScale { get; set; }
        float ElapsedTime { get; }

        /// <summary>Schedule a callback after delaySeconds (scaled by TimeScale).</summary>
        TimerHandle Delay(Action onFinished, float delaySeconds, bool loop = false);

        void Stop(TimerHandle handle);
        void ClearAll();
        void Tick(float unscaledDeltaTime);
    }
}
