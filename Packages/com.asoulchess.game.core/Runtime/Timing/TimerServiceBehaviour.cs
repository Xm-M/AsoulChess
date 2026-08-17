using UnityEngine;

namespace AsoulChess.Game.Core.Timing
{
    /// <summary>Scene host that ticks an <see cref="ITimerService"/> each frame.</summary>
    public sealed class TimerServiceBehaviour : MonoBehaviour
    {
        ITimerService _service;

        public ITimerService Service => _service;

        public void Bind(ITimerService service)
        {
            _service = service;
        }

        void Update()
        {
            _service?.Tick(Time.unscaledDeltaTime);
        }
    }
}
