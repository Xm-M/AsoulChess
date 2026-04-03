# 项目概览

## 项目统计

| 项目 | 数量 |
|------|------|
| 总脚本数 | 232个 |
| 核心脚本 | 约50个 |
| 场景数 | 6个 |
| 预制体 | 154个 |

## 目录结构

```
Assets/
├── Script/                 # 核心游戏逻辑 (232个脚本)
│   ├── Manage/            # 管理器模块
│   ├── Chess/             # 棋子系统
│   ├── Buff/              # Buff系统
│   ├── LevelSystem/       # 关卡系统
│   ├── Event/             # 事件系统
│   ├── UI/                # UI系统
│   ├── Weapon/            # 武器系统
│   ├── Fetter/            # 羁绊系统
│   ├── State/             # 状态机
│   ├── bullet/            # 子弹系统
│   ├── Effect/            # 特效系统
│   └── Map/               # 地图/格子系统
├── Prefab/                # 预制体资源
├── Scenes/                # 场景文件
├── Resources/             # 运行时资源
├── SO/                    # ScriptableObject配置
├── Animation/             # 动画资源
└── Audio/                 # 音频资源
```

## 核心架构模式

### 1. 单例管理器模式
全局管理器使用单例模式：
- `GameManage` - 游戏主管理器
- `LevelManage` - 关卡管理器
- `EventController` - 事件管理器
- `ObjectPool` - 对象池管理器

### 2. 组件化设计 (Chess)
棋子采用多 Controller 设计：
```
Chess (MonoBehaviour)
├── PropertyController   # 属性管理
├── SkillController      # 技能管理
├── StateController      # 状态管理
├── BuffController       # Buff管理
├── MoveController       # 移动控制
├── AttackController     # 攻击控制
└── AnimatorController   # 动画控制
```

### 3. 事件驱动架构
通过 `EventController` 实现模块解耦：
```csharp
// 订阅事件
EventController.Instance.AddListener(EventName.GameStart.ToString(), OnGameStart);

// 触发事件
EventController.Instance.TriggerEvent(EventName.GameStart.ToString());
```

### 4. 对象池模式
`ObjectPool` 管理游戏对象复用，减少GC压力。

## 数据流

```
关卡流程:
LevelManage → LevelController → 游戏逻辑

棋子生命周期:
ChessFactory → Chess.InitChess() → Chess.WhenChessEnterWar() → Chess.Death()

属性计算:
DamageMessage → PropertyController.GetDamage() → 属性计算 → UI显示
```

## 关键枚举

### EventName (事件名)
游戏核心事件：GameStart, GameOver, WhenDeath, WhenAttackTakeDamages 等

### DamageType (伤害类型)
Physical(物理), Magic(技能), Real(真实), Miss(未命中), Heal(治疗)

### StateName (状态名)
角色状态：AttackState, DeathState, DizzyState, Confusion 等

## UI系统架构

### 主要UI面板

```
PlantsShop (选卡栏)
├── currentSelectIcons     # 选中的卡牌（下方）
├── currentShopIcons       # 场上卡片（上方）- 对应 ShopIcon
└── allSelectIcons         # 所有可选卡牌

ItemPanel (道具栏/小卡容器)
├── itemPool               # 对象池缓存
└── 动态生成的 Item_PlantCard (羁绊小卡)

ShopIcon (主卡)
├── good                   # PropertyCreator 配置
├── coldDown               # CD时间
└── t                      # 当前CD计时(私有字段)

Item_PlantCard (小卡/一次性卡)
├── creator                # PropertyCreator 配置
├── plantOver              # 是否已使用(私有字段)
└── Recycle()              # 使用后回收到对象池
```

### UI管理
- `UIManage` - 静态方法管理所有View
- `View` 基类 - 所有UI面板继承此类
- `UIItem` 基类 - 可复用的UI元素（如Item_PlantCard）

## 羁绊系统 (Fetter)

### 触发机制
```csharp
// 以结束乐队羁绊为例
FetterEffect() {
    EventController.Instance.AddListener<Chess>(
        EventName.WhenPlantChess.ToString(), 
        OnPlantChess
    );
}

OnPlantChess(Chess chess) {
    // 放置特定标签植物时生成小卡
    Item_PlantCard card = UIManage.GetView<ItemPanel>().Create<Item_PlantCard>();
    card.InitCard(...);
}
```

### 关键注意点
- 小卡使用后被**回收到对象池**（隐藏，非销毁）
- 读档时需要清理对象池，避免重复生成

## 存档系统 (Save System)

### 架构设计
```
SaveManager (单例)
├── SaveDataCollector    # 数据收集（静态类）
├── SaveDataApplier      # 数据恢复（静态类）
└── SaveData             # 存档数据结构

存档数据包含:
├── LevelSaveData        # 关卡状态（波数、时间等）
├── ChessSaveData        # 棋子数据（属性、位置、Buff）
├── ShopSaveData         # 选卡栏状态
│   ├── selectedCardIds  # 选中卡牌
│   ├── shopIconData     # 主卡CD
│   └── plantCards       # 小卡状态
└── TimerSaveData        # 计时器
```

### 关键技术点
1. **反射访问私有字段**: 获取CD计时、波数等
2. **数据收集器模式**: 将游戏状态提取为可序列化数据
3. **JSON序列化**: 使用JsonUtility，便于调试
4. **原子写入**: 先写临时文件再替换，防止存档损坏

### 常见问题与解决方案
| 问题 | 原因 | 解决方案 |
|------|------|---------|
| UIManage.instance不存在 | UIManage是静态类 | 使用 `UIManage.GetView<T>()` |
| PropertyCreator没有gameObject | 继承ScriptableObject | 直接访问字段，不用gameObject |
| 小卡无限使用 | 回收到对象池 | 读档时清理对象池缓存 |
| 羁绊重复生成小卡 | WhenPlantChess事件 | 读档前清理所有小卡 |

## 扩展点

1. **新棋子类型**: 继承 Chess 类，配置 PropertyCreator
2. **新Buff**: 继承 Buff 或 TimeBuff 类
3. **新技能**: 实现 ISkill 接口
4. **新武器**: 继承 Weapons 类
5. **新关卡**: 继承 LevelController 类
6. **新存档数据**: 在SaveData.cs添加数据结构，在Collector/Applier实现收集/恢复
