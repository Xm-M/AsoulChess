# Unity 2D Animation API Reference

## Animator 组件 API

### 概述
Animator是Unity Mecanim动画系统的核心组件，用于控制游戏对象的动画播放。它通过Animator Controller资源管理动画状态机，实现复杂的动画逻辑。

---

## 核心属性（Properties）

### runtimeAnimatorController
**类型**：`RuntimeAnimatorController`
**描述**：当前Animator使用的动画控制器资源
**访问**：读/写
**性能**：⚠️ 运行时切换Controller会触发Rebinding，开销较大

**示例**：
```csharp
// 获取当前Controller
RuntimeAnimatorController controller = animator.runtimeAnimatorController;

// ⚠️ 警告：运行时切换Controller性能开销大
// animator.runtimeAnimatorController = newController;
```

---

### speed
**类型**：`float`
**描述**：动画播放速度倍率，1.0为正常速度
**访问**：读/写
**性能**：✅ 轻量级属性，可频繁调用

**示例**：
```csharp
// 正常速度
animator.speed = 1.0f;

// 2倍速播放
animator.speed = 2.0f;

// 暂停动画
animator.speed = 0f;

// 反向播放
animator.speed = -1.0f;
```

**应用场景**：
- 慢动作效果（speed = 0.5f）
- 快进效果（speed = 2.0f）
- 暂停游戏时停止所有动画（speed = 0f）

---

### cullingMode
**类型**：`AnimatorCullingMode`
**描述**：控制当对象在摄像机视野外时的动画更新方式
**访问**：读/写
**性能**：✅ 重要性能优化属性

**枚举值**：
- `AlwaysAnimate`：始终更新动画（即使不可见）
- `CullUpdateTransform`：不可见时停止动画更新，但更新Transform
- `CullCompletely`：不可见时完全停止动画更新

**示例**：
```csharp
// 始终播放动画（适用于UI或重要动画）
animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

// 屏幕外停止动画（适用于大多数游戏对象）
animator.cullingMode = AnimatorCullingMode.CullCompletely;

// 屏幕外停止动画但继续更新位置
animator.cullingMode = AnimatorCullingMode.CullUpdateTransform;
```

**性能建议**：
- 背景角色：使用CullCompletely
- 移动角色：使用CullUpdateTransform
- UI动画：使用AlwaysAnimate

---

### updateMode
**类型**：`AnimatorUpdateMode`
**描述**：控制Animator的更新时机
**访问**：读/写

**枚举值**：
- `Normal`：按帧率更新（默认）
- `AnimatePhysics`：按物理帧更新（与FixedUpdate同步）
- `UnscaledTime`：不受Time.timeScale影响（适用于UI暂停菜单）

**示例**：
```csharp
// 正常更新（默认）
animator.updateMode = AnimatorUpdateMode.Normal;

// 物理更新（适用于根运动动画）
animator.updateMode = AnimatorUpdateMode.AnimatePhysics;

// 不受时间缩放影响（暂停菜单动画）
animator.updateMode = AnimatorUpdateMode.UnscaledTime;
```

---

### layerCount
**类型**：`int`（只读）
**描述**：当前Animator Controller中的动画层数量
**性能**：✅ 轻量级属性

**示例**：
```csharp
int layerCount = animator.layerCount;
Debug.Log($"动画层数量: {layerCount}");

for (int i = 0; i < layerCount; i++)
{
    // 遍历所有层
    var stateInfo = animator.GetCurrentAnimatorStateInfo(i);
}
```

---

### parameters
**类型**：`AnimatorControllerParameter[]`（只读）
**描述**：获取Animator Controller中定义的所有参数
**性能**：✅ 轻量级属性

**示例**：
```csharp
foreach (var param in animator.parameters)
{
    Debug.Log($"参数名: {param.name}, 类型: {param.type}");
}
```

---

### applyRootMotion
**类型**：`bool`
**描述**：是否应用根运动（Root Motion）
**访问**：读/写
**说明**：启用后，角色的移动由动画驱动而非代码控制

**示例**：
```csharp
// 启用根运动（动画驱动移动）
animator.applyRootMotion = true;

// 禁用根运动（代码控制移动）
animator.applyRootMotion = false;
```

**应用场景**：
- 走路、跑步动画：启用根运动
- 射击、攻击动画：禁用根运动

---

### hasRootMotion
**类型**：`bool`（只读）
**描述**：当前动画是否包含根运动数据

---

### isHuman
**类型**：`bool`（只读）
**描述**：当前Avatar是否为Humanoid类型

---

