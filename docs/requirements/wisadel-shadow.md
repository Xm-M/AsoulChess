# 功能需求卡片: 魂灵之影（Wisadel Shadow）

## 基本信息
- **功能名称**: 魂灵之影
- **所属模块**: Skill / Chess / Buff（维什戴尔子系统）
- **需求类型**: 功能扩展（维什戴尔召唤物落地配置 + 行为补全）
- **优先级**: P0（维什戴尔完整体验依赖）
- **预估复杂度**: L1
- **预估耗时**: 3–5 小时（Prefab/资产接线 + 索敌小改 + 联调）
- **提出日期**: 2026-06-25

## 功能描述

### 详细描述
魂灵之影是维什戴尔的**不可手动种植**召唤物，落点在主人 `IGridFindTarget` 攻击格内的空地上，最多 3 个。影不普攻，每 3 秒对攻击范围内**无残影标记**的敌人施加法术伤害、眩晕，并挂上 `Buff_WisadelMark`，为主人的「好礼」爆炸链提供标记来源。

**召唤时机**（已实现）：
| 时机 | 数量 | 入口 |
|------|------|------|
| 维什戴尔入场 | 1 | `PassiveSkillEffect_Wisadel` |
| 大招「爆裂黎明」 | 2 | `SkillEffect_WisadelBurst` |

**代码现状**：
- ✅ `WisadelShadowPlacer` — BFS 落点、上限 3、生命周期管理
- ✅ `PassiveSkillEffect_WisadelShadow` — 周期挂标逻辑（stub 可运行）
- ⚠️ Prefab / PropertyCreator **未完成接线**
- ⚠️ 维什戴尔 Prefab 的 `shadowCreator` **误指向虹夏.asset**，需改链

### 用户故事
作为玩家，我希望维什戴尔入场和大招能召唤魂灵之影，影会自动给敌人挂残影标记，以便配合维什戴尔的余震与爆炸打出连锁伤害。

## 现有业务上下文

### 相关现有功能
| 功能 | 模块 | 关系 | 说明 |
|------|------|------|------|
| `PassiveSkillEffect_Wisadel` | Skill | 主人 | 入场/离场召影与销毁 |
| `SkillEffect_WisadelBurst` | Skill | 主人 | 大招额外召 2 影 |
| `WisadelShadowPlacer` | Skill | 召唤 | 统一落点 BFS |
| `Buff_WisadelMark` | Buff | 共享 | 影与主人共用标记 |
| `WisadelGridHelper` | Skill | 共享 | 攻击格集合、OverlapBox 索敌 |
| `PropertyCreator` + `CreateChess` | Chess | 创建 | 与 M3、僵王召怪同路径 |
| `TimerManage` | Manage | 周期 | 影的 3s 心跳 |

### 需求类型判定理由
召唤逻辑与被动脚本已写好，本需求是**资产接线 + 行为细节确认 + 小量代码补全**，属于维什戴尔的功能扩展，非全新模块。

### 集成点
- **调用**: `ChessTeamManage.CreateChess`、`TimerManage.AddTimer`、`WisadelDamageHelper.DealDamage`、`Buff_WisadelMark.ApplyOrRefresh`
- **Context**: `WisadelKeys.Master`（影读主人）、`WisadelKeys.Shadows`（主人管列表）
- **事件**: 主人 `OnRemove` → `DestroyAllShadows`；影 `OnRemove` → 从列表移除

### 对现有功能的影响
- **接口变更**: 无
- **行为变更**: 仅维什戴尔与魂灵之影 Prefab/资产
- **数据变更**: 补全 `魂灵之影.asset` 字段；修正维什戴尔 `shadowCreator` 引用

## 魂灵之影设计规格

### 1. 棋子属性（`魂灵之影.asset`）

| 字段 | 推荐值 | 说明 |
|------|--------|------|
| `chessName` | 魂灵之影 | 显示名 |
| `Hp` / `HpMax` | **1000** | 用户指定；可被僵尸打死 |
| `AR` | **20** | 用户指定双抗 |
| `attack` | 0 | 无武器普攻 |
| `chessPre` | `魂灵之影.prefab` | **当前为空，必须补链** |
| `chessTileType` | `TileType.Land`（与维什戴尔一致 = 1） | 只能落在陆地格 |
| `plantType` | `MainPlant` | 占一格主植物位 |
| `plantFunction` | `MainPlant` | `!tile.stander && 地形匹配` |
| `plantTags` | `明日方舟`（可选） | 不参与羁绊计数时可留空 |
| `price` / 商店 | **不进商店** | 不设 `PlantCardPre`；勿加入 `allChess` 奖励池 |

### 2. Prefab 组件清单（`魂灵之影.prefab`）

