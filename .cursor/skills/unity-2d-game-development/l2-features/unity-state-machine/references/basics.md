# State Machine 基础概念

## 什么是状态机？

状态机（State Machine）是一种行为模型，由有限数量的状态、状态之间的转换条件以及触发转换的事件组成。在游戏开发中，状态机广泛用于管理对象行为的切换逻辑。

---

## 状态机类型

### 1. 有限状态机（FSM）

最基础的状态机类型，每个时刻只能处于一个状态。

**核心要素**：
- 状态（State）：对象的某种行为模式
- 转换（Transition）：状态之间的切换条件
- 事件（Event）：触发状态转换的条件
- 动作（Action）：状态内部执行的行为

### 2. 层次状态机（HSM）

支持状态嵌套的状态机，子状态继承父状态的转换。

**优点**：
- 状态组织更清晰
- 共享转换逻辑
- 复杂行为分层管理

### 3. Animator状态机

Unity内置的动画状态机，基于Mecanim系统。

**特点**：
- 可视化编辑
- 与动画系统深度集成
- 支持动画混合和过渡

---

## Unity中的状态机应用

### 1. 动画控制（Animator）

```
使用场景：
- 角色动画切换（Idle → Run → Jump）
- 动画混合和过渡
- 动画参数控制

实现方式：
- Animator Controller
- 动画参数（Float/Int/Bool/Trigger）
- 状态转换条件
```

### 2. AI行为控制

```
使用场景：
- 敌人AI（Patrol → Chase → Attack）
- NPC行为（Idle → Talk → Walk）
- 宠物AI（Follow → Wait → Play）

实现方式：
- 代码实现的FSM
- Animator状态机
- 第三方框架（如NodeCanvas）
```

### 3. 游戏流程控制

```
使用场景：
- 游戏状态（Menu → Playing → Paused → GameOver）
- 关卡状态（Loading → Ready → Running → Complete）
- UI状态（Hidden → Showing → Visible → Hiding）

实现方式：
- GameManager + 枚举状态
- ScriptableObject状态
- 状态模式设计
```

---

## Animator状态机基础

### 创建Animator Controller

```
1. 右键 Project窗口 → Create → Animator Controller
2. 双击打开Animator窗口
3. 添加状态和转换
4. 设置参数控制
```

### 状态类型

| 状态类型 | 用途 | 特点 |
|---------|------|------|
| Entry | 初始状态 | 自动进入 |
| Exit | 结束状态 | 退出状态机 |
| Any State | 任意状态 | 可从任何状态转换 |
| Layer | 状态层 | 支持多层动画 |

### 参数类型

```
Float  - 浮点数（速度、方向）
Int    - 整数（状态ID）
Bool   - 布尔值（是否跳跃）
Trigger- 触发器（一次性触发）
```

### 转换条件

```
转换条件示例：
- Speed > 0.1 （速度大于0.1时转换）
- IsJumping == true （跳跃状态）
- AttackTrigger （触发攻击）

转换设置：
- Has Exit Time：是否等待动画结束
- Exit Time：退出时间点（0-1）
- Transition Duration：过渡时间
- Interruption Source：中断来源
```

---

## 代码实现的状态机

### 状态模式基础

```csharp
// 状态接口
public interface IState
{
    void Enter();   // 进入状态
    void Execute(); // 执行状态逻辑
    void Exit();    // 退出状态
}

// 状态机管理器
public class StateMachine
{
    private IState currentState;
    
    public void ChangeState(IState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }
    
    public void Update()
    {
        currentState?.Execute();
    }
}
```

### 枚举状态机

```csharp
public enum EnemyState
{
    Idle,
    Patrol,
    Chase,
    Attack,
    Dead
}

public class Enemy : MonoBehaviour
{
    private EnemyState currentState;
    
    void Update()
    {
        switch (currentState)
        {
            case EnemyState.Idle:
                UpdateIdle();
                break;
            case EnemyState.Patrol:
                UpdatePatrol();
                break;
            // ...
        }
    }
}
```

