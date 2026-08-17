using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AsoulChess.Game.Core.Scenes
{
    /// <summary>
    /// Async unload + additive load with optional transition view (AVZ SceneManage core).
    /// </summary>
    public sealed class SceneLoadService : ISceneLoadService
    {
        readonly MonoBehaviour _runner;
        ISceneTransitionView _transition;

        AsyncOperation _operation;
        float _sliderValue;
        const float ProgressSpeed = 1f;

        public string CurrentSceneName { get; private set; } = string.Empty;
        public bool IsLoading { get; private set; }
        public float LoadProgress { get; private set; }

        public SceneLoadService(MonoBehaviour runner, ISceneTransitionView transition = null)
        {
            _runner = runner ?? throw new ArgumentNullException(nameof(runner));
            _transition = transition;
        }

        public void SetTransitionView(ISceneTransitionView transition) => _transition = transition;

        public void LoadScene(string sceneName, Action onLoaded = null, Action beforeUnload = null)
        {
            if (string.IsNullOrEmpty(sceneName)) return;
            _runner.StartCoroutine(LoadRoutine(sceneName, onLoaded, beforeUnload));
        }

        public void NotifyLoadOver() => _transition?.PlayLoadOver();

        public void PlayWinTransition() => _transition?.PlayWin();

        IEnumerator LoadRoutine(string sceneName, Action onLoaded, Action beforeUnload)
        {
            IsLoading = true;
            beforeUnload?.Invoke();
            _transition?.OnLoadStarted();

            if (!string.IsNullOrEmpty(CurrentSceneName))
            {
                var unload = SceneManager.UnloadSceneAsync(CurrentSceneName);
                if (unload != null)
                    yield return unload;
            }

            CurrentSceneName = sceneName;
            _operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (_operation == null)
            {
                IsLoading = false;
                yield break;
            }

            _operation.allowSceneActivation = false;
            _sliderValue = 0f;

            while (_operation != null)
            {
                LoadProgress = _operation.progress >= 0.9f ? 1f : _operation.progress;
                _transition?.OnLoadProgress(_sliderValue);

                if (Mathf.Abs(_sliderValue - LoadProgress) > 0.01f)
                    _sliderValue = Mathf.Lerp(_sliderValue, LoadProgress, Time.unscaledDeltaTime * ProgressSpeed);
                else
                    _sliderValue = LoadProgress;

                if (_sliderValue >= 0.9f)
                {
                    _operation.allowSceneActivation = true;
                    _sliderValue = 0f;
                    _operation = null;
                    break;
                }

                yield return null;
            }

            onLoaded?.Invoke();
            _transition?.OnLoadFinished();
            IsLoading = false;
        }
    }
}