| 组件 | 状态 | 待办 |
|------|------|------|
| `Chess` | ✅ 有 | `propertyController.creator` 已指向正确 asset |
| `equipWeapon` | ❌ weapon=null | 配置 `Weapon_Sample` interval=3 + 自定义 findTarget/attack |
| `passiveSkill` | ❌ null | 配置 `PassiveSkill` → `PassiveSkillEffect_WisadelShadow` |
| `activeSkill` | — | 不需要 |
| `stateGraph` | ❌ null | 接植物 Idle 状态图（参考维什戴尔/M3） |
| `animatorController` | ⚠️ 有组件无 Controller | 接 `魂灵之影.controller` 或 override |
| `moveController.tileMethod` | ❌ null | 接 `StandTile`（植物站立） |
| `BoxCollider2D` | ✅ | 尺寸按精灵调整 |

### 3. 周期行为 — **改为标准武器攻击循环**（用户确认 2026-06-25）

用户要求：**不走 Timer 心跳**，改用与普通植物相同的攻击状态机 + 攻击动画，`Weapon.interval = 3`。

```
Weapon_Sample:
  interval = 3
  findTarget = FindTarget_WisadelShadowUnmarked（新建）
  attackFunction = WisadelShadowCloseAttack（新建，或 CloseAttack + 动态伤害）

攻击流程:
  AttackState → findTarget → 播 attack 动画 → CloseAttack
  → 伤害 = 主人 ATK × magicRatio（0.5），DamageType.Magic，ElementType.AOE
  → 附带 DizznessBuff
  → OnAttack / 命中后挂 Buff_WisadelMark（主人为 owner）

PassiveSkillEffect_WisadelShadow 职责收缩为:
  - 入场校验 master 引用
  - 监听 equipWeapon.OnAttack → ApplyOrRefresh(mark)
  - 主人死亡时停止攻击（stateController 或 OnRemove）
```

**索敌**（`FindTarget_WisadelShadowUnmarked`）：
1. 从 `SkillContext[WisadelKeys.Master]` 取主人
2. 收集主人在 `IGridFindTarget` 攻击格内的敌人（OverlapBox，与 `WisadelGridHelper` 一致）
3. 过滤：无本主人的 `WisadelMark`
4. 取**距主人最近**的 1 名（用户确认）
5. 无目标 → 不攻击、不播动画

**首击时机**：由 `interval=3` + 正常 `AttackState` 节奏决定（入场后走标准攻击 CD，非 Timer 立即触发）。

### 3b. 棋子属性（用户确认数值）

| 字段 | 值 |
|------|-----|
| `Hp` / `HpMax` | **1000** |
| `AR` | **20**（双抗/物抗，按 Property 字段配置） |
| 可被僵尸击杀 | 是 |

### 4. 落点规则（`WisadelShadowPlacer`，已实现）

```
上限: 3 个 / 每名维什戴尔
起点: 主人 standTile
搜索: 四邻 BFS，仅进入 IGridFindTarget.relativeCells 绝对格
合法: shadowCreator.IfCanPlant(tile) && 列可种 && 格未被占
排序: 曼哈顿近 → y 小 → x 小
批量: 每落一个加入 occupied，下一个 BFS 跳过
满员: 静默跳过，不报错
```

### 5. 生命周期

```
创建: CreateChess → context[Master]=主人 → 加入主人 Shadows 列表
主人离场: DestroyAllShadows → 全部 Death
影死亡: OnRemove → 从 Shadows 列表移除
主人死亡: 现有 DestroyAllShadows 在 OnChessRemove；影侧 timer 检测 master.IfDeath 停止 tick
```

### 6. 维什戴尔 Prefab 修正

```
PassiveSkillEffect_Wisadel.shadowCreator  → 魂灵之影.asset (guid: 1d21b8000c8907548b033f2b16f8c68a)
SkillEffect_WisadelBurst.shadowCreator    → 同上
（当前错误指向虹夏.asset guid: 93e4d5ea...）
```

### 7. 维什戴尔「隐匿」— 四邻有影则无法选中（用户补充 2026-06-25）

明日方舟第二天赋「部署影」的简化版：**邻格有己方魂灵之影时隐匿，否则解除**。在本项目中映射为 `Chess.UnSelectable()` / `ResumeSelectable()`（与气球僵尸、土豆地雷武装等同机制）。

#### 触发条件

```
设 master = 维什戴尔，tileM = master.standTile
若 tileM 为空 → 不隐匿

四邻格 = MapManage.NearTile(tileM)（上下左右 4 格，不含对角）

隐匿 = ∃ 存活魂灵之影 shadow ∈ master.Context[Shadows]
         且 shadow.standTile ∈ 四邻格

隐匿中 → master.UnSelectable()（layer → Unselectable）
否则   → master.ResumeSelectable()（恢复 Player layer）
```

**只认本主人的影**：通过 `WisadelKeys.Shadows` 列表判定，不认其他维什戴尔的影，也不按名字匹配。

#### 刷新时机（事件驱动，不每帧轮询）

| 事件 | 调用 |
|------|------|
| 维什戴尔入场召影后 | `WisadelStealthHelper.Refresh(master)` |
| `WisadelShadowPlacer` 每次成功落影后 | 同上 |
| 魂灵之影死亡 / 离场（`OnShadowRemoved`） | 同上 |
| 维什戴尔离场 | `OnChessRemove` 销毁影；死亡时 Buff/Selectable 随棋子回收 |

