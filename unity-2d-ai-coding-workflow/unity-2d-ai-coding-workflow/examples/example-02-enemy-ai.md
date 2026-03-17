# 示例案例2：敌人AI系统

## 项目背景

**游戏类型**：2D横版动作游戏
**开发团队**：2人独立团队（1程序+1美术）
**开发周期**：1天

## 需求描述

实现一个简单的敌人AI系统，包含：
- 巡逻状态
- 追击状态
- 攻击状态
- 状态切换逻辑

## 完整开发流程

### 第一步：架构分析（简化版）

由于是小型项目，使用简化版架构分析：

```
@architecture-design-analyzer
需求: 敌人AI系统，包含巡逻、追击、攻击三种状态
模块: AI系统
```

**AI快速分析**：
- 系统定位：AI系统
- 设计模式：有限状态机（FSM）
- 影响范围：仅限AI模块，风险低
- 预估耗时：1天

**决策**：直接开发，无需详细分析报告

---

### 第二步：生成敌人数据配置

```
@unity-scriptableobject-config
配置名: EnemyData
类型: character
验证: yes
```

**AI生成**：
```csharp
[CreateAssetMenu(fileName = "EnemyData", menuName = "Enemy/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Basic Info")]
    [SerializeField] private int enemyId;
    [SerializeField] private string enemyName;
    
    [Header("Combat")]
    [Range(10, 1000)]
    [SerializeField] private int maxHealth = 100;
    
    [Range(1, 100)]
    [SerializeField] private int attackDamage = 10;
    
    [Header("Movement")]
    [Range(0.1f, 10f)]
    [SerializeField] private float patrolSpeed = 2f;
    
    [Range(0.1f, 10f)]
    [SerializeField] private float chaseSpeed = 4f;
    
    [Header("AI")]
    [Range(0.1f, 5f)]
    [SerializeField] private float attackRange = 1.5f;
    
    [Range(1f, 20f)]
    [SerializeField] private float detectRange = 8f;
    
    // ... 属性访问器和验证逻辑
}
```

---

### 第三步：生成敌人控制器

```
@unity-2d-character-controller
```

**AI渐进式披露**：

```
AI: 请选择移动模式：
1. 平台跳跃
2. 俯视移动
3. 横版格斗
```

**用户选择**：平台跳跃（但这是敌人，需要自定义）

**修正**：直接描述需求

```
用户: 需要敌人AI控制器，不是玩家控制器
```

```
AI: 明白！需要AI控制器。

请确认功能：
- ☑️ 巡逻移动
- ☑️ 追击玩家
- ☑️ 攻击逻辑
- ☐ 跳跃（敌人不需要）

状态机需求？
```

**用户确认**：需要简单的状态机

```
AI: 收到！

生成敌人AI控制器，包含：
- 简单FSM状态机
- 巡逻、追击、攻击三种状态
- 状态切换逻辑

开始生成...
```

**AI生成核心代码**：
```csharp
using UnityEngine;

public enum EnemyState
{
    Patrol,
    Chase,
    Attack
}

public class EnemyController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private EnemyData enemyData;
    
    [Header("Detection")]
    [SerializeField] private Transform player;
    [SerializeField] private LayerMask playerLayer;
    
    private EnemyState currentState;
    private float patrolDirection = 1f;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        currentState = EnemyState.Patrol;
        
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }
    
    private void Update()
    {
        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                break;
            case EnemyState.Chase:
                Chase();
                break;
            case EnemyState.Attack:
                Attack();
                break;
        }
        
        UpdateAnimations();
    }
    
    private void Patrol()
    {
        // 巡逻移动
        rb.velocity = new Vector2(patrolDirection * enemyData.PatrolSpeed, rb.velocity.y);
        
        // 检测玩家
        if (DetectPlayer())
        {
            currentState = EnemyState.Chase;
        }
        
        // 边界检测（简化）
        if (Physics2D.Raycast(transform.position, Vector2.right * patrolDirection, 1f, LayerMask.GetMask("Ground")) == false)
        {
            patrolDirection *= -1f;
        }
    }
    
    private void Chase()
    {
        if (player == null) return;
        
        float distance = Vector2.Distance(transform.position, player.position);
        
        // 在攻击范围内
        if (distance <= enemyData.AttackRange)
        {
            currentState = EnemyState.Attack;
            return;
        }
        
        // 超出检测范围
        if (distance > enemyData.DetectRange)
        {
            currentState = EnemyState.Patrol;
            return;
        }
        
        // 追击移动
        float direction = player.position.x > transform.position.x ? 1f : -1f;
        rb.velocity = new Vector2(direction * enemyData.ChaseSpeed, rb.velocity.y);
    }
    
    private void Attack()
    {
        if (player == null) return;
        
        float distance = Vector2.Distance(transform.position, player.position);
        
        // 超出攻击范围
        if (distance > enemyData.AttackRange)
        {
            currentState = EnemyState.Chase;
            return;
        }
        
        // 攻击逻辑（简化）
        rb.velocity = Vector2.zero;
        animator.SetTrigger("Attack");
    }
    
    private bool DetectPlayer()
    {
        if (player == null) return false;
        
        float distance = Vector2.Distance(transform.position, player.position);
        return distance <= enemyData.DetectRange;
    }
    
    private void UpdateAnimations()
    {
        animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
        animator.SetInteger("State", (int)currentState);
    }
}
```

---

### 第四步：代码审查

```
@unity-code-review
代码: [粘贴代码]
类型: monobehaviour
```

**AI审查结果**：

✅ **通过项**：
- 状态机逻辑清晰
- 组件引用正确
- 空值检查到位

⚠️ **警告项**：
- 边界检测逻辑过于简单
- 攻击冷却时间缺失

❌ **错误项**：
- 缺少攻击伤害逻辑
- 状态切换缺少过渡

**根据审查优化代码**

---

### 第五步：测试验证

- [x] 巡逻状态正常
- [x] 玩家检测正常
- [x] 追击状态正常
- [x] 攻击状态正常
- [x] 状态切换流畅

---

### 第六步：文档沉淀

记录经验：
- 简单FSM实现方法
- 敌人AI行为设计
- 状态切换最佳实践

---

## 效率对比

| 任务 | 传统方式 | AI辅助 | 提升 |
|------|---------|--------|------|
| 架构分析 | 30分钟 | 5分钟 | 83%↑ |
| 数据配置 | 15分钟 | 3分钟 | 80%↑ |
| AI逻辑 | 120分钟 | 30分钟 | 75%↑ |
| 代码审查 | 20分钟 | 5分钟 | 75%↑ |
| **总计** | **3小时** | **45分钟** | **75%↑** |

---

## 经验总结

### 成功要点
1. ✅ 小型项目可简化架构分析
2. ✅ AI渐进式披露确保需求准确
3. ✅ 代码审查发现潜在问题

### 改进方向
1. 添加攻击冷却机制
2. 优化状态切换过渡
3. 增加更多AI行为（如逃跑、巡逻路径）
