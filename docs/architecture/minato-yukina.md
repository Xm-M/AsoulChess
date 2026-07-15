# 架构设计: 凑友希那

## 系统定位
- **所属模块**: Skill（被动）+ Chess（棋子配置）+ Buff（压力协议复用）
- **上游依赖**: Map（邻格）、Weapon（`OnAttack`）、BuffController
- **下游影响**: 邻格友方压力数值 / 紫砂阈值行为（间接）
- **横向关联**: GBC/老仓育压力体系（同名「压力」Buff）

## 设计决策（推荐）

**模式**: `ISkillEffect` 被动 + `equipWeapon.OnAttack` 事件（对齐棒球手）

**压力施加**（对齐 Tomo / 老仓育）:
1. 邻格友方无「压力」→ `AddBuff(guestTemplate.Clone())`
2. 再（或仅对已有）`BuffReset(new Buff_StressBuff_Death { extraStress = N })`  
   → **保证每次攻击含首次都 +N**（仅 `AddBuff` 首次只会 `BuffEffect` 把 stress 置 0，不会 +extraStress）

**邻格**: 四向 `(±1,0)/(0,±1)`；可在 `OkuwakiGridHelper` 增 `CollectNeighbor4Tiles`，或被动内局部实现（L1 推荐局部，避免无关耦合）。

**射击**: Prefab 配 `Weapon_Sample` + `ShootBullet` + `StraightFindTarget`/`IGridFindTarget`；子弹可先复用现有资源。

### 备选
| 方案 | 优点 | 缺点 |
|------|------|------|
| A. 推荐：被动 OnAttack + Ensure/BuffReset | 与现有压力协议一致 | — |
| B. 改 ShootBullet 内嵌副作用 | 少一个被动类 | 污染通用攻击函数，不推荐 |
| C. 子弹 hit 时给邻居加压 | 与「攻击时」语义弱绑定 | 空射/未命中行为不清 |

**采用方案 A。**

## 主要新增文件
- `Assets/Script/Skill/ISkillPassive/BangDream/PassiveSkillEffect_Yukina.cs`（路径可按项目习惯放 `邦多利`/`BangDream`）

## 配置改动（不改核心系统）
- `Assets/Resources/ChessData/Player/凑友希那.asset`
- `Assets/Prefab/ChessPrefab/邦多利/凑友希那/凑友希那.prefab`
- 可选：子弹 Prefab；`开始.unity` allChess 注册

## 风险
| 风险 | 等级 | 缓解 |
|------|------|------|
| 友方已是压力子类（MMK 等） | 低 | 同名 `BuffReset` 走子类覆盖；回归测一轮 |
| OnAttack 与动画次数 | 低 | 对齐现有远程 Prefab 触发 |
| 首次只 AddBuff 不加值 | 中 | 实现必须 Ensure + BuffReset |

## 复杂度
L1；预估 2–4 小时。
