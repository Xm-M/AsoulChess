# 功能需求卡片: 南瓜罩（Support 承伤植物）

## 基本信息
- **功能名称**: 南瓜罩类 Support 植物 — 同格主力承伤
- **所属模块**: 植物种植（`IPlantFunction` / `Tile`）+ 被动技能（`ISkillEffect`）+ 伤害管线（`PropertyController`）
- **需求类型**: 功能扩展
- **优先级**: P1
- **预估复杂度**: L2
- **预估耗时**: 8–16 小时（含预制体、数值与边界测试）
- **提出日期**: 2026-03-30

## 功能描述

### 详细描述
新增一株 **Support 类型**植物（类比 PVZ 南瓜罩）：与**同一格**上的 **Main 主力植物**（代码中即 `Tile.stander`，`PlantType.MainPlant`）共存。

**被动**：监听**当前格 Main 植物**的受伤事件；当该 Main 即将/实际受到伤害时，将伤害**转移到南瓜罩自身**（由南瓜罩结算 HP / 飘字 / 动画等，Main 本体不扣这次伤害）。

### 用户故事
作为玩家，我希望在已有主力植物上再套一层南瓜罩，让子弹打在主力上的伤害由罩子承担，以便保护高价值 Main。

## 术语对照（需求 ↔ 工程）

| 需求用语 | 工程实现 |
|----------|----------|
| MainType 主力 | `PlantType.MainPlant`；格上主植物引用为 `Tile.stander`（`Tile.PlantChess` 时仅 Main 会写入 `stander`） |
| Support | `PlantType.SupportPlant`；`SupportPlant.ifCanPlant` 允许与同格其他植物共存（同 `plantType` 不可重复） |
| 监听受伤 | **推荐** `PropertyController.onSetDamage`（在 `GetDamage` 扣血**之前**），见下节 |

## 现有业务上下文

### 相关现有功能
| 功能 | 说明 |
|------|------|
| `Tile.stander` / `chessesIntile` | Main 仅占 `stander`；Support 在 `chessesIntile` 中 |
| `SupportPlant` | 同格可叠，禁止同 `plantType` 重复 |
| `onSetDamage` / `onGetDamage` | `onSetDamage` 先于减血与 `onGetDamage`（见 `Property.GetDamage`） |
| 护甲类 | `ArmorBase` 等用 `onSetDamage` 改写 `DamageMessege` — 承伤转移宜同类思路 |

### 集成点（实现草案）

1. **种植规则**  
   - 使用现有 **`SupportPlant`**，或新增 **`IPlantFunction`** 子类：要求 **`tile.stander != null`** 且 `stander` 为 Main（`plantType == MainPlant`），避免空地上单种南瓜（若策划需要「仅套娃」）。

2. **订阅目标**  
   - `WhenEnterGame`（或 `standTile` 稳定后）：取 `user.moveController.standTile.stander` 作为 **protectee**；若与自身相同或 null 则不解锁技能效果。

3. **伤害转移（推荐 `onSetDamage`）**  
   - 在 protectee 的 `onSetDamage` 上监听；若 `mes.damageType` 为治疗/未命中等需与策划对齐是否转移。  
   - 对**应转移到南瓜**的伤害：  
     - 克隆或复制 `DamageMessege`，将 **`damageTo` 改为南瓜**，调用 **`pumpkin.propertyController.GetDamage(mesCopy)`**（或经 `TakeDamage` 链路，注意与暴击/吸血来源一致）。  
     - 将 **原 `mes.damage` 置 0**（或对 Main 跳过扣血），避免 Main 仍吃伤害。  
   - **注意**：若在 `onSetDamage` 早段执行，后续仍走 Main 的 AR/闪避 — 需约定是「整段伤害搬走」还是「仅最终值」；通常应 **尽早把 Main 侧伤害清零** 并只在南瓜上跑完整 `GetDamage`。

4. **防递归**  
   - 南瓜自身 `GetDamage` 不得再次触发「替 Main 挡刀」逻辑（标志位或判断 `damageTo`）。

5. **生命周期**  
   - Main 死亡 / 离场 / `stander` 变更：移除监听，必要时重新绑定新 Main。  
   - 南瓜死亡 / `OnRemove`：`RemoveListener`，避免泄漏。

## 技术要求
- **依赖**: `Tile`、`PropertyController`、`PassiveSkill` + 运行时组件（推荐 MonoBehaviour 被动运行时类，便于 `OnDestroy`）
- **性能**: 每株南瓜 1 个 listener；禁止在 `onSetDamage` 内高频分配（可对象池/结构体策略由实现定）

## 功能清单

### 核心（必须）
- [ ] Support 植物；与同格 Main 的种植规则与策划一致
- [ ] Main 受伤害 → 南瓜承担（Main 本次不扣血或扣 0）
- [ ] 南瓜死后 Main 恢复正常受击
- [ ] Main 死后南瓜取消监听或失效

### 待澄清
- [ ] **是否允许**无 Main 时种植南瓜？
- [ ] **治疗**、**真实伤害**、**Miss** 是否转移？
- [ ] 南瓜是否参与 **Buff 附着**（如中毒仍上在 Main）— 通常仅转移 `damage` 数值，不转移 `takeBuff`，需策划确认

## 验收标准
- [ ] 同格 Main + 南瓜：子弹打 Main 判定下，仅南瓜掉血（或飘字在南瓜）
- [ ] 仅南瓜死：Main 随后可正常受伤
- [ ] 仅 Main 死：南瓜不崩溃、监听已移除
- [ ] 无重复结算、无死循环递归

## 风险评估
| 风险 | 应对 |
|------|------|
| `onGetDamage` 监听已晚于扣血 | 必须用 `onSetDamage` 或等价前置钩子 |
| 与现有护甲/分担 Buff 顺序 | 明确 listener 执行顺序（UnityEvent 注册顺序）或统一用单一承伤入口 |

## 关联 Context
- `workflow_v2/context/index.md`
- 模块: Chess / Property / Map Tile

## 为何不用纯 `onGetDamage`
`onGetDamage` 在 **HP 已扣除之后** 触发（`Property.GetDamage`）；若仅监听它再「治疗 Main + 伤害南瓜」，浮点、暴击显示、亡语时机易不一致。**需求实现应以前置链路（`onSetDamage`）为主**。
