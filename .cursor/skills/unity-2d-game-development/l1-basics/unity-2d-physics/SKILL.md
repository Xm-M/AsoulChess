---
name: unity-2d-physics
description: Unity 2D物理系统核心功能，包括刚体、碰撞器、射线检测等，是2D游戏物理效果的基础。
---

# Unity 2D物理系统

## 概述

Unity 2D物理系统用于处理2D游戏中的物理效果，包括刚体运动、碰撞检测、触发器、射线检测等。这是2D游戏开发的核心系统，几乎所有需要物理效果的游戏都会用到。

## API白名单 ⚠️ 强制遵守

### 推荐使用的API

| API | 用途 | 使用场景 | 性能等级 |
|-----|------|---------|---------|
| `Rigidbody2D` | 2D刚体组件 | 角色移动、物理对象 | ⭐⭐⭐ 高性能 |
| `Rigidbody2D.velocity` | 读写刚体速度 | 移动控制 | ⭐⭐⭐ 高性能 |
| `Rigidbody2D.AddForce()` | 施加力 | 跳跃、推力、爆炸 | ⭐⭐⭐ 高性能 |
| `Rigidbody2D.AddForceAtPosition()` | 在指定位置施加力 | 物理交互 | ⭐⭐ 中等性能 |
| `Rigidbody2D.MovePosition()` | 移动到指定位置 | 精确移动控制 | ⭐⭐⭐ 高性能 |
| `Collider2D` | 2D碰撞器基类 | 碰撞检测 | ⭐⭐⭐ 高性能 |
| `BoxCollider2D` | 盒形碰撞器 | 矩形碰撞体 | ⭐⭐⭐ 高性能 |
| `CircleCollider2D` | 圆形碰撞器 | 圆形碰撞体 | ⭐⭐⭐ 高性能 |
| `CapsuleCollider2D` | 胶囊碰撞器 | 角色碰撞体 | ⭐⭐⭐ 高性能 |
| `PolygonCollider2D` | 多边形碰撞器 | 复杂形状 | ⭐⭐ 中等性能 |
| `Physics2D.Raycast()` | 射线检测 | 地面检测、瞄准 | ⭐⭐⭐ 高性能 |
| `Physics2D.OverlapCircle()` | 圆形范围检测 | 范围检测 | ⭐⭐⭐ 高性能 |
| `Physics2D.OverlapBox()` | 盒形范围检测 | 区域检测 | ⭐⭐⭐ 高性能 |
| `PhysicsMaterial2D` | 物理材质 | 摩擦力、弹力 | ⭐⭐⭐ 高性能 |
| `ContactFilter2D` | 接触过滤器 | 碰撞过滤 | ⭐⭐⭐ 高性能 |
| `RaycastHit2D` | 射线检测结果 | 射线信息 | ⭐⭐⭐ 高性能 |
| `Collider2D.IsTouching()` | 碰撞检测 | 检测是否接触 | ⭐⭐ 中等性能 |

### 禁止使用的API

| API | 禁用原因 | 替代方案 |
|-----|---------|---------|
| `GameObject.Find()` | 性能极差，每帧遍历所有对象 | 使用单例模式或缓存引用 |
| `GameObject.FindWithTag()` | 性能差 | 使用单例或事件系统 |
| `Object.FindObjectOfType<>()` | 性能极差 | 使用单例或依赖注入 |
| `Transform.Find()` | 性能差 | 缓存子对象引用 |
| `GetComponent<>()`在Update/FixedUpdate中 | 性能差，频繁调用 | 在Awake中缓存引用 |
| `Physics2D.OverlapCircleAll()` | 返回数组产生GC | 使用OverlapCircleNonAlloc |
| `Physics2D.OverlapPointAll()` | 返回数组产生GC | 使用OverlapPointNonAlloc |
| `Physics2D.RaycastAll()` | 返回数组产生GC | 使用RaycastNonAlloc |
| `Physics2D.LinecastAll()` | 返回数组产生GC | 使用LinecastNonAlloc |

### 性能警告API

| API | 警告原因 | 优化建议 |
|-----|---------|---------|
| `Physics2D.OverlapCircle()`频繁调用 | 每帧多次调用影响性能 | 限制调用频率，使用计时器 |
| `Physics2D.Raycast()`多次调用 | 每帧多次射线检测影响性能 | 合并射线检测，使用LayerMask |
| `Rigidbody2D.mass`频繁修改 | 触发物理引擎重新计算 | 避免运行时频繁修改质量 |
| `Collider2D.enabled`频繁切换 | 触发物理引擎重建 | 使用Trigger代替enabled切换 |
| `Physics2D.IgnoreCollision()` | 影响物理引擎性能 | 少量使用，避免频繁调用 |
| `Rigidbody2D.simulated`频繁切换 | 触发物理引擎重建 | 使用对象池代替 |
| `PolygonCollider2D`复杂形状 | 碰撞检测开销大 | 简化碰撞体形状或使用多个简单碰撞体 |

