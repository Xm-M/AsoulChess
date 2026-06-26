# 功能需求卡片: 碰撞伤害体（非 Chess 绑定）

## 基本信息
- **功能名称**: CollisionDamageEmitter（碰撞伤害发射体）
- **所属模块**: Map / Prop（建议 `Assets/Script/Map/` 或 `Assets/Script/Prop/`）
- **需求类型**: 新增功能（通用战斗组件，供道具/关卡/特效复用）
- **优先级**: P1
- **预估复杂度**: L1
- **预估耗时**: 0.5～1 人日（含闪电预制体接入示例）
- **提出日期**: 2026-06-11
- **状态**: 已实现（`Assets/Script/Map/CollisionDamageEmitter.cs`）

## 策划确认（2026-06-11）

| 项 | 决定 |
|----|------|
| 方案 | **A — `CollisionDamageEmitter`** |
| 闪电道具 | **不接入**（通用类供其他用途） |
| **移动（V1）** | **预制体用 Animator 驱动位移/表现**；`CollisionDamageEmitter` **不负责移动逻辑**，碰撞体随 Transform 运动即可 |
| **移动（后续 P2）** | 类似 `Bullet` + `IBulletMove` 的弹道式移动特效，**本期不做** |

## 功能描述

### 详细描述
提供类似 **小推车 `CarArmor` 碰撞碾压** 的体验，但 **不挂载在、不依赖 `Chess user`**：

- 预制体带 `Collider2D`（Trigger）
- 碰到敌方 `Chess` 时按 **`DamageMessege` 模板** 造成伤害
- 可选：命中特效、同一目标冷却、命中次数上限、存活时间后回池

用于：环境机关、道具生成碾压体、动画驱动的横扫特效等（**非**闪电）。

### 预制体结构（V1 推荐）

```
Root（Animator 播位移/横扫动画）
├── Visual（Sprite/粒子，可选）
└── HitArea（Collider2D Trigger + CollisionDamageEmitter）
```

- 碰撞器挂在会随动画移动的节点上（或与 Root 同节点）
- **无需** `Rigidbody2D` / 代码 `velocity`；动画改 `transform` 即带动碰撞检测

### 用户故事
作为策划/开发，我希望有一个「带碰撞体的伤害预制体」模板，以便道具和关卡不必伪造 Chess 也能碾压/灼烧敌人。

## 现有业务分析（Context）

### CarArmor 为何不能直接用

```33:44:g:\AsoulChess\AVZ\Assets\Script\Weapon\Armor\CarArmor.cs
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag(user.tag))
        {
            Chess chess =  collision.GetComponent<Chess>();
            dm.damageFrom = user;
            dm.damageTo = chess;
            dm.damage = user.propertyController.GetAttack();
            onHit?.Invoke( );
            user.propertyController.TakeDamage(dm);
```

- 继承 `ArmorBase`，**必须有 `Chess user`**
- 伤害数值来自 **`user.propertyController.GetAttack()`**
- 走 **`user.propertyController.TakeDamage(dm)`**（攻击方 Chess 出手）

小推车本身是 **`PropertyCreator` 创建的 Chess**（`EnterWarPlugin_CarCreate`），`CarArmor` 是棋子子物体上的防具/碾压碰撞器，**不是独立实体**。

### 现有最接近的「非 Chess 伤害体」

| 类型 | 脚本 | 碰撞伤害 | 绑定 Chess |
|------|------|----------|------------|
| 钉耙 | `TileEffect_Rake` | ✅ Trigger → `GetDamage` | ❌ |
| 酒渍 | `TileEffect_WineZone` | ❌ 仅 Buff | ❌ |
| 闪电（当前） | `PropEffect_Lightning` | ❌ 逻辑直接 `GetDamage` | ❌ |

**结论**：能做，且项目里已有 **`TileEffect_Rake` 先例**；但尚无 **通用、可配置 `DamageMessege`、可随物体移动** 的组件（钉耙绑格子且一次性秒杀）。

## 需求类型判定

