# 功能需求卡片: 可复用系统打包规划（文档先行）

## 基本信息
- **功能名称**: 可复用系统打包规划（Reusable Systems Packaging Plan）
- **所属模块**: 工程基建 / Framework（跨 Manage、Event、Chess、LevelSystem）
- **需求类型**: 新增功能（**文档资产**）+ 后续分阶段功能重构（本期不做代码）
- **优先级**: P1
- **预估复杂度**: L1（本期纯文档）；后续实现约 A 级
- **预估耗时**: 本期 0.5～1 小时（文档）；实现另行评估
- **提出日期**: 2026-07-27

## 功能描述

### 详细描述

为 AVZ（AsoulChess）梳理一套「把部分系统抽成可跨项目复用资产」的完整规划：交付形态、分层边界、分期路线、风险与验收标准。  
**本期只产出规划文档，不修改运行时代码、不创建 UPM 包实体。**

用户尚未做过类似工作，选定策略为：
1. 交付形态倾向 **Unity UPM 包**
2. **第一期实现范围已确认：仅 Core**（事件 / 计时 / 对象池 / 服务接口）；不做 Stats / 角色 / 关卡
3. 当前阶段先消化「怎么做」（规划文档），再决定何时开工实现 Core

### 用户故事

作为开发者，我希望有一份可执行的打包复用路线图，以便在不动现有玩法的前提下，逐步把通用能力抽到其他 Unity 项目中复用，而不是一次性硬拷代码导致失败。

## 现有业务上下文

### 相关现有功能

| 功能 | 模块 | 关系 | 说明 |
|------|------|------|------|
| EventController | Event | 相似 / 待抽离 | 全局事件总线，复用价值高；`EventName` 含大量游戏特化枚举 |
| TimerManage | Manage | 相似 / 待抽离 | 计时器池 + 倍速；依赖 Event 与 GameManage 生命周期 |
| ObjectPool | Manage | 相似 / 待抽离 | 对象池；部分 API 直接引用 Chess/Tile |
| PropertyController | Chess | 后续候选 | 数值结算；与棋子/伤害消息强绑定 |
| Chess + Controllers | Chess | 后续候选 | 角色骨架可抽象，玩法内容不可整体搬走 |
| LevelManage / LevelController | LevelSystem | 后期候选 | 关卡流程高度游戏特化 |
| GameManage | Manage | 被依赖 / 需解耦 | 单例中枢，是抽包最大障碍 |

### 需求类型判定理由

- **本期**：文档资产新增（与 `plant-deckbuilding-design-doc` 同类）
- **远期**：工程基建 + 渐进重构（引入 asmdef / UPM，切断对 `GameManage` 的硬依赖）
- 不是新增局内玩法，也不应被当成「导出 .unitypackage 就能用」

### 集成点（远期实现时）

- **调用的现有接口**: `EventController`、`TimerManage`、`ObjectPool`、`IManager`
- **触发的现有事件**: 保留游戏侧 `EventName`；Core 包应使用字符串或泛型事件 ID，避免把 AVZ 枚举塞进通用包
- **需要的新接口**: 服务定位 / `IGameServices`（或等价物），替代到处写 `GameManage.instance`

### 对现有功能的影响

- **本期（文档）**: 无运行时影响
- **远期（实现）**:
  - **接口变更**: 管理器获取方式可能从单例直取改为接口注入
  - **行为变更**: 目标是行为等价；需回归局内计时、对象池、事件订阅清理
  - **数据变更**: 无存档格式变更预期

## 技术要求

- **Unity版本**: 与当前工程一致（2022+）
- **依赖模块**: Event、Manage（Timer/ObjectPool）；远期可选 Chess 数值层
- **性能要求**: 抽包后不低于现有 Event/Timer/Pool 基线
- **兼容性要求**: AVZ 玩法向后兼容；包版本需可独立演进
- **平台支持**: 与当前 2D 项目一致

## 功能清单

### 核心功能（必须 · 本期）

- [x] 明确推荐交付形态（UPM）与反模式（直接整包拷贝 / 裸 .unitypackage）
- [x] 给出分层模型（Core / Stats / Entity / GameContent）
- [x] 给出分期路线图与每期验收标准
- [x] 标注当前代码中的耦合风险与抽离顺序
- [x] 需求流程审批通过（2026-07-27）

### 扩展功能（可选 · 非本期）

- [ ] 创建实际 UPM 包仓库 / `Packages/` 本地包
- [ ] 引入 Assembly Definition
- [ ] 抽出 Core 并让 AVZ 引用
- [ ] 空壳模板工程

## 验收标准

- [ ] 规划文档路径明确，可作为后续开发输入
- [ ] 分层与分期清晰，第一期范围可独立开工
- [ ] 明确「什么不该打包」
- [ ] 审批通过后再进入任何代码改动

## 风险评估

| 风险点 | 概率 | 影响 | 应对措施 |
|-------|------|------|---------|
| 低估 Chess/Level 耦合，想一次抽完 | 高 | 高 | 强制分期；第一期只做 Core |
| EventName 枚举污染通用包 | 高 | 中 | 游戏事件留在 AVZ；Core 只提供总线 |
| ObjectPool 含 Chess 专用逻辑 | 中 | 中 | 池化通用 API 进包；棋子专用逻辑留游戏侧 |
| 文档完成后长期不落地 | 中 | 低 | 规划中写「最小验证里程碑」 |

## 关联 Context

- 涉及模块: Manage, Event, Chess, LevelSystem
- 参考文档: `context/index.md`, `context/modules/Manage.md`, `context/modules/Event.md`, `context/modules/Chess.md`, `context/modules/LevelSystem.md`
- 依赖关系: `context/architecture/dependency-graph.md`
- 架构决策: `context/architecture/decisions.md`

## 产出物

- 需求卡片: `docs/requirements/reusable-systems-packaging.md`（本文）
- 规划正文: `docs/architecture/reusable-systems-packaging-plan.md`
