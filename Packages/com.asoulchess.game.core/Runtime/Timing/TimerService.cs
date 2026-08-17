using System;
using System.Collections.Generic;

namespace AsoulChess.Game.Core.Timing
{
    /// <summary>
    /// Pooled timer service (inspired by AVZ TimerManage, without level/event coupling).
    /// Drive via <see cref="Tick"/> or <see cref="TimerServiceBehaviour"/>.
    /// </summary>
    public sealed class TimerService : ITimerService
    {
        readonly List<TimerHandle> _active = new List<TimerHandle>();
        readonly Queue<TimerHandle> _pool = new Queue<TimerHandle>();

        public float TimeScale { get; set; } = 1f;
        public float ElapsedTime { get; private set; }

        public TimerHandle Delay(Action onFinished, float delaySeconds, bool loop = false)
        {
            if (onFinished == null)
                throw new ArgumentNullException(nameof(onFinished));
            if (delaySeconds < 0f)
                delaySeconds = 0f;

            var handle = _pool.Count > 0 ? _pool.Dequeue() : new TimerHandle();
            handle.Reset(onFinished, ElapsedTime + delaySeconds, delaySeconds, loop);
            _active.Add(handle);
            return handle;
        }

        public void Stop(TimerHandle handle)
        {
            if (handle == null || handle.IsFinished) return;
            handle.MarkFinished();
        }

        public void ClearAll()
        {
            for (int i = 0; i < _active.Count; i++)
            {
                _active[i].MarkFinished();
                _pool.Enqueue(_active[i]);
            }
            _active.Clear();
        }

        public void Tick(float unscaledDeltaTime)
        {
            if (TimeScale <= 0f || unscaledDeltaTime <= 0f) return;

            ElapsedTime += unscaledDeltaTime * TimeScale;

            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var handle = _active[i];
                if (handle.IsFinished)
                {
                    _active.RemoveAt(i);
                    _pool.Enqueue(handle);
                    continue;
                }

                if (ElapsedTime < handle.FinishAt) continue;

                var callback = handle.OnFinished;
                if (!handle.Loop)
                {
                    handle.MarkFinished();
                    _active.RemoveAt(i);
                    _pool.Enqueue(handle);
                }
                else
                {
                    handle.FinishAt = ElapsedTime + handle.DelaySeconds;
                }

                callback?.Invoke();
            }
        }
    }
}
