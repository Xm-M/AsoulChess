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
    public override void Init()
    {
        EventController.Instance.AddListener(EventName.GameOver.ToString(), Hide);
        EventController.Instance.AddListener(EventName.GameStart.ToString(), () => start = true);
        cards=new List<Item_PlantCard>();
        creators = new List<PropertyCreator>();
    }
    public void InitCreator(List<PropertyCreator> cs,float interval=5)
    {
        creators.AddRange(cs);
        this.interval = iconSize*2/moveSpeed;
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
            newcard.InitCard(creators[Random.Range(0, creators.Count)],
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
        base.Hide();
        List<Item_PlantCard> ncards = new List<Item_PlantCard>(cards);
        for (int i = 0; i < ncards.Count; i++)
            ncards[i].Recycle();
        cards.Clear();
        creators.Clear();
        start = false;
    }
    public override void Show()
    {
        base.Show();
        t = 0;
    }
}
