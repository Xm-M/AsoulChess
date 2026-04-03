# 存档系统 (Save System)

## 概述

全局单例管理器，负责游戏存档/读档的核心功能。支持完整游戏状态保存，包括棋子、关卡、UI状态等。

## 核心组件

### 1. SaveManager (单例)
**文件**: `Assets/Script/Save/SaveManager.cs`

主要职责:
- 单例生命周期管理
- 存档文件读写
- 快捷键监听 (F5存档, F9读档)
- 事件通知 (SaveComplete, LoadComplete)

```csharp
// 关键配置
public string saveFileName = "savegame.sav";
public bool enableSaveHotkeys = true;

// 使用方法
SaveManager.Instance.SaveGame();  // 手动存档
SaveManager.Instance.LoadGame();  // 手动读档
bool hasSave = SaveManager.Instance.HasSaveFile;
```

### 2. SaveDataCollector (静态类)
**文件**: `Assets/Script/Save/SaveDataCollector.cs`

从游戏场景收集数据，转换为可序列化格式。

收集的数据:
- **关卡数据**: 波数、时间、min/max时间
- **棋子数据**: 玩家/敌人棋子的属性、位置、Buff、技能CD
- **UI数据**: 选卡栏状态、卡片CD、小卡(Item_PlantCard)状态
- **资源数据**: 阳光值

```csharp
// 使用
GameSaveData data = SaveDataCollector.Collect();
```

### 3. SaveDataApplier (静态类)
**文件**: `Assets/Script/Save/SaveDataApplier.cs`

将存档数据应用到游戏场景。

恢复流程:
1. 暂停游戏
2. 恢复关卡状态 (currentWave, t, mintime, maxtime)
3. 清理现有棋子和小卡
4. 重新创建棋子并恢复属性/Buff
5. 恢复UI状态 (阳光、卡片CD)
6. 恢复游戏运行状态

```csharp
// 使用
SaveDataApplier.Apply(saveData);
```

### 4. 数据结构和类
**文件**: `Assets/Script/Save/SaveData.cs`

#### GameSaveData (根数据结构)
```csharp
public class GameSaveData
{
    public string version;              // 存档版本
    public long saveTime;               // 时间戳
    public LevelSaveData levelData;     // 关卡状态
    public List<ChessSaveData> playerChesses;  // 玩家棋子
    public List<ChessSaveData> enemyChesses;   // 敌人棋子
    public TimerSaveData timerData;     // 计时器
    public ShopSaveData shopData;       // 选卡栏状态
}
```

#### 子数据结构
- **LevelSaveData**: levelName, sceneName, isGameRunning, currentWave, gameTime, levelControllerTime, minTime, maxTime
- **ChessSaveData**: chessId, chessName, position, rotation, teamTag, property, buffs, skill, currentState
- **ShopSaveData**: selectedCardIds, shopIconData, plantCards, isShopActive, sunLight
- **PropertySaveData**: hp, maxHp, attack, attackRate, crit, critDamage, dodgeRate, extraDamage, ar, extraDefence, lifeStealing, healRate, speed, acceleRate, attackRange, price, rarity, size
- **BuffSaveData**: buffType, buffName, remainingTime, customData
- **SkillSaveData**: currentCooldown, totalCooldown, canUse
- **ShopIconSaveData**: cardId, remainingCD, totalCD (主卡CD)
- **PlantCardSaveData**: cardId, position, isUsed (小卡状态)

## 关键技术实现

### 1. 反射访问私有字段

用于获取/设置Unity组件的私有字段:

```csharp
// 获取私有字段
private static T GetFieldValue<T>(object obj, string fieldName)
{
    var field = obj.GetType().GetField(fieldName,
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    return (T)field?.GetValue(obj);
}

// 设置私有字段
private static void SetFieldValue(object obj, string fieldName, object value)
{
    var field = obj.GetType().GetField(fieldName,
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    field?.SetValue(obj, value);
}
```

