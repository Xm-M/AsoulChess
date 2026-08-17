using System;

namespace AsoulChess.Game.Core.Timing
{
    public interface IGameplayTimerService
    {
        float GameTime { get; }
        float TimeSpeed { get; set; }
        bool IsSimulationRunning { get; set; }

        GameTimer AddTimer(Action onFinished, float delayTime, bool loop = false);
        void Update();
        void ClearAll();
        void SetGameTimeForLoad(float gameTime);
    }
}
