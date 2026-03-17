# Unity 2D物理系统 - API完整参考文档

## 一、Rigidbody2D（2D刚体）

### 概述
Rigidbody2D是Unity 2D物理系统的核心组件，控制物体的物理行为，包括移动、旋转、受力等。

### 核心属性

#### 1. velocity（速度）
**类型**：`Vector2`  
**用途**：读写刚体的当前速度  
**性能**：⭐⭐⭐ 高性能  

**语法**：
```csharp
// 读取速度
Vector2 currentVelocity = rigidbody2D.velocity;

// 设置速度
rigidbody2D.velocity = new Vector2(5f, 0f);
```

**使用场景**：
- 角色移动控制（直接设置水平速度）
- 跳跃（设置垂直速度）
- 检测物体是否静止（velocity.magnitude < threshold）

**最佳实践**：
```csharp
// ✅ 推荐：直接设置velocity控制移动
void Move(float horizontalInput)
{
    _rb.velocity = new Vector2(horizontalInput * _moveSpeed, _rb.velocity.y);
}

// ✅ 推荐：跳跃时设置垂直速度
void Jump()
{
    _rb.velocity = new Vector2(_rb.velocity.x, _jumpForce);
}
```

**注意事项**：
- 在FixedUpdate中修改velocity，避免抖动
- 不要频繁修改velocity，可能导致不自然的物理效果

#### 2. mass（质量）
**类型**：`float`  
**默认值**：1.0  
**用途**：设置物体的质量  
**性能**：⭐⭐⭐ 高性能  

**语法**：
```csharp
rigidbody2D.mass = 2.0f; // 设置质量为2
float mass = rigidbody2D.mass; // 读取质量
```

**使用场景**：
- 不同质量的物体受到不同的力
- 物理碰撞时的质量影响

**最佳实践**：
```csharp
// ✅ 推荐：在Inspector中设置，避免运行时修改
[SerializeField] private float _mass = 1.0f;

void Awake()
{
    _rb.mass = _mass;
}
```

**注意事项**：
- 避免运行时频繁修改mass，会触发物理引擎重新计算
- 质量为0的物体在物理碰撞中会表现异常

#### 3. gravityScale（重力缩放）
**类型**：`float`  
**默认值**：1.0  
**用途**：缩放重力对物体的影响  
**性能**：⭐⭐⭐ 高性能  

**语法**：
```csharp
rigidbody2D.gravityScale = 2.0f; // 重力效果加倍
rigidbody2D.gravityScale = 0f; // 不受重力影响
```

**使用场景**：
- 角色下落时增加重力（更快的下落感）
- 漂浮物体（减少重力）
- 飞行角色（不受重力影响）

**最佳实践**：
```csharp
// ✅ 推荐：角色跳跃时调整重力
void UpdateJump()
{
    // 下落时增加重力，跳跃时减少重力
    if (_rb.velocity.y < 0)
    {
        _rb.gravityScale = _fallingGravityScale; // 2.0
    }
    else
    {
        _rb.gravityScale = _jumpingGravityScale; // 1.0
    }
}
```

#### 4. bodyType（刚体类型）
**类型**：`RigidbodyType2D` 枚举  
**选项**：
- `Dynamic`（动态）：受力和碰撞影响，最常用
- `Kinematic`（运动学）：不受力影响，可通过脚本控制
- `Static`（静态）：不移动，仅用于碰撞

**语法**：
```csharp
rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
```

**使用场景**：
- `Dynamic`：角色、敌人、可移动物体
- `Kinematic`：平台、电梯、由脚本控制的物体
- `Static`：墙壁、地面、固定障碍物

**最佳实践**：
```csharp
// ✅ 推荐：玩家角色使用Dynamic
[SerializeField] private RigidbodyType2D _bodyType = RigidbodyType2D.Dynamic;

// ✅ 推荐：移动平台使用Kinematic
public class MovingPlatform : MonoBehaviour
{
    private Rigidbody2D _rb;
    
    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.bodyType = RigidbodyType2D.Kinematic;
    }
    
    void FixedUpdate()
    {
        // Kinematic刚体使用MovePosition移动
        _rb.MovePosition(targetPosition);
    }
}
```

#### 5. constraints（约束）
**类型**：`RigidbodyConstraints2D` 枚举  
**选项**：
- `None`：无约束
- `FreezePositionX`：冻结X轴移动
- `FreezePositionY`：冻结Y轴移动
- `FreezeRotation`：冻结旋转

