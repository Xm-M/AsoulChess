# 功能需求卡片: 羁绊效果文案补全

## 基本信息

- **功能名称**: 5 个羁绊 `fetterEffectDescription` 补全
- **所属模块**: Fetter / 内容配置
- **需求类型**: 功能优化（UI 文案）
- **优先级**: P1
- **预估复杂度**: L1
- **预估耗时**: 0.5 小时
- **提出日期**: 2026-05-20

## 功能描述

### 详细描述

为已有完整运行时逻辑的 5 个羁绊 SO 补全 `fetterEffectDescription`，使 `FetterPanel` / `FetterIcon` 悬停提示能展示效果说明。文案依据 `Fetter.cs`、`Fetter_Instruments.cs`、`Fetter_HoukagoTeaTime.cs` 中的实现与现有羁绊（贝斯、鼓手、结束乐队）的表述风格对齐。

### 用户故事

作为玩家，我希望在选卡/羁绊面板看到每个羁绊的效果说明，以便理解凑羁绊的收益。

## 现有业务上下文

### 相关现有功能

| 功能 | 模块 | 关系 | 说明 |
|------|------|------|------|
| `Fetter.fetterEffectDescription` | Fetter | 数据源 | 悬停 tooltip 读取字段 |
| `FetterIcon` / `FetterPanel` | UI | 展示 | 无代码改动 |
| 内容配置审计 | tools | 前置 | `content_config_gaps.json` 已标记缺口 |

### 需求类型判定理由

11 个注册羁绊中 6 个已有描述；本次仅补 SO 字符串，不新增 `Fetter` 子类或逻辑。

### 集成点

- **调用的现有接口**: 无（只改 SO）
- **触发的现有事件**: 无

### 对现有功能的影响

- **接口变更**: 无
- **行为变更**: 无（仅 UI 文案可见性）
- **数据变更**: 5 个 `Assets/SO/Fetter/*.asset`

## 补全文案清单

| SO | 类 | 文案（草案） |
|----|-----|-------------|
| `Mygo.asset` | `Mygo` | Mygo 全员在场时：Mygo 卡牌冷却减半；成员获得 10% 增伤与 10% 减伤 |
| `无刺有刺.asset` | `TogenashiTogeari` | 无刺有刺全员在场时：成员获得 GBC 增益；每 25 秒释放刺雨（持续 10 秒），对全场敌人造成暗属性持续伤害 |
| `主唱.asset` | `Vocal` | 主唱攻击时有概率额外发射一发子弹：2 人 20%、3 人 45%、4 人 70%、5 人 100% |
| `键盘.asset` | `KeyBoard` | 键盘手为周围 3×3 范围内的单位提供护甲光环；自身获得双倍护甲加成（2～5 人分档，满层约 100 护甲） |
| `放学后茶会.asset` | `HoukagoTeaTime` | 放学后茶会成员获得 15% 额外治疗效果；场上多名茶会成员时，所受伤害均摊为真实伤害 |

## 功能清单

### 核心功能（必须）

- [ ] 上述 5 个 SO 写入 `fetterEffectDescription`
- [ ] 文案与代码数值一致（Mygo buff SO 为 0.1；刺雨 coldDown/continueTime 来自 SO）

### 扩展功能（可选）

- [ ] 已有描述 6 个羁绊补充分档细节（本次不做）

## 验收标准

- [ ] 5 个 SO 字段非空
- [ ] 进战斗选卡后 `FetterPanel` 悬停可见对应文案
- [ ] `tools/audit_content_config.py` 羁绊缺口归零（或仅剩 icon 等待办）

## 风险评估

| 风险点 | 概率 | 影响 | 应对措施 |
|-------|------|------|---------|
| 文案与实现数值偏差 | 低 | 低 | 对照 SO 序列化值与代码注释 |
| Unity YAML 格式 | 低 | 中 | 沿用现有 SO 单行引号格式 |

## 关联 Context

- `docs/game-design/content-config-audit.md`
- `context/index.md`（Fetter 模块）
