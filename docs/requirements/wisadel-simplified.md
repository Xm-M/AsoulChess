# 功能需求卡片: 维什戴尔（精简：被动整合投掷手 + 三技能 + 魂灵之影）

## 基本信息
- **功能名称**: 维什戴尔（Wisadel）植物技能
- **所属模块**: Skill / Buff / Chess / Weapon
- **需求类型**: 新增功能（新角色）
- **优先级**: P1
- **预估复杂度**: L2
- **预估耗时**: 10–14 小时（含魂灵之影预制体与数值调试）
- **提出日期**: 2026-06-25

## 功能描述

### 详细描述
移植明日方舟维什戴尔，**仅保留第一天赋「好礼」+ 三技能「爆裂黎明」+ 魂灵之影召唤物**。  
**投掷手职业特性不单独做 Weapon 子类**，统一整合进被动 `PassiveSkillEffect_Wisadel`。

裁剪：一技能、二技能、第二天赋迷彩、爆裂锁定。  
**第二天赋「部署影」机制保留**：并入被动入场 + 统一召唤落点逻辑（无迷彩）。  
**大招**：恢复 **6 发弹药** 机制（非纯 8s 持续）；普攻为 **ShootBullet**；双套动画 **Blend 0/1** 切换（见下文）。

### 用户故事
作为玩家，我希望维什戴尔普攻附带小范围余震与标记爆炸，大招召唤魂灵之影并进入短时强化，以便在 PVZ 关卡中获得高爆发远程输出体验。

## 现有业务上下文

### 相关现有功能
| 功能 | 模块 | 关系 | 说明 |
|------|------|------|------|
| `PassiveSkill` + `onTakeDamage` 被动 | Skill | 相似 | `Passive_Mygo.OnAttackFear`（伤害后回调） |
| `ColdSkill` + `SkillFinish_Duration` | Skill | 依赖 | 三技能持续型，无需 `ByBulletNum` |
| `ISkillEffect_BigRabbit` | Skill | 相似 | 四邻格 + `IfCanPlant` 落点参考 |
| `Effect_Snow.TryBfsPlaceFirstEmptyIce` | Effect | 相似 | 八向 BFS 扩展参考 |
| `MapManage.NearTile` | Map | 依赖 | 四邻格遍历 |
| `DizznessBuff` | Buff | 依赖 | 爆炸 / 影法术眩晕 |
| `SkillEffect_RandomExplode` + `IFindTarget` | Skill | 相似 | 九宫格 AOE 伤害 |
| `ArkNights_Skill/SkillEffect_Mon3tr` | Skill | 参考 | 明日方舟角色移植先例 |

### 需求类型判定理由
全新植物角色，在现有 `SkillController`（`passiveSkill` + `activeSkill`）上扩展，**不修改**核心接口。

### 集成点
- **调用**: `propertyController.onTakeDamage`（Bullet 过滤）、`buffController.AddBuff`、`ChessTeamManage.CreateChess`、`TimerManage`、`ObjectPool`
- **触发**: 入场 `passiveSkill.UseSkill` 注册监听；大招 `ColdSkill` 手动释放
- **伤害元素**: 子弹 `ElementType.Bullet`；余震/爆炸 `ElementType.AOE`（防递归）
- **SkillContext 键**: `WisadelKeys.Shadows`（`List<Chess>`）、`WisadelKeys.BurstActive`（bool）、`WisadelKeys.ExplodeProcRate`（float）

### 对现有功能的影响
- **接口变更**: 无
- **行为变更**: 仅维什戴尔及魂灵之影 Prefab
- **数据变更**: 无

## 技能设计

### 武器层（ShootBullet + 双模式）
- `Weapon_Sample` + `ShootBullet` + **`IGridFindTarget`**（格子攻击范围，与圣聆初雪等同型）
- **主伤害由子弹结算**；投掷手余震 / 好礼在 **`onTakeDamage`（Bullet 命中后）** 追加
- 常态：`interval` = 普攻间隔，`bullet` = 普通弹，`Blend` = 0（idle / attack）
- 大招期间：**仅**换爆裂 `bullet`（**不改** `findTarget` / `interval` / 攻击方式），`Blend` = 1（skillidle / skillattack）
- 参考：`PassiveSkill_Yui`（`ChangeFloat` + 换 `shoot.bullet` + 换 `findTarget`）

