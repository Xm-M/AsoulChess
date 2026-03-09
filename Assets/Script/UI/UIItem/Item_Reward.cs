using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Item_Reward : UIItem
{
    //public Transform reward;
    public AnimationCurve curve;
    public float totalTime = 1;
    public float timeSpeed = 2;
    public float height = 1.25f;
    public float moveSpeed = 1;
    public float moveDuration = 1f;
    //private RectTransform rectTransform;
    private bool isMoving = false;
    bool click;
    public void SetRewardPos(Vector3 pos)
    {
        //reward.transform.position= pos;
        float speed=moveSpeed;
        Vector2 center = MapManage_PVZ.instance.tiles[MapManage_PVZ.instance.mapSize.x / 2, MapManage.instance.mapSize.y / 2].transform.position;
        if( pos.x > center.x)
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
        while (elapsed < totalTime&&!click)
        {
            //t(curve的x参数)=elspsed(实际经过的时间)/总时间
            t = elapsed + x0;
            float yOffset = (curve.Evaluate(t) - y0);
            startPos = new Vector2(startPos.x + moveSpeed * Time.deltaTime, starty + (yOffset * height));
            transform.position = Camera.main.WorldToScreenPoint(startPos);
            elapsed += Time.deltaTime * timeSpeed;

            yield return null;
        }
        LevelManage.instance.GamePause();
        // 最终归位
        //rectTransform.anchoredPosition = targetPos;
    }
    public void MoveToCenter()
    {
        if (!isMoving)
        {
            StartCoroutine(MoveToCenterCoroutine());
        }
    }

    IEnumerator MoveToCenterCoroutine()
    {
        isMoving = true;
        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector2 startPos = rectTransform.anchoredPosition;
        Vector2 endPos = Vector2.zero; // 中心位置
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            float t = elapsed / moveDuration;
            t = Mathf.SmoothStep(0, 1, t); // 缓动效果
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        rectTransform.anchoredPosition = endPos;
        isMoving = false;
    }
    public override void OnPointerClick(PointerEventData eventData)
    {
        if (!click)
        {
            //Debug.Log("点到游戏胜利了");
            LevelManage.instance.GamePause();
            MapManage.instance.BGMPlayer.PlayAudio("游戏胜利");
            MapManage.instance.BGMPlayer.SetLoop(false);
            GetComponent<Animator>().Play("win");
            UIManage.GetView<PlantsShop>().Hide();
            UIManage.Close<ItemPanel>();
            //("win");
            MoveToCenter();
            SceneManage.instance.Win();
        }
    }
    public void Win()
    {
        LevelManage.instance.GameOver(true);//所以说我的游戏结束实际是绑定在了生成这个上面
    }
    public override void Recycle()
    {
        UIManage.GetView<ItemPanel>().Recycle<Item_Reward>(this);
        click = false;
    }
}
