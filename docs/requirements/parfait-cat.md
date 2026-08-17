# 功能需求卡片: 芭菲猫（要乐奈动物形态）

## 基本信息
- **功能名称**: 芭菲猫
- **所属模块**: Skill / Chess（MyGO）
- **需求类型**: 新增功能（附带对 MatchaParfaitBuff 识别的小扩展）
- **优先级**: P1
- **预估复杂度**: L1
- **预估耗时**: 2–3 小时
- **提出日期**: 2026-08-11
- **需求 ID**: parfait-cat

## 功能描述
### 详细描述
芭菲猫是要乐奈的动物形态单位，关系对齐「天素罗 ↔ 长崎素世」：
- 独立可种植的 `MainPlant`
- `fetterMemberId = 要乐奈`（计入 MyGO 乐奈槽）
- **不变身**成完整要乐奈
- **常态不普攻**（关闭 `AttackAble`）

被动技能：
- 每间隔 **n 秒**（默认 **2s**，可配置）对自身所在格 **3×3** 范围内全部敌人施加 **减速 Buff（`ColdBuff`）**
- 单次施加的减速/冰冻持续 **5 秒**（可配置）
- 当自身持有 **抹茶芭菲（`MatchaParfaitBuff`）** 时，脉冲改为施加 **冰冻 Buff（`FreezyBuff`）**，直到抹茶芭菲 Buff 结束再恢复为减速
- 抹茶芭菲对芭菲猫：**仅**切换光环减速→冰冻，不做要乐奈的 `ReturnCD` 特判

本轮范围：被动逻辑 + Prefab / PropertyCreator 接线；不含卡池/关卡投放配置。

### 用户故事
作为玩家，我希望种植芭菲猫作为要乐奈的动物形态控场单位，常态周期性减速周围敌人；食用抹茶芭菲后短暂强化为冰冻光环，以便在不占用完整乐奈输出位的情况下提供控场。

## 现有业务上下文

### 相关现有功能
| 功能 | 模块 | 关系 | 说明 |
|------|------|------|------|
| 天素罗被动 | Skill | 相似 | 独立 MainPlant + fetterMemberId + 关普攻；本需求不变身 |
| 要乐奈 / MatchaParfaitBuff | Skill/Buff | 依赖 | 芭菲猫吃抹茶芭菲后光环升级；不扩展 ReturnCD |
| ColdBuff / FreezyBuff | Buff | 依赖 | 减速 / 真正冰冻 |
| IGridFindTarget / 九宫格工具 | Weapon/Skill | 复用 | 3×3 索敌 |
| 芭菲猫 Prefab/Asset | Resource | 接线 | 已有空壳，需挂被动与配置 |

### 需求类型判定理由
- 新棋子能力与独立被动，属新增功能
- 仅小改 `MatchaParfaitBuff` 识别逻辑（若需按 asset 名判断），不改变要乐奈既有行为

### 集成点
- **调用的现有接口**:
  - `TimerManage.AddTimer`（周期脉冲）
  - `IGridFindTarget` 或等价九宫格 Overlap 收集敌人
  - `BuffController.AddBuff`（ColdBuff / FreezyBuff）
  - `equipWeapon.AttackAble = false`
- **触发的现有事件**: 无新增全局事件；依赖棋子 `OnRemove` 清理计时器
- **需要的新接口**: `PassiveSkillEffect_ParfaitCat`（或同名 ISkillEffect）

### 对现有功能的影响
- **接口变更**: 无
- **行为变更**:
  - `MatchaParfaitBuff`：可选扩展识别「芭菲猫」仅用于文档一致性；本需求不要求 ReturnCD
  - 芭菲猫自身持有 `MatchaParfaitBuff` 时由被动检测 `buffDic` 切换脉冲类型
- **数据变更**: `芭菲猫.asset` / Prefab 技能引用与文案

## 已锁定决策
| # | 决策 | 选择 |
|---|------|------|
| 1 | 与要乐奈关系 | A：独立种植，`fetterMemberId=要乐奈`，不变身 |
| 2 | Buff 类型 | A：减速=`ColdBuff`，冰冻=`FreezyBuff` |
| 3 | 单次持续 | **5 秒** |
| 4 | 抹茶芭菲效果 | A：只切光环减速→冰冻 |
| 5 | 本轮范围 | A：被动 + Prefab/Asset 接线 |

## 技术要求
- **Unity版本**: 项目现行版本
- **依赖模块**: Skill, Buff, Chess, Map, TimerManage
- **性能要求**: 每 2s 对 3×3（最多 9 格）Overlap，体量与现有格子索敌同级
- **兼容性要求**: 向后兼容；不影响要乐奈 / 抹茶芭菲既有行为
- **平台支持**: 与现网一致

## 功能清单
### 核心功能（必须）
- [ ] 新增被动 `ISkillEffect`：周期 3×3 施加 ColdBuff / FreezyBuff
- [ ] 检测自身 `MatchaParfaitBuff` 切换脉冲类型
- [ ] 关闭普攻；死亡/离场清理 Timer
- [ ] Prefab 挂被动；`PropertyCreator`：`MainPlant`、`fetterMemberId=要乐奈`、Mygo 标签、基础数值
- [ ] 可配置：间隔 n（默认 2）、Buff 持续（默认 5）、Cold/Freezy 模板引用
- [ ] 每次脉冲在所在格生成可配置特效 `pulseEffect`（无敌也播；减速/冰冻共用）

### 扩展功能（可选，本轮不做）
- [ ] 卡池 / 关卡投放
- [ ] 定时变身要乐奈
- [ ] 抹茶芭菲对芭菲猫 ReturnCD

## 验收标准
- [ ] 种植芭菲猫后不普攻，约每 2s 对 3×3 敌人施加减速（持续约 5s）
- [ ] 对芭菲猫使用抹茶芭菲后，脉冲改为冰冻，直到芭菲 Buff 结束恢复减速
- [ ] 计入 MyGO 要乐奈槽（与天素罗计素世同理）
- [ ] 铲除/死亡无 Timer 泄漏
- [ ] 要乐奈吃抹茶芭菲行为不变

## 风险评估
| 风险点 | 概率 | 影响 | 应对措施 |
|-------|------|------|---------|
| FreezyBuff 过强（5s 周期冰冻） | 中 | 中 | 持续/间隔可配；验收时调参 |
| ColdBuff 与 FreezyBuff 命名易混 | 低 | 低 | 代码注释与 Inspector Label 标明 |
| Prefab 未配武器导致 AttackAble NRE | 中 | 低 | 判空再关攻击 |

## 关联 Context
- 涉及模块: Skill, Buff, Chess
- 参考文档: `context/modules/Skill.md`, `context/modules/Buff.md`, `context/modules/Chess.md`
- 依赖关系: `context/architecture/dependency-graph.md`
- 相似需求: `docs/requirements/tiansuluo.md`
