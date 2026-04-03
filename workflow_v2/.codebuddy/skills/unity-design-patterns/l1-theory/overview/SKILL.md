---
name: design-patterns-overview
description: 设计模式总览，系统介绍创建型、结构型、行为型三大类经典设计模式，以及Unity特有模式，帮助理解模式选择原则和应用场景。
---

# 设计模式总览

## 概述

设计模式（Design Patterns）是软件开发中经过验证的、可复用的解决方案。本Skill系统介绍23种经典设计模式和Unity特有模式，帮助开发者在Unity游戏开发中选择合适的模式。

## 设计模式分类

### 三大类别

#### 1. 创建型模式（Creational Patterns）

**核心目标**：处理对象创建机制

| 模式名称 | 一句话描述 | Unity适用性 |
|---------|-----------|------------|
| 单例模式 | 确保一个类只有一个实例 | ⭐⭐⭐⭐⭐ |
| 工厂方法 | 定义创建对象的接口，让子类决定实例化 | ⭐⭐⭐⭐⭐ |
| 抽象工厂 | 创建相关或依赖对象的家族 | ⭐⭐⭐⭐ |
| 对象池模式 | 通过复用减少对象创建开销 | ⭐⭐⭐⭐⭐ |
| 建造者模式 | 分步骤构建复杂对象 | ⭐⭐⭐ |
| 原型模式 | 通过复制现有对象创建新对象 | ⭐⭐⭐⭐ |

**选择原则**：
- 全局唯一访问点 → 单例模式
- 需要复用对象 → 对象池模式
- 根据类型创建对象 → 工厂方法
- 创建复杂对象族 → 抽象工厂

---

#### 2. 结构型模式（Structural Patterns）

**核心目标**：处理类或对象的组合

| 模式名称 | 一句话描述 | Unity适用性 |
|---------|-----------|------------|
| 适配器模式 | 将不兼容的接口转换为期望接口 | ⭐⭐⭐ |
| 装饰器模式 | 动态添加功能到对象 | ⭐⭐⭐⭐ |
| 外观模式 | 为复杂子系统提供统一接口 | ⭐⭐⭐⭐ |
| 组合模式 | 将对象组合成树形结构 | ⭐⭐⭐⭐⭐ |
| 享元模式 | 共享细粒度对象减少内存占用 | ⭐⭐⭐⭐ |
| 代理模式 | 控制对象访问 | ⭐⭐⭐ |

**选择原则**：
- Unity Transform层级 → 天然组合模式
- 需要动态增强功能 → 装饰器模式
- 简化复杂API → 外观模式
- 大量相同对象 → 享元模式

---

#### 3. 行为型模式（Behavioral Patterns）

**核心目标**：处理对象间的通信和职责分配

| 模式名称 | 一句话描述 | Unity适用性 |
|---------|-----------|------------|
| 观察者模式 | 定义对象间一对多依赖关系 | ⭐⭐⭐⭐⭐ |
| 状态模式 | 允许对象在状态改变时改变行为 | ⭐⭐⭐⭐⭐ |
| 命令模式 | 将请求封装为对象 | ⭐⭐⭐⭐⭐ |
| 策略模式 | 定义算法族，使其可互换 | ⭐⭐⭐⭐ |
| 迭代器模式 | 顺序访问集合元素 | ⭐⭐⭐ |
| 中介者模式 | 集中管理对象间通信 | ⭐⭐⭐ |
| 模板方法模式 | 定义算法骨架，子类实现细节 | ⭐⭐⭐ |

**选择原则**：
- 状态切换行为变化 → 状态模式
- 事件通知机制 → 观察者模式
- 操作需要撤销/重做 → 命令模式
- 算法需要运行时切换 → 策略模式

---

#### 4. Unity特有模式

| 模式名称 | 核心思想 | 适用场景 |
|---------|---------|---------|
| ScriptableObject数据容器 | 数据与逻辑分离，Inspector配置 | 角色属性、物品定义、技能配置 |
| ScriptableObject事件通道 | 全局事件发布订阅 | 模块间通信、解耦系统 |
| MonoBehaviour生命周期 | 利用Unity生命周期管理 | 所有游戏对象行为控制 |
| Unity组件模式 | 组合优于继承 | GameObject功能组合 |
| 场景管理模式 | 多场景加载与切换 | 场景管理、关卡切换 |
| Prefab变体模式 | 基础Prefab + 变体覆盖 | 同类对象变体管理 |

---

## 模式选择原则

### 原则1：根据问题域选择

```
问题类型              推荐模式
─────────────────────────────
对象创建问题     →   创建型模式
类/对象组合问题   →   结构型模式
对象通信问题      →   行为型模式
```

### 原则2：根据Unity特性选择

```
Unity特性            推荐模式
─────────────────────────────
Transform层级        →   组合模式
GameObject组件       →   组件模式
MonoBehaviour生命周期 → 状态模式
Inspector配置        →   ScriptableObject模式
```

### 原则3：根据性能需求选择

```
性能需求              推荐模式
─────────────────────────────
减少GC              →   对象池模式
减少内存占用         →   享元模式
优化事件系统         →   观察者模式
```

---

## 设计原则（SOLID）

### S - 单一职责原则（Single Responsibility）

**定义**：一个类应该只有一个引起它变化的原因

