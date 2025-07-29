using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Events;
using UnityEngine.UI;
/// <summary>
/// 压力值统计用的
/// 压  现在不上等等再去已经撕票了怎么办  
/// </summary>
public class StressArmor : ArmorBase
{
    [LabelText("压力值显示")]
    public GameObject ShowText;//用于显示数字的
    [LabelText("压力文本")]
    public Text stressText;
    [LabelText("压力阈值")]
    public float stressMax=10f;//压力阈值 
    [LabelText("当压力值满时")]
    public UnityEvent OnStressGetMax;
    [LabelText("增加值")]
    public float addStress=0.1f;
    float current;

    public void AddStress(float value)
    {
        current += value;
        stressText.text = ((int)current).ToString();
        //ShowText.GetComponent<Animator>().Play("stressShow");   
        if (current > stressMax)
        {
            OnStressGetMax?.Invoke();
            
        }
    }

    public bool CheckStress()
    {
        return current > stressMax;
    }

    public override void BrokenArmor()
    {
        user.propertyController.onSetDamage.RemoveListener(GetDamage);
    }

    public override void GetDamage(DamageMessege dm)
    {
        AddStress(addStress);
    }

    public override void InitArmor()
    {
        user.WhenEnterGame.AddListener(ResetArmor);
    }

    public override void ResetArmor(Chess chess)
    {
        current = 0;
        chess.propertyController.onGetDamage.AddListener(GetDamage);
    }
}