### avatar
**类型**：`Avatar`
**描述**：当前使用的Avatar资源（仅Humanoid类型）
**访问**：读/写

---

### velocity / angularVelocity
**类型**：`Vector3`（只读）
**描述**：上一帧的角色线速度/角速度（仅根运动动画）

---

## 核心方法（Methods）

### Play
**签名**：
```csharp
public void Play(string stateName, int layer = -1, float normalizedTime = float.NegativeInfinity);
public void Play(int stateNameHash, int layer = -1, float normalizedTime = float.NegativeInfinity);
```

**描述**：立即播放指定的动画状态，无过渡效果

**参数**：
- `stateName`：状态名称（字符串）
- `stateNameHash`：状态名称的哈希值（性能优化）
- `layer`：目标图层索引，-1表示基础层
- `normalizedTime`：动画起始时间（0-1），0为开头，1为结尾

**返回值**：无

**性能**：
- ⚠️ 字符串版本：每次调用计算哈希
- ✅ 哈希版本：直接使用缓存值，性能更优

**示例**：
```csharp
// ⚠️ 字符串版本（不推荐频繁调用）
animator.Play("Idle");

// ✅ 哈希版本（推荐）
private static readonly int IdleHash = Animator.StringToHash("Idle");
animator.Play(IdleHash);

// 指定图层播放
animator.Play(IdleHash, 0);

// 从动画中间播放
animator.Play(IdleHash, 0, 0.5f); // 从50%位置开始
```

**注意事项**：
- Play()会立即切换状态，无过渡效果
- 适合初始状态设置或强制切换

---

### CrossFade
**签名**：
```csharp
public void CrossFade(string stateName, float normalizedTransitionDuration, int layer = -1, float normalizedTimeOffset = float.NegativeInfinity);
public void CrossFade(int stateHash, float normalizedTransitionDuration, int layer = -1, float normalizedTimeOffset = float.NegativeInfinity);
```

**描述**：平滑过渡到指定动画状态

**参数**：
- `stateName/stateHash`：目标状态
- `normalizedTransitionDuration`：过渡持续时间（归一化，0-1）
- `layer`：图层索引
- `normalizedTimeOffset`：目标状态起始时间偏移

**返回值**：无

**性能**：
- ⚠️ 字符串版本：每次计算哈希
- ✅ 哈希版本：推荐使用

**示例**：
```csharp
// ✅ 推荐用法：使用哈希缓存
private static readonly int WalkHash = Animator.StringToHash("Walk");
private static readonly int RunHash = Animator.StringToHash("Run");

// 平滑过渡到Walk状态，过渡时间为0.25秒
animator.CrossFade(WalkHash, 0.25f);

// 过渡到Run状态，过渡时间为动画长度的25%
animator.CrossFade(RunHash, 0.25f, 0, 0f);

// ⚠️ 不推荐：字符串版本
animator.CrossFade("Walk", 0.25f);
```

**与Play的区别**：
- `Play`：立即切换，无过渡
- `CrossFade`：平滑过渡，有混合效果

**应用场景**：
- 角色状态切换（Idle → Walk → Run）
- 武器切换动画
- 表情切换

---

### CrossFadeInFixedTime
**签名**：
```csharp
public void CrossFadeInFixedTime(string stateName, float fixedTransitionDuration, int layer = -1, float fixedTimeOffset = 0f);
public void CrossFadeInFixedTime(int stateHash, float fixedTransitionDuration, int layer = -1, float fixedTimeOffset = 0f);
```

**描述**：平滑过渡到指定状态，使用固定时间（秒）而非归一化时间

**参数**：
- `fixedTransitionDuration`：过渡持续时间（秒）
- `fixedTimeOffset`：目标状态起始时间偏移（秒）

**示例**：
```csharp
// 在0.3秒内平滑过渡到Walk状态
animator.CrossFadeInFixedTime(WalkHash, 0.3f);

// 从动画的第0.5秒开始播放
animator.CrossFadeInFixedTime(WalkHash, 0.3f, 0, 0.5f);
```

**与CrossFade的区别**：
- `CrossFade`：过渡时间是归一化值（0-1），相对于动画长度
- `CrossFadeInFixedTime`：过渡时间是固定秒数

---

### SetBool / GetBool
**签名**：
```csharp
public void SetBool(string name, bool value);
public void SetBool(int id, bool value);
public bool GetBool(string name);
public bool GetBool(int id);
```

**描述**：设置/获取布尔类型动画参数

**参数**：
- `name/id`：参数名称或哈希值
- `value`：布尔值

**返回值**：布尔值（GetBool）

