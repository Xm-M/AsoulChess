---
name: state-machine-pattern
description: 状态模式与有限状态机FSM实现，适用于角色状态管理、敌人AI行为、游戏流程控制等场景，提供完整的状态切换、状态历史、分层状态机支持。
---

# 状态模式与有限状态机

## 概述

状态模式（State Pattern）允许对象在其内部状态改变时改变其行为。在Unity游戏开发中，状态机（Finite State Machine, FSM）是状态模式最常见的应用形式，广泛用于角色状态管理、敌人AI、游戏流程控制等场景。

## 理论基础

### 状态模式核心思想

```
对象行为 = f(当前状态)
状态改变 → 行为改变
```

### 状态机三要素

1. **状态（State）**：对象在特定时刻的行为模式
2. **转换（Transition）**：状态之间的切换条件
3. **事件（Event）**：触发状态转换的外部或内部条件

### 状态机类型

| 类型 | 特点 | 适用场景 |
|-----|------|---------|
| 有限状态机（FSM） | 状态数量有限 | 角色状态、简单AI |
| 分层状态机（HFSM） | 状态可嵌套 | 复杂AI、游戏流程 |
| 下推自动机（PDA） | 支持状态历史 | 菜单系统、撤销系统 |

---

## 完整实现代码

### 基础状态接口

```csharp
using UnityEngine;

/// <summary>
/// 状态接口，定义状态的生命周期方法
/// </summary>
public interface IState
{
    /// <summary>
    /// 进入状态时调用
    /// </summary>
    void Enter();
    
    /// <summary>
    /// 每帧更新时调用
    /// </summary>
    void Execute();
    
    /// <summary>
    /// 退出状态时调用
    /// </summary>
    void Exit();
    
    /// <summary>
    /// 处理输入事件
    /// </summary>
    void HandleInput();
}
```

---

### 状态机核心实现

```csharp
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 有限状态机FSM实现
/// </summary>
public class StateMachine
{
    // 当前状态
    private IState currentState;
    
    // 状态字典（状态名 -> 状态对象）
    private Dictionary<string, IState> states = new Dictionary<string, IState>();
    
    // 状态历史（用于回退）
    private Stack<string> stateHistory = new Stack<string>();
    
    /// <summary>
    /// 添加状态
    /// </summary>
    public void AddState(string name, IState state)
    {
        if (!states.ContainsKey(name))
        {
            states[name] = state;
        }
    }
    
    /// <summary>
    /// 切换状态
    /// </summary>
    public void ChangeState(string stateName)
    {
        if (!states.ContainsKey(stateName))
        {
            Debug.LogError($"State {stateName} not found!");
            return;
        }
        
        // 记录历史
        if (currentState != null)
        {
            stateHistory.Push(GetCurrentStateName());
        }
        
        // 状态切换
        currentState?.Exit();
        currentState = states[stateName];
        currentState.Enter();
    }
    
    /// <summary>
    /// 回退到上一个状态
    /// </summary>
    public void GoToPreviousState()
    {
        if (stateHistory.Count > 0)
        {
            string previousState = stateHistory.Pop();
            ChangeState(previousState);
        }
    }
    
    /// <summary>
    /// 每帧更新
    /// </summary>
    public void Update()
    {
        currentState?.Execute();
    }
    
    /// <summary>
    /// 处理输入
    /// </summary>
    public void HandleInput()
    {
        currentState?.HandleInput();
    }
    
    /// <summary>
    /// 获取当前状态名
    /// </summary>
    public string GetCurrentStateName()
    {
        foreach (var pair in states)
        {
            if (pair.Value == currentState)
                return pair.Key;
        }
        return null;
    }
    
    /// <summary>
    /// 检查是否在指定状态
    /// </summary>
    public bool IsInState(string stateName)
    {
        return GetCurrentStateName() == stateName;
    }
}
```

---

### 角色状态机示例