### 被动「好礼 + 投掷手 + 入场影」（`PassiveSkillEffect_Wisadel`）

**伤害触发（重要）**：**不用 `equipWeapon.OnAttack`**（开火帧，子弹未命中）。  
在维什戴尔 `propertyController.onTakeDamage` 上监听，且仅处理：

```csharp
dm.damageFrom == user
&& (dm.damageElementType & ElementType.Bullet) != 0
```

维什戴尔普攻/爆裂弹须在命中前为 `DamageMessege` 打上 **`ElementType.Bullet`**（子弹 Prefab 的 `Dm` 默认值，或 `InitBullet` 后赋值；参考 `Bullet_HealSelf`）。

**余震 / 爆炸** 使用 **`ElementType.AOE`**（爆炸可加 `| ElementType.Explode`），**不会**再次进入 Bullet 分支，避免无限连触发。

**单次 Bullet 命中处理顺序**（`onTakeDamage` 回调内；此时直击伤害已由 `TakeDamage` → `GetDamage` 结算完毕）：

```
main = dm.damageTo

1. 好礼增伤（若 main 命中前已有本维什戴尔的 WisadelMark）
   → 追加一笔 15% ATK 伤害（ElementType.AOE，非 Bullet）

2. 投掷手余震
   → main + 九宫格其他敌人各 50% ATK（ElementType.AOE）

3. 好礼爆炸
   → 对本次余震命中的、带 WisadelMark 的单位独立判定
   → 15%（大招 Buff 期间 100%）九宫格 150% ATK + 眩晕（ElementType.AOE | Explode）

4. 挂标
   → 对 main 刷新 Buff_WisadelMark
```

**可选补充**：若希望 15% 增伤并入**同一发**子弹数字而非追加伤害，可在子弹 Prefab 挂 `IBulletEffect_Wisadel`（在 `TakeDamage` 前读标记改 `Dm.damage`）。默认方案用追加 AOE 伤害，实现更简单。

**弹药计数（大招）**：仍在 `onTakeDamage` 且 `ElementType.Bullet` 时 `ammo--`（每发**命中**扣 1，与方舟「打完后结束」一致；若需「开火即扣」再改）。

**入场**（`SkillEffect` 注册时）：

```
若当前魂灵之影数量 < 3
  → 调用 WisadelShadowPlacer.TrySummon(master, count: 1)
否则
  → 跳过（已满不再召唤）
```

监听 ~~`equipWeapon.OnAttack`~~ → **`propertyController.onTakeDamage`（仅 `ElementType.Bullet`）**：

```
设 main = 本次普攻主目标（weapon 索敌列表 [0]）

── 好礼：攻击前已有残影 ──
若 main 已有 Buff_WisadelMark（本维什戴尔施加）
  → 补伤害 15% ATK（等价好礼对主目标增伤）

── 投掷手：余震 ──
对 main 造成 50% ATK 物理伤害（第二段）
对 main 所在格九宫格内其他敌人各造成 50% ATK 物理伤害

── 好礼：爆炸 ──
对每个受到本次余震的单位：
  若其带有 WisadelMark
  → 判定爆炸（常态 15%；大招 Buff 期间 100%）
  → 成功：以该单位为中心九宫格 AOE，150% ATK 物理 + DizznessBuff 0.8s

── 好礼：挂标记 ──
对 main 刷新 Buff_WisadelMark（不可叠加，维什戴尔离场时清除其施加的全部标记）
```

**九宫格**：主目标格 + 周围 8 格有敌人的格子（与 PVZ 地图 `MapManage` 对齐）。

### 三技能「爆裂黎明」（弹药 + 武器/动画切换）

