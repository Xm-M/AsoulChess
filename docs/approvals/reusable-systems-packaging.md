# 需求开发审批报告

## 基本信息
- **需求名称**: 可复用系统打包规划（文档先行）
- **所属模块**: 工程基建 / Framework
- **分析日期**: 2026-07-27
- **需求 ID**: `reusable-systems-packaging`

## 需求摘要

为 AVZ 制定跨项目复用路线：以 **UPM** 交付，**第一期只做 Core**（事件 / 计时 / 对象池 / 服务接口）；本期只交付规划文档，不改代码。

**需求类型**: 新增功能（文档资产）+ 远期渐进重构  
**与现有功能的关系**: 概念上对应 Event / TimerManage / ObjectPool；玩法模块（Chess、Level）不纳入第一期

## 分析结果汇总

### Context 复用
✅ 已读取项目 Context（`context/index.md`，2026-03-30 / v2.0.0）
- 涉及模块: Manage, Event, Chess, LevelSystem
- 参考文档: `context/architecture/dependency-graph.md`, `context/architecture/decisions.md`

### 现有业务分析
- **相关现有功能**: EventController, TimerManage, ObjectPool, GameManage 单例中枢
- **需求类型判定**: 文档先行；实现时为工程基建 + 解耦重构
- **范围锁定**: 用户确认 **先 Core**
- **对现有功能的影响（本期）**: 无运行时影响

### 需求卡片
✅ `docs/requirements/reusable-systems-packaging.md`

### 规划正文
✅ `docs/architecture/reusable-systems-packaging-plan.md`

### Skills白名单检查

已覆盖的技术点（**针对远期 Core 实现**，非本期编码）:
✅ 对象池 → `@unity-object-pool`（自定义池 / `UnityEngine.Pool`）
✅ 预制体复用相关 → `@unity-prefab-system`
✅ 场景（ObjectPool 历史用法）→ `@unity-scene-management`
✅ 设计模式（单例 / 观察者 / 对象池）→ `@unity-design-patterns`

未覆盖的技术点:
⚠️ **Unity UPM / package.json / 本地 Git 包引用** — Skills 中无专项文档  
   → 建议：实现 Core 前查阅 Unity Package Manager 官方文档；事后可沉淀为内部 Skill  
⚠️ **Assembly Definition 工程拆分** — 无专项 Skill  
   → 建议：实现时按 Unity asmdef 官方指南；保持 Runtime 不引用 Odin

检查结论: **本期文档通过**；实现期需关注 UPM/asmdef（少量未覆盖，可继续）

### 架构分析
✅ `docs/architecture/analysis/reusable-systems-packaging.md`
- 设计模式: 分层 + UPM + 接口注入
- 风险等级（本期）: 低

### 影响面分析
✅ 本期：**确认无运行时影响**（纯文档）  
远期 Core 实现需另开需求并做完整影响面分析

## 风险总评

| 维度 | 评级 | 说明 |
|------|------|------|
| 技术可行性 | 高 | 路线清晰；Core 边界明确 |
| 改动范围（本期） | 小 | 仅文档 |
| 性能风险（本期） | 无 | — |
| 时间评估（本期） | 已完成 | 文档已落盘 |
| 远期 Core 实现 | 中等范围 | 约数天级，需单独审批 |

## 建议的Skills使用清单（动手实现 Core 时）

- `@unity-object-pool` — 池化 API 规范
- `@unity-prefab-system` — 池与预制体协作
- `@unity-design-patterns` — 观察者 / 单例收敛为接口
- Unity 官方 Package Manager 文档 — UPM 发布与引用

## 开发优先级建议

- **P0（本期）**: 审批通过规划文档；冻结「第一期 = Core」
- **P1（下次开工）**: 另开 `@requirement-workflow` →「实现 Core UPM 包」
- **P2**: Stats / Entity 骨架（Core 验证通过后）
- **不做（明确排除）**: 本期及第一期不打包完整数值表、棋子、关卡内容

## 决策审批

✅ **通过** - 规划文档可作为后续依据；暂不写代码  
⬜ 修改后通过 - 需要调整方案  
⬜ 驳回 - 暂不采纳

审批意见: 用户确认理解（Core→UPM，非整包 AVZ）；第一期范围锁定 Core  
日期: 2026-07-27

---

### 审批通过说明

本期交付是 **规划**，不是开发任务。本流程已结束，**不要**对本需求 ID 启动代码开发。

若之后要真正做 Core 包，请新开：

```
@requirement-workflow 实现 Core UPM 包（事件/计时/对象池）
```