**语法**：
```csharp
// 冻结旋转
rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;

// 冻结X轴移动
rigidbody2D.constraints = RigidbodyConstraints2D.FreezePositionX;

// 组合约束
rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
```

**使用场景**：
- 角色不需要旋转（冻结旋转）
- 横版游戏角色不需要Z轴移动（实际是冻结旋转）
- 固定在某条线上的物体

**最佳实践**：
```csharp
// ✅ 推荐：角色控制器冻结旋转
void Awake()
{
    _rb = GetComponent<Rigidbody2D>();
    _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
}
```

### 核心方法

#### 1. AddForce()（施加力）
**类型**：方法  
**参数**：
- `force`：Vector2，施加的力
- `mode`：ForceMode2D，力的模式（Force或Impulse）

**语法**：
```csharp
// 持续施加力（受质量影响）
rigidbody2D.AddForce(Vector2.right * 10f, ForceMode2D.Force);

// 瞬间施加力（冲量，不受质量影响）
rigidbody2D.AddForce(Vector2.up * 10f, ForceMode2D.Impulse);
```

**使用场景**：
- `ForceMode2D.Force`：持续推力、风力、水流
- `ForceMode2D.Impulse`：跳跃、爆炸、击退

**最佳实践**：
```csharp
// ✅ 推荐：跳跃使用Impulse
void Jump()
{
    if (IsGrounded())
    {
        _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
    }
}

// ✅ 推荐：风力使用Force
void ApplyWind(Vector2 windDirection)
{
    _rb.AddForce(windDirection * _windStrength, ForceMode2D.Force);
}
```

**注意事项**：
- Force受mass影响，Impulse不受mass影响
- 在FixedUpdate中调用AddForce

#### 2. MovePosition()（移动到指定位置）
**类型**：方法  
**参数**：
- `position`：Vector2，目标位置

**语法**：
```csharp
rigidbody2D.MovePosition(targetPosition);
```

**使用场景**：
- Kinematic刚体的移动
- 需要精确控制位置的物体
- 避免物理穿透

**最佳实践**：
```csharp
// ✅ 推荐：Kinematic刚体使用MovePosition
public class MovingPlatform : MonoBehaviour
{
    private Rigidbody2D _rb;
    
    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.bodyType = RigidbodyType2D.Kinematic;
    }
    
    void FixedUpdate()
    {
        Vector2 targetPosition = Vector2.Lerp(_rb.position, _endPosition, _speed * Time.fixedDeltaTime);
        _rb.MovePosition(targetPosition);
    }
}
```

**注意事项**：
- 仅适用于Kinematic类型的刚体
- 在FixedUpdate中调用

## 二、Collider2D（2D碰撞器）

### 概述
Collider2D定义物体的碰撞体形状，用于碰撞检测和物理交互。

### BoxCollider2D（盒形碰撞器）

**用途**：矩形碰撞体  
**性能**：⭐⭐⭐ 高性能  

**核心属性**：
- `size`：Vector2，碰撞体大小
- `offset`：Vector2，碰撞体偏移
- `isTrigger`：bool，是否为触发器

**语法**：
```csharp
BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
boxCollider.size = new Vector2(1f, 2f);
boxCollider.offset = new Vector2(0f, 0.5f);
boxCollider.isTrigger = false;
```

**使用场景**：
- 矩形物体（箱子、平台、角色）
- UI元素的碰撞检测
- 墙壁、障碍物

**最佳实践**：
```csharp
// ✅ 推荐：角色使用BoxCollider2D
void SetupCollider()
{
    BoxCollider2D collider = GetComponent<BoxCollider2D>();
    collider.size = new Vector2(0.8f, 1.8f); // 角色大小
    collider.offset = Vector2.zero;
    collider.isTrigger = false;
}
```

### CircleCollider2D（圆形碰撞器）

**用途**：圆形碰撞体  
**性能**：⭐⭐⭐ 高性能  

**核心属性**：
- `radius`：float，碰撞体半径
- `offset`：Vector2，碰撞体偏移

**语法**：
```csharp
CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();
circleCollider.radius = 0.5f;
circleCollider.offset = Vector2.zero;
```

**使用场景**：
- 圆形物体（球、子弹、炸弹）
- 角色头部碰撞体
- 简化的碰撞检测

