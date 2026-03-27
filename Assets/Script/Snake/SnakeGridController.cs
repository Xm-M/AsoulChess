using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 贪吃蛇关卡辅助：维护蛇头引用与离场清理。格子移动由蛇头 <see cref="MoveController"/> +
/// <see cref="FindTileMethod_Snake"/> 驱动，不再在此做步进/瞬移/身体增长。
/// </summary>
public class SnakeGridController : MonoBehaviour
{
    [FoldoutGroup("引用"), LabelText("蛇头 Chess(可空，由关卡生成)")]
    public Chess head;

    [FoldoutGroup("引用"), LabelText("蛇关卡(空则运行时查找)")]
    public LevelController_Snake snakeLevel;

    readonly List<Chess> _segments = new List<Chess>();

    void Awake()
    {
        if (snakeLevel == null)
            snakeLevel = FindObjectOfType<LevelController_Snake>();
    }

    /// <summary>由 <see cref="LevelController_Snake"/> 在 <c>GameStart</c> 生成蛇头后调用。</summary>
    public void AssignHeadFromLevel(Chess spawnedHead)
    {
        head = spawnedHead;
        RegisterHeadOnly();
    }

    /// <summary>过关/离场时清掉蛇头与已登记节，避免残留占用对象池。</summary>
    public void ClearSnakeForLevelEnd()
    {
        if (head != null && head.moveController != null)
            head.moveController.OnReachTile.RemoveListener(OnHeadReachTile);

        for (int i = 0; i < _segments.Count; i++)
        {
            Chess c = _segments[i];
            if (c != null && !c.IfDeath) c.Death();
        }

        _segments.Clear();
        head = null;
    }

    void OnEnable()
    {
        EventController.Instance.AddListener(EventName.GameStart.ToString(), OnGameStartEvent);
        EventController.Instance.AddListener(EventName.GameOver.ToString(), OnGameOverOrLeave);
        EventController.Instance.AddListener(EventName.WhenLeaveLevel.ToString(), OnGameOverOrLeave);
    }

    void OnDisable()
    {
        EventController.Instance.RemoveListener(EventName.GameStart.ToString(), OnGameStartEvent);
        EventController.Instance.RemoveListener(EventName.GameOver.ToString(), OnGameOverOrLeave);
        EventController.Instance.RemoveListener(EventName.WhenLeaveLevel.ToString(), OnGameOverOrLeave);
    }

    void OnGameStartEvent()
    {
        if (head != null)
            RegisterHeadOnly();
    }

    void OnGameOverOrLeave()
    {
        ClearSnakeForLevelEnd();
    }

    void RegisterHeadOnly()
    {
        for (int i = _segments.Count - 1; i >= 0; i--)
        {
            Chess c = _segments[i];
            if (c != null && c != head)
                c.Death();
        }

        _segments.Clear();
        if (head == null || head.moveController.standTile == null)
        {
            Debug.LogError("[SnakeGridController] 蛇头或未站在 Tile 上");
            return;
        }

        _segments.Add(head);

        if (head.moveController != null)
        {
            head.moveController.OnReachTile.RemoveListener(OnHeadReachTile);
            head.moveController.OnReachTile.AddListener(OnHeadReachTile);
        }

        var snakeTile = head.moveController?.tileMethod as FindTileMethod_Snake;
        snakeTile?.ResetForNewRun(head);
    }

    void OnHeadReachTile(Chess c, Tile t)
    {
        if (c != head) return;
        var snakeTile = head.moveController?.tileMethod as FindTileMethod_Snake;
        snakeTile?.OnReachTile(t);
    }

    void LateUpdate()
    {
        var snakeTile = head?.moveController?.tileMethod as FindTileMethod_Snake;
        snakeTile?.AfterFrameSyncPath();
    }

    /// <summary>
    /// 由 <see cref="SnakeArmor"/> 等在触发碰撞时调用：仅当 <paramref name="food"/> 属于本场
    /// <see cref="LevelController_Snake.ActiveFood"/> 时吃掉（通知关卡补刷并移除食物棋子）。
    /// </summary>
    /// <param name="eater">一般为蛇头；若指定且与当前登记的 <see cref="head"/> 不一致则忽略。</param>
    public bool TryEatFood(Chess food, Chess eater = null)
    {
        if (food == null || food.IfDeath) return false;
        if (eater != null && headEaterMismatch(eater)) return false;
        if (snakeLevel == null)
            snakeLevel = FindObjectOfType<LevelController_Snake>();
        if (snakeLevel == null) return false;

        IReadOnlyList<Chess> foods = snakeLevel.ActiveFood;
        bool listed = false;
        for (int i = 0; i < foods.Count; i++)
        {
            if (foods[i] == food)
            {
                listed = true;
                break;
            }
        }

        if (!listed) return false;

        int sunDrop = 0;
        var foodCreator = food.propertyController?.creator;
        if (foodCreator != null && foodCreator.baseProperty != null)
            sunDrop = foodCreator.baseProperty.price;
        Vector3 sunStartWorld = food.transform.position;
        Tile sunTargetTile = head?.moveController?.standTile ?? food.moveController?.standTile;

        snakeLevel.NotifyFoodEaten(food);
        if (!food.IfDeath) food.Death();

        var snakeTile = head?.moveController?.tileMethod as FindTileMethod_Snake;
        snakeTile?.NotifyFoodEatenGrow();

        if (sunDrop > 0 && sunTargetTile != null && SunLightPanel.instance != null)
        {
            var itemPanel = UIManage.GetView<ItemPanel>();
            if (itemPanel != null && itemPanel.Create<SunLight>() is SunLight sun)
                sun.InitSunLightDropPop(sunTargetTile, sunDrop, sunStartWorld);
        }

        return true;
    }

    bool headEaterMismatch(Chess eater)
    {
        return head != null && eater != head;
    }

    /// <summary>供食物生成等排除蛇身占格；无蛇或未初始化时写入空集。</summary>
    public void CopySnakeOccupiedCellsTo(HashSet<Vector2Int> into)
    {
        if (into == null) return;
        into.Clear();
        var m = head?.moveController?.tileMethod as FindTileMethod_Snake;
        m?.CopyOccupiedCellsTo(into);
    }
}
