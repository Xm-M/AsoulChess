---
name: unity-2d-animation
description: Unity 2D Animation动画系统，基于Mecanim。包括Animator组件控制、动画状态机、动画参数系统。支持精灵动画、帧动画、骨骼动画等多种2D动画类型。
---

# Unity 2D Animation Skill

## 功能概述

Unity 2D Animation Skill 提供完整的2D动画系统开发能力，涵盖动画创建、状态机控制、参数管理和性能优化。基于Unity Mecanim动画系统，支持精灵动画、帧动画、骨骼动画等多种2D动画类型。

核心功能包括：
- **Animator组件控制**：播放、暂停、速度调节、状态切换
- **动画状态机**：可视化配置动画状态和过渡条件
- **动画参数系统**：Bool、Float、Int、Trigger四种参数类型
- **动画混合**：多图层动画叠加、Blend Tree混合
- **性能优化**：参数缓存、剔除模式、更新频率控制

---

## API白名单分类

### ✅ 推荐使用（性能优化、最佳实践）

#### 参数缓存优化
```csharp
// 使用StringToHash缓存参数名
private static readonly int SpeedHash = Animator.StringToHash("Speed");
private static readonly int IsJumpingHash = Animator.StringToHash("IsJumping");

// 使用缓存的哈希值
animator.SetFloat(SpeedHash, currentSpeed);
animator.SetBool(IsJumpingHash, true);
```

#### 状态切换
- `Play(int stateNameHash)` - 直接播放指定状态
- `CrossFade(int stateHash, float transitionDuration)` - 平滑过渡到目标状态
- `SetTrigger(int hash)` - 触发器参数（用于一次性事件）
- `SetBool/SetFloat/SetInteger(int hash, value)` - 设置参数值

#### 状态查询（仅在必要时调用）
- `GetCurrentAnimatorStateInfo(int layerIndex)` - 获取当前状态信息
- `IsInTransition(int layerIndex)` - 判断是否正在过渡

#### 动画控制
- `speed` - 控制动画播放速度
- `updateMode` - 设置更新模式（Normal/AnimatePhysics/UnscaledTime）
- `cullingMode` - 剔除模式（优化屏幕外动画）

---

### ⚠️ 性能警告（谨慎使用）

#### 字符串参数（避免频繁调用）
```csharp
// ⚠️ 性能警告：每次调用都会进行字符串哈希计算
animator.Play("Walk"); // 不推荐
animator.SetFloat("Speed", 1.0f); // 不推荐

// ✅ 推荐：使用哈希缓存
animator.Play(WalkHash); // 推荐
animator.SetFloat(SpeedHash, 1.0f); // 推荐
```

#### Update中频繁查询状态
```csharp
// ⚠️ 性能警告：避免在Update中频繁调用
void Update()
{
    // 每帧调用，性能开销大
    var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
    if (stateInfo.IsName("Jump"))
    {
        // 处理逻辑
    }
}

// ✅ 推荐：使用事件或状态标记
private bool isJumping = false;
void Update()
{
    if (isJumping && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
    {
        OnJumpComplete();
        isJumping = false;
    }
}
```

#### Layer数量过多
- 建议控制在5层以内，过多图层会显著增加计算开销

---

### ❌ 禁止使用（性能杀手、已知问题）

#### 运行时频繁切换AnimatorController
```csharp
// ❌ 禁止：运行时切换Controller会触发Rebinding，开销极大
animator.runtimeAnimatorController = newController; // 严重性能问题
```

#### 禁用包含Animator的GameObject
```csharp
// ❌ 禁止：禁用GameObject会触发Rebinding
gameObject.SetActive(false); // 导致重新绑定，性能开销大

// ✅ 推荐：禁用Animator组件或使用CullingMode
animator.enabled = false; // 或
animator.cullingMode = AnimatorCullingMode.CullCompletely;
```

#### 过度使用Humanoid动画
- Humanoid比Generic动画CPU开销高15-20%
- 仅在需要动画重定向时使用Humanoid

