using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoneyManage : MonoBehaviour
{
    public static MoneyManage instance;
    void Awake()
    {
        if (instance == null) instance = this;
    }
    public class moneyMessege
    {
        public int baseAdd=16;
        public int level = 0;
        //public int maxChess = 1;
        float[] maxArray = { 100, 200, 300, 450, 600 };
        float[] addSpeedArray = { 1, 1.25f, 1.5f, 1.75f, 2f };
        float[] levelCost = { 50, 100, 150, 225, 300 };
        public float currenMoney=10000;
        public float maxMoney=100;
        public float addSpeed=1;
        public void LevelUp()
        {
            if (level < maxArray.Length-1&&currenMoney > levelCost[level])
            {
                currenMoney -= levelCost[level];
                level++;
                addSpeed = addSpeedArray[level];
                maxMoney = maxArray[level];
                //maxChess++;
            }
        }
        public void Update()
        {
            if (currenMoney < maxMoney)
            {
                currenMoney += addSpeed*Time.deltaTime*baseAdd;
            }
            if (currenMoney > maxMoney) currenMoney = maxMoney;
        }
        public bool CostMoney(float cost)
        {
            if (currenMoney >= cost)
            {
                currenMoney -= cost;
                return true;
            }
            return false;
        }
        public void Reset()
        {
            level = 0;
            maxMoney = maxArray[0];
            addSpeed = addSpeedArray[0];
            currenMoney = 1000;
            //maxChess = 8;
        }
    }
    public Dictionary<string, moneyMessege> teamM = new Dictionary<string, moneyMessege>();
    private void Start()
    {
        teamM.Add("Player", new moneyMessege());
        teamM.Add("Enemy", new moneyMessege());
        //teamM["Enemy"].maxChess = 8;
        //EventController.Instance.AddListener(EventName.RestartGame.ToString(), Reset);
        //EventController.Instance.AddListener(EventName.GameOver.ToString(), Reset);

    }
    public void WhenLose()
    {
        teamM["Player"].LevelUp();
    }
    public void Reset()
    {
        foreach (var a in teamM)
        {
            a.Value.Reset();
        }
    }
    public bool BuySomething(string tag,float cost)
    {
        //if (teamM[tag].maxChess > GameManage.instance.teams[tag].team.Count)
        //{
        //    return teamM[tag].CostMoney(cost);
        //}
        return false;
    }
 
    void Update()
    {
        if(GameManage.instance.ifGameStart)
        foreach(var a in teamM)
        {
            a.Value.Update();
        }
    }
    public void LevelUp(string tag)
    {
        teamM[tag].LevelUp();
    }
}
