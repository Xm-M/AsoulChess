using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SunLightPanel : MonoBehaviour
{
    public static SunLightPanel instance;
    public Text sunLightText;
    private void Awake()
    {
        instance = this;
    }
    public int sunLight{get;private set;}
    public void ChangeSunLight(int num)
    {
        sunLight += num;
        sunLightText.text = sunLight.ToString();
        EventController.Instance.TriggerEvent(EventName.WhenSunLightChange.ToString());
    }

}
