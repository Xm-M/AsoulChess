# 架构设计: 章鱼祥致盲被动

## 系统定位

- **所属模块**: Skill（被动）+ Buff（致盲）+ Chess（Property 伤害钩子）
- **上游依赖**: `PropertyController.TakeDamage` / `GetDamage`、`BuffController`、`DamagePanel`
- **下游影响**: 被致盲单位的输出；章鱼祥 Prefab / PropertyCreator
- **横向关联**: 武器射击路径（与八九寺相同，不改框架）

## 设计决策（采用）

### 推荐方案：ISkillEffect + TimeBuff + onBeforeTakeDamage

```
章鱼祥攻击命中
  → onTakeDamage（章鱼祥）
  → 25% → AddBuff(Buff_Blind, 2s) 到目标
被致盲单位攻击
  → TakeDamage 开头 onBeforeTakeDamage
  → Buff_Blind 将 damageType = Miss
  → GetDamage：Miss → 伤害 0 + ShowMiss（对齐闪避）
```

| 方案 | 优点 | 缺点 |
|------|------|------|
| **A. onBeforeTakeDamage + Buff_Blind（推荐）** | 改写发生在结算前；Buff 自洽；可复用 | 需 Property 增 1 个事件 |
| B. GetDamage 查 damageFrom 是否有致盲 | 无新事件 | Property 耦合 Buff 名 |
| C. 仅改武器 DM | 无 Property 改动 | 技能/直接 TakeDamage 漏网 |

**采用 A。**

### 关键类型

- `PassiveSkillEffect_ZhangyuXiang`：`ISkillEffect`，订阅 `onTakeDamage` / `OnRemove`
- `Buff_Blind : TimeBuff`：`buffName = "致盲"`，`continueTime = 2`，订阅 `onBeforeTakeDamage`
- `PropertyController.onBeforeTakeDamage`：在暴击/增伤/`GetDamage` 之前 Invoke

### Miss 飘字

闪避在 `GetDamage` 内 `ShowMiss`。当前显式 `DamageType.Miss` **不飘字**。

本期：`GetDamage` 在 `Miss` 分支置 0 后调用 `ShowMiss`（尊重 `suppressFloatingDamage`），并像 Heal 一样早退，避免与闪避二次飘字。棒球喂球若会误飘，为其补 `suppressFloatingDamage`。

## 影响面（架构层）

| 文件 | 改动 |
|------|------|
| `Property.cs` | 新增 `onBeforeTakeDamage`；`TakeDamage` 开头 Invoke；`GetDamage` Miss 飘字+早退 |
| 新被动脚本 | Mygo 被动 |
| 新/同文件 Buff | `Buff_Blind` |
| `章鱼祥.asset` / Prefab | 数值与接线 |
| 棒球喂球（条件） | 若 Miss 早退后误飘，补 suppress |

## 风险

| 风险 | 概率 | 影响 | 应对 |
|------|------|------|------|
| 全局 Miss 飘字行为变化 | 低 | 低 | suppress 白名单 |
| 监听泄漏 | 低 | 中 | BuffOver / OnRemove 成对解绑 |
| Prefab SerializeReference 手改 YAML 易错 | 中 | 中 | 对照八九寺/黄瓜睦 YAML |

## 复杂度

**L1** — 局部扩展，无新系统。
