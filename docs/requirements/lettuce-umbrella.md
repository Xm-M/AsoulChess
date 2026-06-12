# 功能需求卡片: 莴苣保护伞

## 基本信息
- **功能名称**: 莴苣保护伞
- **所属模块**: Skill / Plant
- **需求类型**: 功能扩展
- **优先级**: P1
- **提出日期**: 2026-05-20

## 功能描述
以自身为中心的 3×3 范围内：
- **敌方子弹**：优先弹飞出屏幕，不支持改向的弹道直接销毁
- **蹦极僵尸**：仅在下跳阶段打断，seek 到 skill 返回段动画后离场，不抱走植物

## 实现要点
- `PassiveSkill_LettuceUmbrella`：Timer 高频扫描（默认 0.05s）
- `PlantUmbrellaBulletKnock`：子弹弹飞/销毁/出屏回收
- `BungeeRetreatHelper`：下跳判定 + `InterruptedByUmbrella` 上下文
- `SkillEffect_Bungee`：打断时跳过抱走逻辑

## Prefab 配置
在莴苣植物 `passiveSkill` 的 `PassiveSkill` 上挂载 `PassiveSkill_LettuceUmbrella`。
可选配置 Animator Trigger `Deflect` 作为伞面反馈。

## 验收标准
- [ ] 3×3 内敌方子弹被弹飞或销毁，不再伤害植物
- [ ] 下跳中的蹦极木偶被弹回并离场，不触发抱走
- [ ] 已抱走阶段（UseSkill 后）不可打断
