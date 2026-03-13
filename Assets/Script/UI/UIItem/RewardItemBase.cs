using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 胜利奖励基类：位置曲线动画、居中移动、点击通用逻辑
/// </summary>
public abstract class RewardItemBase : UIItem
{
    public AnimationCurve curve;
    public float totalTime = 1;
    public float timeSpeed = 2;
    public float height = 1.25f;
    public float moveSpeed = 1;
    public float moveDuration = 1f;

    protected bool isMoving;
    protected bool click;

    /// <summary>
    /// 设置奖励在世界坐标的位置，开始曲线飞行动画
    /// </summary>
    public virtual void SetRewardPos(Vector3 pos)
    {
        float speed = moveSpeed;
        Vector2 center = MapManage_PVZ.instance.tiles[MapManage_PVZ.instance.mapSize.x / 2, MapManage.instance.mapSize.y / 2].transform.position;
        if (pos.x > center.x) speed *= -1;
        StartCoroutine(CurveMove(pos, totalTime, 0, height, speed, timeSpeed));
    }

    protected IEnumerator CurveMove(Vector3 startPos, float totalTime, float x0, float height, float moveSpeed, float timeSpeed)
    {
        transform.position = Camera.main.WorldToScreenPoint(startPos);
        float elapsed = 0f;
        float t = x0;
        float y0 = curve.Evaluate(t);
        float starty = startPos.y;

        while (elapsed < totalTime && !click)
        {
            t = elapsed + x0;
            float yOffset = (curve.Evaluate(t) - y0);
            startPos = new Vector2(startPos.x + moveSpeed * Time.deltaTime, starty + (yOffset * height));
            transform.position = Camera.main.WorldToScreenPoint(startPos);
            elapsed += Time.deltaTime * timeSpeed;
            yield return null;
        }
        LevelManage.instance.GamePause();
    }

    public void MoveToCenter()
    {
        if (!isMoving)
            StartCoroutine(MoveToCenterCoroutine());
    }

    protected IEnumerator MoveToCenterCoroutine()
    {
        isMoving = true;
        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector2 startPos = rectTransform.anchoredPosition;
        Vector2 endPos = Vector2.zero;
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            float t = elapsed / moveDuration;
            t = Mathf.SmoothStep(0, 1, t);
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rectTransform.anchoredPosition = endPos;
        isMoving = false;
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (click) return;
        click = true;

        LevelManage.instance.GamePause();
        MapManage.instance.BGMPlayer.PlayAudio("游戏胜利");
        MapManage.instance.BGMPlayer.SetLoop(false);
        UIManage.GetView<PlantsShop>().Hide();

        MoveToCenter();
        GetComponent<Animator>().Play("win");
        SceneManage.instance.Win(); // 播放 loadScene 的 winscene 动画
    }
}
