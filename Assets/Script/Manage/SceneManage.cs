using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using PixelsoftGames.PixelUI;
using UnityEngine.UI;
using UnityEngine.Events;

public class SceneManage : MonoBehaviour
{
    public static SceneManage instance;
    public float loadValue;
    public float sliderValue;
    public float speed = 1;
    private AsyncOperation operation;
    public Image image;  //加载的背景图
    public ClockDemo clock; //这个其实就是进度条
    int n=0;//场景计数
    public GameObject canvers;
    string currentScene ;
    private void Start()
    {
        if(instance == null)instance = this;
        else Destroy(gameObject);
        currentScene = "开始";
    }
    public void LoadOver()
    {
        GetComponent<Animator>().Play("LoadOver");
    }
    public void LoadScene(LevelData roomType, UnityAction LoadOver = null, UnityAction beforeLoad = null)
    {
        EventController.Instance.TriggerEvent(EventName.WhenSceneLoad.ToString());
        StartCoroutine(AsyncLoading(roomType.sceneName,LoadOver,beforeLoad));
    }
    public void LoadScene(string name, UnityAction LoadOver = null, UnityAction beforeLoad = null)
    {
        StartCoroutine(AsyncLoading(name, LoadOver,beforeLoad));
        
    }
    IEnumerator AsyncLoading(string sceneName,UnityAction LoadOver=null,UnityAction beforeLoad=null)
    {
        beforeLoad?.Invoke();
        operation= SceneManager.UnloadSceneAsync(currentScene);
        yield return operation;
        // 异步加载场景
        
        GetComponent<Animator>().SetBool("load",true);
         
        canvers.SetActive(false);
        currentScene = sceneName;
        operation = SceneManager.LoadSceneAsync(sceneName,LoadSceneMode.Additive);
        // 阻止当加载完成自动切换 
        operation.allowSceneActivation = false;
        yield return operation;
        LoadOver?.Invoke();
    }

    public void Win()
    {
        GetComponent<Animator>().Play("winscene");
    }

    private void Update()
    {
        if (operation != null)
        {
            loadValue = operation.progress;
            if (operation.progress >= 0.9f)
            {
                // operation.progress的值最大为0.9
                loadValue = 1.0f;
            }
            if (loadValue != sliderValue)
            {
                // 使用 unscaledDeltaTime，避免暂停时(timeScale=0)加载进度卡住
                sliderValue = Mathf.Lerp(sliderValue, loadValue, Time.unscaledDeltaTime * speed);
                // 避免插值运算一直进行
                if (Mathf.Abs(sliderValue - loadValue) < 0.01f)
                {
                    sliderValue = loadValue;
                }
                clock.load = (int)(24 * sliderValue);
                //SetCloce();
            }
            if (sliderValue >= 0.9)
            {
                operation.allowSceneActivation = true;
                GetComponent<Animator>().SetBool("load", false);
                sliderValue = 0;
                operation = null;
                canvers.SetActive(true);
            }
        }
    }
}
