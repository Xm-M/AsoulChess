using UnityEngine;

/// <summary>
/// 精灵翻转控制示例
/// 演示如何使用flipX/Y实现角色面向控制
/// </summary>
public class ExampleSpriteFlip : MonoBehaviour
{
    [Header("移动设置")]
    [Tooltip("移动速度")]
    public float moveSpeed = 5f;
    
    [Tooltip("是否自动移动(用于演示)")]
    public bool autoMove = false;
    
    [Header("翻转设置")]
    [Tooltip("初始面向方向(true=左, false=右)")]
    public bool initiallyFacingLeft = false;
    
    [Tooltip("是否使用Y轴翻转(一般不使用)")]
    public bool flipY = false;
    
    [Header("动画设置")]
    [Tooltip("是否启用翻转动画效果")]
    public bool animateFlip = false;
    
    [Tooltip("翻转动画时长")]
    public float flipAnimationDuration = 0.1f;
    
    // SpriteRenderer组件引用
    private SpriteRenderer spriteRenderer;
    
    // 当前面向方向
    private bool facingLeft = false;
    
    // 是否正在翻转动画中
    private bool isFlipping = false;
    
    void Start()
    {
        // 获取SpriteRenderer组件
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("未找到SpriteRenderer组件!");
            enabled = false;
            return;
        }
        
        // 设置初始面向
        facingLeft = initiallyFacingLeft;
        spriteRenderer.flipX = facingLeft;
        spriteRenderer.flipY = flipY;
        
        Debug.Log($"初始面向: {(facingLeft ? "左" : "右")}");
    }
    
    void Update()
    {
        if (autoMove)
        {
            // 自动移动演示
            AutoMoveDemo();
        }
        else
        {
            // 键盘控制移动
            KeyboardControl();
        }
    }
    
    /// <summary>
    /// 键盘控制移动
    /// </summary>
    void KeyboardControl()
    {
        float horizontal = 0f;
        float vertical = 0f;
        
        // 水平移动
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            horizontal = -1f;
        }
        else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            horizontal = 1f;
        }
        
        // 垂直移动
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            vertical = 1f;
        }
        else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            vertical = -1f;
        }
        
        // 应用移动
        if (horizontal != 0 || vertical != 0)
        {
            Vector3 movement = new Vector3(horizontal, vertical, 0).normalized;
            transform.position += movement * moveSpeed * Time.deltaTime;
            
            // 根据移动方向翻转精灵
            if (horizontal != 0)
            {
                SetFacingDirection(horizontal < 0);
            }
        }
        
        // 测试翻转快捷键
        if (Input.GetKeyDown(KeyCode.F))
        {
            Flip();
        }
    }
    
    /// <summary>
    /// 自动移动演示
    /// </summary>
    void AutoMoveDemo()
    {
        // 左右来回移动
        float pingPong = Mathf.PingPong(Time.time * moveSpeed, 10f) - 5f;
        transform.position = new Vector3(pingPong, transform.position.y, transform.position.z);
        
        // 根据移动方向翻转
        float velocity = Mathf.Cos(Time.time * moveSpeed);
        if (Mathf.Abs(velocity) > 0.1f)
        {
            SetFacingDirection(velocity < 0);
        }
    }
    
    /// <summary>
    /// 设置面向方向
    /// </summary>
    /// <param name="faceLeft">是否面向左边</param>
    public void SetFacingDirection(bool faceLeft)
    {
        // 如果方向没有变化,不做处理
        if (facingLeft == faceLeft)
            return;
        
        facingLeft = faceLeft;
        
        if (animateFlip && !isFlipping)
        {
            // 使用动画翻转
            StartCoroutine(AnimateFlip());
        }
        else
        {
            // 直接翻转
            spriteRenderer.flipX = facingLeft;
        }
        
        Debug.Log($"面向已更改为: {(facingLeft ? "左" : "右")}");
    }
    
    /// <summary>
    /// 翻转动画协程
    /// 实现翻转时的缩放动画效果
    /// </summary>
    System.Collections.IEnumerator AnimateFlip()
    {
        isFlipping = true;
        
        float timer = 0f;
        Vector3 originalScale = transform.localScale;
        
        while (timer < flipAnimationDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / flipAnimationDuration;
            
            // 在动画中间点翻转
            if (progress >= 0.5f && spriteRenderer.flipX != facingLeft)
            {
                spriteRenderer.flipX = facingLeft;
            }
            
            // 缩放动画
            float scaleX = Mathf.Cos(progress * Mathf.PI);
            scaleX = Mathf.Abs(scaleX);
            transform.localScale = new Vector3(scaleX, originalScale.y, originalScale.z);
            
            yield return null;
        }
        
        // 恢复原始缩放
        transform.localScale = originalScale;
        isFlipping = false;
    }
    
    /// <summary>
    /// 切换翻转状态
    /// </summary>
    public void Flip()
    {
        SetFacingDirection(!facingLeft);
    }
    
    /// <summary>
    /// 设置Y轴翻转
    /// </summary>
    /// <param name="flip">是否翻转Y轴</param>
    public void SetFlipY(bool flip)
    {
        flipY = flip;
        spriteRenderer.flipY = flipY;
        Debug.Log($"Y轴翻转已设置为: {flip}");
    }
    
    /// <summary>
    /// 获取当前面向方向
    /// </summary>
    /// <returns>是否面向左边</returns>
    public bool IsFacingLeft()
    {
        return facingLeft;
    }
    
    /// <summary>
    /// 面向指定目标点
    /// </summary>
    /// <param name="targetPosition">目标位置</param>
    public void FaceTowards(Vector3 targetPosition)
    {
        // 计算目标相对位置
        float directionX = targetPosition.x - transform.position.x;
        
        // 根据X轴方向设置面向
        if (directionX != 0)
        {
            SetFacingDirection(directionX < 0);
        }
    }
    
    /// <summary>
    /// 面向指定目标对象
    /// </summary>
    /// <param name="target">目标对象</param>
    public void FaceTowards(Transform target)
    {
        if (target != null)
        {
            FaceTowards(target.position);
        }
    }
    
    // 在Inspector中绘制辅助信息
    void OnDrawGizmosSelected()
    {
        if (spriteRenderer != null)
        {
            // 绘制面向方向指示器
            Gizmos.color = Color.blue;
            Vector3 direction = facingLeft ? Vector3.left : Vector3.right;
            Vector3 start = transform.position;
            Gizmos.DrawLine(start, start + direction * 2f);
            
            // 绘制箭头
            Gizmos.DrawWireSphere(start + direction * 2f, 0.2f);
        }
    }
    
    // 在Inspector中修改值时自动应用
    void OnValidate()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipY = flipY;
        }
    }
}