#### 实现建议

```
Buff_WisadelCamouflage（新建，非计时 Buff）
  BuffEffect  → target.UnSelectable()
  BuffOver    → target.ResumeSelectable()

WisadelStealthHelper.Refresh(Chess master)
  shouldHide = HasAdjacentOwnedShadow(master)
  if shouldHide && !hasBuff → AddBuff(WisadelCamouflage)
  if !shouldHide && hasBuff → BuffOver()

PassiveSkillEffect_Wisadel 入场末尾 Refresh
WisadelShadowPlacer.TrySummon 成功落影后 Refresh
OnShadowRemoved 内 Refresh
```

#### 游戏体验说明

- **僵尸索敌**：直线路径 `GetEnemyLayer("Player")` 射线**打不到** `Unselectable` 层 → 隐匿时多数远程/直线僵尸无法以维什戴尔为目标（与气球僵尸同理）。
- **同格啃食**：若僵尸已站在维什戴尔格且走 `tile.stander` 啃食，需联调确认；若漏判可在 P1 对 `stander==master && master.IfSelectable` 做额外防护。
- **非真实伤害**：`Chess` 注释写明 Unselectable 仅承受 `DamageType.Real`；普通啃咬/子弹通常不造成伤害（与方舟迷彩一致）。
- **铲子**：无法选中隐匿中的维什戴尔（预期行为）。
- **维什戴尔自身攻击**：不受影响，仍可正常 ShootBullet。

#### 验收（隐匿）

- [ ] 维什戴尔四邻无影：可被僵尸索敌/选中（正常）
- [ ] 四邻至少 1 个己方魂灵之影：进入隐匿（`IfSelectable==true`、layer=Unselectable）
- [ ] 邻格影全部死亡后：自动解除隐匿
- [ ] 大招/入场召影后邻格有影：立即隐匿
- [ ] 多株维什戴尔：各自只统计自己的 `Shadows` 列表


## 代码补全清单（开发阶段）

### P0
- [ ] 补全 `魂灵之影.asset`（chessPre、plantFunction、chessTileType、HP1000/AR20）
- [ ] 补全 `魂灵之影.prefab`（Weapon interval=3、passiveSkill、stateGraph、animator、tileMethod）
- [ ] 新建 `FindTarget_WisadelShadowUnmarked` + `WisadelShadowCloseAttack`（或等价组合）
- [ ] 重构 `PassiveSkillEffect_WisadelShadow`：OnAttack 挂标，移除 Timer
- [ ] 新建 `Buff_WisadelCamouflage` + `WisadelStealthHelper`（四邻有影隐匿）
- [ ] 修正维什戴尔 Prefab 的 `shadowCreator` 引用
- [ ] 联调：入场 1 影、大招 2 影、满 3 不召、3s 攻击动画 + 挂标 + 隐匿开关

### P1
- [ ] 周期攻击时播放 `attack` 动画（`animatorController.PlayAttack` 或 Trigger）
- [ ] 挂标/命中特效 `ObjectPool`
- [ ] 召唤特效（维什戴尔侧 `summonEffect` 字段已预留）

### P2
- [ ] 魂灵之影独立 `attack.anim` + Animator 状态机
- [ ] 多株维什戴尔同场影视觉区分

## 验收标准
- [ ] 种下维什戴尔后，攻击格内 BFS 出现 1 个魂灵之影（合法格不足时静默失败）
- [ ] 大招在未满 3 影时额外召唤最多 2 个
- [ ] 影每 3s 对攻击范围内无标记敌人造成法术伤害 + 眩晕 + 挂 `WisadelMark`
- [ ] 影与主人标记互通，主人子弹可引爆影挂的标
- [ ] 影可被僵尸击杀；主人离场时全部销毁
- [ ] 魂灵之影不出现在商店/奖励植物池
- [ ] **隐匿**：四邻有己方影时无法选中；影消失后恢复

## 风险评估
| 风险点 | 概率 | 影响 | 应对措施 |
|--------|------|------|----------|
| PropertyCreator 未链 Prefab | 高 | 高 | P0 第一项 |
| shadowCreator 指错 asset | 高 | 高 | 已识别，改引用 |
| 攻击格内无空位 | 中 | 低 | 静默跳过，与需求一致 |
| 索敌 standTile 滞后 | 中 | 中 | OverlapBox 索敌 |
| 影占格阻挡玩家种植 | 低 | 中 | 落在攻击格空位，设计预期行为 |
| 隐匿与同格啃食 | 中 | 中 | 先靠 Unselectable 层；联调不足再补 stander 判断 |

## 关联 Context
- `context/modules/Skill.md` — ISkillEffect + Timer 规范
- `context/modules/Chess.md` — PropertyCreator / CreateChess
- `docs/requirements/wisadel-simplified.md` — 维什戴尔主需求
