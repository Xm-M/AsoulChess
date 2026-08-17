# 架构设计: senjougahara-hitagi-stationery

**日期**: 2026-08-17  
**需求**: `docs/requirements/senjougahara-hitagi-stationery.md`

## 1. 系统定位
- **归属**: Weapon（随机连发）+ Bullet/Effect（四文具）+ PassiveSkill（压力同步）+ Property（负 Size）
- **模式**: `IAttackFunction` + `ISkillEffect` + 现有 `Bullet`/`Buff`；**禁止**技能专用运行时 MonoBehaviour

## 2. 设计决策

### 2.1 攻击：`HitagiStationeryAttack : IAttackFunction`
```
Attack(user, targets):
  n = 1 + floor(GetStress(user) / 20)
  user.StartCoroutine(FireSalvo(user, targets, n, interval))
FireSalvo:
  for i in 0..n-1:
    SpawnRandomStationery(user, targets)
    if i < n-1: yield WaitForSeconds(interval)
```
- 四 Prefab 列表等概率 `Random.Range(0,4)`
- `interval` 可配（建议默认 0.15～0.25s）
- 注意：若同帧多次 `Attack`（攻速极高）需防协程叠乘——用「攻击序号 / 停旧协程」或武器层已有 interval 保证

### 2.2 四文具
| 文具 | 实现要点 |
|------|----------|
| 铅笔 | 普通 `Bullet`，`Dm.damageElementType |= Puncture` |
| 尺子 | `MaxHitNum=3`，`Cutting` |
| 订书机 | `IBulletEffect` 或命中回调里 `AddBuff(DizznessBuff 0.5s)` |
| 橡皮擦 | `Buff_HitagiErase`：叠层；`layers>=5` → `target.Death()` |

子弹移动复用现有直线弹 `IBulletMove`（与豌豆等一致）。

### 2.3 被动：`PassiveSkillEffect_Hitagi`
入场：
1. 挂/确保压力 Buff（Tomo 式模板可配）
2. 订阅压力变化（监听 `Buff_StressBuff_Death.BuffReset` 后读 context stress，或统一 stress 事件若已有）
3. `SyncFromStress(user)`：
   - `tier = stress / 20`（整除）
   - `SetSizeAllowNegative(1 - tier)`
   - 缓存 `tier` 供攻击读取（或攻击时现场读 stress）

压力减少：同一 `SyncFromStress`，弹数与 Size 回升。

### 2.4 负 Size
**不改** `ChangeSize` 默认 `Max(..., 1)`（避免波及全单位）。

新增例如：
```csharp
public void SetSizeRaw(int size) { Data.Size = size; } // 无下限
```
仅黑仪被动调用。

### 2.5 Prefab / Asset
- `战场原黑仪.asset`：名称、描述、attack/射程/CD、chessPre、Size 初始 1
- Prefab：`Weapon_Sample` + `HitagiStationeryAttack` + `StraightFindTarget`；被动挂上；`stateGraph` = **攻击模板**（无主动）
- 四文具子弹 Prefab（可先单色占位区分）

## 3. 影响面摘要
| 类型 | 范围 |
|------|------|
| 新增 | Attack、Passive、EraseBuff、可选 BulletEffect、4 弹 Prefab |
| 修改 | `PropertyController.SetSizeRaw`（或等价）、黑仪 Asset/Prefab |
| 回归 | Size 碾压、压力植物、远程弹池 |

## 4. 风险
- 高压连发拉长攻击窗口 → interval / 可选 maxTier
- 橡皮擦斩杀 vs 不死抗性 → 走 `Death()` 与项目一致
- 协程在离场未停 → `OnRemove` StopCoroutine
