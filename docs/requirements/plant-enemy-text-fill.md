# 功能需求卡片: 植物/僵尸 PropertyCreator 文案补全

## 基本信息

- **功能名称**: RG-CONTENT-02 棋子 SO 文案补全
- **所属模块**: 内容配置 / PropertyCreator
- **需求类型**: 功能优化（UI 文案 / SO 配置）
- **优先级**: P1
- **预估复杂度**: L2（124 张 SO，纯配置）
- **预估耗时**: 8–16 小时（策划+分批验收）
- **提出日期**: 2026-06-30

## 功能描述

为 `Assets/Resources/ChessData/Player/`（70）与 `Enemy/`（54）补全 `chessDescription`、`chessShortDescription`、`chessEffect`，使商店、肉鸽选卡、图鉴、RunInfo 通过 `PlantCreatorDetailHelper.BuildDescriptionText()` 正常展示。

文案来源优先级：已有 SO 片段 → 技能代码注释 → `plant-deckbuilding-design.md` §9 定位 → 策划补写。

### 用户故事

作为玩家，我希望选卡和图鉴能看到每个棋子/僵尸的描述、简介与效果，以便理解构筑与对策。

## 现有业务上下文

| 功能 | 模块 | 关系 |
|------|------|------|
| `PropertyCreator` 三字段 | ChessData SO | 数据源 |
| `PlantCreatorDetailHelper` | UI | 只读展示 |
| `audit_content_config.py` | tools | 缺口扫描（已修复） |

### 当前缺口（2026-06-30 审计）

| 类别 | 总数 | 三字段全空 | 有部分文案 | 三字段齐全 |
|------|------|------------|------------|------------|
| 植物 | 70 | 59 | 11 | 0 |
| 僵尸 | 54 | 54 | 0 | 0 |

### 集成点

- 无代码变更；仅改 SO
- 验收：`python tools/audit_content_config.py`

### 字段约定

| 字段 | UI | 内容 |
|------|-----|------|
| `chessDescription` | 首段 | 角色定位 / 一句话背景 |
| `chessEffect` | 【效果】 | 机制与数值要点 |
| `chessShortDescription` | 【简介】 | 玩家向短句概括 |

## 功能清单

### P0 — 肉鸽可用主力（§9 非「未完成」）

- [x] ~60 张 plant 三字段补全（实际 70/70）
- [x] 11 张部分文案补全缺失字段（保留原有简介，只补空字段）

### P1 — 僵尸全量

- [x] 54 张 enemy 补全

### P2 — 未完成占位

- [x] 千夏、田井中律等标注「开发中/未完成」（文案已写入 SO，机制待实装）

## 实施记录（2026-06-30）

- 工具：`tools/chess_text_data.py`（文案源）+ `tools/apply_chess_text.py`（只填空字段）
- 审计：`python tools/audit_content_config.py` → 植物 70/70、僵尸 54/54 三字段齐全

## 验收标准

- [ ] `audit_content_config.py`：P0 plant `noText` = 0；P1 enemy `noText` = 0
- [ ] 图鉴/商店悬停可见【效果】【简介】
- [ ] 已有非空字段不被覆盖（除非策划明确要求）

## 风险评估

| 风险 | 概率 | 影响 | 措施 |
|------|------|------|------|
| 文案与实现不符 | 中 | 中 | 对照 Skill 类 XML 注释 |
| 一次性改动过大 | 中 | 低 | 工具化 `apply_chess_text.py` + catalog |

## 关联 Context

- `docs/game-design/plant-deckbuilding-design.md` §9
- `docs/game-design/content-config-audit.md`
- `tools/content_config_gaps.json`
