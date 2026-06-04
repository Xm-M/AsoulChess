# 功能需求卡片: 秋山澪双形态技能（切换技能效果）

## 基本信息
- **功能名称**: 秋山澪 — 常规产阳 / 溢出转阳 切换
- **所属模块**: Skill / HoukagoTeaTime / UI(SunLight)
- **需求类型**: 功能扩展（复用已有 Mio 溢出与产阳草稿）
- **优先级**: P1
- **预估复杂度**: L2
- **提出日期**: 2026-05-20

## 实现状态（2026-05-20）

- [x] `MultySkill_Mio` — `toggleSkill` / `sunNormalSkill` / `overflowHealSkill`；`skill` 0/1/2
- [x] `SkillEffect_MioToggleMode` — context `mioOverflowMode`
- [x] 常规产阳 — 子技能复用 `SkillEffect_CreateSunLight` + `SkillReady_MioNormalSunMode`
- [x] `SkillEffect_OverflowHeal` + `SkillReady_MioOverflowHeal`
- [ ] 秋山澪 prefab / 动画 / Animator `AnimtorController_Multy`（策划配置）

## 策划确认（待填）
- [ ] 切换技能 CD / 动画（建议：与唯相同，进场即放、无动画双触发）
- [ ] 模式 0 产阳间隔与数量（对齐向日葵 `SkillConfig` / `baseDamage[0]`）
- [ ] 模式 1 溢出阈值、每档阳光数、是否保留生命偷取系数
- [ ] 模式 1 溢出是否在满血治疗时也累计（当前 `PassiveSkill_AkiyamaMioOverflow` 逻辑）
- [ ] 默认模式：0（常规产阳）

## 功能描述

点击主动技能 **只负责切换形态**（不改武器），两种形态对应 **不同技能效果管线**：

| 模式 | 名称 | 效果 |
|------|------|------|
| **0** | 常规产阳 | 类似向日葵：按固定间隔生成阳光（`SkillEffect_CreateSunLight`） |
| **1** | 溢出转阳 | 被动累计溢出治疗；当缓存 **≥ 阈值** 时生成阳光并扣除对应溢出量 |

### 与唯/䌷的差异

| 角色 | 切换改变什么 |
|------|----------------|
| 唯 / 䌷 | 普攻寻敌、子弹、AttackAble、Buff |
| **澪** | **产阳逻辑**（被动 Timer + 溢出监听），主动仅翻转 `mioSkillMode` |

## 现有代码（可直接复用）

| 已有 | 路径 | 说明 |
|------|------|------|
| 溢出累计 | `PassiveSkill_AkiyamaMioOverflow` | `onSetDamage` 治疗溢出 → `akiOverflowHealBuffer` |
| 产阳+溢出结算 | `SkillEffect_AkiyamaMioSun` | 一次技能内 `baseSun + buffer×ratio×(1+偷取)`，**需拆分** |
| 向日葵产阳 | `SkillEffect_CreateSunLight` | `InitSunLight(tile, baseDamage[0])` |
| 切换进场 | `ColdSkill_YuiToggle` + `ISkillFireUseSkillOnEnter` | 避免动画 `UseSkill` 双触发 |
| Context 存档 | `SkillController.ShouldSkipKey` 已跳过 `akiOverflowHealBuffer` | 溢出缓冲不存档合理 |

澪 prefab 当前 **未配技能**；`虹夏.prefab` 误挂了 `PassiveSkill_AkiyamaMioOverflow`（配置迁移时注意）。

## 推荐实现方案（首选）

### 架构：主动「只切换」+ 被动「按模式产阳」

```
┌─────────────────────────────────────────────────────────┐
│ 主动 ColdSkill_YuiToggle + SkillEffect_MioToggleMode      │
│   → context.mioSkillMode ^= 1                           │
│   → OnValueChange → PassiveSkill_Mio.SyncMode()         │
└─────────────────────────────────────────────────────────┘
                          │
          ┌───────────────┴───────────────┐
          ▼                               ▼
   mode == 0                         mode == 1
   Passive: Timer                    Passive: onSetDamage
   每 interval 秒                    累计 overflow buffer
   CreateSunLight()                   每 tick / 每次溢出后：
                                     if buffer >= threshold
                                       产 1 格阳光，buffer -= threshold
```

