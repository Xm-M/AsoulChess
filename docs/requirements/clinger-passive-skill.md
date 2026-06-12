# 功能需求卡片: 攀附者被动（追随霸凌者行 + 攀附攻速）

## 基本信息
- **功能名称**: 攀附者被动技能（PassiveSkill_Clinger / Buff_Zombie_ClingerAttach）
- **所属模块**: 技能系统（`ISkillEffect` 被动）+ `MoveController`（纵向对齐行）+ Buff（攻速）
- **需求类型**: 功能扩展（学校僵尸「攀附者」新增被动，复用现有技能/Buff/移动框架）
- **优先级**: P1
- **预估复杂度**: L2
- **预估耗时**: 3–5 小时（含 Prefab 挂载与关卡验证）
- **提出日期**: 2026-05-20

## 功能描述

### 详细描述
**攀附者**入场后周期性检测同阵营场上是否存在 **霸凌者**（`PropertyCreator.chessName == "霸凌者"` 且存活）：

1. **行对齐**：若存在霸凌者，随机选取一名霸凌者，将其所在行（`standTile.mapPos.y`）作为目标行；若攀附者当前不在该行，则 **仅纵向移动**（保持当前列 `mapPos.x` 不变）对齐到目标行。
2. **攀附 Buff**：场上有霸凌者时，为攀附者施加 **攀附着 Buff**，攻速 **+100%**（`Buff_BaseValueBuff_AttackSpeed.speed = 1.0`，即 `acceleRated` 叠加 +1）；场上无霸凌者时 **移除** 该 Buff 的攻速增益。

### 用户故事
作为关卡设计者，我希望攀附者在霸凌者存在时自动贴到同一行并获得双倍攻速，霸凌者全灭后失去加成，以强化「仗势欺人」类学校关卡的分工与联动。

## 现有业务上下文

### 相关现有功能
| 功能 | 模块 | 关系 | 说明 |
|------|------|------|------|
| `Passive_Zombie_BullyMan` | Skill/School | 联动对象 | 霸凌者被动，标记植物承伤 |
| `PassiveSkill_TeacherZombie` | Skill/School | 相似模式 | Timer 周期检测 + AddBuff |
| `Passive_Zombie_BullyMan` | Skill/School | 参考 | Timer + `OnRemove` 清理 |
| `MoveController.MoveToTarget(Tile)` | Chess | 依赖 | 协程位移，用于纵向对齐 |
| `Buff_BaseValueBuff_AttackSpeed` | Buff | 依赖 | `ChangeAcceleRate(speed)` |
| 攀附者 Prefab | ChessPrefab | 挂载点 | 当前 `passiveSkill` 为空 |

### 需求类型判定理由
在学校僵尸技能体系上 **扩展** 攀附者能力，不修改核心 `SkillController` / `BuffController` 接口；实现方式为新增 `ISkillEffect` + 自定义 `Buff` 类，与 `PassiveSkill_TeacherZombie` 一致。

### 集成点
- **调用**: `ChessTeamManage.GetTeam(user.tag)`、`MoveController.MoveToTarget(Tile)`、`buffController.AddBuff` / Buff 移除
- **触发**: 被动 `SkillEffect` 在入场时启动；`Timer` 周期 tick；`user.OnRemove` 停止 Timer
- **判定霸凌者**: `chess.propertyController.creator.chessName == "霸凌者"` && `!chess.IfDeath`

### 对现有功能的影响
- **接口变更**: 无
- **行为变更**: 仅攀附者 Prefab 增加被动配置
- **数据变更**: 无

## 技术要求
- **Unity 版本**: 与项目一致
- **依赖模块**: Skill、Buff、Chess（MoveController）、Manage（TimerManage）
- **性能要求**: 单单位 0.2–0.5s 检测间隔，与同屏学校僵尸数量可接受
- **兼容性**: 遵循 `Assets/Script/Skill/` 禁止运行时 AddComponent 规则
- **存档**: Buff 由现有 Buff 存档链路覆盖；Timer 状态丢失可接受（与 TeacherZombie 一致）

## 功能清单

### 核心功能（必须）
- [ ] `PassiveSkill_Clinger`：`ISkillEffect`，周期扫描霸凌者
- [ ] 多霸凌者时 **随机选一行** 对齐（仅 Y，保持 X）
- [ ] `Buff_Zombie_ClingerAttach`：含 `Buff_BaseValueBuff_AttackSpeed`（speed=1.0）
- [ ] 无霸凌者时移除攀附 Buff（`BuffOver` 撤销攻速）
- [ ] 避免重复 AddBuff；移动中不重复发起纵向移动
- [ ] `OnRemove` 停止 Timer 并清理 Buff
- [ ] 攀附者 Prefab 配置 `passiveSkill` → `PassiveSkill` → `PassiveSkill_Clinger`

### 扩展功能（可选）
- [ ] 攀附视觉特效（ObjectPool）
- [ ] 霸凌者死亡/新生时 EventController 即时刷新（当前 Timer 轮询即可）

## 验收标准
- [ ] 场上有 1 名霸凌者：攀附者纵向移动到霸凌者同行，攻速约为基础 2 倍
- [ ] 场上有多名霸凌者：随机选一行对齐（可多次 tick 换行）
- [ ] 霸凌者全部死亡：攻速增益消失（`acceleRated` 恢复）
- [ ] 新霸凌者入场：再次获得 Buff 并对齐行
- [ ] 攀附者死亡/离场：无 Timer 泄漏

## 风险评估
| 风险点 | 概率 | 影响 | 应对措施 |
|--------|------|------|----------|
| `MoveToTarget` 与 `HorMove` 冲突 | 中 | 中 | 仅在 `mapPos.y` 不等且未在攀附移动中时触发 |
| `MoveToTile` 未走 `ChessLeave/Enter` | 低 | 低 | 与现有矿工/位移技能一致，仅改 standTile |
| 同 buffName 叠加 | 低 | 中 | 被动侧维护 `_attachBuffActive` 或检查 buffDic |

## 关联 Context
- `context/index.md` — Skill、Buff 模块
- `context/modules/Skill.md` — ISkillEffect 扩展规范
- `docs/requirements/miner-zombie-passive.md` — 被动 + MoveController 参考
