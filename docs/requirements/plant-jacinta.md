# 功能需求卡片: 角色嘉辛塔

## 基本信息
- **功能名称**: 嘉辛塔（撑伞 / 产阳持续技）
- **所属模块**: Chess / Skill / PlantUmbrella
- **需求类型**: 新增功能（角色配置 + 技能效果）
- **优先级**: P1
- **预估复杂度**: L2
- **预估耗时**: 4–6 小时（不含完整美术）
- **提出日期**: 2026-08-14
- **需求 ID**: plant-jacinta

## 功能描述
### 详细描述
实现植物角色「嘉辛塔」：常态不攻击；主动为**持续技能**（持续 25s，结束后 CD 15s）。技能开启期间：
1. 类似桃金娘，由动画事件触发产阳（单次产量略低于向日葵/虹夏）；
2. 类似薇薇安，弹开保护范围内敌方子弹（可打断蹦极）。

美术本期不强制：逻辑与 Prefab/SO 接线；图与动画事件由策划在 Editor 补。

### 用户故事
作为玩家，我希望种下嘉辛塔后能在技能期间获得阳光并挡弹，以便在遮挡真空期仍能稳住经济。

## 现有业务上下文

### 相关现有功能
| 功能 | 模块 | 关系 | 说明 |
|------|------|------|------|
| 桃金娘 | Skill | 相似 | `ColdSkill` + `DurationFinish` + `SkillEffect_CreateSunLight` |
| 薇薇安 | Skill | 相似 | `PassiveSkill_LettuceUmbrella` 常驻挡弹 |
| `PlantUmbrellaBulletKnock` | Skill | 复用 | 弹飞子弹工具 |
| 嘉辛塔空壳 | Resources/Prefab | 被补全 | SO/Prefab 已存在但未配置 |

### 需求类型判定理由
新角色，框架已有；组合「持续产阳 + 仅技能期挡弹」，需新 `ISkillEffect`（或等效组合），不新建战斗系统。

### 集成点
- **调用**: `ColdSkill` / `SkillController.UseSkill`（动画事件）/ `onSkillOver` 清理挡弹
- **触发**: 入场后 auto CD 就绪 → 播 skill → 持续 25s
- **数据**: `PropertyCreator` + `SkillConfig_Cold` + Prefab 技能序列化

### 对现有功能的影响
- **接口变更**: 无（新增 effect 类）
- **行为变更**: 无（不影响桃金娘/薇薇安）
- **数据变更**: 补全嘉辛塔 SO/Prefab；新建技能 SO

## 已锁定决策
| # | 结论 |
|---|------|
| 1 | 产阳：动画事件点产，对齐桃金娘流程 |
| 2 | 挡弹：与薇薇安同范围（`protectTileRadius=1` ≈ 3×3）；含打断蹦极 |
| 3 | 造价 **50**；因挡弹有真空期，单次阳光 **建议 20**（虹夏/桃金娘为 25） |
| 4 | 本期逻辑为主，美术后续补 |
| 5 | CD：**技能结束后**再计 15s（现有 ColdSkill） |
| 6 | 持续：`SkillRuntimeInfo_Duration.maxTime = 25` |
| 7 | 常态 `attack=0`，不普攻 |

## 技术要求
- **依赖模块**: Skill、Chess、PlantUmbrella、UI(SunLight)
- **规范**: `ISkillEffect` + 事件/计时器；禁止技能专用运行时 `AddComponent` Mono
- **兼容性**: 向后兼容

## 功能清单
### 核心功能（必须）
- [x] 新建 `SkillEffect_Jacinta`：技能期启动挡弹，`onSkillOver`/`OnRemove` 对称清理；每次 `UseSkill` 产阳
- [x] `SkillConfig_Cold`：持续 25、CD 15、`baseDamage[0]=20`、auto
- [x] 补全 `嘉辛塔.asset`（名/价/血/标签/描述/chessPre）
- [x] Prefab：挂 `ColdSkill` + DurationFinish(maxTime=25) + effect；无武器攻击
- [x] 文案：短描述体现「技能产阳 · 技能挡弹」

### 扩展（可选）
- [ ] 预警范围 `warnTileRadius` 与薇薇安对齐可调（已默认 3）
- [ ] 正式立绘 / 动画事件帧（需在 skill 动画挂 `UseSkill`）

## 验收标准
- [ ] 嘉辛塔可购买种植，常态不普攻
- [ ] 技能持续约 25s，结束后约 15s 才可再放
- [ ] 技能中动画事件能产阳（单次约 20）
- [ ] 技能中 3×3 内敌弹被弹开；技能结束后不再挡弹
- [ ] 桃金娘 / 薇薇安无回归

## 风险评估
| 风险点 | 概率 | 影响 | 应对措施 |
|-------|------|------|---------|
| 动画未挂 UseSkill 事件 | 高 | 中 | 文档标明；可用临时测试帧或手动调 |
| 挡弹与产阳同 effect 重复订阅 | 中 | 中 | 首次启动挡弹，幂等；Over 必清理 |
| 产量手感偏差 | 中 | 低 | baseDamage 可调 |

## 关联 Context
- 撑伞体系：`docs/game-design/plant-roster.md`
- 技能规范：`.cursor/rules/unity-skill-no-runtime-monobehaviour.mdc`
