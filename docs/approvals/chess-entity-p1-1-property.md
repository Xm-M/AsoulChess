# 需求开发审批报告 — Chess/Entity P1-1

## 基本信息

- **需求名称**: PropertyController 与伤害管线
- **分析日期**: 2026-08-10
- **用户确认**: 项目正常运行，可开始 P1-1

## 需求摘要

迁入 `EntityStats`、`DamageInfo`、伤害类型枚举与完整 `PropertyController` 战斗公式；UI/Animator/Buff 通过 C# 事件由 AVZ 订阅。本步不改 AVZ `Property.cs`。

**需求类型**: 功能重构  
**风险等级**: 中（AVZ Property 引用面大，但本步仅动 Packages）

## Skills 白名单

| 技术点 | Skill | 结论 |
|--------|-------|------|
| 事件委托 | unity-design-patterns | ✅ |
| SerializeField/Stats | unity-scriptableobject-config | ✅ |
| Random 暴击闪避 | — | UnityEngine ✅ |

## 影响面

| 区域 | 影响 |
|------|------|
| `Packages/.../PropertyController.cs` | 重写扩展 |
| `Packages/.../Combat/*` | 新增 |
| AVZ `Property.cs` | **无**（P1-1b） |
| Skill/Weapon 引用 `DamageMessege` | **无** |

## 决策

✅ **通过** — 框架侧 P1-1 已实现（2026-08-10）

## 下一步

- Unity 验证 Entity Demo
- **P1-1b**（可选）：AVZ Shim 接 DamagePanel
- **P1-2**：BuffController
