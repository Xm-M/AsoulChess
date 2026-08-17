# 功能需求卡片: 司霆惊蛰 · 1 技能（浩气长存 · 简化版）

## 基本信息
- **功能名称**: 司霆惊蛰 · 浩气长存（v1 简化）
- **所属模块**: Chess / Skill / Weapon（IGridFindTarget）/ Effect
- **需求类型**: 功能扩展（在现有 `惊蜇` Prefab 骨架上补全）
- **优先级**: P1
- **预估复杂度**: L2
- **预估耗时**: 4–6 小时
- **提出日期**: 2026-07-26

## 功能描述

### 详细描述
为明日方舟棋子「司霆惊蛰」（基于 `Assets/Prefab/ChessPrefab/明日方舟/惊蜇/`）实现 **v1 简化版**：

- **不做**：天赋「明断」随机落雷、飞行/对空阻挡、3 充能、40s 蓄攻、第二天赋「追责」
- **被动**：种植后常态 **`UnSelectable()`**（铲子不可选）；无普攻
- **1 技能「浩气长存」**：CD 就绪 + 点击 → `SkillState` → 动画 `UseSkill` 帧结算
  - 按 **三个可配置的 `IGridFindTarget` 扇区**（左 / 前 / 右）定义作用格；索敌与伤害仍 **每扇区独立**（同格最多 3 段）
  - **落雷特效**：从 **自身格（中心）** 起，沿 **十字方向**（前/后/左/右，相对朝向）**逐格、按距离一圈圈 outward** 生成；**不是同一帧全屏齐射**
  - 开技能期间 **`ResumeSelectable()`**；`SkillOver` 后恢复 **`UnSelectable()`**

