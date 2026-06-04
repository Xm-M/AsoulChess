# 功能需求卡片: 琴吹䌷双形态技能

## 基本信息
- **功能名称**: 琴吹䌷（放学后茶会）双形态切换
- **所属模块**: Skill / Weapon(IFindTarget) / Buff / HoukagoTeaTime
- **需求类型**: 功能扩展（新角色技能，复用平泽唯切换框架）
- **优先级**: P1
- **预估复杂度**: L2
- **提出日期**: 2026-05-20

## 功能描述

### 详细描述
琴吹䌷为 **双形态植物**，通过主动技能在两种形态间切换（架构对齐 `平泽唯` + `PassiveSkill_Yui` / `SkillEffect_YuiToggleHealMode` / `ColdSkill_YuiToggle`）。

| 形态 | 名称（策划） | 行为 |
|------|-------------|------|
| **形态 A** | 回复状态 | 寻敌：同一行、位于自身 **右方** 的所有友方；攻击方式：对每名目标各发射 **1 枚治疗弹**（建议 `ShootBullet_ShootAllTarget`） |
| **形态 B** | 坚毅形态 | **不攻击**（`AttackAble = false` + 空寻敌）；获得 Buff：**50% 额外减伤**（`extraDefence += 0.5`）+ **体型 +10**（`ChangeSize(10)`） |

切换主动可 **复用** `ColdSkill_YuiToggle`（进 `SkillState` 立即 `UseSkill`）；效果实现为 **䌷专用** `SkillEffect_TsumugiToggleMode` + **被动** `PassiveSkill_Tsumugi`（监听 `SkillContext.OnValueChange` 同步武器/Buff）。

### 用户故事
作为玩家，我希望琴吹䌷能在「行内治疗」与「坚毅承伤」之间切换，以便在后排支援队友或前排减伤扛线。

## 现有业务上下文

### 相关现有功能
| 功能 | 模块 | 关系 | 说明 |
|------|------|------|------|
| 平泽唯双形态 | HoukagoTeaTime | 相似（高） | context 开关 + 被动同步寻敌/子弹/动画 |
| `FindTarget_Self` | IFindTarget | 可对照 | 唯发呆仅自身 |
| `ShootBullet_ShootAllTarget` | Weapon | 依赖 | 多目标各一发子弹 |
| `Buff_BaseValueBuff_ExtraDefence` | Buff | 复用 | 坚毅 50% 减伤 |
| `Buff_BaseValueBuff_Size` | Buff | 复用 | 体型 +10 |
| `Buff_HoukagoTeaTime` | Fetter | 羁绊 | 种植 tag 后共享乐队 Buff，与形态无关 |
| 琴吹䌷 prefab | Prefab | 待改 | 当前误绑 `PassiveSkill_Yui`、空 `ColdSkill`，需替换 |

### 需求类型判定理由
在已有技能框架上新增角色配置与少量新类型（行内右方寻敌、坚毅复合 Buff），不是新系统。

### 集成点
- **调用**: `SkillController.context` Set/TryGet、`OnValueChange`
- **调用**: `Weapon_Sample.findTarget`、`ShootBullet`/`ShootBullet_ShootAllTarget`、`AttackController.AttackAble`
- **调用**: `buffController.AddBuff` / `TryOverBuff`（坚毅 Buff 随形态进出）
- **主动**: `ColdSkill_YuiToggle` + `SkillReady_MouseDown`（与唯同 SO 或复制一份䌷专用 SO）
- **事件**: `OnRemove` 解除 `OnValueChange` 监听

### 对现有功能的影响
- **接口变更**: 新增 `FindTarget_SameRowAlliesRight`（或等价命名），无破坏性修改
- **行为变更**: 无；仅䌷 prefab / ChessData
- **已知坑**: 唯的 `skill.anim` 含 `UseSkill` 事件，与 `ColdSkill_YuiToggle` 重复触发；**䌷 `skill.anim`/`skill2.anim` 应删除动画事件或勿双绑**

