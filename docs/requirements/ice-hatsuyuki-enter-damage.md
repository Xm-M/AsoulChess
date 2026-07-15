# 功能需求卡片: 初雪冰道踩入伤害

## 基本信息
- **功能名称**: 初雪冰道踩入伤害（Hatsuyuki Ice Enter Damage）
- **需求 ID**: ICE-HATSUYUKI-001
- **所属模块**: Effect（`Effect_Snow`）+ Map（`IceCell`）+ Skill（`PassiveSkillEffect_Hatsuyuki`）
- **需求类型**: 功能扩展
- **优先级**: P0
- **预估复杂度**: L1
- **预估耗时**: 2–4 小时
- **提出日期**: 2026-07-06
- **定稿日期**: 2026-07-06

## 功能描述

### 详细描述
在现有铺冰体系上，为**圣聆初雪**铺出的冰块增加**踩入一次伤害**：

- **仅初雪铺的冰**生效（冰车、开局雪关、雪怪等铺冰**无**此效果）。
- **与冰格 `ownerTag`（Player/Enemy）无关**；己方初雪与敌方初雪铺的冰均可带伤害。
- **伤害目标**为铺冰那只初雪的**敌对阵营**：`ChessTeamManage.GetEnemyTeam(hatsuyuki.tag)`。
- **触发时机**：单位**进入**该冰格时结算一次（非周期 DOT）；离开后再进入可再次受伤。
- **初雪死亡后**：该初雪铺的冰**不再伤人**（冰格视觉与挡板行为不变）。
- **多只初雪**：每格仅一个 `HatsuyukiSource`；每次进格**只结算一次**，不按初雪数量叠加。
- **伤害公式**：`铺冰初雪.propertyController.GetAttack() × iceEnterDamageCoeff`（系数配置在被动技能上）。
- **P0 不做减速**；不做 Boss 特殊免疫配置（走通用 `GetDamage`）。

**生效条件（须同时满足）**：

1. 该格 `IceCell` 带有有效的 `HatsuyukiSource`；
2. `HatsuyukiSource` 存活（`!IfDeath`）；
3. 受害者属于 `GetEnemyTeam(HatsuyukiSource.tag)`；
4. 受害者触发「进入该格」事件（见技术方案）。

### 用户故事
作为使用圣聆初雪的玩家，我希望敌人踩上初雪铺的冰时受到与初雪攻击力相关的伤害，且初雪离场后冰不再伤人，以便冰道兼具控场与输出，而不影响冰车等其他铺冰来源。

## 现有业务上下文

### 相关现有功能
| 功能 | 模块 | 关系 | 说明 |
|------|------|------|------|
| `Effect_Snow` | Effect | **扩展** | 全图冰字典、`PlaceOrRefreshIce` / `TryBfsPlaceFirstEmptyIce` |
| `IceCell` | Map | **扩展** | 单格冰、归属 tag、融化、挡种植 |
| `PassiveSkillEffect_Hatsuyuki` | Skill | **扩展** | 定时 BFS 铺冰；当前写死 `ownerTag = "Player"` |
| `PassiveSkillEffect_IceTrail` | Skill | 无影响 | 冰车铺冰，不应带初雪标记 |
| `GameStartPlugin_Snow` | LevelSystem | 无影响 | 开局敌方冰，无初雪来源 |
| `MoveController.OnReachTile` | Chess | **集成** | 移动单位进格事件 |
| `Tile.stander` | Map | **集成** | 原地被铺上冰时的补结算 |
| `Fetter.RainDamage` | Fetter | 相似 | 周期扫敌伤害（本需求为进格一次，非 DOT） |
| `PropEffect_Rake` + `WhenChessEnterWar` | Prop/Event | 相似 | 全场监听新棋子进场的模式参考 |

### 需求类型判定理由
- 铺冰、冰格管理已存在；本需求在 `IceCell` 标记来源并在 `Effect_Snow` 统一结算进格伤害，属于**功能扩展**。
- 不新增独立 MonoBehaviour 技能脚本（初雪被动已有 `HatsuyukiPassiveRuntime`，仅扩展调用）。