| 项 | 数值（可调） |
|----|-------------|
| CD | `baseCd ≈ 45s`，手动释放；**CD 在 6 发打完后再转**（见实现说明） |
| 弹药 | **6 发**；每发 **Bullet 命中**（`onTakeDamage`）扣 1；打完结束大招 |
| 召唤 | 开技能瞬间尝试召 **2** 影（满 3 不召） |
| 武器 | 切爆裂 `interval` + 爆裂 `bullet`（仍 `ShootBullet`） |
| 动画 | `animatorController.ChangeFloat(1)` → Blend Tree 播 skillidle / skillattack |
| 期间效果 | 攻击力 +80%；`ExplodeProcRate = 1`；索敌仍用常态 `IGridFindTarget` |
| 结束 | 第 6 发后：`ChangeFloat(0)`、恢复武器参数、移除 Burst Buff；影保留 |

#### 状态机与技能结束（两阶段）

```
阶段 A — 起手（SkillState，短）
  进入 SkillState → ISkillFireUseSkillOnEnter → UseSkill
    → 召影 + EnterBurstMode（Blend=1、换弹/间隔、ammo=6、加 Buff）
  IsSkillFinished（起手）= 起手动画播完（AnimFinish / 短状态名）
  → 退出 SkillState 回到 Attack/Idle（可继续普攻）

阶段 B — 连射（AttackState，6 发）
  每发 Bullet 命中：被动余震/好礼 + ammo--
  ammo == 0 → EndBurstMode（Blend=0、恢复武器）→ 主动调用 SkillOver 转 CD
```

**注意**：标准 `SkillState.Exit` 会调 `SkillOver`；`ColdSkill_WisadelBurst` 需 **重写 `SkillOver`**：若 `BurstActive` 仍为 true 则**不重置 CD**（仅结束起手阶段）。真正 CD 在 6 发打完后由被动/EndBurst 再调一次 `SkillOver`。

**不推荐**整段卡在 `SkillState` 里打完 6 发。`SkillFinish_WisadelBurstAmmo` 读 Context 弹药（Bullet 命中递减），不用 `AttackTimeFinish`（绑 `onUseSkill`）。

### 魂灵之影召唤落点（`WisadelShadowPlacer`）⭐ 新增

**统一入口**：被动入场、三技能召唤均调用同一静态/工具类，避免两套逻辑。

**前置条件**：

- 主人 `SkillContext[WisadelKeys.Shadows]` 中存活影数量 **≥ 3** → **直接返回，不召唤**
- 单次请求召唤 `count` 个时，实际召唤 `min(count, 3 - 当前数量)` 个

**落点搜索（四邻 BFS，限定在 `IGridFindTarget` 攻击格内）**：

```
起点 = 主人 standTile
收集主人武器上 IGridFindTarget.relativeCells → 绝对 mapPos 集合（与索敌一致）

四邻 BFS，仅进入上述攻击格：
  合法部署格 = shadowCreator.IfCanPlant && 列可种 && 未被占用

同层排序：曼哈顿近 → mapPos.y 小 → mapPos.x 小
```

**攻击范围内判定**（魂灵之影种植 / 影周期索敌）：

```
tile.mapPos（或敌人 standTile.mapPos）∈ IGridFindTarget 绝对攻击格集合
（不用 GetAttackRange() 世界距离）
```

同一次批量召唤（如大招 2 个）：每成功落子后，将该格加入「已占用」集合，下一次 BFS 跳过。

**创建后**：

- `ChessTeamManage.CreateChess(shadowCreator, tile, master.tag)`
- 写入影的 `SkillContext`：`master` 引用
- 加入主人 `WisadelKeys.Shadows` 列表

### 魂灵之影（独立 Chess，**保留**）

| 项 | 说明 |
|----|------|
| 创建 | 经 `WisadelShadowPlacer` 落子；`PropertyCreator` + `CreateChess` |
| 普攻 | **无**（`AttackAble = false` 或无 weapon） |
| 周期行为 | 每 **3s**：在主人攻击范围内找 **1** 名**无** `WisadelMark` 的敌人 |
| 效果 | 法术伤害（倍率配置）+ `DizznessBuff` + 施加 `WisadelMark` |
| 上限 | 每名维什戴尔最多 **3** 影，由主人 `SkillContext` 列表管理 |
| 生命周期 | 主人离场时全部 `Death`；影死亡时从列表移除 |

实现：`PassiveSkillEffect_WisadelShadow` 挂在魂灵之影 `passiveSkill` 上，入场读取 `SkillContext` 中的 `master` 引用。

