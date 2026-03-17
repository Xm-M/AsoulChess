# Character Movement 基础概念

## 什么是角色移动系统？

角色移动系统是游戏中最核心的系统之一，负责处理玩家输入并将其转化为角色的物理运动。在Unity 2D游戏中，角色移动系统需要考虑物理引擎、输入系统、碰撞检测等多个子系统的协同工作。

---

## 三种主要移动模式

### 1. 平台跳跃模式（Platformer）

**特点**：
- 左右移动 + 跳跃
- 受重力影响
- 需要地面检测
- 角色翻转

**经典游戏**：
- 《空洞骑士》
- 《Celeste》
- 《超级马里奥》

**核心要素**：
```
必需功能：
├── 水平移动
├── 跳跃
├── 地面检测
├── 角色翻转
└── 重力控制

常见可选功能：
├── 二段跳/多段跳
├── 冲刺/闪避
├── 攀爬
├── 下蹲
├── 游泳
├── 抓墙
└── 蹬墙跳
```

### 2. 俯视移动模式（Top-Down）

**特点**：
- 四方向移动
- 无重力影响
- 通常无跳跃
- 角色朝向控制

**经典游戏**：
- 《塞尔达传说》
- 《元气骑士》
- 《挺进地牢》

**核心要素**：
```
必需功能：
├── 四方向移动
├── 角色朝向
└── 碰撞处理

常见可选功能：
├── 对角线移动
├── 冲刺
├── 推箱子
└── 八方向限制
```

### 3. 横版格斗模式（Fighter）

**特点**：
- 复杂状态管理
- 精确帧控制
- 输入缓冲
- 连招系统

**经典游戏**：
- 《街头霸王》
- 《任天堂大乱斗》
- 《铁拳》

**核心要素**：
```
必需功能：
├── 移动控制
├── 状态机
├── 攻击系统
└── 输入缓冲

常见可选功能：
├── 连招系统
├── 格挡/闪避
├── 特殊技能
└── 能量条
```

---

## 物理系统基础

### Rigidbody2D

Rigidbody2D是Unity 2D物理系统的核心组件，负责对象的物理模拟。

**关键属性**：

| 属性 | 说明 | 推荐值 |
|------|------|--------|
| Body Type | 物理类型 | Dynamic（玩家） |
| Mass | 质量 | 1 |
| Linear Drag | 线性阻力 | 0 |
| Angular Drag | 角阻力 | 0.05 |
| Gravity Scale | 重力缩放 | 2-4（平台跳跃） |
| Constraints | 约束 | Freeze Rotation Z |

**Body Type选择**：

```
Dynamic（动态）：
├── 用于：玩家、敌人、移动物体
├── 特点：受物理引擎控制
└── 推荐：所有角色控制器

Kinematic（运动学）：
├── 用于：平台、电梯、触发器
├── 特点：不受物理力影响
└── 推荐：移动平台

Static（静态）：
├── 用于：地面、墙壁
├── 特点：完全不移动
└── 推荐：场景固定物体
```

### Collider2D

碰撞器定义对象的物理边界。

**常用类型**：

| 碰撞器 | 适用场景 | 性能 |
|--------|---------|------|
| BoxCollider2D | 方形角色 | ⭐⭐⭐⭐⭐ |
| CapsuleCollider2D | 人形角色 | ⭐⭐⭐⭐ |
| CircleCollider2D | 圆形角色 | ⭐⭐⭐⭐⭐ |

**推荐配置**：
```
玩家角色：
├── CapsuleCollider2D（主碰撞器）
│   ├── Size: (0.5, 1.0)
│   └── Offset: (0, 0)
└── 无需额外碰撞器

俯视角色：
├── CircleCollider2D
│   ├── Radius: 0.25
│   └── Offset: (0, -0.25)
└── 或BoxCollider2D
```

---

## 输入系统基础

### 新输入系统 vs 旧输入系统

| 特性 | 新Input System | 旧Input Manager |
|------|---------------|-----------------|
| 状态 | ✅ 推荐 | ⚠️ 逐步废弃 |
| 性能 | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ |
| 灵活性 | 高 | 低 |
| 学习曲线 | 中等 | 简单 |
| 多设备支持 | ✅ 原生 | ❌ 手动 |
| 事件驱动 | ✅ | ❌ |

### 新输入系统使用

**安装步骤**：
```
1. Package Manager → Input System → Install
2. Project Settings → Player → Active Input Handling → Both
3. 创建Input Actions资产
4. 绑定Input Action资产到脚本
```

**Input Action配置**：
```
PlayerControls.inputactions
├── Move（Vector2）
│   └── Binding: WASD / Left Stick
├── Jump（Button）
│   └── Binding: Space / Button South
└── Dash（Button）
    └── Binding: Shift / Button West
```

---

## 地面检测基础

### 为什么需要地面检测？

```
目的：
1. 确定是否可以跳跃
2. 播放正确的动画状态
3. 应用不同的物理参数
4. 触发地面特效

不准确的后果：
❌ 空中可以跳跃
❌ 动画状态错误
❌ 物理行为异常
```

### 检测方法对比

| 方法 | 精确度 | 性能 | 适用场景 |
|------|--------|------|---------|
| Raycast | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | 单点精确检测 |
| OverlapCircle | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | 区域检测（推荐） |
| OverlapBox | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | 方形区域 |
| OnTriggerEnter | ⭐⭐⭐ | ⭐⭐⭐⭐ | 事件驱动 |

### 推荐实现

