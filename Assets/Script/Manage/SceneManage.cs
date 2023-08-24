using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using PixelsoftGames.PixelUI;
using UnityEngine.UI;

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
    private void Start()
    {
        if(instance == null)instance = this;
        else Destroy(gameObject);
    }
    public void LoadScene(RoomType roomType)
    {
        EventController.Instance.TriggerEvent(EventName.WhenSceneLoad.ToString());
        StartCoroutine(AsyncLoading(roomType.sceneName));
    }
    public void LoadScene(string name)
    {
         SceneManager.LoadScene(name);
        
    }
    IEnumerator AsyncLoading(string sceneName)
    {
        // 异步加载场景
        canvers.SetActive(false);
        GetComponent<Animator>().SetBool("load",true);
        operation = SceneManager.LoadSceneAsync(sceneName);
        // 阻止当加载完成自动切换
        operation.allowSceneActivation = false;

        yield return operation;
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
                // 插值运算（进度条向当前加载进度趋近）
                sliderValue = Mathf.Lerp(sliderValue, loadValue, Time.deltaTime * speed);
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