**最佳实践**：
```csharp
// ✅ 推荐：子弹使用CircleCollider2D
void SetupBulletCollider()
{
    CircleCollider2D collider = GetComponent<CircleCollider2D>();
    collider.radius = 0.1f; // 小半径
    collider.isTrigger = true; // 触发器
}
```

### CapsuleCollider2D（胶囊碰撞器）

**用途**：胶囊形状碰撞体  
**性能**：⭐⭐⭐ 高性能  

**核心属性**：
- `size`：Vector2，碰撞体大小
- `direction`：CapsuleDirection2D，胶囊方向（Vertical或Horizontal）

**语法**：
```csharp
CapsuleCollider2D capsuleCollider = GetComponent<CapsuleCollider2D>();
capsuleCollider.size = new Vector2(0.5f, 2f);
capsuleCollider.direction = CapsuleDirection2D.Vertical;
```

**使用场景**：
- 角色碰撞体（更自然的碰撞）
- 需要圆角的矩形物体

**最佳实践**：
```csharp
// ✅ 推荐：角色使用CapsuleCollider2D（比Box更自然）
void SetupCharacterCollider()
{
    CapsuleCollider2D collider = GetComponent<CapsuleCollider2D>();
    collider.size = new Vector2(0.5f, 1.8f);
    collider.direction = CapsuleDirection2D.Vertical;
}
```

### PolygonCollider2D（多边形碰撞器）

**用途**：复杂形状碰撞体  
**性能**：⭐⭐ 中等性能  

**核心属性**：
- `points`：Vector2[]，碰撞体顶点数组
- `pathCount`：int，路径数量

**语法**：
```csharp
PolygonCollider2D polygonCollider = GetComponent<PolygonCollider2D>();
// 通常由Sprite自动生成，手动设置较复杂
```

**使用场景**：
- 复杂形状物体（地形、不规则物体）
- Sprite自动生成的碰撞体

**注意事项**：
- 性能较差，避免复杂形状
- 简化碰撞体或使用多个简单碰撞体替代

### 共同属性

#### isTrigger（触发器）
**类型**：`bool`  
**默认值**：`false`  
**用途**：设置为触发器，不产生物理碰撞，只检测进入事件  

**使用场景**：
- 收集物品（金币、道具）
- 触发区域（进入场景、触发对话）
- 检测区域（敌人视野、陷阱触发）

**最佳实践**：
```csharp
// ✅ 推荐：收集物品使用Trigger
public class CollectibleItem : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 收集物品逻辑
            Destroy(gameObject);
        }
    }
}
```

#### bounds（碰撞体边界）
**类型**：`Bounds`  
**用途**：获取碰撞体的世界空间边界  

**语法**：
```csharp
Bounds bounds = collider2D.bounds;
Vector3 center = bounds.center;
Vector3 size = bounds.size;
Vector3 extents = bounds.extents; // size的一半
```

**使用场景**：
- 射线检测起点计算
- 碰撞体可视化
- 判断物体是否在视野内

**最佳实践**：
```csharp
// ✅ 推荐：使用bounds计算射线起点
bool CheckGround()
{
    Collider2D collider = GetComponent<Collider2D>();
    float groundCheckDistance = 0.1f;
    
    RaycastHit2D hit = Physics2D.Raycast(
        collider.bounds.center,
        Vector2.down,
        collider.bounds.extents.y + groundCheckDistance,
        _groundLayer
    );
    
    return hit.collider != null;
}
```

## 三、Physics2D（2D物理静态类）

### 射线检测

#### Physics2D.Raycast()（射线检测）
**类型**：静态方法  
**返回值**：`RaycastHit2D`  

**参数**：
- `origin`：Vector2，射线起点
- `direction`：Vector2，射线方向
- `distance`：float，射线距离（可选）
- `layerMask`：int，层级过滤（可选）
- `minDepth`：float，最小深度（可选）
- `maxDepth`：float，最大深度（可选）

**语法**：
```csharp
// 基础射线检测
RaycastHit2D hit = Physics2D.Raycast(origin, direction);

// 带距离限制
RaycastHit2D hit = Physics2D.Raycast(origin, direction, distance);

// 带LayerMask过滤（推荐）
RaycastHit2D hit = Physics2D.Raycast(origin, direction, distance, layerMask);
```

