# 架构设计: hanekawa-tsubasa-black-form

**日期**: 2026-08-17  
**需求**: `docs/requirements/hanekawa-tsubasa-black-form.md`

## 1. 系统定位
- **归属**: Skill（被动形态状态机）+ Buff（压力/吸血）+ Weapon（近战）+ Move（离格追杀）+ 常态 ColdSkill 产阳
- **模式**: `ISkillEffect` 被动驱动；**禁止**技能专用运行时 MonoBehaviour
- **上游**: `Buff_StressBuff_Death`、`MoveController.MoveToTarget`、`IGridFindTarget`、`SkillEffect_CreateSunLight`
- **下游**: 仅羽川翼 Prefab/Asset；不改全局关卡逻辑

## 2. 设计决策（推荐）

### 2.1 推荐方案：单一被动 `PassiveSkillEffect_Hanekawa`
入场：
1. 挂压力 Buff（模板可配，`stressLimit` 高，不靠自死阈值）
2. 常态：`equipWeapon.AttackAble = false`；产阳由 Prefab **ColdSkill + CreateSunLight** 负责
3. 订阅压力变化 / 轮询：`stress >= transformThreshold(90)` → `EnterBlackForm()`
4. 黑形态 Timer：每秒 `extraStress = -2` 经 `BuffReset` 降压（或项目既有减压 API）
5. `stress <= 0` → `ExitBlackForm()`（回原格）

**备选**：主动 SkillState 变身 — 已否决（Q2 自动）。

### 2.2 黑形态进出
```
EnterBlackForm:
  _homeTile = standTile
  _baseMaxHpAtTransform = GetHpMax()
  ChangeHpMax(+_baseMaxHpAtTransform * 5)  // 或等价 Property API
  Heal / ChangeHp(+同样数值)               // Q3 当前血同步抬
  吸血 +0.3（Buff 或 ChangeLifeSteeling）
  停产阳（暂停 ColdSkill / AttackAble 产阳侧）
  AttackAble = true
  预订 _homeTile（见 2.4）
  ChessLeave 当前格后出击（stander 预订保留）
  Animator → 黑形态
  启动索敌 Timer

ExitBlackForm:
  停索敌/停移动
  尝试 MoveToTarget(_homeTile) → 到达后：
    若无法入驻原格 → Death
    否则解除预订、ChessEnter、撤销 HP/吸血、AttackAble=false、恢复产阳、Animator 常态
```

### 2.3 索敌与近战
- Prefab：`Weapon_Sample` + `CloseAttack` + 可配 `IGridFindTarget`（大范围）
- 被动内索敌循环（可复用武器 findTarget）：
  - 取范围内敌人，按与自身距离排序，选最近
  - 无目标：待机（不移动）
  - 有目标：算邻格（与目标相邻且可站/空），`MoveToTarget`；到位后依赖普攻状态机近战
- 途经格：`MoveToTarget(Tile)` 只改 `standTile` 引用，**不**把途经格 `stander` 设为自己（避免占别人格）

### 2.4 原格预订（推荐）
种植判定：`MainPlant` → `!tile.stander`。

**推荐实现**（改动最小）：
1. 离格时 `ChessLeave` 会清 `stander`
2. 立即 `_homeTile.stander = user`（预订），**不**把 user 加回 `chessesIntile`
3. 回格成功：`stander` 已是自己则 `ChessEnter` 补齐列表；清理逻辑写进被动 `Cleanup`
4. 死亡/铲除：`OnRemove` 若 `_homeTile.stander == user` 则清空

**备选**：`TileEffect` 预订 + 改 `MainPlant.ifCanPlant` — 影响面更大，不首选。

### 2.5 无法回原格 → Death
到达 `_homeTile` 前/后判定「不可入驻」：
- `stander != null && stander != self`
- 或其他特殊封锁（地形/插件）
→ 调用 `user.Death()`，Cleanup 预订。

### 2.6 Prefab / Asset
| 项 | 配置 |
|----|------|
| Asset | 名/描述/price=150/攻速移速/Hp；chessPre |
| 常态 | ColdSkill + CreateSunLight；技能模版或产阳用状态图 |
| 黑形态 | 近战武器 + IGridFindTarget；被动 Hanekawa；Animator 双形态 |
| 压力阈值/降速/吸血/HP 倍率 | 被动序列化字段，默认 90 / 2 / 0.3 / 5 |

## 3. 数据流
```
外部加压 → Buff_Stress
    ↓ ≥90
EnterBlack → HP/吸血/近战/预订/降压Tick
    ↓ 索敌Timer
最近敌人 → MoveToTarget(邻格) → CloseAttack
    ↓ stress≤0
回 _homeTile → 成功退形态 / 失败 Death
```

## 4. 风险
| 风险 | 应对 |
|------|------|
| stander 预订与真实位置不一致 | 文档化；Cleanup 必清；回归种植/南瓜/铲除 |
| 降压与外部加压同帧 | BuffReset 累加语义已存在，按现有压力协议 |
| 变身中再达 90 | 黑形态内不重复 Enter；仅 Exit 后可再变 |
| 产阳 ColdSkill 黑形态仍转 CD | Enter 时显式暂停/屏蔽 skill ready |

## 5. 确认点（架构）
请确认是否采用：**单一被动驱动 + stander 预订原格 + 近战武器复用**。  
（默认按此方案进入影响面/审批；若要改预订为 TileEffect 请说明。）
