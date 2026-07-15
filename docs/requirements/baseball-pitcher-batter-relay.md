# 功能需求卡片：棒球投手 × 挥棒手站位连招

## 基本信息

- **需求 ID**：BB-001
- **功能名称**：Baseball Pitcher–Batter Relay（投手喂球 → 挥棒手临时远程）
- **所属模块**：Skill / Weapon / Chess
- **需求类型**：功能扩展
- **优先级**：P1
- **预估复杂度**：L2
- **预估耗时**：1～2 天（含两个 plant Prefab 接线）
- **提出日期**：2026-07-07
- **最后修订**：2026-07-07（用户确认 A/B/C/D + 独立友方索敌）

## 功能描述

### 详细描述

实现两个 plant 被动技能：

| 角色 | 独立行为 | 站位连招 |
|------|----------|----------|
| **棒球投手** | 远程投掷攻击敌人 | 若攻击范围内有挥棒手 **且** 自身索敌范围内有敌人 → 喂球给挥棒手 |
| **棒球挥棒手** | 近战挥棒攻击敌人 | 接到喂球 → **仅本次攻击**切远程出球 → 打完回近战 |

**已定规则（用户 2026-07-07）**

1. 远程切换**只在接到球的那次攻击**；攻击结束后恢复近战。
2. **无敌人不喂球**：以投手**自身**正常敌人索敌为准（`FindEnemy`  enemy 分支 = 0 → 不攻击、不喂球）。
3. **所有后方投手**均可喂球；挥棒手处于 **AttackState** 时忽略新喂球（投手仍可按 CD 发出，球丢弃）。
4. 喂球 **零伤害**（`DamageType.Miss` 或 damage=0，不挂 Buff）。
5. 条件满足时投手 **持续喂球**（每次普攻 CD 优先喂挥棒手）。
6. 站位连招与 **羁绊/Fetter 无关**。
7. 挥棒手须在投手 **攻击范围** 内（非固定正前方 1 格）；**须用独立友方检测**，不能复用 `StraightFindTarget`（只扫敌层）。
8. 多颗喂球：攻击中丢弃；攻击结束后 **只响应下一颗** 到达的喂球（D1，不排队连打）。

**布局示意**（plant 朝右对僵尸）：

```text
[投手] … [挥棒手] … [敌人]
  ↑ 后方      ↑ 可在投手攻击范围任意前向格
```

### 用户故事

作为玩家，我希望投手和挥棒手相邻排布时能形成「喂球 → 击球」连招，以便用站位替代硬凑羁绊获得额外输出节奏。

作为策划，我希望两个 plant 单独可用，仅在前向攻击范围内形成联动，且不改动通用 `Bullet` 对敌逻辑。

## 现有业务上下文

### 相关现有功能

| 功能 | 模块 | 关系 | 说明 |
|------|------|------|------|
| `Weapon_Sample` + `IFindTarget` | Weapon | 依赖 | 投/挥默认武器 |
| `ShootBullet` / `CloseAttack` | Weapon | 依赖 | 远程 / 近战 |
| `GridFindTargetGeometry` | Weapon | 复用 | 格子几何、forwardX |
| `WisadelBurstMode` | Skill | 相似 | 临时换弹 / 动画层 |
| `PassiveSkillEffect_MinerZombie` | Skill | 相似 | 无 Mono 被动写法 |
| `Bullet.OnTriggerEnter2D` | bullet | 约束 | 同 tag 不命中，需专用喂球弹 |
| `Fetter` / `plantTags` | Fetter | 无关 | 连招不读羁绊 |

### 需求类型判定理由

在现有 `ISkillEffect` + 武器索敌上扩展两个被动；不新增全局 Manager，不修改 `AttackController` 核心逻辑。

### 集成点

- **调用**：`Weapon_Sample.FindEnemy` / `TakeDamage`；`stateController.ChangeState(AttackState)`；`GridFindTargetGeometry`；`ObjectPool` 喂球弹
- **触发**：`WhenPlantChess` / `OnRemove` 刷新范围；`propertyController.onGetDamage`（挥棒手接球）
- **新接口**：`BaseballRelayHelper`、`FindTarget_BaseballPitcherRelay`、`FindBatterInRelayRange`（友方层）

### 对现有功能的影响

- **接口变更**：无公共 API 破坏；可选扩展 `ElementType.PitchBall` 或用 `SkillContext` 标记喂球
- **行为变更**：仅挂载对应被动的 plant 受影响
- **数据变更**：`plantTags` 增加 `棒球投手` / `棒球挥棒手`

## 技术要求

