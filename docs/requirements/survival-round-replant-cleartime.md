# 功能需求卡片: survival-round-replant-cleartime

## 基本信息
- **功能名称**: 生存轮界 ClearTime + 双次种植（无 Buff 展示 → 插件 → 重种挂 Buff）
- **所属模块**: LevelSystem / Endless / SaveSystem
- **需求类型**: 功能重构 + Bug修复
- **优先级**: P0
- **预估复杂度**: L2
- **预估耗时**: 4–6 小时
- **提出日期**: 2026-08-14

## 功能描述
### 详细描述
生存模式将「进入下一轮」与「读档进轮」统一为**模拟重载关卡**：

1. **轮末**：`ClearTime()` 清空全部 Timer；快照场上植物（`propertyCreator` + 格子 + HP），**不保存 Buff / 技能中途状态**。
2. **进轮展示（选卡前）**：按快照种植**无 Buff** 版本，让玩家看到场上有什么。
3. **开战**：GameStart 插件基于当前场上植物跑完 → **全部消灭** → 按同一快照**再种一遍**（`WhenChessEnterWar` 重挂被动/Buff/攻击 Timer）。

读档走同一路径，从而避开 `Buff_Mujica_Doloris.buffFrom` 等运行时 Chess 引用无法还原导致的 NRE。

### 用户故事
作为生存模式玩家，我希望每轮开战都像重开一局那样干净地重建植物与计时器，以便没有残留 Timer / 坏 Buff，同时选卡阶段仍能看到场上阵容。

## 现有业务上下文

### 相关现有功能
| 功能 | 模块 | 关系 | 说明 |
|------|------|------|------|
| 轮末 Timeline / 选卡 / GameStart | LevelSystem Endless | 被重构 | 在插件后增加「灭→重种」 |
| `TimerManage.ClearTime` | Manage | 依赖 | 轮末整表清 Timer |
| `RestorePlayerPlants` / `CapturePlayerPlants` | SaveSystem | 修改 | 生存不存/不读 buffs；支持「仅展示」与「正式进战」两种种植 |
| `WhenChessEnterWar` | Chess | 依赖 | 重种后挂 Buff/Timer |
| GameStart 插件（含羁绊等） | LevelSystem | 依赖 | 须在「无 Buff 展示种」之后、「灭再种」之前执行 |

### 需求类型判定理由
- 改变生存轮界生命周期（ClearAll + 双次种植），属流程重构。
- 修复读档恢复带 `buffFrom` 的 Buff 空引用，属 Bug 修复。
- 不新开模块，扩展现有 Endless / 存档路径。

### 集成点
- **调用的现有接口**: `TimerManage.ClearTime`、`ChessTeamManage.CreateChess`、`Tile.PlantChess`、棋子离场/回收、`LevelData.GameStartPlugin.StadgeEffect`、`CapturePlayerPlants` / `RestorePlayerPlants`
- **触发的现有事件**: `GameStart`、`WhenChessEnterWar`、`WhenLeaveLevel`（若灭植物走死亡/回收需确认是否误触发关卡逻辑）
- **需要的新接口**: 如 `RestorePlayerPlants(plants, restoreBuffs: false)`、`ReplantPlayerPlantsFromSnapshot`、生存专用 `CapturePlayerPlantsSurvival`（无 buffs）

### 对现有功能的影响
- **接口变更**: 种植/恢复 API 区分「展示种」与「进战种」；生存 Capture 跳过 buffs
- **行为变更**: 每轮开战前植物实例会被销毁并重建一次；选卡阶段为无 Buff 实例
- **数据变更**: 生存存档 `playerPlants[].buffs` 为空或不写；旧存档有 buffs 时读档忽略

## 关联 Context
- 涉及模块: LevelSystem, Buff, Manage, Chess, SaveSystem
- 参考文档: `context/modules/LevelSystem.md`, `context/modules/Buff.md`, `context/modules/Manage.md`
- 依赖关系: `context/architecture/dependency-graph.md`

## 技术要求
- **Unity版本**: 项目现行版本
- **依赖模块**: LevelSystem, SaveSystem, Chess, TimerManage, UI（选卡）
- **性能要求**: 单轮重种一次全场植物，可接受；避免同帧重复进战事件风暴（可 `WhenChessEnterWar(triggerEvents:)` 控制）
- **兼容性要求**: 生存存档向前兼容（忽略旧 buffs）；冒险模式默认**仍保留**原 Buff 存读（本次范围仅 SurvivalMode），除非另行确认
- **平台支持**: 与现网一致

## 功能清单
### 核心功能（必须）
- [ ] 轮末：`ClearTime` + 快照（creatorId/tile/hp，无 buffs）
- [ ] 同会话进下一轮 / 读档：选卡前种植无 Buff 展示阵
- [ ] GameStart：插件跑完 → 消灭场上玩家植物 → 按快照重种并进战
- [ ] 生存 Capture/Restore **不读写 buffs**（忽略旧档 buffs）
- [ ] 止血：即使误走 `AddBuffFromSave`，`Buff_Mujica_Doloris` 等对 `buffFrom==null` 安全（可选防御）

### 扩展功能（可选）
- [ ] 重种时保留 HP（与快照一致）vs 满血重开（需产品确认，默认**保留快照 HP**）
- [ ] 展示种是否跳过部分进战特效/音效，减少选卡阶段噪音

## 验收标准
- [ ] 轮末→选卡→开战：无残留阳光 Timer 的 MissingReference；植物攻击/被动 Timer 正常
- [ ] 读档进生存轮：不再因 `Buff_Mujica_Doloris.BuffEffect` NRE 中断
- [ ] 选卡阶段能看到场上植物；开战插件（如羁绊）基于展示阵生效后，重种仍正确
- [ ] 冒险模式 Buff 存读行为未被误改（若约定仅生存）
- [ ] 代码审查通过

## 风险评估
| 风险点 | 概率 | 影响 | 应对措施 |
|-------|------|------|---------|
| GameStart 插件依赖「已进战完整态」植物 | 中 | 高 | 插件阶段用展示种；重种后再补一次仅依赖进战的逻辑（若有） |
| 灭植物触发 Death/掉落/统计 | 中 | 中 | 使用静默回收（forRestore/无事件）路径，避免当战斗死亡 |
| 双次种植事件重复（羁绊叠层） | 中 | 高 | 展示种 `triggerEvents:false` 或插件只跑一遍；重种再触发进战事件 |
| 旧存档含坏 buffs | 低 | 中 | 生存 Restore 跳过 buffs |

## 已确认决策（2026-08-14）
1. 快照来源：轮末/存档的 propertyCreator+格子（+HP），当作重载关卡模拟 — **A**
2. Buff：不存不读；靠重种 + EnterWar / 被动重挂 — **A**
3. 顺序：展示种（creator）→ GameStart 插件 → 全灭 → 重种 — **A**
4. 读档：与正常流程相同（无 Buff 展示 → 选卡 → 插件 → 重种）