## 技术要求
- **依赖模块**: Skill、Buff、Chess、Weapon、Manage（Timer）
- **架构约束**: `Assets/Script/Skill/` 禁止技能专用运行时 `AddComponent`；协程/Timer 用 `Chess` / `TimerManage`
- **性能**: 单次 Bullet 命中最多九宫格余震 + 1 次爆炸 AOE，可接受
- **兼容性**: 全新增脚本与 Prefab，零侵入现有角色

## 功能清单

### 核心功能（P0）
- [ ] `WisadelShadowPlacer` — 四邻 BFS 落点 + 上限 3 + 批量召唤
- [ ] `Buff_WisadelMark` — 残影标记，绑定 `ownerChess`
- [ ] 维什戴尔子弹 Prefab：`Dm.damageElementType = ElementType.Bullet`
- [ ] `PassiveSkillEffect_Wisadel` — 入场召 1 影 + `onTakeDamage`(Bullet) 余震/好礼
- [ ] `ColdSkill_WisadelBurst` — 起手 `SkillOver` 与满弹 `SkillOver` 分流
- [ ] `SkillEffect_WisadelBurst` — EnterBurstMode：召 2 影 + 换武器/Blend + ammo=6
- [ ] `SkillFinish_WisadelBurstAmmo` + Context 弹药（Bullet 命中递减）
- [ ] `WisadelBurstMode` — 保存/恢复 `interval`、`bullet`、`findTarget`（参考 Yui）
- [ ] `Buff_WisadelBurst` — 攻加成、必爆、索敌权重
- [ ] Animator：Blend Tree 0=idle/attack，1=skillidle/skillattack
- [ ] `PassiveSkillEffect_WisadelShadow` — 魂灵之影周期挂标记 + 法伤 + 眩晕
- [ ] `WisadelKeys` — SkillContext 键常量
- [ ] 维什戴尔 Prefab：`passiveSkill` + `activeSkill` 配置
- [ ] 魂灵之影 Prefab + `PropertyCreator`

### 扩展功能（P1）
- [ ] 爆炸 / 召唤 / 影射击特效（ObjectPool）
- [ ] 大招期间攻击范围扩大（改 `IFindTarget` 或 `GetAttackRange` Buff）

## 验收标准
- [ ] **入场**：维什戴尔种下后自动生成 1 个魂灵之影（合法四邻 BFS 落点）
- [ ] **落点**：先四邻再向外 BFS，不超出攻击范围；满 3 影不再召唤
- [ ] 普攻：主目标 100%（武器）+ 余震 50%（主目标 + 九宫格其他敌人）
- [ ] 被动：挂残影；再命中带残影目标有 15% 九宫格爆炸 + 眩晕
- [ ] 大招：召影 + Blend=1 + 6 发爆裂弹；打完 Blend=0；期间标记必爆
- [ ] 魂灵之影：周期对无标记敌人挂标记 + 眩晕
- [ ] 维什戴尔离场：标记清除、召唤物销毁
- [ ] 无一技能、二技能、迷彩

## 风险评估
| 风险点 | 概率 | 影响 | 应对措施 |
|--------|------|------|----------|
| OnAttack 过早 / 子弹未命中 | 高 | 中 | 改 `onTakeDamage` + `ElementType.Bullet` 过滤 |
| 余震/爆炸递归触发被动 | 中 | 高 | 余震/爆炸仅用 `ElementType.AOE`，不用 Bullet |
| 好礼增伤与挂标顺序 | 中 | 中 | 爆炸看命中前已有标记；本发结束后再挂标 |
| 魂灵之影与主人 SkillContext 断联 | 低 | 中 | 入场写入 master；主人 OnRemove 遍历销毁 |
| 多株维什戴尔同场 | 低 | 低 | Mark 绑定 owner，爆炸只响应本体的标记 |
| BFS 无合法格 | 中 | 低 | 静默跳过召唤，不报错 |
| 攻击范围为 `IGridFindTarget` 格子 | 低 | 低 | `WisadelGridHelper.TryCollectAttackCells` 与索敌共用几何 |

## 关联 Context
- `context/modules/Skill.md`
- `context/modules/Buff.md`
- `context/modules/Chess.md`
