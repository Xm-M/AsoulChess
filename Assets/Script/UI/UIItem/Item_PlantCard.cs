using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
/// <summary>
/// 这个是传送带的那个卡
/// </summary>
public class Item_PlantCard : UIItem
{
    public static Item_PlantCard select;
    public PropertyCreator creator;
    public Image goodImage;
    bool ifselect;
    public UnityEvent WhenRecycle;
    public AudioPlayer au;
    bool plantOver;

    public AnimationCurve curve;
    public float totalTime = 1;
    public float timeSpeed = 2;
    public float height = 1.25f;
    public float moveSpeed = 1;
    public float moveDuration = 1f;
    //private RectTransform rectTransform;
    private bool isMoving = false;
    bool click;
    public void InitCard(Vector3 pos, PropertyCreator p, UnityAction WhenReycle = null)
    {
        InitCard(p,WhenReycle);
        float speed = moveSpeed;
        Vector2 center = MapManage_PVZ.instance.tiles[MapManage_PVZ.instance.mapSize.x / 2, MapManage.instance.mapSize.y / 2].transform.position;
        if (pos.x > center.x)
        {
            speed *= -1;
        }
        //moveSpeed
        StartCoroutine(CurveMove(pos, totalTime, 0, height, speed, timeSpeed));

    }
    IEnumerator CurveMove(Vector3 startPos, float totalTime, float x0, float height, float moveSpeed, float timeSpeed)
    {

        transform.position = Camera.main.WorldToScreenPoint(startPos);
        float elapsed = 0f;
        float t = x0;
        float y0 = curve.Evaluate(t);

        float starty = startPos.y;
        while (elapsed < totalTime && !click)
        {
            //t(curve的x参数)=elspsed(实际经过的时间)/总时间
            t = elapsed + x0;
            float yOffset = (curve.Evaluate(t) - y0);
            startPos = new Vector2(startPos.x + moveSpeed * Time.deltaTime, starty + (yOffset * height));
            transform.position = Camera.main.WorldToScreenPoint(startPos);
            elapsed += Time.deltaTime * timeSpeed;

            yield return null;
        }

        // 最终归位
        //rectTransform.anchoredPosition = targetPos;
    }


    public void InitCard(PropertyCreator p,UnityAction WhenReycle=null)
    {
        plantOver = false;
        creator = p;
        ifselect = false;
        goodImage.sprite = p.chessSprite;
        goodImage.color = new Color(1, 1, 1, 1);
        if (WhenReycle!=null)WhenRecycle.AddListener(WhenReycle);
    }
    public void InitCard(PropertyCreator p,Vector2 StartPos,Vector2 endPos,float moveSpeed,UnityAction WhenReycle = null)
    {
        InitCard(p,WhenReycle);
        StartCoroutine(CardMove(StartPos, endPos, moveSpeed));
    }
    IEnumerator CardMove(Vector2 startPos,Vector2 endPos,float moveSpeed)
    {
         
        transform.position = startPos;
        while (Vector2.Distance(transform.position,endPos)>0.1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, endPos, moveSpeed * Time.deltaTime);
            yield return null;
            if (plantOver) break;
        } 
    }


    //然后这里还要写一个闪烁的效果，和一个过了时间就消失的代码
   public override void OnPointerClick(PointerEventData eventData)
    {
        if (!ifselect)
        {
            ifselect = true;
            goodImage.color = new Color(1, 1, 1, 0);
            //这里调用PrePlantImage的效果
            PrePlantImage.instance.TryToPlant(creator, CancelPlant, Recycle);
            if (select != null) select.CancelPlant();
            select = this;
            //au.RandomPlay();
            au.PlayAudio("种植");
        }
    }
    public void CancelPlant()
    {
        ifselect = false;
        goodImage.color = new Color(1, 1, 1, 1);
        if (select == this) select = null;
    }
    public override void Recycle()
    {
        transform.SetParent(UIManage.GetView<ItemPanel>().transform);
        UIManage.GetView<ItemPanel>().Recycle<Item_PlantCard>(this);
        WhenRecycle?.Invoke();
        plantOver = true;
        if(select==this) select = null;
    }
}