- **Unity版本**：与项目一致
- **依赖模块**：Skill、Weapon、Chess、ObjectPool
- **性能要求**：与现有 plant 普攻同量级；友方扫描仅在 `WhenPlantChess`/索敌时，无每帧全图扫描
- **兼容性要求**：向后兼容；未挂被动的 plant 行为不变
- **实现约束**：`Assets/Script/Skill/` 禁止随意 `AddComponent` 技能专用 MonoBehaviour（见 `.cursor/rules/unity-skill-no-runtime-monobehaviour.mdc`）

## 架构设计（摘要）

### 双通道索敌

```text
投手每次 FindEnemy / 普攻前：

  [1] 敌人门控（B1）
      weapon.findTarget（Straight / Grid / …）→ 敌人数 == 0 ?
      → 是：不攻击、不喂球、结束

  [2] 挥棒手检测（独立友方通道，解决 StraightFindTarget 扫不到友军）
      FindBatterInRelayRange(pitcher):
        - 若 weapon.findTarget is IGridFindTarget → 复用 relativeCells，
          OverlapBox 友方层，筛 plantTags 含「棒球挥棒手」
        - 否则 → 沿 transform.right、距离 GetAttackRange() 的
          RaycastAll / BoxCast 友方层（仅前向，不含身后）
        - 多个挥棒手 → 取沿 forward 最近者

  [3] 合并 FindTarget_BaseballPitcherRelay
      敌人门控通过 && 找到挥棒手 → targets = [挥棒手]
      敌人门控通过 && 无挥棒手   → 走原 enemy findTarget
```

### 喂球与接球

- **喂球弹** `Bullet_PitchBall`（或 `PitchToAllyAttack`）：可命中同 tag；零伤害；带 Pitch 标记。
- **挥棒手** `PassiveSkillEffect_BaseballBatter`：`onGetDamage` 识别 Pitch + `damageFrom` 为投手友方。
  - 若 `AttackState` → 丢弃（C1）。
  - 否则：保存近战 `findTarget`+`attackFunction` → 临时换 `ShootBullet` → `ChangeState(AttackState)` → `SkillOver`/攻击结束回调恢复近战（D1：无队列）。

### 角色识别

- `plantTags`：`棒球投手`、`棒球挥棒手`（默认；可 SO 覆盖）。

### 击球伤害

- 远程一击 = 挥棒手当前攻击力 × `battedDamageRate`（默认 1，SO 可配）。

## 功能清单

### 核心功能（必须）

- [ ] `BaseballRelayHelper`（友方范围、标签、敌人门控）
- [ ] `FindBatterInRelayRange` / `FindTarget_BaseballPitcherRelay`
- [ ] `PassiveSkillEffect_BaseballPitcher`
- [ ] `PassiveSkillEffect_BaseballBatter`（临时远程 + 恢复近战）
- [ ] `Bullet_PitchBall` 或等价零伤害喂球通道
- [ ] 两个 plant Prefab + PropertyCreator 接线

### 扩展功能（可选）

- [ ] 喂球 / 击球专用 VFX、音效
- [ ] Editor Gizmo 显示 relay 范围

## 验收标准

- [ ] 仅投手：正常远程打敌；无敌人时不攻击
- [ ] 仅挥棒手：正常近战打敌
- [ ] 投手攻击范围内有挥棒手 + 投手索敌有敌：投手喂球，挥棒手本次远程出球，随后回近战
- [ ] 无敌人：投手不喂球、不攻击
- [ ] 挥棒手 AttackState 中：新喂球不触发连招
- [ ] 多投手：均可喂同一挥棒手；不排队连打（D1）
- [ ] 喂球不扣挥棒手血量
- [ ] 羁绊层数变化不影响上述行为
- [ ] 未挂被动的 plant 回归不受影响

## 风险评估

| 风险点 | 概率 | 影响 | 应对措施 |
|--------|------|------|----------|
| StraightFindTarget 投手无格子配置 | 中 | 中 | 友方通道 fallback 射线/BoxCast 友方层 |
| 强制 Attack 与状态机冲突 | 中 | 中 | 仅 Idle/非 Skill 切入；攻击中丢弃喂球 |
| 临时换武器未恢复 | 低 | 高 | `AttackState.Exit` / `onSkillOver` 对称恢复 |
| 友方误伤其他 plant | 低 | 中 | 标签过滤 + 仅 PitchBall 可命中友方 |

## 关联 Context

- 涉及模块：Skill、Chess
- 参考文档：`context/modules/Skill.md`、`context/modules/Chess.md`
- 依赖关系：`context/architecture/decisions.md`（ISkillEffect 规范）