**性能**：
- ⚠️ 字符串版本：每次计算哈希
- ✅ 哈希版本：推荐使用

**示例**：
```csharp
// ✅ 推荐用法
private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");

// 设置布尔参数
animator.SetBool(IsGroundedHash, true);
animator.SetBool(IsRunningHash, false);

// 获取布尔参数
bool isGrounded = animator.GetBool(IsGroundedHash);
```

**应用场景**：
- 地面检测状态（IsGrounded）
- 移动状态（IsMoving）
- 战斗状态（IsAttacking）

---

### SetFloat / GetFloat
**签名**：
```csharp
public void SetFloat(string name, float value);
public void SetFloat(int id, float value);
public float GetFloat(string name);
public float GetFloat(int id);
```

**描述**：设置/获取浮点类型动画参数

**参数**：
- `name/id`：参数名称或哈希值
- `value`：浮点值

**返回值**：浮点值（GetFloat）

**性能**：
- ⚠️ 字符串版本：每次计算哈希
- ✅ 哈希版本：推荐使用

**示例**：
```csharp
// ✅ 推荐用法
private static readonly int SpeedHash = Animator.StringToHash("Speed");
private static readonly int VelocityXHash = Animator.StringToHash("VelocityX");

// 设置浮点参数
animator.SetFloat(SpeedHash, 5.5f);
animator.SetFloat(VelocityXHash, moveInput.x);

// 获取浮点参数
float currentSpeed = animator.GetFloat(SpeedHash);
```

**应用场景**：
- 移动速度（Speed）
- 方向（DirectionX、DirectionY）
- Blend Tree参数（Velocity、Turn）

---

### SetInteger / GetInteger
**签名**：
```csharp
public void SetInteger(string name, int value);
public void SetInteger(int id, int value);
public int GetInteger(string name);
public int GetInteger(int id);
```

**描述**：设置/获取整数类型动画参数

**参数**：
- `name/id`：参数名称或哈希值
- `value`：整数值

**返回值**：整数值（GetInteger）

**示例**：
```csharp
// ✅ 推荐用法
private static readonly int ComboCountHash = Animator.StringToHash("ComboCount");
private static readonly int WeaponTypeHash = Animator.StringToHash("WeaponType");

// 设置整数参数
animator.SetInteger(ComboCountHash, 3);
animator.SetInteger(WeaponTypeHash, (int)WeaponType.Sword);

// 获取整数参数
int combo = animator.GetInteger(ComboCountHash);
```

**应用场景**：
- 连击计数（ComboCount）
- 武器类型（WeaponType）
- 角色状态编号（StateIndex）

---

### SetTrigger / ResetTrigger
**签名**：
```csharp
public void SetTrigger(string name);
public void SetTrigger(int id);
public void ResetTrigger(string name);
public void ResetTrigger(int id);
```

**描述**：
- `SetTrigger`：触发一个Trigger参数（用于一次性事件）
- `ResetTrigger`：重置Trigger参数

**参数**：
- `name/id`：参数名称或哈希值

**返回值**：无

**特性**：
- Trigger被触发后自动重置
- 适合一次性动画（跳跃、攻击、受伤）

**示例**：
```csharp
// ✅ 推荐用法
private static readonly int JumpTriggerHash = Animator.StringToHash("Jump");
private static readonly int AttackTriggerHash = Animator.StringToHash("Attack");

// 触发跳跃动画
animator.SetTrigger(JumpTriggerHash);

// 触发攻击动画
animator.SetTrigger(AttackTriggerHash);

// 重置Trigger（取消触发）
animator.ResetTrigger(JumpTriggerHash);
```

**应用场景**：
- 跳跃（Jump）
- 攻击（Attack）
- 受伤（Hurt）
- 技能释放（Skill）

**注意事项**：
- Trigger触发后会自动重置
- 如果状态机未及时响应，Trigger可能被忽略

---

### GetCurrentAnimatorStateInfo / GetNextAnimatorStateInfo
**签名**：
```csharp
public AnimatorStateInfo GetCurrentAnimatorStateInfo(int layerIndex);
public AnimatorStateInfo GetNextAnimatorStateInfo(int layerIndex);
```

**描述**：
- `GetCurrentAnimatorStateInfo`：获取当前动画状态信息
- `GetNextAnimatorStateInfo`：获取下一个动画状态信息（过渡期间）

**参数**：
- `layerIndex`：图层索引

**返回值**：`AnimatorStateInfo` 结构体

**性能**：⚠️ 避免在Update中频繁调用

