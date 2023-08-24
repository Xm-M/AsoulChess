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
    public Image image;  //���صı���ͼ
    public ClockDemo clock; //�����ʵ���ǽ�����
    int n=0;//��������
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
        // �첽���س���
        canvers.SetActive(false);
        GetComponent<Animator>().SetBool("load",true);
        operation = SceneManager.LoadSceneAsync(sceneName);
        // ��ֹ����������Զ��л�
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
                // operation.progress��ֵ���Ϊ0.9
                loadValue = 1.0f;
            }
            if (loadValue != sliderValue)
            {
                // ��ֵ���㣨��������ǰ���ؽ���������
                sliderValue = Mathf.Lerp(sliderValue, loadValue, Time.deltaTime * speed);
                // �����ֵ����һֱ����
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
