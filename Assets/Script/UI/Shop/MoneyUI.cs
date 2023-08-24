using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using UnityEngine.Profiling;
public class MoneyUI : MonoBehaviour
{
    public Text current;
    public Text max;
    public Text level;
    public Text left;
    public string tag= "Player";
    StringBuilder currentSB=new StringBuilder();
    StringBuilder maxSB= new StringBuilder();
    StringBuilder levelSB= new StringBuilder();
    StringBuilder leftSB=new StringBuilder();

    void Update()
    {
        //Profiler.BeginSample("new string");
        int currentm = (int)MoneyManage.instance.teamM[tag].currenMoney;
        current.text = currentm.ToString();
        max.text = MoneyManage.instance.teamM[tag].maxMoney.ToString();
        level.text = MoneyManage.instance.teamM[tag].level.ToString();
        //left.text = (MoneyManage.instance.teamM[tag].maxChess - GameManage.instance.teams[tag].team.Count).ToString();
        //Profiler.EndSample();
    }
}