**返回值RaycastHit2D**：
- `collider`：Collider2D，检测到的碰撞器（为null表示未检测到）
- `point`：Vector2，碰撞点
- `normal`：Vector2，碰撞点法线
- `distance`：float，射线起点到碰撞点的距离
- `rigidbody`：Rigidbody2D，检测到的刚体

**使用场景**：
- 地面检测
- 瞄准系统
- 视线检测
- 前方障碍物检测

**最佳实践**：
```csharp
// ✅ 推荐：地面检测使用Raycast + LayerMask
[SerializeField] private LayerMask _groundLayer;

bool IsGrounded()
{
    Collider2D collider = GetComponent<Collider2D>();
    float groundCheckDistance = 0.1f;
    
    RaycastHit2D hit = Physics2D.Raycast(
        collider.bounds.center,
        Vector2.down,
        collider.bounds.extents.y + groundCheckDistance,
        _groundLayer
    );
    
    return hit.collider != null;
}

// ✅ 推荐：使用OnDrawGizmos可视化调试
void OnDrawGizmos()
{
    if (_collider == null) return;
    
    Gizmos.color = Color.red;
    Gizmos.DrawRay(
        _collider.bounds.center,
        Vector2.down * (_collider.bounds.extents.y + 0.1f)
    );
}
```

**性能优化**：
```csharp
// ⚠️ 警告：每帧多次Raycast影响性能
void Update()
{
    // 限制射线检测频率
    if (Time.frameCount % 3 == 0)
    {
        CheckGround();
    }
}

// ✅ 推荐：使用LayerMask减少检测范围
RaycastHit2D hit = Physics2D.Raycast(
    origin,
    direction,
    distance,
    LayerMask.GetMask("Ground", "Platform") // 只检测特定层
);
```

#### Physics2D.RaycastNonAlloc()（无GC射线检测）
**类型**：静态方法  
**返回值**：`int`（检测到的碰撞器数量）  

**参数**：
- `origin`：Vector2
- `direction`：Vector2
- `results`：RaycastHit2D[]（预分配数组）
- `distance`：float
- `layerMask`：int

**语法**：
```csharp
RaycastHit2D[] results = new RaycastHit2D[10];
int hitCount = Physics2D.RaycastNonAlloc(origin, direction, results, distance, layerMask);

for (int i = 0; i < hitCount; i++)
{
    RaycastHit2D hit = results[i];
    // 处理检测结果
}
```

**使用场景**：
- 需要检测多个碰撞器
- 避免GC分配（高性能场景）

**最佳实践**：
```csharp
// ✅ 推荐：预分配数组，避免每帧分配
private RaycastHit2D[] _raycastResults = new RaycastHit2D[5];

void CheckCollisions()
{
    int hitCount = Physics2D.RaycastNonAlloc(
        transform.position,
        Vector2.down,
        _raycastResults,
        10f,
        _groundLayer
    );
    
    for (int i = 0; i < hitCount; i++)
    {
        // 处理每个检测结果
    }
}
```

### 范围检测

#### Physics2D.OverlapCircle()（圆形范围检测）
**类型**：静态方法  
**返回值**：`Collider2D`（检测到的第一个碰撞器）  

**参数**：
- `point`：Vector2，圆心
- `radius`：float，半径
- `layerMask`：int，层级过滤（可选）

**语法**：
```csharp
Collider2D hitCollider = Physics2D.OverlapCircle(point, radius, layerMask);

if (hitCollider != null)
{
    // 检测到碰撞器
}
```

**使用场景**：
- 敌人视野检测
- 爆炸范围检测
- 拾取范围检测

**最佳实践**：
```csharp
// ✅ 推荐：敌人视野检测
bool CanSeePlayer()
{
    Collider2D playerCollider = Physics2D.OverlapCircle(
        transform.position,
        _viewRadius,
        _playerLayer
    );
    
    return playerCollider != null;
}

// ✅ 推荐：使用OnDrawGizmos可视化
void OnDrawGizmos()
{
    Gizmos.color = Color.yellow;
    Gizmos.DrawWireSphere(transform.position, _viewRadius);
}
```

#### Physics2D.OverlapCircleNonAlloc()（无GC圆形检测）
**类型**：静态方法  
**返回值**：`int`（检测到的碰撞器数量）  

**参数**：
- `point`：Vector2
- `radius`：float
- `results`：Collider2D[]（预分配数组）
- `layerMask`：int