```csharp
using UnityEngine;

/// <summary>
/// 玩家角色状态机示例
/// </summary>
public class PlayerStateMachine : MonoBehaviour
{
    private StateMachine stateMachine;
    
    // 组件引用
    public Animator animator;
    public Rigidbody2D rb;
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public bool isGrounded;
    
    // 状态类定义
    private class IdleState : IState
    {
        private PlayerStateMachine player;
        
        public IdleState(PlayerStateMachine player)
        {
            this.player = player;
        }
        
        public void Enter()
        {
            player.animator.SetBool("IsMoving", false);
        }
        
        public void Execute()
        {
            // 空闲状态逻辑
        }
        
        public void Exit()
        {
            // 退出空闲状态
        }
        
        public void HandleInput()
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            
            // 检测移动输入
            if (h != 0 || v != 0)
            {
                player.stateMachine.ChangeState("Move");
            }
            
            // 检测跳跃输入
            if (Input.GetKeyDown(KeyCode.Space) && player.isGrounded)
            {
                player.stateMachine.ChangeState("Jump");
            }
        }
    }
    
    private class MoveState : IState
    {
        private PlayerStateMachine player;
        
        public MoveState(PlayerStateMachine player)
        {
            this.player = player;
        }
        
        public void Enter()
        {
            player.animator.SetBool("IsMoving", true);
        }
        
        public void Execute()
        {
            // 移动逻辑
            float h = Input.GetAxis("Horizontal");
            player.rb.velocity = new Vector2(h * player.moveSpeed, player.rb.velocity.y);
            
            // 翻转角色朝向
            if (h > 0)
                player.transform.localScale = Vector3.one;
            else if (h < 0)
                player.transform.localScale = new Vector3(-1, 1, 1);
        }
        
        public void Exit()
        {
            player.animator.SetBool("IsMoving", false);
        }
        
        public void HandleInput()
        {
            float h = Input.GetAxis("Horizontal");
            
            // 检测停止移动
            if (h == 0)
            {
                player.stateMachine.ChangeState("Idle");
            }
            
            // 检测跳跃
            if (Input.GetKeyDown(KeyCode.Space) && player.isGrounded)
            {
                player.stateMachine.ChangeState("Jump");
            }
        }
    }
    
    private class JumpState : IState
    {
        private PlayerStateMachine player;
        private float jumpTime;
        
        public JumpState(PlayerStateMachine player)
        {
            this.player = player;
        }
        
        public void Enter()
        {
            player.animator.SetTrigger("Jump");
            player.rb.velocity = new Vector2(player.rb.velocity.x, player.jumpForce);
            jumpTime = 0f;
        }
        
        public void Execute()
        {
            jumpTime += Time.deltaTime;
            
            // 检测落地
            if (player.isGrounded && jumpTime > 0.1f)
            {
                player.stateMachine.ChangeState("Idle");
            }
        }
        
        public void Exit()
        {
            // 退出跳跃状态
        }
        
        public void HandleInput()
        {
            // 空中可以控制水平移动
            float h = Input.GetAxis("Horizontal");
            player.rb.velocity = new Vector2(h * player.moveSpeed, player.rb.velocity.y);
        }
    }
    
    void Start()
    {
        stateMachine = new StateMachine();
        
        // 添加状态
        stateMachine.AddState("Idle", new IdleState(this));
        stateMachine.AddState("Move", new MoveState(this));
        stateMachine.AddState("Jump", new JumpState(this));
        
        // 设置初始状态
        stateMachine.ChangeState("Idle");
    }
    
    void Update()
    {
        stateMachine.HandleInput();
        stateMachine.Update();
    }
}
```

---

### 敌人AI状态机示例

```csharp
using UnityEngine;

/// <summary>
/// 敌人AI状态机
/// </summary>
public class EnemyAI : MonoBehaviour
{
    private StateMachine stateMachine;
    
    public Transform player;
    public float detectionRange = 10f;
    public float attackRange = 2f;
    public float moveSpeed = 3f;
    
    private float health = 100f;
    
    // 巡逻状态
    private class PatrolState : IState
    {
        private EnemyAI enemy;
        private Vector3[] patrolPoints;
        private int currentPointIndex;
        
        public PatrolState(EnemyAI enemy, Vector3[] points)
        {
            this.enemy = enemy;
            this.patrolPoints = points;
        }
        
        public void Enter()
        {
            Debug.Log("开始巡逻");
        }
        
        public void Execute()
        {
            // 巡逻移动
            Vector3 target = patrolPoints[currentPointIndex];
            enemy.transform.position = Vector3.MoveTowards(
                enemy.transform.position,
                target,
                enemy.moveSpeed * Time.deltaTime
            );
            
            // 到达巡逻点，切换下一个
            if (Vector3.Distance(enemy.transform.position, target) < 0.5f)
            {
                currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
            }
        }
        
        public void Exit() { }
        
        public void HandleInput()
        {
            // 检测玩家
            float distance = Vector3.Distance(enemy.transform.position, enemy.player.position);
            
            if (distance < enemy.detectionRange)
            {
                enemy.stateMachine.ChangeState("Chase");
            }
        }
    }
    
    // 追击状态
    private class ChaseState : IState
    {
        private EnemyAI enemy;
        
        public ChaseState(EnemyAI enemy)
        {
            this.enemy = enemy;
        }
        
        public void Enter()
        {
            Debug.Log("开始追击");
        }
        
        public void Execute()
        {
            // 追击玩家
            enemy.transform.position = Vector3.MoveTowards(
                enemy.transform.position,
                enemy.player.position,
                enemy.moveSpeed * Time.deltaTime
            );
        }
        
        public void Exit() { }
        
        public void HandleInput()
        {
            float distance = Vector3.Distance(enemy.transform.position, enemy.player.position);
            
            // 进入攻击范围
            if (distance < enemy.attackRange)
            {
                enemy.stateMachine.ChangeState("Attack");
            }
            // 失去目标
            else if (distance > enemy.detectionRange * 1.5f)
            {
                enemy.stateMachine.ChangeState("Patrol");
            }
        }
    }
    
    // 攻击状态
    private class AttackState : IState
    {
        private EnemyAI enemy;
        private float attackCooldown;
        
        public AttackState(EnemyAI enemy)
        {
            this.enemy = enemy;
        }
        
        public void Enter()
        {
            Debug.Log("开始攻击");
            attackCooldown = 1f;
        }
        
        public void Execute()
        {
            attackCooldown -= Time.deltaTime;
            
            if (attackCooldown <= 0)
            {
                // 执行攻击
                Debug.Log("攻击！");
                attackCooldown = 1f;
            }
        }
        
        public void Exit() { }
        
        public void HandleInput()
        {
            float distance = Vector3.Distance(enemy.transform.position, enemy.player.position);
            
            // 玩家离开攻击范围
            if (distance > enemy.attackRange)
            {
                enemy.stateMachine.ChangeState("Chase");
            }
        }
    }
    
    void Start()
    {
        stateMachine = new StateMachine();
        
        Vector3[] patrolPoints = new Vector3[]
        {
            transform.position + Vector3.left * 5f,
            transform.position + Vector3.right * 5f
        };
        
        // 添加状态
        stateMachine.AddState("Patrol", new PatrolState(this, patrolPoints));
        stateMachine.AddState("Chase", new ChaseState(this));
        stateMachine.AddState("Attack", new AttackState(this));
        
        // 设置初始状态
        stateMachine.ChangeState("Patrol");
    }
    
    void Update()
    {
        stateMachine.HandleInput();
        stateMachine.Update();
    }
}
```

