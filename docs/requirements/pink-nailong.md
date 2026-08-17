# 功能需求卡片: 粉色奶龙（千早爱音动物形态）

## 基本信息
- **功能名称**: 粉色奶龙
- **所属模块**: Skill / Chess（MyGO）
- **需求类型**: 新增功能
- **优先级**: P1
- **预估复杂度**: L1
- **预估耗时**: 2–3 小时
- **提出日期**: 2026-08-11
- **需求 ID**: pink-nailong

## 功能描述
千早爱音的动物形态：独立 MainPlant，`fetterMemberId=千早爱音`，不变身，常态不普攻。  
主动（ColdSkill，默认 CD 15s）：在 `IGridFindTarget` 配置范围内选最近一只敌方，消灭后于同格生成我方「奶龙僵尸」（Creator 可替换；暂用普通僵尸占位）。

## 已锁定决策
| # | 结论 |
|---|------|
| 1 | 独立种植，`fetterMemberId=千早爱音`，不变身 |
| 2 | 目标 Death → CreateChess(奶龙僵尸, Player) |
| 3 | 奶龙僵尸：独立 Creator/Prefab，数值同普通僵尸；粉色奶龙已接线 |
| 4 | 索敌用 IGridFindTarget（SkillReady_IfTargetInRange） |
| 5 | CD 可配，默认 15s；范围=技能+Prefab 接线 |

## 验收标准
- [ ] 不普攻
- [ ] CD 到且前方配置格有敌时自动转化最近一只
- [ ] 生成单位为我方 tag；替换 Creator 后无需改代码
- [ ] 计入 MyGO 爱音槽
- [ ] 成功转化后在目标世界坐标生成可配置特效 `convertEffect`
