using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using PixelsoftGames.PixelUI;
using AsoulChess.Game.Core.Scenes;
using AsoulChess.Game.Core.Services;

public class SceneManage : MonoBehaviour, ISceneTransitionView
{
    public static SceneManage instance;

    public float loadValue;
    public float sliderValue;
    public float speed = 1;
    public Image image;
    public ClockDemo clock;
    public GameObject canvers;

    SceneLoadService _loadService;

    void Start()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        _loadService = new SceneLoadService(this, this);
        GameServices.Register(sceneLoader: _loadService);

        if (GameManage.instance != null)
            GameManage.instance.sceneManage = this;
    }

    public void LoadOver() => _loadService?.NotifyLoadOver();

    public void LoadScene(LevelData roomType, UnityAction LoadOver = null, UnityAction beforeLoad = null)
    {
        EventController.Instance.TriggerEvent(EventName.WhenSceneLoad.ToString());
        LoadScene(roomType.sceneName, LoadOver, beforeLoad);
    }

    public void LoadScene(string name, UnityAction LoadOver = null, UnityAction beforeLoad = null)
    {
        _loadService?.LoadScene(name, () => LoadOver?.Invoke(), () => beforeLoad?.Invoke());
    }

    public void Win() => _loadService?.PlayWinTransition();

    void ISceneTransitionView.OnLoadStarted()
    {
        GetComponent<Animator>()?.SetBool("load", true);
        if (canvers != null) canvers.SetActive(false);
    }

    void ISceneTransitionView.OnLoadProgress(float normalizedProgress)
    {
        loadValue = normalizedProgress;
        if (loadValue != sliderValue)
        {
            sliderValue = Mathf.Lerp(sliderValue, loadValue, Time.unscaledDeltaTime * speed);
            if (Mathf.Abs(sliderValue - loadValue) < 0.01f)
                sliderValue = loadValue;
            if (clock != null)
                clock.load = (int)(24 * sliderValue);
        }
    }

    void ISceneTransitionView.OnLoadFinished()
    {
        GetComponent<Animator>()?.SetBool("load", false);
        sliderValue = 0;
        if (canvers != null) canvers.SetActive(true);
    }

    void ISceneTransitionView.PlayLoadOver() => GetComponent<Animator>()?.Play("LoadOver");

    void ISceneTransitionView.PlayWin() => GetComponent<Animator>()?.Play("winscene");
}
