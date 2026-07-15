# 架构设计: 多首的怪物

## 系统定位
- **所属**: Skill（被动）+ Weapon（`IAttackFunction`）+ Chess 数据
- **上游**: 种植 LevelUp、外部压力源
- **下游**: 子弹池、ResumeState、分身视觉
- **不采用**: 维什戴尔实体 Shadow

## 推荐方案

### 数据流
```
外部压力 → Buff_Stress / context["stress"]
  → ≥50: stress=0, cloneCount++, SpawnHeadVisual()

武器 Attack 门控（本行有敌）
  → MultiHeadShootAttack
      → 发 0: 本行目标；起点 = 本体 weaponPos；直线 Move
      → 发 i: 行序[i] 索敌；起点 = cloneVisuals[i].position
               若目标行 == 本行 → 直线
               若目标行 != 本行 → DiagonalThenLane（斜入该行 Y 后再直线）

### 跨行弹道（新增，现有库无）
现有 `IBulletMoveRight` / `LineMove` / `Parabola` **都不**是「先斜入邻行再直行」。

新增例如 `BulletMove_DiagonalThenLane`：
1. **Init**：记录 `laneY`（目标行世界 Y，由 Attack 按 map 行写入 Bullet/context）
2. **阶段 A**：向航点 `(spawn.x + enterAheadX, laneY)` 斜向 `MoveTowards`
3. **到达后阶段 B**：沿 `transform.right`（或固定朝右）直线，等同三线射手入轨后的行为

分身只在本格左右偏移，**不**把子弹生成点挪到邻格；斜飞段负责换行。

致命伤
  → cloneCount>0: --clone, DestroyOneHead, HP=1, ResumeState
  → else: 正常死亡
```

### 行序算法
以 `standTile.mapPos.y` 为 home：
`[home, home-1, home+1, home-2, home+2, ...]`（跳过越界行）  
第 `i` 个分身使用 `order[i % order.Count]`。

### 关键类型
| 类型 | 职责 |
|------|------|
| `PassiveSkillEffect_MultiHeadMutsumi` | 压力、计数、视觉、复活 |
| `MultiHeadShootAttack` | 1+N 发行弹；分身弹起点读各头 Transform；跨行弹挂 DiagonalThenLane |
| `BulletMove_DiagonalThenLane` | **新增**两阶段弹道 |
| `MultiHeadMutsumiKeys` / helper | context 键、行序、生成头、头列表供 Attack 取位 |
| 分身头 Prefab | Sprite+Animator，无物理；作为发射锚点 |

### 发射点实现要点
- 被动把分身头实例列表写入 `SkillContext`（或 Attack 可访问的同一列表）。
- `MultiHeadShootAttack` **不能**只调现有 `ShootBullet`（其写死 `user.equipWeapon.weaponPos`）。
- 对分身：`InitBullet(user, head.position, target, dir)`；对本体：仍用 `weaponPos`。
- 跨行：子弹 Prefab 使用 `BulletMove_DiagonalThenLane`，Attack 在 Init 前写入目标行 `laneY`。
- 若某分身头已销毁但 count 未同步，跳过该发。

### 设计模式
- Strategy：`IAttackFunction` 替换射击
- Observer：压力 / `onGetDamage`
- Object Pool：子弹；头可用池或 Instantiate 子物体

## 风险
**中**（新攻击路径 + 视觉同步）；Mortis 回归需测。
