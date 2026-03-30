using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 手机端方向输入：在 Canvas 上挂四个 Button，OnClick 分别绑定 <see cref="QueueUp"/> / Down / Left / Right；
/// 或拖 <see cref="snakeMethodOverride"/> / <see cref="snakeDriver"/> 自动解析蛇头的 <see cref="FindTileMethod_Snake"/>。
/// </summary>
public class SnakeMobileInputBridge : MonoBehaviour
{
    [Tooltip("空则运行时 FindObjectOfType")]
    [SerializeField]
    SnakeGridController snakeDriver;

    //[Tooltip("若已能直接引用蛇头 Move 上的 FindTileMethod_Snake，可填此项（优先于 snakeDriver）")]
    FindTileMethod_Snake snakeMethodOverride;

    public void QueueUp() => Queue(Vector2Int.up);

    public void QueueDown() => Queue(Vector2Int.down);

    public void QueueLeft() => Queue(Vector2Int.left);

    public void QueueRight() => Queue(Vector2Int.right);

    public void Queue(Vector2Int direction)
    {
        FindTileMethod_Snake m = snakeMethodOverride != null ? snakeMethodOverride : ResolveFromDriver();
        m?.TryQueueDirection(direction);
    }

    FindTileMethod_Snake ResolveFromDriver()
    {
        SnakeGridController drv = snakeDriver != null ? snakeDriver : FindObjectOfType<SnakeGridController>();
        Chess head = drv != null ? drv.head : null;
        return head != null && head.moveController != null
            ? head.moveController.tileMethod as FindTileMethod_Snake
            : null;
    }
    private void Awake()
    {
        if(GameManage.instance.mode!=GameMode.Phone)
            gameObject.SetActive(false);
    }
}

///// <summary>
///// 可选：全屏或大块透明 <see cref="UnityEngine.UI.Graphic"/> 上挂此脚本，用滑动起止向量判方向；
///// 需超过 <see cref="minSwipePixels"/> 才生效，减少误触。若仍担心不准，可只用 <see cref="SnakeMobileInputBridge"/> 的按钮。
///// </summary>
//[RequireComponent(typeof(UnityEngine.UI.Graphic))]
//public class SnakeSwipeOverlay : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
//{
//    [SerializeField]
//    SnakeMobileInputBridge bridge;

//    [Min(8f)]
//    [SerializeField]
//    float minSwipePixels = 56f;

//    [Tooltip("若竖直滑动与地图上下相反，勾选")]
//    [SerializeField]
//    bool invertSwipeY;

//    Vector2 _start;

//    public void OnPointerDown(PointerEventData eventData)
//    {
//        _start = eventData.position;
//    }

//    public void OnPointerUp(PointerEventData eventData)
//    {
//        if (bridge == null) return;
//        Vector2 delta = eventData.position - _start;
//        float minSqr = minSwipePixels * minSwipePixels;
//        if (delta.sqrMagnitude < minSqr) return;

//        float dy = invertSwipeY ? -delta.y : delta.y;
//        if (Mathf.Abs(delta.x) > Mathf.Abs(dy))
//            bridge.Queue(delta.x > 0f ? Vector2Int.right : Vector2Int.left);
//        else
//            bridge.Queue(dy > 0f ? Vector2Int.up : Vector2Int.down);
//    }
//}
