---
name: asoulchess-context
description: AsoulChess/AVZ Unity 2D 自走棋项目的完整上下文知识库。当需要了解项目架构、模块依赖、核心类API、设计模式、编码规范时自动加载。包含 GameManage、Chess、Buff、Event、LevelSystem、SaveSystem、UI、Fetter 等所有核心模块的详细文档。
---

# AsoulChess 项目上下文

## 项目概况

AsoulChess 是 Unity 2D 类 PVZ 策略塔防自走棋游戏，采用组件化架构。

- **脚本目录**: `Assets/Script/`（单数，非 Scripts）
- **脚本数**: ~232 个
- **核心单例**: `GameManage.instance`
- **UI框架**: Pixel UI（View基类）
- **编辑器扩展**: Odin Inspector

## 快速参考

### 核心单例访问

```csharp
GameManage.instance              // 游戏主管理器
LevelManage.instance             // 关卡管理器
EventController.Instance         // 事件控制器
ChessTeamManage.Instance         // 棋子队伍管理
ObjectPool.Instance              // 对象池
UIManage.GetView<T>()            // UI面板（静态类，无instance）
SunLightPanel.instance           // 阳光面板
SaveManager.Instance             // 存档管理器
```

### 核心数据流

```
游戏主流程: GameManage → LevelManage → LevelController → ChessManage → Chess
属性计算:   Chess.AttackController → DamageMessege → PropertyController → UI.DamagePanel
Buff 流程:  BuffController → Buff → TimerManage → Event → Buff.BuffOver
事件通信:   任意模块 → EventController → 其他模块
存档流程:   SaveManager → SaveDataCollector / SaveDataApplier
```

### 关键枚举

| 枚举 | 说明 | 典型值 |
|------|------|--------|
| EventName | 游戏事件 | GameStart, GameOver, WhenDeath, WhenPlantChess, WhenChessEnterWar |
| DamageType | 伤害类型 | Physical(物理), Magic(技能), Real(真实), Miss(未命中), Heal(治疗) |
| StateName | 棋子状态 | AttackState, DeathState, DizzyState, Confusion |

## 模块详情（按需查阅 references/）

开发时根据涉及模块加载对应参考文档：

| 模块 | 文件 | 核心类 | 复杂度 |
|------|------|--------|--------|
| 游戏管理 | `references/GameManage.md` | GameManage, ChessTeamManage, ChessFactory | 高 |
| 棋子系统 | `references/ChessSystem.md` | Chess, PropertyController, SkillController | 高 |
| 棋子模块 | `references/Chess.md` | Chess, 各Controller扩展方式 | 高 |
| Buff系统 | `references/Buff.md` | Buff, TimeBuff, MultyBuff, BuffController | 中 |
| 事件系统 | `references/Event.md` | EventController, EventName | 低 |
| 羁绊系统 | `references/FetterSystem.md` | Fetter, FetterController, Item_PlantCard | 中 |
| 关卡系统 | `references/LevelSystem.md` | LevelManage, LevelController, LevelData | 高 |
| 存档系统 | `references/SaveSystem.md` | SaveManager, SaveDataCollector, SaveDataApplier | 高 |
| UI系统 | `references/UISystem.md` | UIManage, View, PlantsShop, ItemPanel, ShopIcon | 中 |
| UI模块 | `references/UI.md` | UIManage, UIRoot | 中 |
| 架构决策 | `references/decisions.md` | 设计模式、技术选型、编码规范 | - |
| 依赖图谱 | `references/dependency-graph.md` | 模块依赖矩阵、Mermaid图 | - |

## 核心架构模式

### 1. 单例管理器

```
GameManage (DontDestroyOnLoad)
├── SceneManage
├── UIManage
├── TimerManage
├── AudioManage
├── ChessFactory
├── ChessTeamManage
└── FetterController
```

### 2. 棋子组件化设计

```
Chess (MonoBehaviour)
├── PropertyController   # 属性管理、伤害计算
├── SkillController      # 技能释放和CD
├── StateController      # 状态机（Idle, Attack, Dizzy等）
├── BuffController       # Buff添加/移除/更新
├── MoveController       # 移动和路径查找
├── AttackController     # 攻击逻辑
└── AnimatorController   # 动画控制
```

### 3. 事件驱动

```csharp
// 订阅
EventController.Instance.AddListener(EventName.GameStart.ToString(), OnGameStart);
EventController.Instance.AddListener<Chess>(EventName.WhenDeath.ToString(), OnChessDeath);

// 触发
EventController.Instance.TriggerEvent(EventName.GameStart.ToString());
EventController.Instance.TriggerEvent<Chess>(EventName.WhenDeath.ToString(), deadChess);

// 移除（必须在 OnDestroy 中清理！）
EventController.Instance.RemoveListener(EventName.GameStart.ToString(), OnGameStart);
```

## 编码规范

- 类名/方法名: PascalCase (`GameManage`, `InitChess`)
- 私有字段: camelCase (`_rb`, `buffs`)
- 公共字段: PascalCase
- 脚本目录: `Assets/Script/`（单数）
- Controller类统一继承 Controller 基类

## 扩展指南

| 扩展类型 | 方式 |
|----------|------|
| 新棋子 | 继承 Chess 类，配置 PropertyCreator (ScriptableObject) |
| 新Buff | 继承 Buff 或 TimeBuff 类 |
| 新技能 | 实现 ISkill 接口 |
| 新武器 | 继承 Weapons 类 |
| 新关卡 | 继承 LevelController 类 |
| 新存档数据 | SaveData.cs 添加结构，Collector/Applier 实现收集/恢复 |
| 新UI面板 | 继承 View 基类，放入 UIPrefab/Resources |

## 高风险依赖

- **循环依赖**: Chess ↔ BuffController ↔ Buff（通过事件解耦）
- **强耦合**: 所有模块依赖 GameManage 单例
- **性能风险**: 大量使用 UnityEvent，注意 OnDestroy 中清理监听
- **UIManage是静态类**: 使用 `UIManage.GetView<T>()`，不是 `UIManage.instance`
- **PropertyCreator无gameObject**: 继承 ScriptableObject，直接访问字段
- **敌人配置查找**: 玩家在 `GameManage.allChess`，敌人在 `LevelData.zombieList`

## 存档系统注意事项

- 快捷键: F5存档, F9读档
- 存档位置: `%AppData%/LocalLow/DefaultCompany/AsoulChess/Saves/savegame.sav`
- 私有字段需反射访问: ShopIcon.t, LevelController.currentWave, Item_PlantCard.plantOver
- 读档前必须清理对象池中的小卡(Item_PlantCard)，避免重复生成