### 集成点
- **调用**: `Effect_Snow.PlaceOrRefreshIce` / `TryBfsPlaceFirstEmptyIce`、`TryGetIceCell`、`propertyController.GetAttack()`、`GetDamage`、`GetEnemyTeam`
- **事件**: `EventName.WhenChessEnterWar`（新棋子挂 `OnReachTile`）、`MoveController.OnReachTile`（移动进格）
- **新增接口（建议）**:
  - `Effect_Snow.RegisterHatsuyuki(Chess)` / `UnregisterHatsuyuki(Chess)`
  - `Effect_Snow.TryApplyHatsuyukiIceEnterDamage(Chess victim, Vector2Int mapPos)`
  - `IceCell.SetHatsuyukiSource(Chess)` / `ClearHatsuyukiSource()`
  - `PlaceOrRefreshIce(..., Chess hatsuyukiSource = null)` 可选参数

### 对现有功能的影响
- **接口变更**: `PlaceOrRefreshIce` / `TryBfsPlaceFirstEmptyIce` 增加可选 `hatsuyukiSource`；非初雪调用处**不传**，行为与现网一致。
- **行为变更**: 初雪铺冰时 `ownerTag` 改为 `_user.tag`（修复敌方初雪仍写 `Player` 的问题）；非初雪铺冰或覆盖时清除初雪来源标记。
- **数据变更**: `PassiveSkillEffect_Hatsuyuki` 增加 `iceEnterDamageCoeff`（建议默认占位如 `0.3`，Unity 内可调）。

## 技术要求

### 技术方案（定稿）

#### A. 监听注册（非「手动注册每只僵尸」）
由 **`Effect_Snow` 集中管理**，初雪被动只调 Register/Unregister：

1. **第一只初雪** `RegisterHatsuyuki` 时：
   - 订阅 `EventController.WhenChessEnterWar`；
   - 对场上已有全部棋子 `moveController.OnReachTile.AddListener(OnChessReachTile)`。
2. **新棋子进场**（`WhenChessEnterWar`）：`Effect_Snow` 为其挂同一 `OnReachTile` 监听。
3. **最后一只初雪** `UnregisterHatsuyuki` 时：移除 `WhenChessEnterWar` 订阅，并 `RemoveListener` 已挂的 `OnReachTile`（需记录已 hook 的棋子，避免泄漏）。
4. **关卡回收**（`ClearAllIce` / `WeatherManage.RecycleAll`）：确保拆除全部监听。

#### B. 进格伤害结算
`OnChessReachTile(walker, tile)` → `TryApplyHatsuyukiIceEnterDamage(walker, tile.mapPos)`：

```text
若 !TryGetIceCell(mapPos) → return
若 ice.HatsuyukiSource 为空或已死亡 → return
若 walker 不在 GetEnemyTeam(source.tag) → return
damage = source.GetAttack() × coeff（coeff 取自 source 身上 PassiveSkillEffect_Hatsuyuki 配置，或 Effect_Snow 缓存）
GetDamage(damageFrom=source, damageTo=walker)
```

#### C. 原地铺冰补刀
`PlaceOrRefreshIce` / `TryBfsPlaceFirstEmptyIce` 成功且 `hatsuyukiSource != null` 时：

- `ice.SetHatsuyukiSource(hatsuyukiSource)`；
- 若该格 `tile.stander != null`，调用 `TryApplyHatsuyukiIceEnterDamage(tile.stander, mapPos)`（覆盖/刷新寿命**不**重复伤害，仅新铺到该格或 `stander` 首次被标记时由铺冰路径触发一次）。

#### D. 初雪来源清除
以下情况 `ClearHatsuyukiSource()`：

- 非初雪调用 `PlaceOrRefreshIce`（无 `hatsuyukiSource`）覆盖该格；
- 冰车改归属 `SetOwnerAndLifetime` 且来源非初雪铺冰流程。

#### E. 配置
- `PassiveSkillEffect_Hatsuyuki.iceEnterDamageCoeff`（`[Min(0f)]`，序列化可调）。
- **无**全局 `GameStartPlugin` 开关；**无** Boss 豁免字段。