## API使用规范 ⚠️ 强制遵守

### 正确使用示例

```csharp
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // ✅ 正确：在Awake中缓存组件引用
    private Rigidbody2D _rb;
    private Collider2D _collider;
    
    // ✅ 正确：使用SerializeField暴露私有字段
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _jumpForce = 10f;
    [SerializeField] private LayerMask _groundLayer;
    
    void Awake()
    {
        // ✅ 正确：在Awake中获取组件引用
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
    }
    
    void FixedUpdate()
    {
        // ✅ 正确：物理操作在FixedUpdate中执行
        Move();
    }
    
    void Move()
    {
        // ✅ 正确：直接设置velocity控制移动
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        _rb.velocity = new Vector2(horizontalInput * _moveSpeed, _rb.velocity.y);
    }
    
    void Jump()
    {
        // ✅ 正确：使用射线检测地面，添加LayerMask过滤
        float groundCheckDistance = 0.1f;
        RaycastHit2D hit = Physics2D.Raycast(
            _collider.bounds.center,
            Vector2.down,
            _collider.bounds.extents.y + groundCheckDistance,
            _groundLayer
        );
        
        if (hit.collider != null)
        {
            // ✅ 正确：使用AddForce施加跳跃力
            _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
        }
    }
    
    // ✅ 正确：使用OnDrawGizmos可视化调试
    void OnDrawGizmos()
    {
        if (_collider == null) return;
        
        Gizmos.color = Color.red;
        Gizmos.DrawRay(
            _collider.bounds.center,
            Vector2.down * (_collider.bounds.extents.y + 0.1f)
        );
    }
}
```

### 错误使用示例

```csharp
using UnityEngine;

public class BadPlayerMovement : MonoBehaviour
{
    void Update()
    {
        // ❌ 错误：在Update中调用GetComponent
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        
        // ❌ 错误：物理操作在Update中执行（应该用FixedUpdate）
        rb.velocity = new Vector2(5f, rb.velocity.y);
        
        // ❌ 错误：在Update中使用GameObject.Find
        GameObject ground = GameObject.Find("Ground");
        
        // ❌ 错误：使用GameObject.FindWithTag
        GameObject player = GameObject.FindWithTag("Player");
    }
    
    void CheckGround()
    {
        // ❌ 错误：使用Physics2D.OverlapCircleAll产生GC
        Collider2D[] colliders = Physics2D.OverlapCircleAll(
            transform.position,
            1f
        );
        
        foreach (var collider in colliders)
        {
            // 处理碰撞
        }
    }
    
    void Jump()
    {
        // ❌ 错误：没有使用LayerMask，检测所有层
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            Vector2.down,
            1f
        );
        
        // ❌ 错误：没有检查hit.collider是否为空
        if (hit.collider.CompareTag("Ground"))
        {
            GetComponent<Rigidbody2D>().AddForce(Vector2.up * 10f);
        }
    }
}
```

## 功能边界 ⚠️ 强制说明

### 本Skill涵盖范围

- ✅ Rigidbody2D基础使用（刚体组件、速度控制、力的施加）
- ✅ Collider2D系列碰撞器（Box、Circle、Capsule、Polygon）
- ✅ 触发器（Trigger）使用
- ✅ 射线检测（Raycast、Linecast）
- ✅ 范围检测（OverlapCircle、OverlapBox）
- ✅ 物理材质（PhysicsMaterial2D）
- ✅ 碰撞事件（OnCollisionEnter2D、OnTriggerEnter2D）
- ✅ 基础碰撞检测

### 不在本Skill范围内

- ❌ 关节系统（DistanceJoint2D、HingeJoint2D等）→ 见unity-2d-physics-joints Skill（L3级）
- ❌ 物理效果器（AreaEffector2D、PlatformEffector2D等）→ 见unity-2d-physics-effector Skill（L3级）
- ❌ 3D物理系统（Rigidbody、Collider）→ 本项目不涉及，禁止使用
- ❌ 物理引擎底层API → 不涉及
- ❌ 自定义物理模拟 → 不涉及

### 跨Skill功能依赖