**原因**（相对「一个 ColdSkill 里 dispatch 两种 SkillEffect」）：

1. `SkillController` 只有 **一个** `activeSkill`；若产阳也走主动 CD，会和「点击切换」抢同一入口，玩家体验混乱。
2. 向日葵产阳是 **周期行为**，用 `TimerManage` 放在被动里最贴近现有虹夏/向日葵心智，不占用切换动画。
3. 溢出监听本来就在被动（`PassiveSkill_AkiyamaMioOverflow`），模式 1 只需 **按模式开关监听 + 阈值发阳**，改动最小。
4. 与唯/䌷一致：**主动 = 切换**，**被动 = 维持形态副作用**。

### 建议新增/调整类

| 类 | 职责 |
|----|------|
| `SkillEffect_MioToggleMode` | `context["mioSkillMode"]` 0↔1（bool 或 int 均可） |
| `PassiveSkill_Mio` | 进战注册；`SyncMode()` 启停 Timer / 启停溢出监听；默认 mode=0 |
| （可选）`SkillEffect_MioOverflowThresholdSun` | 从 `SkillEffect_AkiyamaMioSun` 抽出：仅 `buffer >= threshold` 时产阳并扣 buffer |
| 复用 `SkillEffect_CreateSunLight` | 模式 0 Timer 回调内调用或内联同等逻辑 |

**不建议**用 `MultySkill`：它是「多个主动抢 CD 谁先 Ready」，不适合「一个按钮切换、另一个周期产阳」。

**不建议**切换时改 `activeSkill.effect` 引用：SerializeReference 运行时替换易丢配置，且与存档/技能 CD UI 耦合。

### 模式 1 阈值发阳（两种子方案）

| 方案 | 触发时机 | 优点 |
|------|----------|------|
| **A（推荐）** | `PassiveSkill_Mio` 用 0.2s Timer 检查 buffer | 与模式 0 同结构；溢出暴涨时一帧可多档（while buffer>=threshold） |
| B | 在 `OnSetDamage` 累加后立刻 while 检查 | 响应最快；需注意一帧多次治疗性能 |

阳光数量建议：

- **每达到一次 threshold 产 1 份阳光**（配置 `sunPerThreshold`），或
- 保留旧公式：`sun = round(buffer * ratio * (1+偷取))` 后 `buffer=0`（与现 `SkillEffect_AkiyamaMioSun` 一致，但去掉 baseSun）

策划需二选一；推荐 **阈值档位** 更直观（「溢出 100 转 25 阳光」）。

### 配置项（Inspector）

```text
PassiveSkill_Mio
  - sunInterval, sunAmount (模式0，或读 SkillConfig SO)
  - overflowThreshold, sunPerThreshold (模式1)
  - overflowHealToSunRatio, useLifeStealing (若保留旧公式)

SkillEffect_MioToggleMode
  - （无或仅默认 mode）

主动
  - ColdSkill_YuiToggle + 共用唯的 SkillConfig SO（CD 可极短/仅动画）
  - SkillReady_MouseDown
```

### 动画 / UI

- `animator.SetInteger("skill", mioSkillMode)` 区分两种形态外观（可选，与 `MultySkill` 同款参数名）。
- 技能 CD 圈：模式 0 显示产阳 Timer 进度；模式 1 可显示 buffer/threshold 比例（P2）。

## 备选方案（不推荐除非要坚持「产阳也走主动」）

**单 ColdSkill + `SkillEffect_MioDispatch`**

- 每次 CD 好：`mode0` 产阳；`mode1` 检查溢出。
- 切换形态需 **第二个输入** 或 **长按/双击**，因只有一个 activeSkill。
- 实现简单但和「点击切换形态」需求冲突。

## 验收标准

- [ ] 进战默认模式 0，按间隔产阳（与向日葵同量级）
- [ ] 点击主动仅切换模式，不误产阳、不双切换
- [ ] 模式 1：满血治疗产生溢出并累计；达到阈值产阳且扣减 buffer
- [ ] 切回模式 0 后停止溢出累计（或停止阈值发阳），Timer 产阳恢复
- [ ] 离场移除 Timer 与 `onSetDamage` 监听

## 关联 Context

- `context/modules/Skill.md`
- `docs/requirements/kotobuki-tsumugi-skills.md`（切换形态对照）
