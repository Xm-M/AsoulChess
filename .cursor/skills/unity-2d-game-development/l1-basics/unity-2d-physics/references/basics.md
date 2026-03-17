# Unity 2D物理系统 - 基础教程

## 一、什么是Unity 2D物理系统？

### 概念解释

Unity 2D物理系统是一个**模拟真实物理效果**的游戏系统，它可以让游戏对象像现实世界中的物体一样运动和交互。

**简单理解**：
- 如果你想让物体**受重力影响下落**
- 如果你想让物体**碰撞后反弹**
- 如果你想让角色**跳跃**
- 如果你想让物体**受力移动**

那么你就需要使用Unity 2D物理系统！

### 生活中的类比

想象一下现实世界：
- 你扔出一个球，球会受重力影响下落（这是重力）
- 球撞到墙壁会反弹（这是碰撞）
- 你推一个箱子，箱子会移动（这是力的作用）

Unity 2D物理系统就是在游戏中**模拟这些真实的物理现象**。

## 二、为什么需要学习2D物理系统？

### 必要性

对于2D游戏开发，物理系统是**最核心的系统之一**。几乎所有2D游戏都会用到：

**平台跳跃游戏**（如《空洞骑士》《Celeste》）：
- 角色移动需要物理系统
- 角色跳跃需要物理系统
- 地面检测需要物理系统

**俯视射击游戏**（如《元气骑士》《挺进地牢》）：
- 角色移动需要物理系统
- 子弹碰撞需要物理系统
- 敌人追踪需要物理系统

**休闲游戏**（如《愤怒的小鸟》《割绳子》）：
- 物体下落需要物理系统
- 碰撞检测需要物理系统
- 物理效果需要物理系统

### 学习收益

掌握2D物理系统后，你可以：
1. ✅ 实现角色移动和跳跃
2. ✅ 实现物体碰撞和反弹
3. ✅ 实现地面检测
4. ✅ 实现物理效果（风力、推力）
5. ✅ 实现碰撞伤害
6. ✅ 实现收集物品

## 三、核心组件介绍

### 1. Rigidbody2D（刚体组件）

#### 什么是刚体？

**刚体**是物理系统的核心组件，它让游戏对象**受物理规则控制**。

**类比理解**：
- 没有刚体 → 像空气中的幽灵，不受任何物理影响
- 有刚体 → 像现实中的物体，受重力、力、碰撞影响

#### 刚体的作用

- ✅ 让物体受重力影响下落
- ✅ 让物体可以受力移动
- ✅ 让物体参与碰撞检测
- ✅ 让物体有速度、质量等物理属性

#### 如何添加刚体？

**步骤1**：选中游戏对象

**步骤2**：点击Add Component按钮

**步骤3**：搜索"Rigidbody 2D"

**步骤4**：点击添加

![添加Rigidbody2D](添加刚体组件的截图)

#### 刚体的关键属性

| 属性 | 说明 | 常用值 |
|------|------|--------|
| **Body Type** | 刚体类型 | Dynamic（动态）、Kinematic（运动学）、Static（静态） |
| **Mass** | 质量 | 1（默认） |
| **Gravity Scale** | 重力缩放 | 1（默认），0（不受重力） |
| **Constraints** | 约束 | Freeze Rotation（冻结旋转） |

#### 刚体类型详解

**Dynamic（动态）**：
- 受重力和力的影响
- 会碰撞和被碰撞
- **最常用**，适用于角色、敌人、可移动物体

**Kinematic（运动学）**：
- 不受重力和力的影响
- 只能通过脚本控制移动
- 适用于平台、电梯

**Static（静态）**：
- 不移动
- 仅用于碰撞检测
- 适用于墙壁、地面

### 2. Collider2D（碰撞器组件）

#### 什么是碰撞器？

**碰撞器**定义了物体的**物理形状**，用于检测碰撞。

**类比理解**：
- 碰撞器就像物体的"外壳"
- 当两个物体的外壳接触时，就发生了碰撞

#### 碰撞器的作用

- ✅ 定义物体的碰撞形状
- ✅ 检测物体间的碰撞
- ✅ 阻止物体互相穿透

#### 碰撞器的类型

| 类型 | 形状 | 适用场景 | 性能 |
|------|------|---------|------|
| **BoxCollider2D** | 矩形 | 墙壁、平台、箱子 | ⭐⭐⭐ 高 |
| **CircleCollider2D** | 圆形 | 球、子弹、炸弹 | ⭐⭐⭐ 高 |
| **CapsuleCollider2D** | 胶囊 | 角色 | ⭐⭐⭐ 高 |
| **PolygonCollider2D** | 多边形 | 复杂形状 | ⭐⭐ 中 |

#### 如何添加碰撞器？

**步骤1**：选中游戏对象

**步骤2**：点击Add Component按钮

