# 棋子系统 (Chess System)

## 概述

棋子是游戏的核心单位，采用组件化设计，每个棋子由多个Controller组成，分别管理不同功能。

## 核心类

### Chess (棋子实体)
**文件**: `Assets/Script/Chess/Chess.cs`

棋子主类，继承 MonoBehaviour。

```csharp
public class Chess : MonoBehaviour
{
    // 核心组件
    public PropertyController propertyController;
    public SkillController skillController;
    public StateController stateController;
    public BuffController buffController;
    public MoveController moveController;
    public AttackController attackController;
    
    // 状态
    public bool IfDeath { get; private set; }
    public string tag;  // "Player" 或 "Enemy"
    
    // 生命周期
    public void InitChess(PropertyCreator creator, string tag) { }
    public void Death() { }
}
```

**关键注意点**:
- `tag` 字段区分队伍: "Player" 或 "Enemy"
- `IfDeath` 为 true 时表示棋子已死亡
- 死亡后不会立即销毁，只是标记状态

### PropertyController (属性控制器)
**文件**: `Assets/Script/Chess/Controller/PropertyController.cs`

管理棋子所有属性。

```csharp
public class PropertyController : MonoBehaviour
{
    public PropertyCreator creator;  // 配置引用
    
    // 基础属性
    public float GetHp() { }
    public float GetMaxHp() { }
    public float GetAttack() { }
    public float GetCrit() { }
    public float GetCritDamage() { }
    public float GetDodge() { }
    public float GetMoveSpeed() { }
    public float GetAttackRange() { }
    public int GetPrice() { }
    public int GetRarity() { }
    
    // 伤害处理
    public void GetDamage(DamageMessege damage) { }
    public void Heal(float amount) { }
}
```

**存档相关**:
- `creator` 用于获取配置ID (chessName)
- 属性通过 `Heal()` 和 `GetDamage()` 精确控制血量
- 其他属性通常通过 Buff 系统修改

### SkillController (技能控制器)
**文件**: `Assets/Script/Chess/Controller/SkillController.cs`

管理技能释放和CD。

```csharp
public class SkillController : MonoBehaviour
{
    public ISkill activeSkill;
    
    public bool IfSkillReady() { }  // 技能是否就绪
    public void SetSkill(ISkill skill) { }
}
```

**存档注意点**:
- `activeSkill.GetSkillConfig()` 可能抛出异常（如MultySkill.currentSkill为null）
- 需要try-catch保护

### BuffController (Buff控制器)
**文件**: `Assets/Script/Chess/Controller/BuffController.cs`

管理Buff的添加和移除。

```csharp
public class BuffController : MonoBehaviour
{
    private List<Buff> buffs;  // 私有字段
    
    public void AddBuff(Buff buff) { }
    public void RemoveBuff(Buff buff) { }
}
```

**存档相关**:
- `buffs` 是私有字段，需要反射访问
- Buff类型通过 `buff.GetType().FullName` 保存
- TimeBuff需要额外保存 `remainingTime`

### StateController (状态控制器)
**文件**: `Assets/Script/Chess/Controller/StateController.cs`

管理状态机。

```csharp
public class StateController : MonoBehaviour
{
    public State currentState;
}
```

## 棋子配置

### PropertyCreator
**文件**: `Assets/Script/Chess/PropertyCreator/PropertyCreator.cs`

ScriptableObject，棋子配置数据。

```csharp
[CreateAssetMenu(fileName = "NewProperty", menuName = "Message/Property")]
public class PropertyCreator : ScriptableObject
{
    public string chessName;        // 棋子唯一ID
    public string chessDescription;
    public Sprite chessSprite;
    public PropertyData baseProperty;  // 基础属性
    public List<string> plantTags;     // 羁绊标签
    
    public bool IfCanPlant(Tile tile) { }  // 检查能否种植
    public bool IfCanBuyCard() { }        // 检查能否购买
}
```

**关键字段**:
- `chessName` - 唯一标识，用于存档查找配置
- `plantTags` - 羁绊标签，如 "结束乐队"
- 注意: PropertyCreator 继承 ScriptableObject，没有 gameObject

