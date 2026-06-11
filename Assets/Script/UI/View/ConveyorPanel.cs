using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    public AudioPlayer shopAudio;
    List<PropertyCreator> creators;
    List<PropertyCreator> fixedPrefix;
    List<PropertyCreator> loopPool;
    int fixedIndex;
    bool columnMode;
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
        fixedPrefix = new List<PropertyCreator>();
        loopPool = new List<PropertyCreator>();
    }
    public void InitCreator(List<PropertyCreator> cs,float interval=-1)
    {
        columnMode = false;
        fixedPrefix.Clear();
        loopPool.Clear();
        fixedIndex = 0;
        creators.Clear();
        creators.AddRange(cs);
        SetInterval(interval);
        BuildFateList(loopPool.Count > 0 ? loopPool : creators);
    }

    /// <summary>排山倒海：固定序前缀 + 随机循环池，出列种植卡。</summary>
    public void InitColumnMode(List<PropertyCreator> fixedPrefixCreators, List<PropertyCreator> loopCreators, float interval = -1)
    {
        columnMode = true;
        creators.Clear();
        fixedPrefix.Clear();
        loopPool.Clear();
        if (fixedPrefixCreators != null) fixedPrefix.AddRange(fixedPrefixCreators);
        if (loopCreators != null) loopPool.AddRange(loopCreators);
        fixedIndex = 0;
        SetInterval(interval);
        BuildFateList(loopPool);
    }

    void SetInterval(float interval)
    {
        if (interval == -1)
            this.interval = iconSize * 2 / moveSpeed;
        else this.interval = interval;
    }

    void BuildFateList(List<PropertyCreator> pool)
    {
        fateList = new List<float>();
        if (pool == null || pool.Count == 0) return;
        int raritySum = 0;
        for (int i = 0; i < pool.Count; i++)
            raritySum += pool[i].baseProperty.rarity;
        if (raritySum <= 0) return;
        fateList.Add((float)pool[0].baseProperty.rarity / raritySum);
        for (int i = 1; i < pool.Count; i++)
            fateList.Add(fateList[i - 1] + (float)pool[i].baseProperty.rarity / raritySum);
    }

    PropertyCreator PickNextCreator()
    {
        if (columnMode)
        {
            if (fixedIndex < fixedPrefix.Count)
                return fixedPrefix[fixedIndex++];
            return PickWeighted(loopPool);
        }
        return PickWeighted(creators);
    }

    PropertyCreator PickWeighted(List<PropertyCreator> pool)
    {
        if (pool == null || pool.Count == 0 || fateList == null || fateList.Count == 0)
            return null;
        float r = Random.Range(0f, 1f);
        int n = pool.Count - 1;
        for (int i = 0; i < fateList.Count; i++)
        {
            if (fateList[i] > r)
            {
                n = i;
                break;
            }
        }
        return pool[n];
    }

    private void Update()
    {
        if (!start) return;
        t+=Time.deltaTime;
        if (t > interval && cards.Count < 10)
        {
            PropertyCreator pick = PickNextCreator();
            if (pick != null)
            {
            t = 0f;
            Item_PlantCard newcard = columnMode
                ? UIManage.GetView<ItemPanel>().Create<Item_PlantCard_Column>()
                : UIManage.GetView<ItemPanel>().Create<Item_PlantCard>();
            newcard.transform.SetParent(iconParent);
            newcard.transform.position = startPos.position;
            newcard.InitCard(pick, () => cards.Remove(newcard));
            cards.Add(newcard);
            }
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
    public void DigPlant(Image image)
    {
        PrePlantImage_Data data = new PrePlantImage_Data();
        data.preSprite = image.sprite;
        PrePlantImage.instance.TryToPlant(() => shopAudio.PlaySub(0, "cancel"), (Chess) => shopAudio.PlaySub(0, "dig"), data, HandItemType.Shovel);
    }
    public override void Hide()
    {
        
        List<Item_PlantCard> ncards = new List<Item_PlantCard>(cards);
        for (int i = 0; i < ncards.Count; i++)
            ncards[i].Recycle();

        cards.Clear();
        creators.Clear();
        fixedPrefix.Clear();
        loopPool.Clear();
        fixedIndex = 0;
        columnMode = false;
        start = false;
        base.Hide();
    }
    public override void Show()
    {
        base.Show();
        t = 0;
    }
}
