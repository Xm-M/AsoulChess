# LevelSystem 模块

## 定位

**关卡生命周期**：选关、载入、局内阶段、胜负、与 `GameManage` / `MapManage` / `LevelController` 协作。

## 核心类型（节选）

| 类型 | 职责 |
|------|------|
| `LevelManage` | 当前关卡数据、切换场景或重置局内 |
| `LevelController` | 抽象局内逻辑；大量子类（如 `LevelController_HammerZombie`、蛇关等） |
| `LevelData` / 配置 SO | 关卡参数、出怪、模式 |
| `ModeManage` | 模式相关（若启用） |
| `ILevelplugIn` / `GameStartPlugin_*` | 关卡插件式扩展（天气、开场等） |

## 依赖

- 依赖 **Manage**、**Map**、**Chess**  
- 触发 **Event**；可能驱动 **UI**  

## 扩展

- 新关卡类型：新增 `LevelController` 派生 + 预制体/ SO 绑定。  
- 避免在基类里写死单关硬编码；用数据驱动分支。
