# Character Movement API参考

## 核心组件

### Rigidbody2D（物理控制）

#### 速度控制

```csharp
// 直接设置速度
public Vector2 velocity { get; set; }

// 只修改水平速度
rb.velocity = new Vector2(moveSpeed * inputX, rb.velocity.y);

// 只修改垂直速度（跳跃）
rb.velocity = new Vector2(rb.velocity.x, jumpForce);
```

#### 力控制

```csharp
// 添加瞬时力（推荐用于跳跃）
public void AddForce(Vector2 force, ForceMode2D mode = ForceMode2D.Force);

// 跳跃示例
rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

// 持续力（不推荐用于移动）
rb.AddForce(Vector2.right * moveForce);
```

#### 重力控制

```csharp
// 重力缩放
public float gravityScale { get; set; }

// 跳跃时调整重力
rb.gravityScale = 2f;  // 默认
rb.gravityScale = 1f;  // 慢速下落
rb.gravityScale = 4f;  // 快速下落
```

#### 约束控制

```csharp
// 冻结旋转（防止角色倾倒）
rb.constraints = RigidbodyConstraints2D.FreezeRotation;

// 冻结位置
rb.constraints = RigidbodyConstraints2D.FreezePositionX;
```

---

### Physics2D（物理检测）

#### 地面检测

```csharp
// 射线检测
public static RaycastHit2D Raycast(
    Vector2 origin,
    Vector2 direction,
    float distance,
    int layerMask = DefaultRaycastLayers
);

// 使用示例
RaycastHit2D hit = Physics2D.Raycast(
    transform.position,
    Vector2.down,
    groundCheckDistance,
    groundLayer
);
bool isGrounded = hit.collider != null;
```

#### 区域检测

```csharp
// 圆形区域检测
public static Collider2D[] OverlapCircleAll(
    Vector2 point,
    float radius,
    int layerMask
);

// 使用示例
Collider2D[] colliders = Physics2D.OverlapCircleAll(
    groundCheckPoint.position,
    groundCheckRadius,
    groundLayer
);
bool isGrounded = colliders.Length > 0;
```

#### 点检测

```csharp
// 单点检测（性能最优）
public static Collider2D OverlapCircle(
    Vector2 point,
    float radius,
    int layerMask
);

// 使用示例
Collider2D groundCollider = Physics2D.OverlapCircle(
    groundCheckPoint.position,
    groundCheckRadius,
    groundLayer
);
bool isGrounded = groundCollider != null;
```

---

### Input System（新输入系统）

#### 输入动作定义

```csharp
// InputAction字段
[SerializeField] private InputAction moveAction;
[SerializeField] private InputAction jumpAction;

// 启用输入
private void OnEnable()
{
    moveAction.Enable();
    jumpAction.Enable();
}

private void OnDisable()
{
    moveAction.Disable();
    jumpAction.Disable();
}
```

#### 读取输入

```csharp
// 移动输入（Vector2）
Vector2 moveInput = moveAction.ReadValue<Vector2>();
float moveX = moveInput.x;

// 跳跃输入（Button）
if (jumpAction.WasPressedThisFrame())
{
    Jump();
}

// 持续按下检测
if (jumpAction.IsPressed())
{
    // 长按逻辑
}
```

#### 输入事件绑定

```csharp
private void Awake()
{
    jumpAction.performed += ctx => Jump();
    jumpAction.canceled += ctx => OnJumpReleased();
}
```

---

## API白名单

### ✅ 推荐使用

| API | 用途 | 性能等级 |
|-----|------|---------|
| Rigidbody2D.velocity | 移动控制 | ⭐⭐⭐⭐⭐ 优秀 |
| Rigidbody2D.AddForce(Impulse) | 跳跃 | ⭐⭐⭐⭐⭐ 优秀 |
| Physics2D.OverlapCircle | 地面检测 | ⭐⭐⭐⭐⭐ 优秀 |
| Physics2D.Raycast | 精确检测 | ⭐⭐⭐⭐ 良好 |
| InputAction | 输入处理 | ⭐⭐⭐⭐⭐ 优秀 |
| rb.constraints | 约束控制 | ⭐⭐⭐⭐⭐ 优秀 |

### ⚠️ 警告使用

| API | 问题 | 替代方案 |
|-----|------|---------|
| Transform.Translate | 绕过物理系统 | 使用Rigidbody2D.velocity |
| rb.AddForce(Force) | 移动不精确 | 使用rb.velocity |
| Physics2D.OverlapCircleAll | 性能差 | 使用OverlapCircle |
| Input.GetAxis | 旧系统，将废弃 | 使用Input System |
| rb.MovePosition | 需要插值 | 使用velocity |