**语法**：
```csharp
Collider2D[] results = new Collider2D[10];
int hitCount = Physics2D.OverlapCircleNonAlloc(point, radius, results, layerMask);

for (int i = 0; i < hitCount; i++)
{
    Collider2D collider = results[i];
    // 处理检测结果
}
```

**使用场景**：
- 需要检测范围内所有碰撞器
- 避免GC分配（高性能场景）

**最佳实践**：
```csharp
// ✅ 推荐：爆炸范围伤害
private Collider2D[] _explosionResults = new Collider2D[20];

void Explode()
{
    int hitCount = Physics2D.OverlapCircleNonAlloc(
        transform.position,
        _explosionRadius,
        _explosionResults,
        _enemyLayer
    );
    
    for (int i = 0; i < hitCount; i++)
    {
        // 对敌人造成伤害
        Enemy enemy = _explosionResults[i].GetComponent<Enemy>();
        enemy?.TakeDamage(_damage);
    }
}
```

#### Physics2D.OverlapBox()（盒形范围检测）
**类型**：静态方法  
**返回值**：`Collider2D`  

**参数**：
- `point`：Vector2，中心点
- `size`：Vector2，盒子大小
- `angle`：float，旋转角度
- `layerMask`：int

**语法**：
```csharp
Collider2D hitCollider = Physics2D.OverlapBox(point, size, angle, layerMask);
```

**使用场景**：
- 矩形区域检测
- 攻击范围检测
- 区域触发检测

**最佳实践**：
```csharp
// ✅ 推荐：攻击范围检测
bool AttackHit()
{
    Collider2D hit = Physics2D.OverlapBox(
        transform.position + transform.right * _attackRange,
        new Vector2(_attackWidth, _attackHeight),
        0f,
        _enemyLayer
    );
    
    return hit != null;
}
```

## 四、PhysicsMaterial2D（物理材质）

### 概述
PhysicsMaterial2D用于定义碰撞体的表面属性，如摩擦力和弹力。

### 核心属性

#### friction（摩擦力）
**类型**：`float`  
**范围**：0 - 1  
**默认值**：0.4  
**用途**：控制物体表面的摩擦力  

**使用场景**：
- 冰面（friction = 0，滑）
- 粗糙地面（friction = 1，不滑）

**最佳实践**：
```csharp
// ✅ 推荐：创建冰面材质
PhysicsMaterial2D iceMaterial = new PhysicsMaterial2D();
iceMaterial.friction = 0f;
iceMaterial.bounciness = 0f;

// 应用到碰撞器
collider.sharedMaterial = iceMaterial;
```

#### bounciness（弹力）
**类型**：`float`  
**范围**：0 - 1  
**默认值**：0  
**用途**：控制物体碰撞后的弹跳程度  

**使用场景**：
- 橡皮球（bounciness = 0.8，弹跳）
- 地面（bounciness = 0，不弹跳）

**最佳实践**：
```csharp
// ✅ 推荐：创建弹跳球材质
PhysicsMaterial2D bouncyMaterial = new PhysicsMaterial2D();
bouncyMaterial.friction = 0.2f;
bouncyMaterial.bounciness = 0.8f;

// 应用到碰撞器
collider.sharedMaterial = bouncyMaterial;
```

### 使用示例

```csharp
// ✅ 推荐：创建不同地面材质
public class GroundMaterials : MonoBehaviour
{
    [SerializeField] private PhysicsMaterial2D _normalGround;
    [SerializeField] private PhysicsMaterial2D _iceGround;
    [SerializeField] private PhysicsMaterial2D _bouncyGround;
    
    void CreateMaterials()
    {
        // 普通地面
        _normalGround = new PhysicsMaterial2D("NormalGround");
        _normalGround.friction = 0.6f;
        _normalGround.bounciness = 0f;
        
        // 冰面
        _iceGround = new PhysicsMaterial2D("IceGround");
        _iceGround.friction = 0f;
        _iceGround.bounciness = 0f;
        
        // 弹跳地面
        _bouncyGround = new PhysicsMaterial2D("BouncyGround");
        _bouncyGround.friction = 0.4f;
        _bouncyGround.bounciness = 0.8f;
    }
}
```

## 五、碰撞事件

### OnCollision系列

#### OnCollisionEnter2D（碰撞开始）
**触发时机**：两个碰撞器开始接触时（非Trigger）  