**Unity示例**：
```csharp
// ❌ 违反单一职责
public class Player : MonoBehaviour
{
    void Update()
    {
        Move();      // 移动逻辑
        Attack();    // 攻击逻辑
        UpdateUI();  // UI逻辑
    }
}

// ✅ 符合单一职责
public class PlayerMovement : MonoBehaviour
{
    void Update() => Move();
}

public class PlayerCombat : MonoBehaviour
{
    void Update() => Attack();
}

public class PlayerUI : MonoBehaviour
{
    void Update() => UpdateUI();
}
```

---

### O - 开闭原则（Open/Closed）

**定义**：软件实体应该对扩展开放，对修改关闭

**Unity示例**：
```csharp
// ✅ 使用策略模式实现开闭原则
public interface IMovementStrategy
{
    void Move(Transform transform);
}

public class WalkMovement : IMovementStrategy
{
    public void Move(Transform transform) { /* 步行逻辑 */ }
}

public class FlyMovement : IMovementStrategy
{
    public void Move(Transform transform) { /* 飞行逻辑 */ }
}

// 添加新移动方式无需修改现有代码
public class TeleportMovement : IMovementStrategy
{
    public void Move(Transform transform) { /* 瞬移逻辑 */ }
}
```

---

### L - 里氏替换原则（Liskov Substitution）

**定义**：子类对象能够替换父类对象出现在父类能够出现的任何地方

**Unity示例**：
```csharp
// ✅ 所有武器都可以替换使用
public abstract class Weapon : MonoBehaviour
{
    public abstract void Attack();
}

public class Sword : Weapon
{
    public override void Attack() { /* 挥剑 */ }
}

public class Bow : Weapon
{
    public override void Attack() { /* 射箭 */ }
}

// 任何Weapon子类都可以替换使用
public void UseWeapon(Weapon weapon)
{
    weapon.Attack();
}
```

---

### I - 接口隔离原则（Interface Segregation）

**定义**：不应该强迫客户依赖于它们不用的方法

**Unity示例**：
```csharp
// ❌ 违反接口隔离
public interface ICharacter
{
    void Move();
    void Attack();
    void CastSpell();  // 非魔法角色不需要
}

// ✅ 符合接口隔离
public interface IMoveable
{
    void Move();
}

public interface ICombat
{
    void Attack();
}

public interface IMagic
{
    void CastSpell();
}

// 战士类只实现需要的接口
public class Warrior : MonoBehaviour, IMoveable, ICombat
{
    public void Move() { }
    public void Attack() { }
}

// 法师类实现所有接口
public class Mage : MonoBehaviour, IMoveable, ICombat, IMagic
{
    public void Move() { }
    public void Attack() { }
    public void CastSpell() { }
}
```

---

### D - 依赖倒置原则（Dependency Inversion）

**定义**：高层模块不应该依赖低层模块，两者都应该依赖抽象

**Unity示例**：
```csharp
// ✅ 使用依赖注入
public interface IInventoryService
{
    void AddItem(string itemId);
}

public class Player : MonoBehaviour
{
    private IInventoryService inventory;
    
    // 依赖注入
    public void SetInventory(IInventoryService inventory)
    {
        this.inventory = inventory;
    }
    
    public void PickupItem(string itemId)
    {
        inventory.AddItem(itemId);
    }
}
```

---

## 常见反模式警告

### ❌ 反模式1：过度使用单例

```csharp
// ❌ 滥用单例
public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance;
    
    void Awake()
    {
        Instance = this;
    }
}

// ✅ 使用依赖注入或事件系统
public class PlayerHealth : MonoBehaviour
{
    public event Action<float> OnHealthChanged;
    
    public void TakeDamage(float damage)
    {
        // ...
        OnHealthChanged?.Invoke(currentHealth);
    }
}
```

---

### ❌ 反模式2：硬编码依赖

```csharp
// ❌ 硬编码依赖
public class Player : MonoBehaviour
{
    private EnemyAI enemyAI;
    
    void Start()
    {
        enemyAI = FindObjectOfType<EnemyAI>(); // 硬编码查找
    }
}

// ✅ 使用依赖注入
public class Player : MonoBehaviour
{
    [SerializeField] private EnemyAI enemyAI; // Inspector配置
}
```

---

### ❌ 反模式3：上帝类（God Object）

```csharp
// ❌ 上帝类
public class GameManager : MonoBehaviour
{
    void Update()
    {
        UpdatePlayer();
        UpdateEnemies();
        UpdateUI();
        UpdateAudio();
        UpdateParticles();
        // ... 几十个方法
    }
}

// ✅ 职责分离
public class GameManager : MonoBehaviour { }
public class PlayerManager : MonoBehaviour { }
public class EnemyManager : MonoBehaviour { }
public class UIManager : MonoBehaviour { }
```

---

## 学习路径建议

### 初学者（第1-2周）

1. 学习SOLID原则
2. 掌握单例模式、观察者模式
3. 理解Unity组件模式

### 中级开发者（第3-5周）

4. 掌握状态模式、命令模式
5. 学习工厂方法、策略模式
6. 理解对象池模式

### 高级开发者（第6-8周）

7. 学习抽象工厂、装饰器模式
8. 掌握分层状态机、事件总线架构
9. 实践模式组合应用

---

## References

- [单例模式理论](../singleton-theory/SKILL.md)
- [状态模式理论](../state-pattern-theory/SKILL.md)
- [观察者模式理论](../observer-pattern-theory/SKILL.md)
- [Game Programming Patterns](http://www.gameprogrammingpatterns.com/)
