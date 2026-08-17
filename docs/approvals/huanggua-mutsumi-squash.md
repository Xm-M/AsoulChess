# 需求开发审批报告：黄瓜睦（窝瓜式下砸）

## 基本信息
- **需求名称**: 黄瓜睦 — 左右索敌 + 水平移动 + 圆形下砸自毁
- **所属模块**: Chess / Skill
- **分析日期**: 2026-05-30

## 需求摘要

Mygo `黄瓜睦`：同排左右各 2 格索敌 → **水平移动**到目标 → 动画下砸 → **圆形范围 ATK 伤害** → 自毁。不用抛物线跳跃；动画用户自做。

**需求类型**: 功能扩展  
**与现有功能关系**: 复用 `MoveToTarget` + OverlapCircle；不影响 AveMujica `若叶睦`

## 用户确认定稿

| 项 | 定稿 |
|----|------|
| 单位 | Mygo/黄瓜睦 |
| 索敌 | 同排 ±1、±2 |
| 流程 | 两段：索敌移动 → 下砸圆形伤 |
| 自身 | 自毁 |
| 移动 | 仅水平 `MoveToTarget`，无 JumpToTarget |
| 动画 | 用户自行制作与接线 |

## Skills 白名单

| 技术点 | Skill | 状态 |
|--------|-------|------|
| 协程 / 计时器 | unity-coroutine-system | ✅ |
| 2D 物理圆形检测 | unity-2d-physics | ✅ |
| Animator 事件 | unity-2d-animation | ✅ |

结论：通过。

## 架构摘要

```
PassiveSkillEffect_HuangguaMutsumi (ISkillEffect)
  WhenEnterWar / SkillEffect:
    Timer 检 ±1±2 格敌人
    → ChessLeave + UnSelectable
    → MoveToTarget(targetTile, speed, onArrive)
    → PlaySkill（下砸动画）
  动画事件 UseSkill / OnSmash:
    OverlapCircle → ATK 伤害
    → Death()
```

- **设计模式**: ISkillEffect 被动驱动两段流程  
- **风险**: 中低（动画接线依赖用户）

## 影响面

| 文件 | 改动 |
|------|------|
| 新建 `PassiveSkillEffect_HuangguaMutsumi.cs`（或等价） | 核心逻辑 |
| （可选）`AnimatorController_Squash` 事件转发 | 事件钩子 |
| `Mygo/黄瓜睦.prefab` | 挂被动、去无关武器/移动 |
| `黄瓜睦.asset` | 文案/数值对齐「过度」定位 |

影响文件约 2–4；高风险点：动画事件未绑导致下砸不结算。

## 风险总评

| 维度 | 评级 |
|------|------|
| 技术可行性 | 高 |
| 改动范围 | 小 |
| 性能风险 | 低 |
| 时间评估 | 1–2h |

## 决策审批

✅ **通过** — 2026-05-30 已实现

实现摘要:
- `PassiveSkillEffect_HuangguaMutsumi`：±1/±2 索敌 → 水平 `MoveToTarget` → `PlaySkill` → `onUseSkill` 圆形 ATK → `Death`
- `AnimatorController_Squash.UseSkill` / `OnSmashHit` 供动画事件绑定
- Mygo `黄瓜睦.prefab` 已挂被动；`黄瓜睦.asset` 文案与 speed=8

动画接线: 下砸动画状态名默认 `skill`；事件绑到 `AnimatorController_Squash.UseSkill`（或 `OnSmashHit`）

日期: 2026-05-30
