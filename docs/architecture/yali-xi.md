# 架构设计: 压力希

## 系统定位
- **所属模块**: Skill（被动）+ Bullet（驻留 DoT）+ Chess（配置）
- **上游**: Map 邻格、`equipWeapon.OnAttack`、Buff 压力协议、ObjectPool/Timer
- **下游**: 邻格友方压力；MyGO Member 与立希同 `fetterMemberId`
- **横向**: 凑友希那（加压方向相反）、立希（仅身份对齐）

## 设计决策（推荐）

### 被动
`PassiveSkillEffect_YaliXi`（名可定）：
1. 定时或事件扫描四邻格友方
2. 对邻格友方 `OnAttack` 订阅；离格/离场成对卸监听
3. 回调里对该友方 `OkuwakiStressSpread.ApplyStressDelta(+1, guestTemplate)`

**备选**: 全局监听所有友军攻击再判邻格——实现简单但每击都扫全队，不推荐。

### 驻留弹
新建 `Bullet_LingerDot`（或等价）：
1. 飞行阶段：沿用 `IBulletMoveRight`（或直线）
2. 命中：立即 `TakeDamage` 一次 → 停移（`bulletMove` 切空停 / 自管 `_lingering`）
3. Timer 每 1s 对 `_lockTarget` 跳伤（ATK×rate）；目标无效则清空锁，等待下一 `OnTriggerEnter2D` 接锁
4. `lingerDuration` 到时 `RecycleBullet`；Init 时重置状态防对象池脏数据

**备选**: 命中后生成独立 AoE 区域实体——多一套 Prefab/生命周期，本需求「弹体停下」用 Bullet 自管更贴切。

### 身份
- `chessName = 压力希（椎名立希）`
- `fetterMemberId = 椎名立希`
- `plantTags` 含 `Mygo`
- **不挂** `PassiveSkill_MygoPassive_Taki` / 恐惧波

### 远程
Prefab：`Weapon_Sample` + `ShootBullet`（子弹指向驻留弹）+ `StraightFindTarget`

## 主要新增
- `PassiveSkillEffect_YaliXi.cs`
- `Bullet_LingerDot.cs`（或同目录新类）
- Prefab：压力希 + 驻留弹；配置 `压力希.asset`

## 风险
| 风险 | 等级 | 缓解 |
|------|------|------|
| 邻格订阅泄漏 | 中 | Dictionary 跟踪 + OnRemove/换格清理 |
| 池化子弹状态 | 中 | InitBullet 全量重置 |
| 与立希同场邻格计数 | 低 | 文档已知；身份 Member 仍只算 1 |

## 复杂度
L2。
