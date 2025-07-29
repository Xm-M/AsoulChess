using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 传送带
/// 1.只有在GameStart事件触发以后才能开始生成  
/// 2.没有删干净 
/// 
/// </summary>
public class ConveyorPanel : View
{
    public Transform iconParent;
    public Transform startPos; 
    public float moveSpeed;
    public float iconSize;
    List<PropertyCreator> creators;
    List<Item_PlantCard> cards;
    float interval;
    float t;
    bool start;
    public List<float> fateList;
    public override void Init()
    {
        EventController.Instance.AddListener(EventName.GameOver.ToString(), Hide);
        EventController.Instance.AddListener(EventName.GameStart.ToString(), () => start = true);
        cards=new List<Item_PlantCard>();
        creators = new List<PropertyCreator>();
    }
    public void InitCreator(List<PropertyCreator> cs,float interval=-1)
    {
        creators.AddRange(cs);
        if (interval == -1)
            this.interval = iconSize * 2 / moveSpeed;
        else this.interval = interval;
        fateList = new List<float>();
        int raritySum = 0;
        Debug.Log(creators.Count);
        for (int i = 0; i < creators.Count; i++)
        {
            raritySum += creators[i].baseProperty.rarity;
        }
        fateList.Add((float)creators[0].baseProperty.rarity / raritySum);
        for (int i = 1; i < creators.Count; i++)
        {
            fateList.Add( fateList[i - 1] + (float)creators[i].baseProperty.rarity / raritySum);
        }
    }
    private void Update()
    {
        if (!start) return;
        t+=Time.deltaTime;
        if (t > interval && cards.Count < 10)
        {
            t = 0;
            Item_PlantCard newcard = UIManage.GetView<ItemPanel>().Create<Item_PlantCard>();
            newcard.transform.SetParent(iconParent);
            newcard.transform.position = startPos.position;
            float r = Random.Range(0, 1f);
            Debug.Log(r);
            int n = 0;
            for (int i = 0; i < fateList.Count; i++)
            {
                if (fateList[i] > r)
                {
                    n = i;
                    break;
                }
            }
            
            //Debug.Log(n);
            //Debug.Log(creators.Count);
            newcard.InitCard(creators[n],
                ()=>cards.Remove(newcard));
            cards.Add(newcard);
        }
        RectTransform rect=null, rectPre;
        for(int i = 0; i < cards.Count; i++)
        {
            rectPre = rect;
            rect = cards[i].GetComponent<RectTransform>();
            //这里是移动Cards的函数
            if (i == 0 && rect.anchoredPosition.x>0)
            {
                rect.anchoredPosition = Vector2.MoveTowards(rect.anchoredPosition,
                    new Vector2(0, rect.anchoredPosition.y), Time.deltaTime * moveSpeed);
            }
            else if (i>0&&rect.anchoredPosition.x - rectPre.anchoredPosition.x>iconSize)
            {
                rect.anchoredPosition = Vector2.MoveTowards(rect.anchoredPosition,
                    new Vector2(rectPre.anchoredPosition.x + iconSize, rect.anchoredPosition.y),
                    Time.deltaTime * moveSpeed);
            }
        }
    }

    public override void Hide()
    {
        
        List<Item_PlantCard> ncards = new List<Item_PlantCard>(cards);
        for (int i = 0; i < ncards.Count; i++)
            ncards[i].Recycle();

        cards.Clear();
        creators.Clear();
        start = false;
        base.Hide();
    }
    public override void Show()
    {
        base.Show();
        t = 0;
    }
}