**语法**：
```csharp
private void OnCollisionEnter2D(Collision2D collision)
{
    // collision.collider：碰撞到的碰撞器
    // collision.gameObject：碰撞到的游戏对象
    // collision.contacts：碰撞点数组
    // collision.relativeVelocity：相对速度
    
    if (collision.gameObject.CompareTag("Enemy"))
    {
        // 碰撞到敌人
    }
}
```

**使用场景**：
- 碰撞伤害
- 碰撞音效
- 碰撞特效

**最佳实践**：
```csharp
// ✅ 推荐：碰撞伤害
private void OnCollisionEnter2D(Collision2D collision)
{
    if (collision.gameObject.CompareTag("Enemy"))
    {
        // 计算碰撞伤害
        float damage = collision.relativeVelocity.magnitude * _damageMultiplier;
        TakeDamage(damage);
        
        // 播放碰撞音效
        AudioManager.Instance.PlayCollisionSound();
    }
}
```

#### OnCollisionStay2D（碰撞持续）
**触发时机**：两个碰撞器持续接触时（每帧触发，非Trigger）  

**语法**：
```csharp
private void OnCollisionStay2D(Collision2D collision)
{
    // 持续碰撞时的逻辑
}
```

**使用场景**：
- 持续接触伤害（岩浆、毒气）
- 移动平台

#### OnCollisionExit2D（碰撞结束）
**触发时机**：两个碰撞器分离时（非Trigger）  

**语法**：
```csharp
private void OnCollisionExit2D(Collision2D collision)
{
    // 碰撞结束时的逻辑
}
```

**使用场景**：
- 离开地面
- 离开平台

### OnTrigger系列

#### OnTriggerEnter2D（触发器开始）
**触发时机**：碰撞器进入Trigger区域时  

**语法**：
```csharp
private void OnTriggerEnter2D(Collider2D other)
{
    // other：进入触发器的碰撞器
    
    if (other.CompareTag("Player"))
    {
        // 玩家进入触发区域
    }
}
```

**使用场景**：
- 收集物品
- 进入区域触发事件
- 陷阱触发

**最佳实践**：
```csharp
// ✅ 推荐：收集物品
public class CollectibleItem : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 增加分数
            GameManager.Instance.AddScore(_scoreValue);
            
            // 播放音效
            AudioManager.Instance.PlayCollectSound();
            
            // 销毁物品
            Destroy(gameObject);
        }
    }
}

// ✅ 推荐：触发区域
public class TriggerZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 触发事件
            EventManager.Instance.OnPlayerEnterZone?.Invoke(_zoneId);
        }
    }
}
```

#### OnTriggerStay2D（触发器持续）
**触发时机**：碰撞器持续在Trigger区域内时  

**语法**：
```csharp
private void OnTriggerStay2D(Collider2D other)
{
    // 持续在触发区域内时的逻辑
}
```

**使用场景**：
- 持续伤害区域
- 安全区域

#### OnTriggerExit2D（触发器结束）
**触发时机**：碰撞器离开Trigger区域时  

**语法**：
```csharp
private void OnTriggerExit2D(Collider2D other)
{
    // 离开触发区域时的逻辑
}
```

**使用场景**：
- 离开区域
- 结束触发

## 六、LayerMask（层级过滤）

### 概述
LayerMask用于过滤射线检测和范围检测，只检测特定层级的对象。

### 创建LayerMask

**方法1：使用LayerMask.GetMask**
```csharp
LayerMask groundLayer = LayerMask.GetMask("Ground");
LayerMask multipleLayers = LayerMask.GetMask("Ground", "Platform", "Wall");
```

**方法2：使用SerializeField（推荐）**
```csharp
[SerializeField] private LayerMask _groundLayer;
[SerializeField] private LayerMask _enemyLayer;
```

### 使用LayerMask

```csharp
// ✅ 推荐：射线检测使用LayerMask
RaycastHit2D hit = Physics2D.Raycast(
    origin,
    direction,
    distance,
    _groundLayer // 只检测Ground层
);

// ✅ 推荐：范围检测使用LayerMask
Collider2D enemy = Physics2D.OverlapCircle(
    point,
    radius,
    _enemyLayer // 只检测Enemy层
);
```

### 性能优势

使用LayerMask可以：
1. 减少检测范围，提高性能
2. 避免检测不需要的对象
3. 减少不必要的碰撞计算

## 七、完整示例代码

