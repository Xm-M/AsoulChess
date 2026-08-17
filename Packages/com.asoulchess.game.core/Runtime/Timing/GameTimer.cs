using System;

namespace AsoulChess.Game.Core.Timing
{
    /// <summary>
    /// Gameplay timer handle (compatible with AVZ TimerManage.Timer API).
    /// </summary>
    public class GameTimer
    {
        Action _onFinish;
        float _finishTime;
        float _delayTime;
        bool _loop;
        bool _isFinished;

        internal Func<float> GetGameTime = () => 0f;

        public bool IsFinish => _isFinished;

        public void Start(Action onFinished, float delayTime, bool loop)
        {
            _onFinish = onFinished;
            _delayTime = delayTime;
            _loop = loop;
            _isFinished = false;
            _finishTime = GetGameTime() + delayTime;
        }

        public void Stop() => _isFinished = true;

        public void Update()
        {
            if (_isFinished) return;
            float gameTime = GetGameTime();
            if (gameTime < _finishTime) return;

            if (!_loop)
                _isFinished = true;
            else
                _finishTime = gameTime + _delayTime;

            _onFinish?.Invoke();
        }

        public float LeftTime() => _finishTime - GetGameTime();

        public void GetSaveState(out float remainingTime, out bool isLoop)
        {
            remainingTime = LeftTime();
            isLoop = _loop;
        }

        public void ResetTime() => _finishTime = GetGameTime() + _delayTime;

        public void ResetTime(float t)
        {
            _delayTime = t;
            _finishTime = GetGameTime() + t;
        }

        public void ChangeDelayTime(float newDelayTime)
        {
            _finishTime = _finishTime - _delayTime + newDelayTime;
            _delayTime = newDelayTime;
        }
    }
}
