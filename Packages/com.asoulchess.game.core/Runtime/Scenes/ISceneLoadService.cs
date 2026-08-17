using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AsoulChess.Game.Core.Scenes
{
    public interface ISceneTransitionView
    {
        void OnLoadStarted();
        void OnLoadProgress(float normalizedProgress);
        void OnLoadFinished();
        void PlayLoadOver();
        void PlayWin();
    }

    public interface ISceneLoadService
    {
        string CurrentSceneName { get; }
        bool IsLoading { get; }
        float LoadProgress { get; }

        void LoadScene(string sceneName, Action onLoaded = null, Action beforeUnload = null);
        void NotifyLoadOver();
        void PlayWinTransition();
    }
}
