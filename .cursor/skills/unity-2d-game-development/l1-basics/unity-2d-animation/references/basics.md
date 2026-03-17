# Unity 2D Animation 基础概念

## 动画系统概述

Unity提供两套动画系统：

### 1. Mecanim动画系统（推荐）
**特点**：
- 使用Animator组件
- 可视化状态机编辑器
- 支持动画混合、重定向、IK
- 性能优异，功能强大

**适用场景**：
- 2D/3D角色动画
- 复杂动画状态管理
- 需要动画混合的场景

### 2. Legacy动画系统（传统）
**特点**：
- 使用Animation组件
- 功能简单，向后兼容
- 适合简单动画

**适用场景**：
- 简单的属性动画
- 旧项目维护

---

## 核心概念

### 1. Animation Clip（动画片段）
**定义**：包含动画数据的资源文件，记录属性随时间变化的曲线

**类型**：
- **外部导入**：从3D建模软件导入（FBX、BLEND）
- **内部创建**：在Unity中直接创建（Window → Animation）

**属性**：
- `length`：动画时长
- `frameRate`：帧率
- `wrapMode`：循环模式（Legacy模式）
- `events`：动画事件

**创建方式**：
1. 选中游戏对象
2. 打开Animation窗口（Ctrl+6）
3. 点击Create New Clip
4. 录制关键帧或导入序列帧

---

### 2. Animator Controller（动画控制器）
**定义**：控制动画播放的状态机资源，管理动画状态和过渡

**组成**：
- **States（状态）**：动画片段的容器
- **Transitions（过渡）**：状态切换的规则
- **Parameters（参数）**：控制状态机的变量

**创建方式**：
1. Project窗口右键
2. Create → Animator Controller
3. 双击打开Animator窗口
4. 添加状态和过渡

---

### 3. Animator Component（动画器组件）
**定义**：挂载在游戏对象上，运行时控制动画播放的组件

**关键属性**：
- `Controller`：关联的Animator Controller
- `Avatar`：人形骨骼映射（仅Humanoid类型）
- `Apply Root Motion`：是否应用根运动
- `Update Mode`：更新模式
- `Culling Mode`：剔除模式

---

### 4. Animation State Machine（动画状态机）
**定义**：管理动画状态和切换的可视化系统

**核心元素**：

#### Entry（入口）
- 状态机的起始点
- 自动连接到默认状态（橙色状态）

#### Exit（出口）
- 状态机的终止点
- 用于退出子状态机

#### Any State（任意状态）
- 可以从任何状态转换到目标状态
- 常用于跳跃、受伤等随时可能触发的事件

#### State（状态）
- 橙色：默认状态
- 灰色：普通状态
- 包含Motion（动画片段）或Blend Tree

---

## Animator组件使用流程

### 步骤1：准备动画资源
```csharp
// 方式1：导入序列帧
// 选中多张Sprite → 拖入Scene → 自动创建Animation Clip

// 方式2：在Animation窗口创建
// Window → Animation → Create New Clip
```

---

### 步骤2：创建Animator Controller
```
1. Project窗口右键
2. Create → Animator Controller
3. 命名（如：PlayerAnimator）
```

---

### 步骤3：配置动画状态机
```
1. 双击Animator Controller打开编辑器
2. 右键 → Create State → Empty
3. 在Motion字段选择Animation Clip
4. 设置默认状态（右键 → Set as Layer Default State）
5. 添加参数（Parameters标签）
```

---

### 步骤4：添加Animator组件
```csharp
// 方式1：通过Inspector
// Add Component → Animator
// 拖入Animator Controller

// 方式2：代码添加
GameObject player = new GameObject("Player");
Animator animator = player.AddComponent<Animator>();
animator.runtimeAnimatorController = Resources.Load<AnimatorController>("PlayerAnimator");
```

---

### 步骤5：脚本控制动画
```csharp
public class PlayerController : MonoBehaviour
{
    private Animator animator;
    
    // 参数哈希缓存
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int JumpTriggerHash = Animator.StringToHash("Jump");
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    
    private void Update()
    {
        // 设置移动速度参数
        float speed = Input.GetAxis("Horizontal");
        animator.SetFloat(SpeedHash, Mathf.Abs(speed));
        
        // 跳跃
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            animator.SetTrigger(JumpTriggerHash);
        }
    }
    
    private bool IsGrounded()
    {
        // 地面检测逻辑
        return Physics2D.Raycast(transform.position, Vector2.down, 0.1f);
    }
}
```