**步骤3**：搜索对应的碰撞器类型（如"Box Collider 2D"）

**步骤4**：点击添加

#### 触发器（Trigger）

**触发器**是一种特殊的碰撞器，它**不会产生物理碰撞**，只会检测物体进入。

**使用场景**：
- 收集物品（金币、道具）
- 进入区域触发事件
- 检测敌人范围

**如何设置触发器**：
勾选碰撞器组件的"Is Trigger"选项即可。

### 3. Rigidbody2D + Collider2D 组合

**重要概念**：
- 只有刚体 → 物体会受重力下落，但会穿透地面
- 只有碰撞器 → 物体不会移动，只是障碍物
- **刚体 + 碰撞器** → 物体会受重力下落，并且碰撞地面

**最佳实践**：
- 角色：Rigidbody2D + CapsuleCollider2D
- 墙壁：Collider2D（不需要刚体）
- 平台：Rigidbody2D（Kinematic）+ BoxCollider2D
- 子弹：Collider2D（Trigger）

## 四、快速上手教程

### 练习1：创建下落的方块

**目标**：让一个方块受重力影响下落

**步骤**：

1. **创建方块**
   - 右键 Hierarchy → 2D Object → Sprites → Square
   - 命名为"FallingCube"

2. **添加刚体**
   - 选中 FallingCube
   - Add Component → Rigidbody 2D
   - 保持默认设置

3. **添加碰撞器**
   - Add Component → Box Collider 2D
   - 保持默认设置

4. **创建地面**
   - 右键 Hierarchy → 2D Object → Sprites → Square
   - 命名为"Ground"
   - 调整位置到方块下方
   - 调整大小：Scale = (3, 0.5, 1)
   - 添加 Box Collider 2D（不需要刚体）

5. **测试**
   - 点击 Play 按钮
   - 观察方块下落并停在地面

**原理说明**：
- 方块有刚体 → 受重力影响下落
- 方块有碰撞器 → 定义碰撞形状
- 地面有碰撞器 → 定义碰撞形状
- 方块碰撞地面 → 停止下落

### 练习2：创建移动的角色

**目标**：让角色左右移动

**步骤**：

1. **创建角色**
   - 右键 Hierarchy → 2D Object → Sprites → Capsule
   - 命名为"Player"

2. **添加组件**
   - Rigidbody 2D
     - Constraints → Freeze Rotation Z（冻结旋转）
   - Capsule Collider 2D

3. **创建脚本**
   - 在Project窗口右键 → Create → C# Script
   - 命名为"PlayerMovement"
   - 双击打开编辑

4. **编写代码**

```csharp
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // 移动速度
    [SerializeField] private float _moveSpeed = 5f;
    
    // 刚体引用
    private Rigidbody2D _rb;
    
    void Awake()
    {
        // 获取刚体组件
        _rb = GetComponent<Rigidbody2D>();
    }
    
    void Update()
    {
        // 获取输入
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        
        // 移动角色
        Move(horizontalInput);
    }
    
    void Move(float horizontalInput)
    {
        // 设置刚体速度
        _rb.velocity = new Vector2(horizontalInput * _moveSpeed, _rb.velocity.y);
    }
}
```

5. **添加脚本**
   - 将脚本拖到Player对象上

6. **测试**
   - 点击 Play 按钮
   - 使用 A/D 或 ←/→ 键移动角色

**代码说明**：
- `GetComponent<Rigidbody2D>()`：获取刚体组件
- `Input.GetAxisRaw("Horizontal")`：获取水平输入（-1到1）
- `_rb.velocity`：设置刚体速度，控制移动

### 练习3：实现角色跳跃

**目标**：让角色可以跳跃

**步骤**：

1. **完善脚本**

