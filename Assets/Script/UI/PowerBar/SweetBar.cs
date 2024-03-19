using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.UI;

public class SweetBar : PowerBar
{
    public float maxSweet;
    public List<float> stageValue;
    public List<Image> stageImage;
    int currentStadge;
    float sweetValue;
    public override float GetBarValue()
    {
        return sweetValue;
    }

    public override void InitBar()
    {
        sweetValue = 0;
        bar.SetValue(0, 1);
        currentStadge = 0;
    }

    public override void UpdateBar(float addValue)
    {
        sweetValue+=addValue;
        while (sweetValue > stageValue[currentStadge]&&currentStadge<stageValue.Count)
        {
            ChangeStage(1);
        }
        bar.SetValue(sweetValue, maxSweet);
        //点亮图标
    }
    public bool ConsumeSweet(float value)
    {
        if (sweetValue < value) return false;
        else
        {
            sweetValue -= value;
            bar.SetValue(sweetValue, maxSweet);
            while (sweetValue > stageValue[currentStadge] && currentStadge > 0)
            {
                ChangeStage(-1);
            }
            //关闭图标
            return true;
        }
    }
    public void ChangeStage(int num)
    {
        currentStadge += num;
        EventController.Instance.TriggerEvent<int>(EventName.WhenSweetChange.ToString(), currentStadge);
    }
    public int GetStage()
    {
        return currentStadge;
    }
}