---

## 最佳实践

### 1. 状态类命名规范

```csharp
// ✅ 推荐：XxxState
public class IdleState : IState { }
public class PatrolState : IState { }
public class AttackState : IState { }

// ❌ 不推荐：无意义命名
public class State1 : IState { }
public class MyState : IState { }
```

---

### 2. 状态数据共享

```csharp
// ✅ 通过构造函数传递上下文
public class MoveState : IState
{
    private PlayerStateMachine context;
    
    public MoveState(PlayerStateMachine context)
    {
        this.context = context;
    }
}

// ❌ 使用静态变量或单例
public class MoveState : IState
{
    public void Execute()
    {
        Player.Instance.Move(); // 强耦合
    }
}
```

---

### 3. 状态转换验证

```csharp
public class StateMachine
{
    // 状态转换规则
    private Dictionary<string, List<string>> transitionRules;
    
    public bool CanTransition(string fromState, string toState)
    {
        if (!transitionRules.ContainsKey(fromState))
            return false;
        
        return transitionRules[fromState].Contains(toState);
    }
    
    public void ChangeState(string stateName)
    {
        if (!CanTransition(GetCurrentStateName(), stateName))
        {
            Debug.LogWarning($"无法从 {GetCurrentStateName()} 切换到 {stateName}");
            return;
        }
        
        // 正常切换...
    }
}
```

---

### 4. 状态进入/退出事件

```csharp
public class StateMachine
{
    public event Action<string> OnStateEnter;
    public event Action<string> OnStateExit;
    
    public void ChangeState(string stateName)
    {
        currentState?.Exit();
        OnStateExit?.Invoke(GetCurrentStateName());
        
        currentState = states[stateName];
        currentState.Enter();
        OnStateEnter?.Invoke(stateName);
    }
}
```

---

## 常见问题

### Q1: 状态机与Animator如何配合？

```csharp
public class MoveState : IState
{
    public void Enter()
    {
        // 触发Animator状态切换
        player.animator.SetBool("IsMoving", true);
    }
    
    public void Execute()
    {
        // 游戏逻辑
    }
}
```

### Q2: 如何实现状态暂停？

```csharp
public class StateMachine
{
    private bool isPaused;
    
    public void Pause()
    {
        isPaused = true;
    }
    
    public void Resume()
    {
        isPaused = false;
    }
    
    public void Update()
    {
        if (!isPaused)
        {
            currentState?.Execute();
        }
    }
}
```

### Q3: 如何处理状态切换动画？

```csharp
public class JumpState : IState
{
    public void Enter()
    {
        // 立即播放跳跃动画
        player.animator.SetTrigger("Jump");
    }
    
    public void Exit()
    {
        // 重置触发器
        player.animator.ResetTrigger("Jump");
    }
}
```

---

## References

- [状态模式理论](../../l1-theory/state-pattern-theory/SKILL.md)
- [观察者模式](../event-system/SKILL.md)
- [命令模式](../command-pattern/SKILL.md)