### ❌ 禁止使用

| API | 原因 |
|-----|------|
| GameObject.Find | 每帧性能极差 |
| GetComponent in Update | 缓存组件引用 |
| Physics2D.RaycastAll | 不必要的开销 |
| rb.drag > 0 | 移动控制不精确 |

---

## 性能优化

### 组件缓存

```csharp
// ✅ 正确：Awake中缓存
private Rigidbody2D rb;
private Animator animator;
private CapsuleCollider2D capsuleCollider;

private void Awake()
{
    rb = GetComponent<Rigidbody2D>();
    animator = GetComponent<Animator>();
    capsuleCollider = GetComponent<CapsuleCollider2D>();
}

// ❌ 错误：Update中获取
private void Update()
{
    var rb = GetComponent<Rigidbody2D>(); // 每帧性能浪费
}
```

### Layer缓存

```csharp
// ✅ 正确：缓存Layer
[SerializeField] private LayerMask groundLayer;

private bool IsGrounded()
{
    return Physics2D.OverlapCircle(
        groundCheckPoint.position,
        groundCheckRadius,
        groundLayer
    );
}

// ❌ 错误：每次计算Layer
private bool IsGrounded()
{
    int groundLayer = LayerMask.GetMask("Ground"); // 每次计算
    return Physics2D.OverlapCircle(..., groundLayer);
}
```

### 物理频率

```csharp
// Project Settings → Physics 2D
// 优化物理更新频率
Time.fixedDeltaTime = 0.02f;  // 50Hz（默认）
// 或更高质量
Time.fixedDeltaTime = 0.0167f; // 60Hz
```

---

## 平台跳跃模式API

### 核心实现

```csharp
public class PlatformerController : MonoBehaviour
{
    [Header("移动参数")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float gravityScale = 2f;
    
    [Header("地面检测")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask groundLayer;
    
    [Header("输入")]
    [SerializeField] private InputAction moveAction;
    [SerializeField] private InputAction jumpAction;
    
    private Rigidbody2D rb;
    private bool isGrounded;
    private bool facingRight = true;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }
    
    private void FixedUpdate()
    {
        CheckGround();
        HandleMovement();
    }
    
    private void HandleMovement()
    {
        float moveInput = moveAction.ReadValue<Vector2>().x;
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
        
        // 翻转
        if (moveInput > 0 && !facingRight) Flip();
        if (moveInput < 0 && facingRight) Flip();
    }
    
    public void Jump()
    {
        if (isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }
    
    private void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );
    }
    
    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}
```

---

## 俯视移动模式API

### 核心实现

```csharp
public class TopDownController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    
    private Rigidbody2D rb;
    private Vector2 moveInput;
    
    private void FixedUpdate()
    {
        rb.velocity = moveInput.normalized * moveSpeed;
    }
    
    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;
    }
}
```

---

## 横版格斗模式API

### 核心实现

```csharp
public class FighterController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    
    private Rigidbody2D rb;
    private bool isDashing;
    private float dashTimer;
    
    private void FixedUpdate()
    {
        if (isDashing)
        {
            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0)
            {
                isDashing = false;
            }
        }
        else
        {
            HandleMovement();
        }
    }
    
    public void Dash(float direction)
    {
        if (!isDashing)
        {
            isDashing = true;
            dashTimer = dashDuration;
            rb.velocity = new Vector2(direction * dashSpeed, 0);
        }
    }
}
```

---

## 常见问题API

### Q: 跳跃高度不一致？

```csharp
// 解决方案：使用速度而非AddForce
rb.velocity = new Vector2(rb.velocity.x, jumpForce);

// 而不是
rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
```

### Q: 移动有延迟？

```csharp
// 问题：使用AddForce
rb.AddForce(Vector2.right * moveForce);

// 解决：直接设置速度
rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
```

### Q: 地面检测不准确？

```csharp
// 问题：检测点位置不对
Physics2D.Raycast(transform.position, Vector2.down, ...);

// 解决：使用专门的检测点
[SerializeField] private Transform groundCheck;
Physics2D.Raycast(groundCheck.position, Vector2.down, ...);
```

---

## 扩展资源

- [Unity 2D物理系统文档](https://docs.unity3d.com/Manual/class-Physics2D.html)
- [Input System官方教程](https://learn.unity.com/project/input-system)
- [2D角色控制最佳实践](https://unity.com/how-to/make-2D-game)
