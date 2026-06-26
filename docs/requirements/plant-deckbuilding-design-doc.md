# 功能需求卡片：植物构筑设计文档（Plant Deckbuilding Design Doc）

## 基本信息

- **功能名称**：植物构筑设计指南（设计文档，非代码功能）
- **所属模块**：Game Design / Roguelike / Chess
- **需求类型**：新增功能（**文档资产**）
- **优先级**：P1（策划与后续 plant 设计依赖）
- **预估复杂度**：L1（纯文档）
- **提出日期**：2026-06-11

## 功能描述

### 详细描述

在 `docs/game-design/` 下新增 **独立设计文档**，记录 PVZ + 肉鸽语境下 plant 构筑的设计原则（资源轴、体系 vs 道具、四象限、price 档位、检查清单），与 [单位制作.md](../单位制作.md)（实现）和肉鸽 requirements（系统）分离。

### 用户故事

作为策划 / 程序，我希望有一份 **可持续更新的构筑设计记录**，以便新增 plant、Prop、肉鸽奖励时有一致的设计语言，而不只在聊天或零散需求里重复解释。

## 现有业务上下文

| 功能 | 关系 |
|------|------|
| `单位制作.md` | 实现流程；本文档补 **设计层** |
| `roguelike-*` requirements | Run 经济 / 奖励 **系统** |
| `band-instrument-bonds-reference.md` | 羁绊 **词库** |

### 需求类型判定理由

**文档资产新增**：不修改运行时代码；为后续 plant / 肉鸽内容提供策划 SSOT（Single Source of Truth）。

## 产出物

- ✅ [docs/game-design/plant-deckbuilding-design.md](../game-design/plant-deckbuilding-design.md)

## 验收标准

- [x] 文档路径在 `docs/game-design/`
- [x] 含资源三层、构筑二维、Plant 四象限、与 STS 对照
- [x] 含 TODO 表（分档 / 体系草案 / 变更日志）
- [x] 含新 plant 设计检查清单
- [x] 交叉引用现有 docs

## 关联 Context

- `context/index.md`
- `context/modules/Chess.md`
