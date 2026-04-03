---
name: unity-state-machine
description: Unity状态机系统，包括Animator动画状态机和代码实现的有限状态机FSM。适用于角色AI、游戏流程、UI状态管理。
---

# Unity State Machine 状态机

## 概述

Unity状态机系统，包括Animator动画状态机和代码实现的有限状态机FSM。适用于角色AI、游戏流程、UI状态管理等场景，是实现复杂逻辑控制的核心工具。

## 状态机类型对比

| 类型 | 特点 | 适用场景 | 复杂度 |
|------|------|----------|--------|
| **FSM** | 有限状态机，状态明确 | 角色移动、攻击、跳跃 | ★★☆☆☆ |
| **HSM** | 层次状态机，支持嵌套 | 复杂角色行为 | ★★★☆☆ |
| **Animator** | Unity内置可视化状态机 | 角色动画控制 | ★★☆☆☆ |
| **行为树** | 树形结构，模块化 | 复杂AI决策 | ★★★★☆ |

## API白名单 ⚠️ 强制遵守

### ✅ 推荐使用的API

| API | 用途 | 性能等级 |
|-----|------|---------|
| `Animator.SetFloat()` | 设置Float参数 | ⭐⭐⭐ 高性能 |
| `Animator.SetInteger()` | 设置Int参数 | ⭐⭐⭐ 高性能 |
| `Animator.SetBool()` | 设置Bool参数 | ⭐⭐⭐ 高性能 |
| `Animator.SetTrigger()` | 触发一次性参数 | ⭐⭐⭐ 高性能 |
| `Animator.StringToHash()` | 参数名转Hash（缓存用） | ⭐⭐⭐ 高性能 |
| `Animator.GetCurrentAnimatorStateInfo()` | 获取当前状态信息 | ⭐⭐⭐ 高性能 |
| `StateMachineBehaviour` | 状态机行为脚本 | ⭐⭐ 中等性能 |

### ⚠️ 性能警告API

| API | 性能问题 | 替代方案 |
|-----|----------|----------|
| 每帧使用字符串参数 | 产生GC | 使用StringToHash缓存 |
| 大量独立状态 | 状态机复杂度高 | 使用Blend Tree合并 |
| 频繁SetTrigger | 可能遗漏重置 | 配合ResetTrigger |

---

## 功能边界 ⚠️ 强制说明

### 本Skill涵盖范围

- ✅ Animator状态机配置
- ✅ 状态转换（Transition）
- ✅ 参数（Parameters）管理
- ✅ StateMachineBehaviour回调
- ✅ 代码FSM实现
- ✅ 状态模式设计

### 不在本Skill范围内

- ❌ 行为树系统 → 使用第三方插件
- ❌ 动画剪辑制作 → 见 unity-2d-animation Skill
- ❌ IK系统 → 不涉及

### 性能限制

| 指标 | 建议值 | 说明 |
|------|--------|------|
| 单角色状态数 | < 20 | 避免状态机过于复杂 |
| 过渡条件数 | < 3 | 单个Transition的条件数 |
| 动画层数 | < 3 | 分层动画数量 |

---

## 渐进式学习路径

### 阶段1：基础使用

```csharp
public class AnimationController : MonoBehaviour
{
    private Animator animator;
    
    // 缓存参数Hash
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int JumpHash = Animator.StringToHash("Jump");
    
    void Awake()
    {
        animator = GetComponent<Animator>();
    }
    
    void Update()
    {
        float speed = Input.GetAxis("Vertical");
        animator.SetFloat(SpeedHash, speed);
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            animator.SetTrigger(JumpHash);
        }
    }
}
```

### 阶段2：代码FSM实现

```csharp
public interface IState
{
    void OnEnter(string prevState);
    void OnExit(string nextState);
    void OnUpdate();
}

public class FiniteStateMachine
{
    private Dictionary<string, IState> states;
    private string currentState;
    
    public void ChangeState(string newState)
    {
        states[currentState]?.OnExit(newState);
        currentState = newState;
        states[currentState]?.OnEnter(currentState);
    }
    
    public void Update()
    {
        states[currentState]?.OnUpdate();
    }
}
```

---

## References

- [API参考文档](references/api-reference.md)
