using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using PixelsoftGames.PixelUI;
using UnityEngine.UI;

public class SceneManage : MonoBehaviour
{
    public static SceneManage instance;
    public List<string> NextStage;
    public List<Sprite> pitures;
    public int stadgeNum;
    public int currentStadge=0;
    public int LevelStadge=0;
    public float loadValue;
    public float sliderValue;
    public float speed = 1;
    private AsyncOperation operation;
    public Image image;  
    public ClockDemo clock; 
    int n=0;//场景计数
    public GameObject canvers;
    private void Start()
    {
        if(instance == null)instance = this;
        else Destroy(gameObject);
    }
    public void LoadScene()
    {
        Debug.Log("load");
        if (operation != null) return;
        if (currentStadge < stadgeNum)
        {
            currentStadge++;
        }
        else
        {
            LevelStadge++;
            currentStadge = 0;
            EventController.Instance.TriggerEvent(EventName.WhenSceneLoad.ToString());
            StartCoroutine(AsyncLoading());
            if (n + 1 < NextStage.Count)
                n++;
            else n = 0;
        }
    }
    public void LoadScene(string name)
    {
        n = 0;
        if (name == "开始") currentStadge = stadgeNum;
        SceneManager.LoadScene(name);
    }
    IEnumerator AsyncLoading()
    {
        // 异步加载场景
        canvers.SetActive(false);
        GetComponent<Animator>().SetBool("load",true);
        operation = SceneManager.LoadSceneAsync(NextStage[n]);
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
                SetCloce();
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
    public void SetCloce()
    {
        if (n>0&& NextStage[n-1] == "雪地")
        {
            clock.SetSnowyDay();
            image.sprite = pitures[0];
            //image.SetNativeSize();
            return;
        }else if (n > 0 && NextStage[n - 1] == "森林")
        {
            clock.SetWindyDay();
            image.sprite = pitures[1];
            //image.SetNativeSize();
            return;
        }else if (n > 0 && NextStage[n - 1] == "地牢")
        {
            clock.SetStormyDay();
            image.sprite = pitures[2];
            //image.SetNativeSize();
            return;
        }else if (n > 0 && NextStage[n - 1] == "腐蚀")
        {
            clock.SetFestivalDay();
            image.sprite = pitures[3];
            //image.SetNativeSize();
            return;
        }
        clock.SetClearDay();
    }
}