```csharp
// 方法1：OverlapCircle（推荐）
private bool CheckGround()
{
    return Physics2D.OverlapCircle(
        groundCheckPoint.position,
        groundCheckRadius,
        groundLayer
    ) != null;
}

// 方法2：Raycast（精确）
private bool CheckGroundRaycast()
{
    RaycastHit2D hit = Physics2D.Raycast(
        groundCheckPoint.position,
        Vector2.down,
        groundCheckDistance,
        groundLayer
    );
    return hit.collider != null;
}

// 方法3：多点检测（大型角色）
private bool CheckGroundMultiple()
{
    Vector2[] checkPoints = new Vector2[]
    {
        new Vector2(transform.position.x - 0.2f, transform.position.y),
        new Vector2(transform.position.x, transform.position.y),
        new Vector2(transform.position.x + 0.2f, transform.position.y)
    };
    
    foreach (var point in checkPoints)
    {
        if (Physics2D.OverlapCircle(point, 0.1f, groundLayer) != null)
            return true;
    }
    return false;
}
```

---

## 移动实现方式对比

### 方式1：直接设置速度（推荐）

```csharp
// ✅ 推荐：精确控制
rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

优点：
- 响应即时
- 速度精确
- 不受摩擦力影响

缺点：
- 需要手动处理加速/减速
```

### 方式2：AddForce

```csharp
// ⚠️ 警告：不精确
rb.AddForce(Vector2.right * moveForce);

优点：
- 自然物理感
- 适合推力效果

缺点：
- 速度不精确
- 需要调整阻力
- 难以精确控制
```

### 方式3：MovePosition

```csharp
// ⚠️ 警告：需要插值
rb.MovePosition(rb.position + moveVector * Time.fixedDeltaTime);

优点：
- 精确位置控制

缺点：
- 可能穿透碰撞器
- 需要正确插值
```

---

## 跳跃实现原理

### 基础跳跃公式

```
跳跃高度 h = v² / (2g)

其中：
- h = 跳跃高度
- v = 起跳速度
- g = 重力加速度

示例计算：
- 目标高度：3米
- 重力：9.81 m/s²
- 起跳速度 = √(2 × 9.81 × 3) ≈ 7.67 m/s
```

### 跳跃类型

```
1. 普通跳跃
   rb.velocity = new Vector2(rb.velocity.x, jumpForce);

2. 可变高度跳跃
   if (jumpInputReleased && rb.velocity.y > 0)
   {
       rb.velocity *= 0.5f; // 松开按键时降低跳跃高度
   }

3. 二段跳
   if (jumpCount < maxJumpCount)
   {
       rb.velocity = new Vector2(rb.velocity.x, jumpForce);
       jumpCount++;
   }

4. 蹬墙跳
   if (isTouchingWall)
   {
       rb.velocity = new Vector2(wallJumpDirection * jumpForce, jumpForce);
   }
```

---

## 性能优化基础

### 基本原则

```
1. 缓存组件引用
   ✅ Awake中GetComponent
   ❌ Update中GetComponent

2. 避免每帧分配
   ✅ 使用已有变量
   ❌ new Vector2() 在Update中

3. 物理操作在FixedUpdate
   ✅ rb.velocity在FixedUpdate
   ❌ rb.velocity在Update

4. 使用Layer过滤
   ✅ LayerMask参数
   ❌ 检测所有Layer
```

### 性能检查清单

```
□ 组件引用已缓存
□ Layer已预计算
□ 物理操作在FixedUpdate
□ 无Update中的new操作
□ 无GameObject.Find
□ 无每帧GetComponent
□ 地面检测使用OverlapCircle
□ 输入使用新Input System
```

---

## 调试技巧

### Gizmos可视化

```csharp
private void OnDrawGizmosSelected()
{
    // 地面检测范围
    Gizmos.color = Color.green;
    Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
    
    // 移动方向
    Gizmos.color = Color.blue;
    Gizmos.DrawRay(transform.position, Vector2.right * moveDirection);
    
    // 速度向量
    Gizmos.color = Color.red;
    Gizmos.DrawRay(transform.position, rb.velocity);
}
```

### 调试信息显示

```csharp
private void OnGUI()
{
    GUILayout.Label($"Speed: {rb.velocity.magnitude:F2}");
    GUILayout.Label($"Grounded: {isGrounded}");
    GUILayout.Label($"Jump Count: {jumpCount}");
}
```

---

## 常见问题

### Q: 角色一直下落？

```
检查项：
1. Collider是否正确配置
2. Ground Layer是否设置
3. groundCheckPoint位置是否正确
4. groundCheckRadius是否太小
```

### Q: 移动有惯性？

```
原因：Linear Drag设置问题

解决：
方案1：设置Linear Drag = 0
方案2：在代码中处理减速
rb.velocity = Vector2.Lerp(rb.velocity, targetVelocity, deceleration);
```

### Q: 跳跃高度不一致？

```
原因：
1. 使用AddForce而非velocity
2. 地面检测时机不对
3. 重力Scale影响

解决：
✅ 使用velocity设置跳跃
✅ 在FixedUpdate中检测地面
✅ 统一重力Scale
```

---

## 扩展学习

### 官方资源
- [Unity 2D物理教程](https://learn.unity.com/tutorial/2d-physics)
- [Input System入门](https://learn.unity.com/project/input-system)
- [2D平台跳跃教程](https://learn.unity.com/project/2d-platformer)

### 进阶主题
- 角色状态机设计
- Coyote Time（土狼时间）
- Jump Buffering（跳跃缓冲）
- 角色动画系统集成
