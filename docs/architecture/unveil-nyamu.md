# 架构设计: 揭幕喵梦

## 系统定位
- **所属模块**: Skill（主动效果）+ Chess（灰烬配置）
- **上游**: `StateController.EnterWarState`、`SkillState` / `ISkillFireUseSkillOnEnter`、Map 格子、`OkuwakiStressSpread`、`ItemPanel`/`SunLight`
- **下游**: 3×3 友军压力与阳光；AveMujica Member 与若麦同 `fetterMemberId`
- **横向**: 史尔特尔/喜多（种植进技能）、压力希（异形态身份模式）

## 设计决策（推荐）

### 主动触发
- Prefab：`EnterWarState = SkillState`
- 主动技能：`ColdSkill` + 薄封装实现 `ISkillFireUseSkillOnEnter`（进态立刻 `UseSkill`）
- `WhenEnter` 可 `UnSelectable()`（灰烬不可铲期间体验对齐 Consumables）
- **`SkillOver` 不调用 `Death()`** —— Death 由技能动画结束事件触发（用户配置）

**不推荐**: 直接用 `ConsumablesSkill`（`SkillOver` 会 `Death()`，与动画 Death 冲突）。

**备选**: 纯 `ColdSkill`，效果只在动画 `UseSkill` 帧触发 —— 依赖动画事件齐全；进态立刻结算更稳。

### 效果类
新建 `SkillEffect_UnveilNyamu`（名可定）：
1. 以 `standTile` 为中心枚举 3×3（`dx,dy ∈ [-1,1]`，**含 0,0**）
2. 每格遍历 `chessesIntile`，同 tag 且未死的友军
3. 对每名友军：`ApplyStressDelta(stressAmount, guestTemplate)`（默认 25）
4. 对每名友军：`Create<SunLight>()` → `InitSunLight(allyTile, sunAmount, pos)`；`sunAmount` 取 `config.baseDamage[0]`（默认 25）

**备选**: 复用 `IGridFindTarget_FindFriend` + 相对九宫格 —— 无武器时仍要挂假武器，本需求无武器，格子枚举更直接。

### 身份
- `chessName = 揭幕喵梦（祐天寺若麦）`
- `fetterMemberId = 祐天寺若麦`
- `plantTags` 含 `AveMujica`
- 无 `Weapon` / 无若麦被动/主动

## 主要新增
- `SkillEffect_UnveilNyamu.cs`（或 AveMujica_Skill 目录下等价名）
- 可选：`ColdSkill_FireOnEnter.cs`（若项目尚无通用「进态立刻放技能且不 Death」类）
- Prefab：`Assets/Prefab/ChessPrefab/AveMujica/揭幕喵梦/`
- 配置：`揭幕喵梦.asset`（或同名 Resources 路径）

## 数据流
```
种植 → EnterWarState=SkillState
     → PlaySkill + FireUseSkillOnEnter
     → SkillEffect：3×3 友军加压 + 产阳
     → 动画结束 Death 事件 → Chess.Death() 回收
```

## 风险
| 风险 | 等级 | 缓解 |
|------|------|------|
| 双 Death | 中 | 禁用 ConsumablesSkill.SkillOver Death |
| 漏自己 | 中 | 3×3 显式含中心 |
| 阳光/压力模板未配 | 低 | Prefab 检查清单 |

## 复杂度
L1～L2。