使用场景:
- ShopIcon.t (CD计时)
- LevelController.currentWave (波数)
- LevelController.t (关卡时间)
- Item_PlantCard.plantOver (小卡使用状态)

### 2. 敌人配置查找

敌人配置不在 `GameManage.allChess` 中，需要从 `LevelData.zombieList` 查找:

```csharp
// 从 allChess 查找 (玩家棋子)
foreach (var creator in GameManage.instance.allChess) { ... }

// 从 zombieList 查找 (敌人棋子)
LevelController controller = LevelManage.instance.currentController;
foreach (var zombieCreator in controller.levelData.zombieList) { ... }
```

### 3. 小卡(Item_PlantCard)处理

**问题**: 小卡使用后被回收到对象池(隐藏而非销毁)，读档时会重复生成。

**解决**:
```csharp
// 读档前清理所有小卡
private static void ClearAllPlantCards()
{
    // 1. 销毁所有活跃小卡
    Item_PlantCard[] plantCards = itemPanel.GetComponentsInChildren<Item_PlantCard>(true);
    foreach (var card in plantCards) Destroy(card.gameObject);
    
    // 2. 清理对象池缓存
    TryClearPlantCardPool(itemPanel);
}
```

### 4. 羁绊事件处理

**问题**: 读档后重新创建棋子会触发 `WhenPlantChess` 事件，导致羁绊重新生成小卡。

**解决**: 读档前清理所有小卡，让羁绊系统重新生成正确数量。

## 使用指南

### 添加新的存档数据

1. **在 SaveData.cs 添加数据结构**:
```csharp
[Serializable]
public class NewFeatureSaveData
{
    public int someValue;
    public string someString;
}
```

2. **添加到 GameSaveData**:
```csharp
public class GameSaveData
{
    // ... 其他字段
    public NewFeatureSaveData newFeatureData;
}
```

3. **在 SaveDataCollector 收集数据**:
```csharp
private static NewFeatureSaveData CollectNewFeatureData()
{
    NewFeatureSaveData data = new NewFeatureSaveData();
    // 从游戏获取数据
    data.someValue = SomeManager.Instance.someValue;
    return data;
}
```

4. **在 SaveDataApplier 恢复数据**:
```csharp
private static void ApplyNewFeatureData(NewFeatureSaveData data)
{
    if (data == null) return;
    // 应用到游戏
    SomeManager.Instance.someValue = data.someValue;
}
```

### 调试技巧

1. **查看存档文件**:
   - 位置: `%AppData%/LocalLow/DefaultCompany/AsoulChess/Saves/savegame.sav`
   - 格式: JSON，可直接用文本编辑器查看

2. **日志输出**:
   - `[SaveDataCollector]` - 收集过程日志
   - `[SaveDataApplier]` - 恢复过程日志
   - `[SaveManager]` - 存档管理器日志

3. **常见问题**:
   - `Object reference not set`: 检查空引用，添加保护代码
   - `找不到类型`: 检查命名空间，使用完整类名
   - `字段不存在`: 检查字段名拼写，确认访问权限

## 依赖关系

```
SaveManager
├── SaveDataCollector
│   ├── GameManage (allChess)
│   ├── LevelManage (currentController, IfGameStart)
│   ├── ChessTeamManage (GetTeam)
│   ├── PropertyController (属性)
│   ├── BuffController (Buff)
│   ├── SkillController (技能)
│   ├── PlantsShop (选卡栏)
│   ├── ItemPanel (小卡)
│   └── SunLightPanel (阳光)
├── SaveDataApplier
│   └── (同上)
└── SaveData (纯数据结构)
```

## 版本历史

- **v1.0**: 基础存档系统
  - 支持棋子属性、位置、Buff
  - 支持关卡进度
  - 支持阳光值
  - 支持卡片CD
  - 支持小卡(Item_PlantCard)
