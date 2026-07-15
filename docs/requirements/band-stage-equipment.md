# 功能需求卡片: 乐队舞台器具

## 基本信息

- **功能名称**: 乐队舞台器具（扩音箱 / 麦克风 / 聚光灯 / 节拍器 / 调音机）
- **所属模块**: Chess + Skill + Buff + Bullet（扩音箱穿格）
- **需求类型**: **新增功能**（5 个新 plant + 若干 Buff + 可选弹道扩展）
- **优先级**: P1
- **预估复杂度**: **L2～A**
- **提出日期**: 2026-07-01
- **状态**: **📄 仅文档沉淀** — 机制方向已写入策划文档，**数值与实现细节后续再定，暂不开发**

## 功能描述

### 详细描述

策划设计见 [`band-stage-equipment-design.md`](../game-design/band-stage-equipment-design.md)。

| 器具 | 定位 | 机制概要（方向） |
|------|------|------------------|
| **扩音箱** | MainPlant | 子弹穿格强化（火炬树桩）；细节待定 |
| **麦克风** | SupportPlant | 种 Main 上 → 主唱类 Buff；细节待定 |
| **聚光灯** | SupportPlant | 种 Main 上 → 宿主**四邻**每个不同友军 Main → 加对应攻击力 |
| **节拍器** | Consume | 一次性消耗；效果方案待定 |
| **调音机** | Consume | 一次性消耗；效果方案待定 |

### 聚光灯（本次新增）

- **种植**：与南瓜罩 / 麦克风相同 — `SupportPlant` + 必须种在已有 Main 的格上。
- **效果**：以 **宿主 Main 所在格** 为中心，统计 **上下左右四邻格** 内存活的友方 Main（`stander`）；每个计数的友军使宿主获得 **`attackPerAlly` 攻击力**（数值待定）。
- **互斥**：同格仅 1 个 Support，与麦克风二选一。

### 用户故事

- 作为玩家，我希望用舞台器具（扩声、追光、排练消耗品）增强 Live 演出感与布局深度。
- 作为玩家，我希望把聚光灯套在 C 位上，并用四邻队友围成十字阵拉高她的输出。

## 现有业务上下文

### 相关现有功能

| 功能 | 关系 |
|------|------|
| `SupportPlant` / `ConsumePlant` / `MainPlant` | 种植规则复用 |
| `PassiveSkillEffect_PumpkinShell` | Support 生命周期参考 |
| `Buff_DrummerAura` | 聚光灯周期性邻格扫描参考 |
| `MapManage.NearTile` | 聚光灯四邻格统计 |
| `ConsumablesSkill` | 节拍器 / 调音机 |

### 集成点（实现阶段再细化）

- 聚光灯：`GetNearTile` 四邻 + `Buff_SpotlightAttack` 动态改攻
- 扩音箱：子弹穿格 Registry（新）
- 麦克风：`Buff_MicVocal`
- 消耗品：`SkillEffect_*` + `ConsumablesSkill.SkillOver`

## 功能清单

### 文档阶段（当前）

- [x] 策划设计文档五器具机制方向
- [x] 聚光灯四邻加攻规则写入
- [ ] 数值拍板
- [ ] 程序实现

### 实现阶段（后续）

- [ ] 扩音箱 / 麦克风 / 聚光灯 / 节拍器 / 调音机脚本与 Asset
- [ ] Prefab / 进池配置

## 验收标准（实现阶段启用）

- [ ] 聚光灯：四邻各 1 Main 时攻击 = 基础 + 4×attackPerAlly；邻格变化后加成更新
- [ ] 聚光灯与麦克风不可同格
- [ ] 其余见策划文档 §9 待确认项拍板后的定稿

## 关联 Context

- `context/modules/Chess.md`, `Skill.md`, `Buff.md`, `Map.md`
- 策划: `docs/game-design/band-stage-equipment-design.md`

## 审批状态

- ✅ **文档沉淀** — 用户确认细节后续再考虑
- ⬜ 数值与方案拍板
- ⬜ 开发审批
