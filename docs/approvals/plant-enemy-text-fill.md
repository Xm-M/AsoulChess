# 需求开发审批报告: 植物/僵尸文案补全

## 基本信息

- **需求名称**: plant-enemy-text-fill (RG-CONTENT-02)
- **所属模块**: 内容配置
- **分析日期**: 2026-06-30

## 需求摘要

批量补全 124 张 PropertyCreator SO 的 `chessDescription` / `chessShortDescription` / `chessEffect`，纯配置、无运行时改动。

**需求类型**: 功能优化（UI 文案）  
**关系**: 依赖已修复的 `audit_content_config.py` 作为验收

## 分析结果

| 维度 | 结论 |
|------|------|
| Context | ✅ `context/index.md` + 设计 doc §9 |
| Skills | ✅ `@unity-scriptableobject-config` |
| 架构 | 豁免 — 无代码 |
| 影响面 | 124 `.asset`；回归：图鉴/商店/选卡 tooltip |

## 实施方式

1. `tools/chess_text_catalog.json` — 文案数据源（按 chessName 索引）
2. `tools/apply_chess_text.py` — 仅填充当前为空的字段
3. 分批：P0 植物 → P1 僵尸

## 决策审批

✅ **通过** — 开始写入 SO

日期: 2026-06-30