## 队伍管理

### ChessTeamManage (单例)
**文件**: `Assets/Script/Manage/ChessTeamManage.cs`

管理所有棋子队伍。

```csharp
public class ChessTeamManage : MonoBehaviour
{
    public static ChessTeamManage Instance;
    
    public Chess CreateChess(PropertyCreator creator, Tile tile, string tag) { }
    public List<Chess> GetTeam(string tag) { }  // "Player" 或 "Enemy"
    public void RemoveFromTeam(Chess chess) { }
}
```

**存档相关**:
- `GetTeam("Player")` - 获取玩家棋子列表
- `GetTeam("Enemy")` - 获取敌人棋子列表
- `CreateChess()` - 读档时重新创建棋子

## 存档数据流

```
存档时:
ChessTeamManage.GetTeam("Player") → 遍历每个Chess
  ├── Chess.propertyController.creator.chessName → chessId
  ├── Chess.transform.position → position
  ├── CollectPropertyData() → property
  ├── CollectBuffData() → buffs (反射获取buffs列表)
  └── CollectSkillData() → skill

读档时:
FindPropertyCreator(chessId) → 查找配置
  ├── GameManage.allChess (玩家棋子配置)
  └── LevelData.zombieList (敌人棋子配置)
  
ChessTeamManage.CreateChess(creator, tile, tag) → 创建新棋子
  ├── ApplyPropertyData() → 恢复血量
  ├── ApplyBuffData() → 重新添加Buff
  └── ApplySkillData() → 恢复技能CD
```

## 关键注意点

### 1. 配置查找

玩家棋子在 `GameManage.allChess` 中查找，敌人棋子在 `LevelData.zombieList` 中查找:

```csharp
// 查找配置
PropertyCreator FindPropertyCreator(string chessId)
{
    // 1. 从 allChess 查找 (玩家棋子)
    foreach (var creator in GameManage.instance.allChess)
        if (creator.chessName == chessId) return creator;
    
    // 2. 从 zombieList 查找 (敌人棋子)
    LevelController controller = LevelManage.instance.currentController;
    foreach (var zombie in controller.levelData.zombieList)
        if (zombie.chessName == chessId) return zombie;
}
```

### 2. 属性恢复

PropertyController 没有直接设置属性的方法，需要通过 Buff 或精确控制血量:

```csharp
// 恢复血量
float currentHp = property.GetHp();
float hpDiff = data.hp - currentHp;
if (hpDiff > 0) property.Heal(hpDiff);
else if (hpDiff < 0) 
{
    DamageMessege damage = new DamageMessege(null, null, -hpDiff, DamageType.Real);
    property.GetDamage(damage);
}
```

### 3. Buff恢复

TimeBuff需要保存剩余时间:

```csharp
// 保存
if (buff is TimeBuff timeBuff)
{
    buffData.remainingTime = GetRemainingTime(timeBuff);
}

// 恢复
if (buff is TimeBuff timeBuff && buffData.remainingTime > 0)
{
    var continueTimeField = buffType.GetField("continueTime");
    continueTimeField.SetValue(timeBuff, buffData.remainingTime);
}
```

## 依赖关系

```
Chess
├── PropertyController
│   ├── PropertyCreator (ScriptableObject配置)
│   └── PropertyData
├── SkillController
│   └── ISkill
├── BuffController
│   └── List<Buff> (私有，需反射访问)
├── StateController
│   └── State
├── MoveController
└── AttackController

ChessTeamManage (单例)
├── CreateChess() → 创建棋子
└── GetTeam() → 获取队伍
```

## 常见问题

| 问题 | 原因 | 解决方案 |
|------|------|---------|
| 找不到棋子配置 | 敌人在zombieList中 | 同时检查allChess和zombieList |
| PropertyCreator没有gameObject | 继承ScriptableObject | 直接访问字段 |
| buffs字段访问失败 | 私有字段 | 使用反射 |
| 血量恢复不对 | GetDamage参数错误 | 使用正数伤害 |
| MultySkill.GetSkillConfig报错 | currentSkill为null | try-catch保护 |