### 示例1：角色移动控制器

```csharp
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移动参数")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _jumpForce = 10f;
    
    [Header("检测参数")]
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _groundCheckDistance = 0.1f;
    
    private Rigidbody2D _rb;
    private Collider2D _collider;
    private bool _isGrounded;
    
    void Awake()
    {
        // ✅ 缓存组件引用
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        
        // ✅ 冻结旋转
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }
    
    void Update()
    {
        // 输入处理
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        
        if (Input.GetButtonDown("Jump") && _isGrounded)
        {
            Jump();
        }
        
        Move(horizontalInput);
    }
    
    void FixedUpdate()
    {
        // ✅ 在FixedUpdate中检测地面
        _isGrounded = CheckGround();
    }
    
    void Move(float horizontalInput)
    {
        // ✅ 直接设置velocity控制移动
        _rb.velocity = new Vector2(horizontalInput * _moveSpeed, _rb.velocity.y);
    }
    
    void Jump()
    {
        // ✅ 使用AddForce施加跳跃力
        _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
    }
    
    bool CheckGround()
    {
        // ✅ 使用Raycast + LayerMask检测地面
        RaycastHit2D hit = Physics2D.Raycast(
            _collider.bounds.center,
            Vector2.down,
            _collider.bounds.extents.y + _groundCheckDistance,
            _groundLayer
        );
        
        return hit.collider != null;
    }
    
    // ✅ 可视化调试
    void OnDrawGizmos()
    {
        if (_collider == null) return;
        
        Gizmos.color = _isGrounded ? Color.green : Color.red;
        Gizmos.DrawRay(
            _collider.bounds.center,
            Vector2.down * (_collider.bounds.extents.y + _groundCheckDistance)
        );
    }
}
```

### 示例2：敌人视野检测

```csharp
using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    [Header("视野参数")]
    [SerializeField] private float _viewRadius = 5f;
    [SerializeField] private LayerMask _playerLayer;
    [SerializeField] private LayerMask _obstacleLayer;
    
    private Transform _player;
    private bool _canSeePlayer;
    
    void Update()
    {
        _canSeePlayer = CheckPlayerInView();
    }
    
    bool CheckPlayerInView()
    {
        // ✅ 使用OverlapCircle检测玩家
        Collider2D playerCollider = Physics2D.OverlapCircle(
            transform.position,
            _viewRadius,
            _playerLayer
        );
        
        if (playerCollider == null) return false;
        
        _player = playerCollider.transform;
        
        // ✅ 使用Raycast检测视线阻挡
        Vector2 direction = (_player.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, _player.position);
        
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            direction,
            distance,
            _obstacleLayer
        );
        
        // 如果没有检测到障碍物，说明可以看到玩家
        return hit.collider == null;
    }
    
    void OnDrawGizmos()
    {
        // 绘制视野范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _viewRadius);
        
        // 绘制到玩家的射线
        if (_player != null && _canSeePlayer)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, _player.position);
        }
    }
}
```

### 示例3：爆炸范围伤害

```csharp
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [Header("爆炸参数")]
    [SerializeField] private float _explosionRadius = 3f;
    [SerializeField] private int _damage = 50;
    [SerializeField] private LayerMask _enemyLayer;
    
    // ✅ 预分配数组避免GC
    private Collider2D[] _explosionResults = new Collider2D[20];
    
    public void Explode()
    {
        // ✅ 使用OverlapCircleNonAlloc避免GC
        int hitCount = Physics2D.OverlapCircleNonAlloc(
            transform.position,
            _explosionRadius,
            _explosionResults,
            _enemyLayer
        );
        
        for (int i = 0; i < hitCount; i++)
        {
            // 对敌人造成伤害
            Enemy enemy = _explosionResults[i].GetComponent<Enemy>();
            if (enemy != null)
            {
                // 根据距离计算伤害
                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                float damageFactor = 1f - (distance / _explosionRadius);
                int actualDamage = Mathf.RoundToInt(_damage * damageFactor);
                
                enemy.TakeDamage(actualDamage);
            }
        }
        
        // 播放爆炸特效
        // 播放爆炸音效
        
        // 销毁爆炸对象
        Destroy(gameObject);
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _explosionRadius);
    }
}
```

---

**文档版本**：v1.0.0  
**更新日期**：2026-03-16  
**Unity版本**：2020.3 LTS - 2022.3 LTS
