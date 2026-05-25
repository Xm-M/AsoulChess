# 功能需求卡片: ISkillEffect_BigRabbit（身边随机召唤）

## 基本信息
- **功能名称**: ISkillEffect_BigRabbit
- **所属模块**: Skill / ISkillEffect
- **需求类型**: 新增功能
- **提出日期**: 2026-04-06

## 功能描述
技能效果：在施法者 **站立格邻接** 的格子中 **随机选一格**，若该格对配置的 **PropertyCreator** 满足 **IfCanPlant**，则 **CreateChess**，阵营与施法者 **tag** 相同。

## 集成点
- 挂到 `SkillBase` / `PassiveSkill` 的 `effect`（或 `SkillEffect_Composite` 子项）。
- 在 Inspector 指定 `spawnCreator`；可选四邻/八邻。

## 验收标准
- [ ] 同 tag 生成
- [ ] 仅合法种植格生成；无合法格时不崩

## 实现文件
- `Assets/Script/Skill/ISkillEffect_Plant/ISkillEffect_BigRabbit.cs`