**AnimatorStateInfo 结构体属性**：
- `shortNameHash`：状态名称哈希值
- `length`：动画长度（秒）
- `normalizedTime`：归一化时间（0-1）
- `speed`：动画速度
- `speedMultiplier`：速度倍率
- `loop`：是否循环播放
- `tagHash`：标签哈希值

**示例**：
```csharp
private static readonly int JumpHash = Animator.StringToHash("Jump");

// ⚠️ 警告：避免在Update中频繁调用
void Update()
{
    var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
    
    // 判断是否正在播放Jump动画
    if (stateInfo.shortNameHash == JumpHash)
    {
        // 判断动画是否播放完成
        if (stateInfo.normalizedTime >= 1.0f)
        {
            Debug.Log("跳跃动画播放完成");
        }
    }
}

// ✅ 推荐：使用状态标记减少调用频率
private bool isJumping = false;

public void Jump()
{
    animator.SetTrigger(JumpTriggerHash);
    isJumping = true;
}

void Update()
{
    if (isJumping)
    {
        var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.normalizedTime >= 1.0f)
        {
            OnJumpComplete();
            isJumping = false;
        }
    }
}
```

---

### IsInTransition
**签名**：
```csharp
public bool IsInTransition(int layerIndex);
```

**描述**：判断指定图层是否正在过渡中

**参数**：
- `layerIndex`：图层索引

**返回值**：`bool`（true表示正在过渡）

**示例**：
```csharp
// 判断是否正在过渡
if (animator.IsInTransition(0))
{
    Debug.Log("动画正在过渡中");
}

// 获取下一个状态信息（仅在过渡期间有效）
if (animator.IsInTransition(0))
{
    var nextState = animator.GetNextAnimatorStateInfo(0);
    Debug.Log($"下一个状态哈希: {nextState.shortNameHash}");
}
```

---

### StringToHash
**签名**：
```csharp
public static int StringToHash(string name);
```

**描述**：将字符串参数名转换为哈希值（静态方法）

**参数**：
- `name`：参数名称或状态名称

**返回值**：`int`（哈希值）

**性能**：✅ 核心优化方法

**示例**：
```csharp
// ✅ 推荐：使用static readonly缓存哈希值
public class PlayerAnimation : MonoBehaviour
{
    // 参数哈希缓存
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int JumpTriggerHash = Animator.StringToHash("Jump");
    
    // 状态哈希缓存
    private static readonly int IdleHash = Animator.StringToHash("Idle");
    private static readonly int WalkHash = Animator.StringToHash("Walk");
    private static readonly int RunHash = Animator.StringToHash("Run");
    
    private Animator animator;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    
    public void UpdateMovement(float speed)
    {
        animator.SetFloat(SpeedHash, speed);
    }
}
```

**性能提升**：
- 字符串版本：每次调用约100-200ns
- 哈希版本：每次调用约10-20ns
- 性能提升：5-10倍

---

### Rebind
**签名**：
```csharp
public void Rebind();
```

**描述**：重新绑定所有动画属性，重置到默认状态

**返回值**：无

**性能**：⚠️ 开销较大

**应用场景**：
- 动态修改骨骼结构后
- 切换Avatar后
- 重置动画状态

**示例**：
```csharp
// 重新绑定动画
animator.Rebind();

// 常见用法：重置角色动画状态
public void ResetAnimation()
{
    animator.Rebind();
    animator.Play(IdleHash);
}
```

---

### Update
**签名**：
```csharp
public void Update(float deltaTime);
```

**描述**：手动更新Animator（不依赖Unity更新循环）

**参数**：
- `deltaTime`：时间增量（秒）

**返回值**：无

**应用场景**：
- 自定义更新频率
- 编辑器模式下预览动画
- 特殊时间控制需求

**示例**：
```csharp
// 手动更新动画（用于自定义时间控制）
void Update()
{
    if (useCustomUpdate)
    {
        animator.Update(customDeltaTime);
    }
}
```

---

## IK相关方法

### SetLookAtPosition
**签名**：
```csharp
public void SetLookAtPosition(Vector3 lookAtPosition);
```

**描述**：设置角色注视目标点（IK功能）

**参数**：
- `lookAtPosition`：注视目标位置

**示例**：
```csharp
// 在OnAnimatorIK回调中设置注视目标
void OnAnimatorIK(int layerIndex)
{
    if (lookAtTarget != null)
    {
        animator.SetLookAtPosition(lookAtTarget.position);
        animator.SetLookAtWeight(1.0f);
    }
}
```

---

### SetLookAtWeight
**签名**：
```csharp
public void SetLookAtWeight(float weight, float bodyWeight = 0f, float headWeight = 1f, float eyesWeight = 0f, float clampWeight = 0.5f);
```

