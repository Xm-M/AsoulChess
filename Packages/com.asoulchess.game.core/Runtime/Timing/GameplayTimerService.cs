using System;
using System.Collections.Generic;
using UnityEngine;

namespace AsoulChess.Game.Core.Timing
{
    /// <summary>
    /// AVZ-compatible timer service: GameTime, loop timers, LeftTime/ResetTime.
    /// </summary>
    public sealed class GameplayTimerService : IGameplayTimerService
    {
        readonly List<GameTimer> _active = new List<GameTimer>();
        readonly Queue<GameTimer> _pool = new Queue<GameTimer>();
        Func<GameTimer> _createTimer = () => new GameTimer();

        public float GameTime { get; private set; }
        public float TimeSpeed { get; set; } = 1f;
        public bool IsSimulationRunning { get; set; } = true;

        public void SetTimerFactory(Func<GameTimer> factory)
        {
            if (factory != null)
                _createTimer = factory;
        }

        public GameTimer AddTimer(Action onFinished, float delayTime, bool loop = false)
        {
            var timer = _pool.Count > 0 ? _pool.Dequeue() : _createTimer();
            timer.GetGameTime = () => GameTime;
            timer.Start(onFinished, delayTime, loop);
            _active.Add(timer);
            return timer;
        }

        public void Update()
        {
            if (!IsSimulationRunning) return;

            GameTime += Time.deltaTime;

            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var timer = _active[i];
                if (timer.IsFinish)
                {
                    _active.RemoveAt(i);
                    _pool.Enqueue(timer);
                    continue;
                }

                timer.Update();
                if (timer.IsFinish)
                {
                    _active.RemoveAt(i);
                    _pool.Enqueue(timer);
                }
            }
        }

        public void ClearAll()
        {
            _active.Clear();
            while (_pool.Count > 0)
                _pool.Dequeue();
        }

        public void SetGameTimeForLoad(float gameTime) => GameTime = gameTime;
    }
}