---

### 步骤6：测试与调试
```
1. 运行游戏
2. 打开Animator窗口
3. 观察状态切换是否正常
4. 调整参数值测试过渡条件
```

---

## 动画状态机配置

### 状态类型

#### 1. Motion State（动作状态）
- 包含单个Animation Clip
- 最常用的状态类型

#### 2. Blend Tree（混合树）
- 混合多个动画片段
- 根据参数值插值混合
- 适用于移动动画（Walk/Run混合）

**创建Blend Tree**：
```
1. 右键 → Create State → From Blend Tree
2. 双击Blend Tree进入编辑
3. 添加Motion字段
4. 设置参数和阈值
```

---

### 过渡配置

#### Transition属性
- **Has Exit Time**：是否等待当前动画播放完成
- **Exit Time**：退出时间（0-1归一化值）
- **Transition Duration**：过渡持续时间
- **Transition Offset**：目标动画起始偏移
- **Interruption Source**：打断源（允许其他过渡打断当前过渡）

#### Conditions（条件）
支持四种参数类型：
1. **Float**：大于/小于/等于某个值
2. **Int**：等于/不等于某个值
3. **Bool**：true/false
4. **Trigger**：触发器被触发时

**示例配置**：
```
Idle → Walk
- Conditions: Speed > 0.1
- Transition Duration: 0.2
- Has Exit Time: false

Walk → Run
- Conditions: Speed > 5.0
- Transition Duration: 0.15
- Has Exit Time: false
```

---

### 动画图层

#### 用途
- 同时播放多个动画
- 分离身体不同部位的动画
- 上半身攻击 + 下半身移动

#### 配置步骤
```
1. Animator窗口 → Layers标签
2. 点击"+"添加新图层
3. 设置Weight（权重，0-1）
4. 添加Avatar Mask（遮罩）
```

#### Avatar Mask
- 限制动画影响的骨骼部位
- 上半身遮罩：启用头部、手臂、躯干
- 下半身遮罩：启用腿部

---

## 动画参数和过渡

### 四种参数类型

#### 1. Float（浮点数）
**用途**：连续变化的值（速度、方向）

**示例**：
```csharp
// 移动速度
animator.SetFloat("Speed", currentSpeed);

// 方向
animator.SetFloat("DirectionX", moveInput.x);
animator.SetFloat("DirectionY", moveInput.y);
```

**过渡条件**：
- Speed > 0.1
- Speed < 5.0
- DirectionX > 0

---

#### 2. Int（整数）
**用途**：离散状态选择（武器类型、连击数）

**示例**：
```csharp
// 武器类型
public enum WeaponType { Sword = 0, Bow = 1, Staff = 2 }
animator.SetInteger("WeaponType", (int)currentWeapon);

// 连击数
animator.SetInteger("ComboCount", comboCount);
```

**过渡条件**：
- WeaponType Equals 0
- ComboCount Equals 3

---

#### 3. Bool（布尔值）
**用途**：开关状态（地面检测、移动状态）

**示例**：
```csharp
// 地面检测
animator.SetBool("IsGrounded", IsOnGround());

// 移动状态
animator.SetBool("IsMoving", speed > 0.1f);

// 潜行状态
animator.SetBool("IsCrouching", isCrouching);
```

**过渡条件**：
- IsGrounded true
- IsMoving false

---

#### 4. Trigger（触发器）
**用途**：一次性事件（跳跃、攻击、受伤）

**特性**：
- 触发后自动重置
- 适合一次性动画切换
- 如果状态机未及时响应可能丢失

**示例**：
```csharp
// 跳跃
if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
{
    animator.SetTrigger("Jump");
    isGrounded = false;
}

// 攻击
if (Input.GetMouseButtonDown(0))
{
    animator.SetTrigger("Attack");
}

// 受伤
public void TakeDamage()
{
    animator.SetTrigger("Hurt");
}
```

**过渡条件**：
- Jump（Trigger触发时）

---

### 过渡条件组合

#### 单条件过渡
```
Idle → Walk
- Conditions: Speed > 0.1
```

#### 多条件过渡（AND关系）
```
Idle → Run
- Conditions: Speed > 5.0, IsGrounded true
（必须同时满足两个条件）
```

#### Any State过渡
```
Any State → Jump
- Conditions: Jump (Trigger)
- Can Transition To Self: false
（从任意状态跳跃）
```