**描述**：设置注视权重

**参数**：
- `weight`：总权重（0-1）
- `bodyWeight`：身体权重
- `headWeight`：头部权重
- `eyesWeight`：眼睛权重
- `clampWeight`：限制权重

---

### SetIKPosition / SetIKRotation
**签名**：
```csharp
public void SetIKPosition(AvatarIKGoal goal, Vector3 goalPosition);
public void SetIKRotation(AvatarIKGoal goal, Quaternion goalRotation);
```

**描述**：设置IK目标位置/旋转

**参数**：
- `goal`：IK目标部位（LeftHand、RightHand、LeftFoot、RightFoot）
- `goalPosition/goalRotation`：目标位置/旋转

**示例**：
```csharp
void OnAnimatorIK(int layerIndex)
{
    // 设置左手IK目标
    animator.SetIKPosition(AvatarIKGoal.LeftHand, targetPosition);
    animator.SetIKRotation(AvatarIKGoal.LeftHand, targetRotation);
    animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1.0f);
    animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1.0f);
}
```

---

## Animation Clip API

### AnimationClip.SetCurve
**签名**：
```csharp
public void SetCurve(string relativePath, Type type, string propertyName, AnimationCurve curve);
```

**描述**：设置动画曲线

**参数**：
- `relativePath`：目标对象路径（相对于根对象）
- `type`：组件类型
- `propertyName`：属性名称
- `curve`：动画曲线

**示例**：
```csharp
// 创建动画片段
AnimationClip clip = new AnimationClip();

// 创建动画曲线
AnimationCurve curve = new AnimationCurve();
curve.AddKey(new Keyframe(0f, 0f));   // 起始位置
curve.AddKey(new Keyframe(1f, 10f));  // 结束位置

// 设置曲线（控制Y轴位置）
clip.SetCurve("", typeof(Transform), "localPosition.y", curve);

// 设置为Legacy模式
clip.legacy = true;
```

---

### AnimationClip.length
**类型**：`float`（只读）
**描述**：动画片段长度（秒）

---

### AnimationClip.frameRate
**类型**：`float`
**描述**：帧率（帧/秒）

---

### AnimationClip.wrapMode
**类型**：`WrapMode`（仅Legacy模式）
**描述**：循环模式

**枚举值**：
- `Once`：播放一次
- `Loop`：循环播放
- `PingPong`：来回播放
- `ClampForever`：播放完保持在最后一帧

---

## AnimatorStateInfo 结构体

### 属性列表

| 属性 | 类型 | 描述 |
|------|------|------|
| `shortNameHash` | int | 状态名称哈希值 |
| `length` | float | 动画长度（秒） |
| `normalizedTime` | float | 归一化时间（0-1） |
| `speed` | float | 动画速度 |
| `speedMultiplier` | float | 速度倍率 |
| `loop` | bool | 是否循环 |
| `tagHash` | int | 标签哈希值 |

### 方法

#### IsName
**签名**：
```csharp
public bool IsName(string name);
```

**描述**：判断状态名称是否匹配

**示例**：
```csharp
var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
if (stateInfo.IsName("Jump"))
{
    Debug.Log("正在播放跳跃动画");
}
```

#### IsTag
**签名**：
```csharp
public bool IsTag(string tag);
```

**描述**：判断状态标签是否匹配

**示例**：
```csharp
var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
if (stateInfo.IsTag("Attack"))
{
    Debug.Log("正在播放攻击动画");
}
```

---

## 性能优化总结

### ✅ 推荐做法
1. **使用StringToHash缓存所有参数名和状态名**
2. **避免在Update中频繁调用GetCurrentAnimatorStateInfo**
3. **使用CullingMode优化屏幕外动画**
4. **控制Active Animator数量≤30个**
5. **动画层数控制在5层以内**

### ⚠️ 性能警告
1. **字符串参数调用**：每次计算哈希，性能开销大
2. **Update中频繁查询状态**：减少不必要的查询
3. **过多动画图层**：每层都有计算开销

### ❌ 禁止操作
1. **运行时频繁切换AnimatorController**
2. **禁用包含Animator的GameObject**（使用Animator.enabled代替）
3. **过度使用Humanoid动画**（比Generic慢15-20%）

---

## 参考资源

- [Unity官方文档 - Animator组件](https://docs.unity3d.com/Manual/class-Animator.html)
- [Unity脚本API - Animator](https://docs.unity3d.com/ScriptReference/Animator.html)
- [Unity性能优化指南 - 动画模块](https://docs.unity3d.com/Manual/MecanimAnimationSystem.html)