# 功能需求卡片: Entity Framework UPM（Chess 体系骨架）

## 基本信息
- **功能名称**: Entity Framework UPM 包（Chess 机制 / 不含内容）
- **所属模块**: `Packages/com.asoulchess.game.entity`（暂定名）
- **需求类型**: 新增功能（工程基建）
- **优先级**: P1
- **预估复杂度**: A 级
- **预估耗时**: 5～10 天（精简重写 + Sample；不含 AVZ 迁移）
- **提出日期**: 2026-07-27
- **上游**: [reusable-systems-packaging](./reusable-systems-packaging.md)、[core-upm-package](./core-upm-package.md)

## 功能描述

### 详细描述

在已有 Core（`com.asoulchess.game.core`）之上，新增 **Entity Framework** UPM 包，把 AVZ「Chess 体系」中的 **可复用机制** 抽成跨项目框架：

**进包（机制 + 骨架）**  
- 实体根对象（对应 `Chess` 的职责抽象，建议命名 `GameEntity` / `BattleUnit`，避免与具体棋子语义绑死）  
- `IEntityController` + 可插拔 Controllers（Property / Skill / Buff / State / Attack 等）  
- 生命周期：Init → EnterCombat → Tick → Death / Leave  
- **属性骨架**：生命/攻击等基础字段、承伤/治疗接口（精简，非完整 `PropertyController` 搬家）  
- **技能骨架**：`ISkill` / `ISkillEffect` / `SkillContext` 风格接口与最小基类  
- **Buff 骨架**：`Buff` / 限时 Buff 基类 + `BuffController` 风格管理  
- 依赖 Core：`IEventBus`、`ITimerService`、`IGameObjectPool`

**不进包（内容）**  
- 具体棋子、Animator 子类、具体 `ISkillEffect_*`、Fetter、商店、关卡  
- **本期不含格子地图**（无 Tile/Map）；实体用世界坐标；预留 `IBoardOccupant` 空接口可选  

**与 AVZ 关系（选项 A）**  
- 只建包 + Sample 演示实体；**不修改**现有 `Chess` 继承关系，不做迁移。

### 用户故事

作为开发者，我希望在新项目中引用「实体 + 属性/技能/Buff 骨架」UPM，以便快速搭出可战斗单位框架，而不拷贝 AVZ 的全部棋子内容。

## 现有业务上下文

### 相关现有功能

| 功能 | 模块 | 关系 | 说明 |
|------|------|------|------|
| Chess + Controllers | Chess | 相似 / 参考 | 组件化实体模式是包的蓝本 |
| Controller 接口 | Chess | 相似 | Init / EnterWar / LeaveWar |
| PropertyController | Chess | 骨架来源 | 只抽象结算管道，不搬 UI/格子逻辑 |
| ISkill / ISkillEffect | Skill | 骨架来源 | 保留接口模式；禁止技能专用运行时 MonoBehaviour |
| Buff / TimeBuff | Buff | 骨架来源 | 用 Core Timer，不绑 GameManage |
| Core UPM | Packages | 依赖 | 事件/计时/对象池 |
| Map / Tile | Map | 本期排除 | 预留扩展点即可 |

### 需求类型判定理由

全新 UPM 与程序集；参考 AVZ Chess 体系做精简重写，非剪切粘贴。属规划中的 Framework 层提前落地（用户明确需要比 Core 更多能力）。

### 用户已确认选型

| 项 | 选择 |
|----|------|
| 范围 | **2** — 框架 + 数值/技能/Buff 骨架 |
| AVZ | **A** — 只建包 + Sample，不迁 Chess |
| 地图 | **1** — 第一期不管格子，世界坐标 + 可选占位接口 |

### 集成点

- **依赖包**: `com.asoulchess.game.core`
- **调用**: Core EventBus / Timer / Pool  
- **AVZ**: 本期零接线  
- **事件**: 包内用字符串常量（如 `entity.death`），不含 AVZ `EventName` 枚举  

### 对现有功能的影响

- **本期**: 无运行时玩法影响（仅新增 Packages）  
- **远期（另开需求）**: AVZ `Chess` 可逐步改为组合/继承包内基类  

## 技术要求

- **Unity版本**: 与当前工程一致（2022.3+）  
- **依赖**: `com.asoulchess.game.core`；不依赖 Odin Runtime（若需 SerializeReference，评估是否仅 Editor 或纯接口列表）  
- **性能**: Sample 内数十实体 Tick 可接受；不对 AVZ 局内热路径负责（未接入）  
- **兼容性**: AVZ 旧 Chess 并行存在  
- **命名空间**: `AsoulChess.Game.Entity`（建议）  
- **包名**: `com.asoulchess.game.entity`（建议，审批时可改）  

## 功能清单

### 核心功能（必须）

- [x] `package.json` + asmdef（引用 `AsoulChess.Game.Core`）  
- [x] `GameEntity`（或等价）生命周期 API  
- [x] `IEntityController` + 至少：Property / Skill / Buff / State（最小状态：Idle/Dead 或 Idle/Attack/Dead）  
- [x] 属性：HP、受伤、死亡触发  
- [x] 技能：ISkill + 一次 Use 的最小路径 + SkillContext  
- [x] Buff：添加/移除/限时（用 Core Timer）  
- [x] Sample：生成实体 → 挂 Buff → 用技能/普攻掉血 → 死亡回调  
- [x] README：边界说明（非 AVZ Chess 1:1；无地图；AVZ 未迁移）  

### 扩展功能（可选 · 本期可不做）

- [ ] Move / Attack 完整索敌（无地图时仅示意）  
- [ ] Animator 桥接基类  
- [ ] 格子 `IBoard` / `ITile`  
- [ ] AVZ Chess 迁移  

## 验收标准

- [x] 空项目或本仓库 Package Manager 可识别 entity 包  
- [x] asmdef 仅引用 Core + Unity，无 AVZ `Assets/Script`  
- [ ] Sample 可演示完整最小战斗循环（无 Tile）（需在 Unity 中 Import + Play 确认）  
- [x] 包内无具体角色/关卡/Fetter/`EventName`  
- [x] AVZ 玩法脚本未被本需求修改  

## 风险评估

| 风险点 | 概率 | 影响 | 应对措施 |
|-------|------|------|---------|
| 范围膨胀成「半个 AVZ」 | 高 | 高 | 严格内容黑名单；Sample 只做通用立方体实体 |
| 与现有 Chess API 不像导致以后难迁 | 中 | 中 | README 写对照表；接口对齐职责而非类名 |
| 无地图导致 Move/Attack 空洞 | 中 | 低 | 本期接受；占位接口即可 |
| SerializeReference / Odin 渗入 Runtime | 中 | 中 | 优先接口 + 普通序列化列表 |
| 工期超预期 | 中 | 中 | 按 P0 最小竖切优先交付 |

## 关联 Context

- `context/modules/Chess.md`, `Skill.md`, `Buff.md`  
- `docs/architecture/reusable-systems-packaging-plan.md`  
- Core: `Packages/com.asoulchess.game.core/`  

## 产出物（预期）

- `Packages/com.asoulchess.game.entity/`  
- 本卡片: `docs/requirements/entity-framework-upm.md`  
- 架构分析 / 审批: `docs/architecture/analysis/`、`docs/approvals/`