---

## 状态机 vs 行为树

| 特性 | 状态机 | 行为树 |
|------|--------|--------|
| 复杂度 | 简单-中等 | 复杂 |
| 可视化 | Animator窗口 | 行为树编辑器 |
| 适用场景 | 角色动画、简单AI | 复杂AI行为 |
| 性能 | 较优 | 较高开销 |
| 学习曲线 | 平缓 | 较陡 |

**选择建议**：
- 简单状态切换 → 状态机
- 复杂AI决策 → 行为树
- 动画控制 → Animator状态机
- 游戏流程 → 简单状态机

---

## 状态机设计模式

### 1. 单例状态机

```csharp
public class GameStateMachine : MonoBehaviour
{
    public static GameStateMachine Instance { get; private set; }
    
    private void Awake()
    {
        Instance = this;
    }
}
```

### 2. 泛型状态机

```csharp
public class StateMachine<T> where T : System.Enum
{
    private T currentState;
    private Dictionary<T, System.Action> stateActions;
    
    public void ChangeState(T newState)
    {
        currentState = newState;
        stateActions[currentState]?.Invoke();
    }
}
```

### 3. 事件驱动状态机

```csharp
public class EventDrivenStateMachine
{
    public event Action<IState> OnStateChange;
    
    public void ChangeState(IState newState)
    {
        OnStateChange?.Invoke(newState);
    }
}
```

---

## Animator性能基础

### 状态数量建议

```
单层状态数量：建议 ≤ 20个
超过20个状态：考虑使用子状态机或分层
```

### 参数优化

```
使用Hash代替字符串：
❌ animator.SetBool("IsJumping", true);
✅ animator.SetBool(Animator.StringToHash("IsJumping"), true);
✅ private static readonly int IsJumpingHash = Animator.StringToHash("IsJumping");
```

### 转换优化

```
减少转换数量：
- 合并相似状态
- 使用Blend Tree
- 避免过度细分
```

---

## 常见状态机设计

### 角色控制器状态

```
PlayerController States:
├── Idle        - 待机
├── Move        - 移动
│   ├── Walk    - 行走
│   └── Run     - 奔跑
├── Jump        - 跳跃
├── Attack      - 攻击
│   ├── MeleeAttack - 近战
│   └── RangedAttack- 远程
├── Hurt        - 受伤
└── Die         - 死亡
```

### 敌人AI状态

```
Enemy AI States:
├── Idle        - 待机
├── Patrol      - 巡逻
├── Chase       - 追击
├── Attack      - 攻击
│   ├── MeleeAttack - 近战攻击
│   └── RangedAttack- 远程攻击
├── Flee        - 逃跑
└── Die         - 死亡
```

### 游戏流程状态

```
Game Flow States:
├── MainMenu    - 主菜单
├── Loading     - 加载中
├── Playing     - 游戏中
├── Paused      - 暂停
└── GameOver    - 游戏结束
```

---

## 常见问题

### Q: 状态转换卡顿？

```
原因：转换条件复杂、动画未预热
解决：
- 简化转换条件
- 使用Animator.CrossFade
- 预热动画状态
```

### Q: 状态不同步？

```
原因：网络延迟、帧率不稳定
解决：
- 使用状态同步机制
- 预测状态转换
- 增加容错判断
```

### Q: 状态机过于复杂？

```
原因：状态过多、转换混乱
解决：
- 分层状态机
- 子状态机
- 迁移到行为树
```

---

## 扩展学习

### 官方文档
- [Unity Animator Controller](https://docs.unity3d.com/Manual/AnimatorControllers.html)
- [State Machine Basics](https://docs.unity3d.com/Manual/StateMachineBasics.html)

### 进阶主题
- 子状态机（Sub-State Machine）
- Blend Tree混合树
- 动画层（Animation Layers）
- 状态机行为脚本（State Machine Behaviour）