**角色移动开发需要**：
- unity-2d-physics（物理系统）← 当前Skill
- unity-input-system（输入系统）
- unity-2d-character-movement（角色移动）→ L2级Skill

**地面检测需要**：
- unity-2d-physics（射线检测）← 当前Skill
- unity-2d-physics（LayerMask）
- unity-2d-character-movement（地面检测方法）→ L2级Skill

**碰撞伤害系统需要**：
- unity-2d-physics（碰撞检测）← 当前Skill
- unity-2d-physics（Trigger）
- 状态管理相关Skill

## 渐进式学习路径

### 阶段1：基础使用（必须掌握）

**学习目标**：掌握Rigidbody2D和Collider2D的基础使用

**核心概念**：
1. Rigidbody2D：控制物体的物理行为
2. Collider2D：定义物体的碰撞体形状
3. 碰撞与触发：理解Collision和Trigger的区别

**快速上手**：
1. 添加Rigidbody2D组件
2. 添加Collider2D组件（BoxCollider2D或CircleCollider2D）
3. 设置Rigidbody2D参数（Body Type、Mass、Gravity Scale）
4. 编写简单的移动代码

**练习项目**：
- 创建一个受重力影响下落的方块
- 创建一个可以左右移动的角色
- 实现简单的碰撞检测

### 阶段2：常用功能（大部分场景需要）

**学习目标**：掌握射线检测、物理材质、触发器

**核心技能**：
1. 射线检测：地面检测、瞄准、视线检测
2. 物理材质：摩擦力、弹力效果
3. 触发器：进入区域触发事件
4. LayerMask：碰撞层过滤

**实用技巧**：
- 使用LayerMask优化射线检测性能
- 使用OnDrawGizmos可视化射线
- 使用PhysicsMaterial2D实现弹跳效果
- 使用Trigger实现收集物品

**练习项目**：
- 实现角色跳跃和地面检测
- 实现弹跳球效果
- 实现拾取物品功能
- 实现敌人检测玩家范围

### 阶段3：进阶功能（特定需求）

**学习目标**：掌握范围检测、碰撞过滤、性能优化

**核心技能**：
1. 范围检测：OverlapCircle、OverlapBox
2. 碰撞过滤：ContactFilter2D
3. NonAlloc API：避免GC的检测方法
4. 碰撞事件详解：OnCollision系列回调

**高级应用**：
- 实现爆炸范围伤害
- 实现敌人视野检测
- 实现复杂的碰撞过滤
- 优化物理检测性能

**练习项目**：
- 实现手雷爆炸效果
- 实现敌人巡逻和追击AI
- 实现多层级碰撞检测

### 阶段4：最佳实践（推荐学习）

**性能优化**：
1. 缓存组件引用，避免频繁GetComponent
2. 使用LayerMask减少检测范围
3. 使用NonAlloc API避免GC
4. 限制射线检测频率
5. 简化Collider形状

**常见问题**：
1. 穿墙问题：调整Rigidbody2D参数，使用Continuous碰撞检测模式
2. 卡顿问题：减少射线检测，优化碰撞体数量
3. 抖动问题：检查FixedUpdate设置，调整物理更新频率
4. 碰撞失效：检查Layer碰撞矩阵，检查Collider是否启用

**避坑指南**：
- 不要在Update中调用GetComponent
- 不要频繁修改Rigidbody2D.mass
- 不要使用复杂的PolygonCollider2D
- 不要忘记设置LayerMask
- 不要忽略性能警告API

## 相关Skills

### 前置Skills
- 无（这是L1级核心基础Skill）

### 后续Skills
- **unity-2d-character-movement**（L2级）：角色移动的完整实现
- **unity-input-system**（L1级）：输入系统集成
- **unity-2d-physics-joints**（L3级）：关节系统高级功能
- **unity-2d-physics-effector**（L3级）：物理效果器高级功能

### 配合使用
- **unity-2d-sprite**（L1级）：配合物理系统的Sprite渲染
- **unity-2d-animation**（L1级）：物理角色的动画控制
- **unity-object-pool**（L2级）：物理对象的对象池管理

## 参考资料索引

详细API文档和教程请查看：
- `references/api-reference.md` - 完整API参考文档
- `references/basics.md` - 基础教程
- `references/advanced.md` - 高级教程
- `references/best-practices.md` - 最佳实践
- `references/common-issues.md` - 常见问题
- `references/performance-guide.md` - 性能优化指南

示例代码：
- `scripts/examples/example-basic.cs` - 基础示例
- `scripts/examples/example-advanced.cs` - 高级示例
- `scripts/examples/example-optimized.cs` - 性能优化示例
