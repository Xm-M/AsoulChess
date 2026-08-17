# 功能需求卡片: 实现 Core UPM 包

## 基本信息
- **功能名称**: Core UPM 包（事件 / 计时 / 对象池）
- **所属模块**: 工程基建 / `Packages/com.asoulchess.game.core`
- **需求类型**: 新增功能（工程基建）
- **优先级**: P1
- **预估复杂度**: L2
- **预估耗时**: 1～3 天（含 Sample 与文档）
- **提出日期**: 2026-07-27
- **上游规划**: [reusable-systems-packaging](./reusable-systems-packaging.md)（已审批）

## 功能描述

### 详细描述

在本仓库以 **Monorepo** 方式新增 UPM 包 `com.asoulchess.game.core`，提供与玩法无关的基础设施：

1. **事件总线**（参考 `EventController`，不含 `EventName`）
2. **计时服务**（参考 `TimerManage`：延时、循环、倍速、Timer 回收；不硬绑关卡事件）
3. **对象池**（参考 `ObjectPool` 的 Create/Recycle；不含 Chess/Tile/`GameManage` 逻辑）
4. **服务入口**（轻量接口或 locator，避免再造上帝单例）

实现策略：**精简重写进包**，不剪切粘贴现有文件。  
**本期不修改 AVZ 玩法代码**（旧 Event/Timer/Pool 保持原样）；用包内 Sample 验证可引用。

### 用户故事

作为开发者，我希望有一个可被空项目引用的 Core UPM 包，以便以后新项目直接安装事件/计时/对象池，而不拷贝 AVZ 业务代码。

## 现有业务上下文

### 相关现有功能

| 功能 | 模块 | 关系 | 说明 |
|------|------|------|------|
| EventController | Event | 相似 / 参考实现 | 总线进包；EventName 留 AVZ |
| TimerManage | Manage | 相似 / 参考实现 | 计时能力进包 |
| ObjectPool | Manage | 相似 / 参考实现 | 通用池进包；棋子逻辑不进 |
| GameManage | Manage | 本期不触碰 | 不改为依赖 Core |

### 需求类型判定理由

全新 UPM 包与程序集；AVZ 运行时零迁移（选项 A）。属工程基建新增，非玩法功能扩展。

### 用户已确认选型

| 项 | 选择 |
|----|------|
| 包位置 | Monorepo：`Packages/com.asoulchess.game.core/` |
| AVZ 接入 | **A**：只建包 + Sample，不动玩法代码 |
| 包名 | `com.asoulchess.game.core` |

### 集成点

- **调用的现有接口**: 无（包不依赖 AVZ）
- **触发的现有事件**: 无
- **AVZ → Core**: 本期不接线；仅 `manifest.json` 可声明本地包依赖（若嵌入 Packages 则 Unity 自动识别）
- **需要的新接口**: `IEventBus`、`ITimerService`、`IGameObjectPool`（或等价命名）、可选 `GameServices`

### 对现有功能的影响

- **接口变更**: 无（AVZ 旧 API 不变）
- **行为变更**: 无
- **数据变更**: 无
- **工程变更**: 新增 Packages 目录内容；可能更新 `manifest.json`

## 技术要求

- **Unity版本**: 与当前工程一致（2022+ / 现有 URP 工程）
- **依赖模块**: 仅 `UnityEngine`（Runtime）；不依赖 Odin、不依赖 AVZ 脚本
- **性能要求**: 不低于现有 Event/Timer/Pool 常用路径的数量级
- **兼容性要求**: 包独立可编译；不破坏 AVZ 现有编译与运行
- **平台支持**: 与当前 2D 项目一致
- **程序集**: `AsoulChess.Game.Core.asmdef`（建议名，可微调）

## 功能清单

### 核心功能（必须）

- [x] `package.json` + Runtime 目录 + `.asmdef`
- [x] 事件总线：Add / Remove / Trigger（无参与泛型）
- [x] 计时服务：延时、循环、停止、倍速、Timer 回收
- [x] GameObject 对象池：Get/Release（或 Create/Recycle）、Clear
- [x] 服务入口（接口注册/获取）
- [x] `Samples~` 最小演示（或 Runtime 旁文档说明如何验证）
- [x] README：如何引用、API 一页纸
- [x] 确认包内无 Chess / Level / EventName / GameManage 引用

### 扩展功能（可选 · 本期可不做）

- [ ] Editor 程序集与调试窗口
- [ ] 泛型非 GameObject 对象池
- [ ] 将 AVZ 改为调用 Core（属后续需求）

## 验收标准

- [x] `Packages/com.asoulchess.game.core` 可被本工程识别
- [ ] Core 程序集单独编译通过，asmdef 无游戏程序集引用（需在 Unity 中确认）
- [ ] Sample 或最小脚本能演示：发事件 → Timer → 从池取物体 → 回收（需 Import Sample 后 Play）
- [x] AVZ 原有场景/玩法代码未因本需求被改坏（本期目标：尽量零改动玩法脚本）
- [x] README 写清引用方式与边界（不含玩法）

## 风险评估

| 风险点 | 概率 | 影响 | 应对措施 |
|-------|------|------|---------|
| API 与旧类同名导致混淆 | 中 | 中 | 使用命名空间 `AsoulChess.Game.Core` |
| Sample 场景依赖未打包资源 | 中 | 低 | Sample 用最简 Cube/空物体 |
| 日后迁移 AVZ 时 API 不贴合 | 中 | 中 | README 注明「参考实现非 1:1 兼容」 |
| UPM 路径/manifest 配错 | 低 | 中 | 按现有 `file:` 包惯例；嵌入 Packages 即可 |

## 关联 Context

- 涉及模块: Event, Manage（参考）；新建 Core 包
- 参考文档: `docs/architecture/reusable-systems-packaging-plan.md`
- 依赖关系: `context/architecture/dependency-graph.md`

## 产出物（预期）

- `Packages/com.asoulchess.game.core/`（实现期）
- 本需求卡片: `docs/requirements/core-upm-package.md`
- 架构分析: `docs/architecture/analysis/core-upm-package.md`
- 审批报告: `docs/approvals/core-upm-package.md`