- **Unity 版本**: 与项目一致
- **依赖模块**: Effect、Map、Skill、Chess、Manage（`ChessTeamManage`）、Event
- **性能要求**: 进格 O(1)；`WhenChessEnterWar` 挂监听 O(棋子数)，与钉耙等插件同级
- **兼容性**: `hatsuyukiSource` 默认 `null`，旧调用方零改动
- **平台支持**: 全平台

## 功能清单

### 核心功能（必须）
- [ ] `IceCell` 增加 `HatsuyukiSource` 及 Set/Clear
- [ ] `Effect_Snow`：`RegisterHatsuyuki` / `UnregisterHatsuyuki`、进格结算、监听生命周期
- [ ] `PlaceOrRefreshIce` / `TryBfsPlaceFirstEmptyIce` 支持 `hatsuyukiSource`，铺冰后 `tile.stander` 补结算
- [ ] `PassiveSkillEffect_Hatsuyuki`：传 `_user.tag`、传 `_user` 为来源、Register/Unregister、`iceEnterDamageCoeff`
- [ ] 初雪死亡后对应冰格不再伤人
- [ ] 冰车/开局雪无伤害

### 扩展功能（可选）
- [ ] P1：踩入减速（`Buff_IceGroundSlow`）
- [ ] 植物种在已有初雪冰上的种植钩子补结算（若 `tile.stander` 铺冰路径未覆盖）

## 验收标准
- [ ] 己方初雪在场：僵尸**走进**初雪冰格 → 伤害 = 该初雪攻击力 × 系数，每进一格一次
- [ ] 初雪死亡：走进其铺的冰 → **不**受伤
- [ ] 两只初雪在场：同一格进格只伤 **1** 次，不按初雪数量乘倍
- [ ] 冰车/开局雪关冰面：无伤害
- [ ] 敌方初雪（若存在）：铺冰 `ownerTag` 正确；其冰伤害打在植物方
- [ ] 冰铺在站立单位脚下（BFS 新格）：该单位受到一次进格等价伤害
- [ ] 初雪全部离场：无 `OnReachTile` / `WhenChessEnterWar` 泄漏
- [ ] 火焰融化后该格不再伤人

## 修订（2026-07-06）：推车列铺冰与索敌

### 需求
初雪铺冰 BFS 与普攻格子索敌可覆盖 **小推车列**（`MapManage_PVZ.roomTile`）。

### 实现
- 逻辑列 `GridFindTargetGeometry.LawnMowerColumnMapX = -1`
- `Effect_Snow` BFS / 铺冰 / 进格伤害支持该列
- `IGridFindTarget` 相对格 `ax == -1` 时检测 `roomTile`

### 验收
- [ ] BFS 从战场铺到推车列（与第 0 列相邻时）
- [ ] 推车列有冰时僵尸 `OnReachTile` 触发进格伤害
- [ ] 解锁普攻后相对格 `(-1,0)` / `(-2,0)` 可索敌推车列敌人

## 修订（2026-07-06）：推车列有冰且有小推车 — 冰块占位

推车列铺冰且该格有存活小推车时，系统生成 **南瓜罩** 作为冰块视觉占位；冰融化时移除。详见 `docs/requirements/ice-mower-column-cover-plant.md`。

### 验收
- [ ] 推车列有冰 + 有推车 → 出现南瓜罩占位
- [ ] 冰融化 → 占位消失
- [ ] 无推车 → 不生成占位

| 风险点 | 概率 | 影响 | 应对措施 |
|--------|------|------|----------|
| `OnReachTile` 监听未拆除 | 中 | 高 | `Unregister` + `ClearAllIce` 双路径清理；记录 `_hookedChess` |
| 铺冰刷新重复扣血 | 中 | 中 | 仅新铺格或 `stander` 首次标记时补刀；`ResetLifetime` 不触发伤害 |
| 系数为 0 时无意义 | 低 | 低 | Inspector 默认非 0 占位 |
| 敌方初雪 `ownerTag` 仍为 Player | 高 | 中 | 本需求一并改为 `_user.tag` |

## 关联 Context
- 涉及模块: Map、Skill、Buff、Manage
- 参考文档: `context/modules/Map.md`、`context/modules/Skill.md`、`context/index.md`
- 依赖关系: `context/architecture/dependency-graph.md`
