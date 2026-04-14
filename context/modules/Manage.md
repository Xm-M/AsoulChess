# Manage 模块

## 定位

全局运行时中枢：**场景协调、队伍与棋子创建、UI 入口、计时器、对象池、天气**等，多数通过 **`GameManage.instance`** 访问。

## 核心类型（节选）

| 类型 | 职责 |
|------|------|
| `GameManage` | 单例；持有 `LevelManage`、`ChessTeamManage`、`UIManage`、`TimerManage`、`ObjectPool` 等引用；游戏阶段切换 |
| `LevelManage` | 关卡列表、当前关、进入/离开关卡 |
| `ChessTeamManage` | 队伍与创建棋子（与 `ChessManage` / `EnemyManage` 等配合） |
| `ChessManage` / `EnemyManage` | 植物方 / 敌方棋子集合与更新 |
| `TimerManage` | 全局计时与延迟 |
| `ObjectPool` | 对象复用 |
| `WeatherManage` | 天气效果（若局内启用） |

## 依赖

- **上游**: 场景入口、Bootstrap  
- **下游**: `LevelSystem`、`Chess`、`UI`、`Event`  

## 扩展注意

- 避免在 `GameManage` 上无限堆叠新业务；能下沉到 `LevelController` 或独立系统的尽量下沉。  
- 读档/重开时注意与各 `Manage` 的 `Clear` / 重绑事件顺序。
