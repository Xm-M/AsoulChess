# 功能需求卡片: SkillEffect_Oblivionis 交替双子弹

## 基本信息
- **功能名称**: Oblivionis 弹道交替
- **所属模块**: Skill / ISkillEffect（AveMujica）
- **需求类型**: 功能扩展
- **提出日期**: 2026-05-19

## 功能描述
将原「单子弹 prefab」改为 **两种子弹分别可配置**，每次释放时根据 **上一次实际发射的子弹** 切换为 **另一种**；首次无记录时发射 **bulletA**。

## 现有业务上下文
| 功能 | 模块 | 关系 |
|------|------|------|
| `SkillEffect_Oblivionis` | ISkillEffect | 扩展 |
| `SkillContext` | SkillController | 存 `int` 键值，可存档 |

## 集成点
- `bullet` 字段通过 `[FormerlySerializedAs("bullet")]` 迁移为 `bulletA`，需在 Inspector 为 `bulletB` 指定第二种子弹 prefab。
- 状态键：`SkillEffect_Oblivionis.ContextKeyLastBulletIndex`（`"OblivionisLastBulletIndex"`），值 `0`/`1`。

## 验收标准
- [ ] 交替顺序：A → B → A → …
- [ ] 每名棋子独立（依赖该棋子的 `SkillContext`）
- [ ] 仅一种 prefab 配置时：另一发尝试失败则回退到已配置的一种，不崩溃

## 实现文件
- `Assets/Script/Skill/ISkillEffect_Plant/AveMujica_Skill/SkillEffect_Oblivionis.cs`