---

## 使用规范

### 性能优化规范

1. **参数缓存**：所有动画参数必须使用StringToHash缓存
2. **状态查询**：避免在Update中频繁调用GetCurrentAnimatorStateInfo
3. **Animator数量**：单个场景Active的Animator不超过30个
4. **Layer控制**：合理使用CullingMode优化屏幕外动画
5. **更新模式**：物理相关动画使用AnimatePhysics模式

### 代码规范

```csharp
// 标准模式：参数缓存 + 状态管理
public class PlayerAnimation : MonoBehaviour
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
    
    public void UpdateMovement(float speed)
    {
        animator.SetFloat(SpeedHash, speed);
    }
    
    public void Jump()
    {
        animator.SetTrigger(JumpTriggerHash);
    }
}
```

---

## 功能边界

### 适用场景
- ✅ 2D精灵动画（角色、道具、UI动效）
- ✅ 帧动画播放（序列帧动画）
- ✅ 骨骼动画（使用2D Animation包）
- ✅ 简单的动画状态机控制
- ✅ 动画参数驱动的状态切换

### 不适用场景
- ❌ 复杂的3D角色动画（建议使用专门的3D动画系统）
- ❌ 程序化动画（建议使用代码直接控制Transform）
- ❌ 物理驱动的动画（建议使用物理系统）
- ❌ 大规模群体动画（建议使用GPU Instancing或ECS）

### 性能限制
- 单场景Active Animator数量：建议≤30
- 动画层数：建议≤5层
- 单个动画Clip关键帧：建议≤1000帧
- 参数数量：建议≤20个

---

## 渐进式学习路径

### 第1阶段：基础动画播放（1-2天）
- 创建Animation Clip
- 添加Animator组件
- 使用Play()播放动画
- 控制播放速度

### 第2阶段：状态机入门（2-3天）
- 创建Animator Controller
- 配置动画状态（State）
- 设置状态过渡（Transition）
- 使用参数控制状态切换

### 第3阶段：参数系统（2-3天）
- 理解四种参数类型（Bool/Float/Int/Trigger）
- 编写参数控制代码
- 使用StringToHash优化性能
- 实现复杂的动画逻辑

### 第4阶段：高级功能（3-5天）
- 动画图层（Layer）和遮罩（Avatar Mask）
- Blend Tree混合动画
- 动画事件（Animation Event）
- 逆向动力学（IK）

### 第5阶段：性能优化（2-3天）
- 分析动画性能瓶颈
- 优化参数调用频率
- 配置Culling Mode
- 使用Animator.OverrideController减少内存占用

---

## 常见问题与解决方案

### 问题1：动画在屏幕外停止播放
**原因**：默认Culling Mode为CullUpdateTransform
**解决**：设置为AlwaysAnimate或根据需求调整

### 问题2：动画切换不流畅
**原因**：过渡时间设置过短或未使用CrossFade
**解决**：增加过渡持续时间，使用CrossFade平滑过渡

### 问题3：性能开销过大
**原因**：Active Animator过多、频繁字符串调用
**解决**：
1. 减少Active Animator数量
2. 使用StringToHash缓存
3. 配置Culling Mode

### 问题4：动画状态查询不准确
**原因**：在过渡期间调用GetCurrentAnimatorStateInfo
**解决**：使用IsInTransition判断是否正在过渡

### 问题5：动画参数设置无效
**原因**：参数名称拼写错误或类型不匹配
**解决**：检查参数名称和类型是否与Animator Controller中一致

---

## 技术要点总结

1. **性能第一原则**：所有参数调用使用StringToHash缓存
2. **状态查询优化**：避免Update中频繁查询，使用事件或标记
3. **合理使用图层**：控制Layer数量，使用Mask限制动画影响范围
4. **剔除模式优化**：根据场景需求配置Culling Mode
5. **动画类型选择**：Generic优于Humanoid（除动画重定向场景）

通过遵循以上规范，可以构建高性能、可维护的2D动画系统。