参考：[PRTS · 司霆惊蛰](https://prts.wiki/w/%E5%8F%B8%E9%9C%86%E6%83%8A%E8%9B%B0) 技能 1（范围与倍率 AVZ 化，不还原动态扩 range）。

### 用户故事
作为玩家，我希望惊蛰常态不能被铲，放技能时落雷从脚下向十字方向一格一格劈出去，再造成三向物理伤害。

## 落雷特效顺序（已确认）

### 范围来源
- 左 / 前 / 右三个 `IGridFindTarget` 的 `relativeCells` **取并集**（去重），只在该并集内的格子上播落雷。
- 中心格 = 相对坐标 **`(0, 0)`**（即 `standTile`）；若并集不含 `(0,0)` 则从并集中 **Manhattan 距离最小** 的格作为起点。

### 传播顺序（十字一圈圈出去）
在 **相对坐标**（随 `transform.right` 镜像，与 `IGridFindTarget` 一致）下：

1. **第 0 波**：中心 `(0, 0)`
2. **第 1 波**：十字四向 `(±1, 0)`、`(0, ±1)` 中属于并集的格
3. **第 2 波**：十字四向距离 2 的格 `(±2, 0)`、`(0, ±2)` …
4. 依此类推直到并集内最远十字臂上的格

同一波内顺序固定：**前 `(1,0)` → 右 `(0,-1)` → 后 `(-1,0)` → 左 `(0,1)`**（右向为 +X 时；镜像后仍按相对坐标排序）。

**不包含斜角格**（仅十字轴，不是九宫格菱形全扫）。

### 时间与伤害（v1 建议）
| 项 | 规则 |
|----|------|
| 特效间隔 | 可配 `strikeInterval`（默认 **0.05~0.08s**） |
| 驱动方式 | `user.StartCoroutine` 逐格 `ObjectPool` 生成落雷 FX |
| 伤害时机 | **v1：全部 FX 排完后再按三扇区各结算一次伤害**（逻辑简单）；若要做「劈到谁伤谁」列为 P1 |
| 空格 | 并集内无敌人的格 **照样播落雷** |

```
波次0:        .
波次1:      ⚡
波次2:    ⚡ ⚡ ⚡
波次3:      ⚡
（示意；实际只走十字轴上的 relativeCells）
```

## 现有业务上下文

### 相关现有功能
| 功能 | 模块 | 关系 | 说明 |
|------|------|------|------|
| `Passive_Item` | Skill | 相似 | 进场 `UnSelectable()` |
| `ColdSkill` + `SkillReady_MouseDown` | Skill | 依赖 | 点击放技能、CD |
| `SkillEffect_Jalapeno` | Skill | 相似 | 范围伤害 + 行特效 |
| `IGridFindTarget` | Weapon | **核心复用** | `relativeCells` + 朝向镜像 + OverlapBox 索敌 |
| `WisadelGridHelper` | Skill | 参考 | 从 `IGridFindTarget` 收集绝对 mapPos / 格心 Overlap |
| `PropEffect_Lightning` | Prop | 参考 | 落雷特效 `ObjectPool` 生成 |
| `Assets/Sprite/Effect/雷电/天雷.prefab` | Effect | 资源 | 默认落雷特效候选 |

### 需求类型判定理由
`惊蜇.prefab` 已有美术与 Animator 骨架，缺被动/主动/状态机配置；新增 3 个 Skill 脚本 + Prefab 配表，不改全局 Skill 协议。

### 集成点
- **被动**: `PassiveSkill` → `PassiveSkillEffect_Leizi` → `UnSelectable()`
- **主动壳**: `ColdSkill_LeiziS1`（`WhenEnter`/`SkillOver` 管选中态）
- **主动效果**: `SkillEffect_LeiziHaogi`（三份 `IGridFindTarget` + 落雷 FX + 伤害）
- **动画**: `SkillState` + 动画事件 `UseSkill`（**不用** `ISkillFireUseSkillOnEnter`）
- **状态机**: `IdleState` ←→ `IfSkillReadyTransition` → `SkillState`；`AnimFinish` 结束

### 对现有功能的影响
- **接口**: 无破坏性改动；仅新增 Script + 改 `惊蜇.prefab`
- **行为**: 不影响其他棋子
- **数据**: 可选 `PropertyCreator` / allChess 注册（配置者自填）

## 已确认规则（v1）

| 项 | 结论 |
|----|------|
| 被动落雷 | **不做** |
| 常态 | `UnSelectable()` |
| 开技能 | `ResumeSelectable()` |
| 技能结束 | `UnSelectable()` |
| 范围配置 | **三个 `IGridFindTarget`**（左/前/右），Inspector 配 `relativeCells` |
| 落雷特效 | 三扇区 **并集** 内格子；**从中心十字逐格 outward**，间隔可配 |
| 伤害 | 物理；倍率 `config.baseDamage[0]` × `GetAttack()`；**每扇区独立结算**（FX 播完后或 P1 随格结算） |
| 充能 | v1 **不做** 3 充能，普通 CD |
| 普攻 | **无** Weapon 或不进 AttackState |

## 技术要求

- **规范**: `ISkillEffect` + `[SerializeReference]`；禁止技能专用运行时 MonoBehaviour
- **格子几何**: 必须与 `IGridFindTarget.FindTarget` 一致（`GridFindTargetGeometry.GetForwardX` / `IsDetectableCell`）
- **特效**: `ObjectPool.instance.Create(prefab)`；**协程逐格**播放，顺序见「落雷特效顺序」
- **依赖**: `MapManage`、`ChessTeamManage.GetEnemyLayer`、`SkillConfig_Cold`、`Chess.StartCoroutine`

## 功能清单

### 核心功能（必须）
- [x] `PassiveSkillEffect_Leizi.cs`
- [x] `ColdSkill_LeiziS1.cs`
- [x] `SkillEffect_LeiziHaogi.cs`（三 `IGridFindTarget` + 格特效 + 扇区伤害）
- [x] `惊蜇.prefab`：被动/主动/StateGraph/动画 `UseSkill`
- [x] `SkillConfig_Cold` asset（CD、baseDamage）

### 扩展功能（可选）
- [ ] Scene Gizmos 预览三扇区（可复用 `IGridFindTarget.DrawGizmos` 思路）
- [ ] 不可选层敌人（气球）补扫，对齐 Jalapeno
- [ ] 3 充能 / 明断被动

## 验收标准

- [ ] 种下后铲子选不中；开技能过程中可被选（若 UI 需要）；技能结束后恢复不可选
- [ ] CD + 点击 → 仅动画 `UseSkill` 帧触发一次效果（非进态立刻放）
- [ ] 左/前/右三份 `relativeCells` 在 Inspector 可独立改，改后伤害范围与特效范围一致
- [ ] **落雷从中心格开始**，沿十字轴 **一圈圈 outward** 生成，肉眼可见逐格延迟
- [ ] 并集内 **每个作用格** 都有落雷（含空格）；三扇区重叠格只 **播一次 FX**、仍 **吃最多 3 段伤害**
- [ ] 无普攻、无随机落雷被动

## 风险评估

| 风险点 | 概率 | 影响 | 应对措施 |
|--------|------|------|----------|
| 三扇区 relativeCells 配错 | 中 | 中 | Prefab 默认配一版；Scene Gizmos |
| 同格多次伤害过强 | 中 | 低 | baseDamage 可配；文档注明 PRTS 行为 |
| 特效过多性能 | 低 | 低 | ObjectPool + 短动画自动 Recycle |
| Prefab 未配 StateGraph | 高 | 高 | 对照史尔特尔 checklist |

## 关联 Context

- 涉及模块: Chess, Skill, State, Map
- 参考: `context/modules/Skill.md`, `context/modules/Chess.md`

## 实现要点（开发用）

### SkillEffect_LeiziHaogi 伪代码

```
cells = UnionRelativeCells(leftGrid, frontGrid, rightGrid)
waves = GroupByCrossRing(cells)   // ring0=(0,0), ring1=十字距1, ring2=十字距2…

user.StartCoroutine(StrikeSequence):
    foreach wave in waves ordered:
        foreach rel in wave ordered (前→右→后→左):
            SpawnFxAtMapCell(user, rel)
            yield Wait(strikeInterval)
    // v1 伤害在序列结束后：
    foreach sector in [left, front, right]:
        sector.FindTarget → ApplyDamage per sector
```

### 默认落雷特效
- `Assets/Sprite/Effect/雷电/天雷.prefab`（Effect 字段可配）