---

## 常见问题与解决方案

### 问题1：动画在屏幕外停止播放

**现象**：角色移出摄像机视野后动画停止

**原因**：默认Culling Mode为CullUpdateTransform

**解决方案**：
```csharp
// 方案1：始终播放动画
animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

// 方案2：屏幕外停止动画（推荐）
animator.cullingMode = AnimatorCullingMode.CullCompletely;

// 方案3：停止动画但更新Transform
animator.cullingMode = AnimatorCullingMode.CullUpdateTransform;
```

---

### 问题2：动画切换不流畅

**现象**：动画切换时有明显的跳跃感

**原因**：
1. 使用Play()直接切换（无过渡）
2. 过渡时间设置过短
3. 过渡条件设置不当

**解决方案**：
```csharp
// ❌ 不推荐：直接切换
animator.Play("Walk");

// ✅ 推荐：使用CrossFade平滑过渡
animator.CrossFade(WalkHash, 0.25f); // 0.25秒过渡

// ✅ 或在Animator Controller中设置Has Exit Time
// Transition Duration: 0.2-0.3秒
```

---

### 问题3：性能开销过大

**现象**：游戏运行卡顿，CPU占用高

**诊断**：
```
1. Unity Profiler → Animation模块
2. 查看Animator.Update耗时
3. 检查Active Animator数量
```

**优化方案**：
```csharp
// 1. 使用StringToHash优化参数调用
private static readonly int SpeedHash = Animator.StringToHash("Speed");
animator.SetFloat(SpeedHash, speed); // 性能提升5-10倍

// 2. 控制Active Animator数量
// 建议单场景≤30个Active Animator
animator.enabled = false; // 禁用屏幕外的Animator

// 3. 配置Culling Mode
animator.cullingMode = AnimatorCullingMode.CullCompletely;

// 4. 避免Update中频繁查询状态
// ❌ 不推荐
void Update()
{
    var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
}

// ✅ 推荐：使用标记
private bool needCheckState = false;
void Update()
{
    if (needCheckState)
    {
        var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        // 检查完成后重置标记
        needCheckState = false;
    }
}
```

---

### 问题4：动画状态查询不准确

**现象**：GetCurrentAnimatorStateInfo返回的状态与实际不符

**原因**：在过渡期间调用，获取的是过渡前的状态

**解决方案**：
```csharp
// 检查是否正在过渡
if (animator.IsInTransition(0))
{
    // 获取下一个状态
    var nextState = animator.GetNextAnimatorStateInfo(0);
    Debug.Log($"正在过渡到: {nextState.shortNameHash}");
}
else
{
    // 获取当前状态
    var currentState = animator.GetCurrentAnimatorStateInfo(0);
}
```

---

### 问题5：动画参数设置无效

**现象**：设置参数后动画没有响应

**排查步骤**：
```
1. 检查参数名称是否拼写正确
2. 检查参数类型是否匹配（Bool/Float/Int/Trigger）
3. 检查过渡条件是否满足
4. 检查Animator Controller是否正确挂载
5. 运行时查看Animator窗口的参数值
```

**调试代码**：
```csharp
// 打印所有参数
foreach (var param in animator.parameters)
{
    Debug.Log($"参数: {param.name}, 类型: {param.type}");
}

// 验证参数是否存在
private static readonly int SpeedHash = Animator.StringToHash("Speed");
bool hasParam = false;
foreach (var param in animator.parameters)
{
    if (param.nameHash == SpeedHash)
    {
        hasParam = true;
        break;
    }
}
if (!hasParam)
{
    Debug.LogError("参数Speed不存在！");
}
```

---

## 最佳实践总结

### 性能优化
1. ✅ 使用StringToHash缓存所有参数
2. ✅ 控制Active Animator数量≤30
3. ✅ 使用Culling Mode优化屏幕外动画
4. ✅ 避免Update中频繁查询状态

### 代码规范
1. ✅ 参数哈希声明为static readonly
2. ✅ 使用枚举管理动画状态
3. ✅ 使用事件驱动动画逻辑
4. ✅ 分离动画控制与游戏逻辑

### 状态机设计
1. ✅ 合理使用Any State简化状态图
2. ✅ 设置合理的过渡时间（0.2-0.3秒）
3. ✅ 使用Blend Tree混合相似动画
4. ✅ 使用Layer分离上下半身动画