**新增功能** — 新增通用 `MonoBehaviour`，不修改 `ArmorBase` / `CarArmor` 契约。

## 方案对比

**已选方案 A**（见下）。方案 B/C 不再推进。

### 方案 A：`CollisionDamageEmitter`

独立 `MonoBehaviour`，**不继承** `ArmorBase` / `TileEffect`。

| 配置项 | 说明 |
|--------|------|
| `damageMessage` | `DamageMessege` 模板（伤害、类型、元素、takeBuff 等） |
| `targetTag` | 默认 `"Enemy"` |
| `hitCooldownPerTarget` | 同一 Chess 再次受伤间隔 |
| `maxHitCount` | 总命中次数上限（0=不限） |
| `lifetime` | 存活秒数后 `ObjectPool.Recycle`（与动画时长对齐配置） |
| `onHit` | 可选 `UnityEvent` |

**不包含（V1）**：`Rigidbody2D`、匀速移动、`IBulletMove` 弹道。

触发逻辑对齐 `TileEffect_Rake`：`target.propertyController.GetDamage(mes)`，默认 `damageFrom=null`。

**优点**：与 CarArmor 用法接近（预制体 + 碰撞），不绑 Chess；道具/关卡/子弹特效都能挂。  
**缺点**：多一个新类型，需注册 ObjectPool。

### 方案 B：扩展 `TileEffect` 子类

例如 `TileEffect_CollisionDamage`：站在格子上或瞬发 Trigger。

**优点**：与地图格子体系一致。  
**缺点**：闪电/飞行物不一定对应单格；`TileEffect` 抽象类强制 `EnterTile` 等，不如 A 灵活。

### 方案 C：继续逻辑层 `GetDamage` + 纯 VFX 预制体

维持当前 `PropEffect_Lightning` 做法。

**优点**：零新类型。  
**缺点**：无法「碰撞体扫过一路碾压」，与 CarArmor 体验不一致。

## 集成点（方案 A）

- **伤害**：`Chess.propertyController.GetDamage(DamageMessege)`
- **生成**：`ObjectPool.instance.Create(prefab)`
- **离关**：监听 `WhenLeaveLevel` / `GameOver` 回池（与 `Bullet` 一致）
- **离关**：监听 `WhenLeaveLevel` / `GameOver` 回池（与 `Bullet` 一致）
- **移动**：由预制体 **Animator** 负责；组件只读当前 `transform` 位置做碰撞

## 范围划分

### V1（本期）
- [x] `CollisionDamageEmitter.cs`（伤害 + 冷却 + lifetime + 离关回收）
- [x] 文档：预制体结构、与 `CarArmor` / `TileEffect_Rake` 对照
- [ ] 示例 prefab 由使用方自行拼（Animator + Collider）

### P2（后续，本期不做）
- [ ] 弹道式环境伤害体（参考 `Bullet` + `IBulletMove`，或轻量 `IEnvDamageMove`）
- [ ] `Rigidbody2D` 匀速推车式移动体

## 功能清单（V1）

## 验收标准

- [ ] 预制体无 Chess，Animator 移动时碰撞体可连续命中敌人
- [ ] `lifetime` 与动画结束时间匹配时正常回池
- [ ] `DamageMessege` 在 Inspector 可配，伤害与类型生效
- [ ] 离关后不残留、不继续监听
- [ ] 与 `CarArmor` 小车共存无层/Tag 冲突（按 Layer 配置）

## 风险评估

| 风险 | 概率 | 影响 | 应对 |
|------|------|------|------|
| Trigger 连续多段伤害 | 中 | 中 | `hitCooldownPerTarget` / `maxHitCount` |
| Layer 与推车冲突 | 低 | 中 | 文档说明 Layer Mask |
| 与 Armor 承伤逻辑混淆 | 低 | 低 | 不走 `ArmorBase`，命名区分 |

## 关联文档

- `docs/requirements/prop-lightning-vfx-damage-template.md`
- `docs/requirements/prop-lightning.md`