## 技术要求
- **Unity**: 项目现行版本
- **依赖模块**: Skill, Buff, Weapon, MapManage, ChessTeamManage
- **约束**: `ISkillEffect` + 事件，禁止技能专用运行时 MonoBehaviour
- **存档**: `tsumugiSturdyMode`（bool）建议写入 SkillContext 存档（非 `ShouldSkipKey`）

## 功能清单

### 核心功能（必须）
- [ ] `FindTarget_SameRowAlliesRight`：同行、`mapPos.x` 大于自身、友方 `Player`、存活、有 `stander`
- [ ] `PassiveSkill_Tsumugi`：默认回复态；监听 context 切换寻敌/攻击函数/Buff/AttackAble/动画变体
- [ ] `SkillEffect_TsumugiToggleMode`：翻转 `tsumugiSturdyMode`
- [ ] 坚毅复合 Buff（或组合现有 BaseValueBuff）：`extraDefence=0.5`，`size=10`；形态切回时 `BuffOver`
- [ ] 䌷 prefab：被动/主动/子弹/坚毅 Buff 模板绑定；移除错误 `PassiveSkill_Yui`
- [ ] 䌷技能动画：**无重复** `UseSkill`（仅 `ISkillFireUseSkillOnEnter` 或仅动画二选一）

### 扩展功能（可选）
- [ ] 回复态无右方队友时不普攻（空 targets）
- [ ] 坚毅态视觉：体型仅 `ChangeSize` 或叠加 `Buff_ChangeScale`（需策划确认）

## 验收标准
- [ ] 进战后默认回复态，右方同行队友各收到治疗弹（人数与目标数一致）
- [ ] 主动切换后进入坚毅：不攻击、减伤 50%、体型 +10；再切回回复态恢复攻击与 Buff 清除
- [ ] 切换只触发一次（无「切了又切回」）
- [ ] 离场/死亡无事件泄漏；读档形态与 Buff 一致
- [ ] 与 `Buff_HoukagoTeaTime` 同时存在无冲突

## 实现建议（开发参考）

```
SkillContext["tsumugiSturdyMode"] : bool
  false → healFindTarget + ShootBullet_ShootAllTarget + healBullet + AttackAble true
  true  → FindTarget_Empty + AttackAble false + Buff_TsumugiSturdy

主动: ColdSkill_YuiToggle → SkillEffect_TsumugiToggleMode
被动: PassiveSkill_Tsumugi（结构同 PassiveSkill_Yui）
```

## 待策划确认（❓）

1. **默认形态**：进战默认「回复」还是「坚毅」？（建议：回复）
2. **「右方」定义**：是否固定为 `mapPos.x` 更大（植物朝右）？是否包含自身？
3. **治疗目标**：仅 `tag==Player` 的 `tile.stander`？是否包含南瓜套等嵌套需单独规则？
4. **「一枚治疗子弹」**：对每名右方队友各 1 发（推荐）还是只发 1 发只打一个目标？
5. **体型 +10**：走 `propertyController.ChangeSize(10)` 还是只放大 `localScale`？
6. **主动 SO**：与唯共用 `平泽唯_发呆切换` 配置还是复制为 `琴吹䌷_形态切换`？

## 风险评估
| 风险点 | 概率 | 影响 | 应对 |
|--------|------|------|------|
| 动画双触发 UseSkill | 高 | 高 | 删 anim 事件，与唯 bug 同源 |
| 同行无目标仍进攻击 CD | 中 | 低 | 空 targets 时 `AttackAble` 或武器层跳过 |
| 坚毅 Buff 未卸 | 中 | 中 | 切形态时显式 TryOverBuff |
| 对象池复用脏数据 | 中 | 中 | OnRemove 清监听；与全局池排查一致 |

## 关联 Context
- `context/modules/Skill.md`
- `context/modules/Buff.md`
- `context/modules/Chess.md`