```csharp
using UnityEngine;

public class PlayerMovement : MonoBehaviour
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
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        
        // 冻结旋转
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }
    
    void Update()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        
        // 检测跳跃输入
        if (Input.GetButtonDown("Jump") && _isGrounded)
        {
            Jump();
        }
        
        Move(horizontalInput);
    }
    
    void FixedUpdate()
    {
        // 在FixedUpdate中检测地面
        _isGrounded = CheckGround();
    }
    
    void Move(float horizontalInput)
    {
        _rb.velocity = new Vector2(horizontalInput * _moveSpeed, _rb.velocity.y);
    }
    
    void Jump()
    {
        // 施加跳跃力
        _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
    }
    
    bool CheckGround()
    {
        // 射线检测地面
        RaycastHit2D hit = Physics2D.Raycast(
            _collider.bounds.center,
            Vector2.down,
            _collider.bounds.extents.y + _groundCheckDistance,
            _groundLayer
        );
        
        return hit.collider != null;
    }
    
    // 可视化调试
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

2. **设置Ground层级**
   - 选中Ground对象
   - Layer → Add Layer
   - 添加一个新层"Ground"
   - 将Ground对象设置为Ground层

3. **设置Player脚本**
   - Player对象的Inspector
   - 找到脚本组件
   - Ground Layer → 选择"Ground"

4. **测试**
   - 点击 Play 按钮
   - 使用 Space 键跳跃

**代码说明**：
- `Physics2D.Raycast()`：射线检测地面
- `_groundLayer`：只检测Ground层
- `_rb.AddForce()`：施加跳跃力
- `ForceMode2D.Impulse`：瞬间力（不受质量影响）

## 五、常见问题解答

### 问题1：为什么角色会旋转？

**原因**：角色碰撞物体时受到物理力的影响，导致旋转。

**解决方法**：
```csharp
// 冻结旋转
_rb.constraints = RigidbodyConstraints2D.FreezeRotation;
```

或者在Inspector中：
- Rigidbody 2D → Constraints → Freeze Rotation Z（勾选）

### 问题2：为什么角色会穿墙？

**原因**：
1. 移动速度过快
2. 碰撞检测模式不正确

**解决方法**：
1. 降低移动速度
2. 设置碰撞检测模式为Continuous：
   - Rigidbody 2D → Collision Detection → Continuous

### 问题3：为什么地面检测不准确？

**原因**：
1. 射线起点或距离设置不正确
2. LayerMask设置错误

**解决方法**：
1. 使用Gizmos可视化射线调试
2. 确保Ground对象的Layer设置正确
3. 确保脚本的Ground Layer设置正确

### 问题4：为什么角色下落很慢？

**原因**：Gravity Scale设置过小。

**解决方法**：
- Rigidbody 2D → Gravity Scale → 设置为 2 或 3

### 问题5：为什么无法检测碰撞？

**原因**：
1. 没有添加碰撞器
2. 碰撞器的isTrigger被勾选
3. Layer碰撞矩阵设置错误

**解决方法**：
1. 确保两个对象都有碰撞器
2. 确保至少一个对象有刚体
3. 检查 Edit → Project Settings → Physics 2D → Layer Collision Matrix

## 六、性能优化建议

### 1. 缓存组件引用

❌ **错误做法**：
```csharp
void Update()
{
    // 每帧获取组件，性能差
    Rigidbody2D rb = GetComponent<Rigidbody2D>();
    rb.velocity = new Vector2(5f, rb.velocity.y);
}
```

✅ **正确做法**：
```csharp
private Rigidbody2D _rb;

void Awake()
{
    // 在Awake中缓存引用
    _rb = GetComponent<Rigidbody2D>();
}

void Update()
{
    _rb.velocity = new Vector2(5f, _rb.velocity.y);
}
```

### 2. 使用LayerMask

❌ **错误做法**：
```csharp
// 检测所有层，性能差
RaycastHit2D hit = Physics2D.Raycast(origin, direction, distance);
```

✅ **正确做法**：
```csharp
[SerializeField] private LayerMask _groundLayer;

// 只检测Ground层，性能好
RaycastHit2D hit = Physics2D.Raycast(origin, direction, distance, _groundLayer);
```

### 3. 避免频繁物理检测

❌ **错误做法**：
```csharp
void Update()
{
    // 每帧多次射线检测，性能差
    CheckGround();
    CheckWall();
    CheckCeiling();
}
```

✅ **正确做法**：
```csharp
void FixedUpdate()
{
    // 在FixedUpdate中检测，减少检测频率
    CheckGround();
}

// 或使用计时器
private float _checkInterval = 0.1f;
private float _lastCheckTime;

void Update()
{
    if (Time.time - _lastCheckTime >= _checkInterval)
    {
        CheckGround();
        _lastCheckTime = Time.time;
    }
}
```

### 4. 简化碰撞器形状

❌ **错误做法**：
- 使用复杂的PolygonCollider2D

✅ **正确做法**：
- 使用简单的BoxCollider2D、CircleCollider2D
- 或使用多个简单碰撞器组合

## 七、下一步学习

完成基础学习后，建议继续学习：

1. **射线检测高级应用**
   - 瞄准系统
   - 视线检测
   - 前方障碍物检测

2. **范围检测**
   - 敌人视野检测
   - 爆炸范围伤害
   - 拾取范围检测

3. **物理材质**
   - 摩擦力效果
   - 弹力效果
   - 不同地面材质

4. **触发器应用**
   - 收集物品系统
   - 区域触发事件
   - 陷阱系统

详细内容请查看：
- `references/advanced.md` - 高级教程
- `references/best-practices.md` - 最佳实践
- `references/common-issues.md` - 常见问题

---

**教程版本**：v1.0.0  
**更新日期**：2026-03-16  
**适用Unity版本**：2020.3 LTS - 2022.3 LTS
