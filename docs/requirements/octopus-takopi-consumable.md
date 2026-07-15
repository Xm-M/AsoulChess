# 功能需求卡片: 章鱼噼（消耗品）

## 基本信息
- **功能名称**: 章鱼噼
- **所属模块**: Skill（消耗品植物）+ 章鱼噼篇剧情 / 学校关卡
- **需求类型**: 新增功能（新消耗品棋子 + 技能效果）
- **优先级**: P0（章鱼噼篇战斗核心道具）
- **预估复杂度**: L2
- **预估耗时**: 4–6 小时（脚本 + Prefab/Asset + 关卡验证）
- **提出日期**: 2026-05-20
- **定稿日期**: 2026-05-20
- **修订日期**: 2026-05-20（保底击杀 + Boss 免疫）

## 功能描述

### 详细描述
**章鱼噼**为本章主题消耗品（`PlantType.Consume`，沿用 `ConsumePlant`），玩家从手牌**种植在己方植物格上**后触发一次性效果。

**种植规则**：仅可种在已有 `tile.stander` 的格（与蛋包饭 / 甜甜圈相同，**`ConsumePlant`**）。

**生效顺序**（实现必须遵守）：

1. **读取宿主压力**：取 `tile.stander`（被种的己方植物）当前压力值。  
   - 优先 `skillController.context.TryGet<int>("stress", out stress)`。  
   - **读不到或宿主无「压力」Buff 时，按 `stress = 0` 处理**（不报错、不中断）。
2. **计算击杀数**：  
   ```text
   击杀数 = max(1, floor(stress / 5))
   ```  
   - **保底 1 只**：即使 `stress == 0`、无压力 Buff、`TryGet` 失败，仍至少尝试消灭 **1** 只符合条件的僵尸。  
   - 上限：`min(击杀数, 当前可击杀敌方数量)`。
3. **选取并击杀僵尸**（重复至击杀数用尽或无可击杀目标）：  
   - **可击杀目标池**：场上存活敌方，且 **非 Boss 级**（见下节）。  
   - **优先霸凌者**（`chessName == "霸凌者"`）；霸凌者不足时，从其余可击杀敌方中随机选取。  
   - 每只 `Death()` 一次，不重复击杀同一单位。
4. **清除宿主 Buff**（**无论实际击杀几只均执行**）：  
   - **压力 buff**：`buffName == "压力"`（`Buff_StressBuff_Death` 及子类）  
   - **目标 buff**：`buffName == "霸凌目标"`（`Buff_Zombie_BullyBuff`）  
   - 同步 `context.Set<int>("stress", 0)`
5. **消耗品回收**：`ConsumablesSkill.SkillOver` → 本体 `Death()`。

**获得方式**：章鱼噼篇 **剧情赠送**（Story 关 / 关卡 Outcome 发卡，配置层实现）。

### Boss 免疫（全局规则）
**所有「清除 / 消灭僵尸」类效果**（含章鱼噼，以及后续同类技能）**不得**以 **Boss 级僵尸** 为目标。

**Boss 判定**（定稿，统一封装 `ZombieEliminationRules` 供章鱼噼及后续复用）：

- **仅一条规则**：`PropertyCreator.plantTags`（羁绊标签）**包含** `"Boss"` → 不可作为清除僵尸类效果的目标。
- Boss 类 Enemy 数据须打标 `plantTags: Boss`（如僵王、息事宁人者、巨型变异蟹等）。

场上**仅有 Boss** 或**无可击杀敌方**时：实际击杀 0 只（保底无合法目标则不杀），**仍执行清 Buff + 消耗品回收**。

### 用户故事
作为章鱼噼篇战斗中的玩家，我希望把章鱼噼种在队友身上——哪怕她暂时没有压力，也能至少驱散一只霸凌者（优先），并解除霸凌标记；同时 Boss 不会被快乐星误杀。

### 效果示例
| 宿主压力 | 计算击杀数 | 场上可击杀敌方 | 预期 |
|----------|------------|----------------|------|
| 读不到 / 0 | **1**（保底） | 3 普通 + 1 霸凌者 | 杀 1 霸凌者 |
| 0 | **1** | 仅 Boss | 不击杀，仍清 Buff |
| 4 | **1** | 2 霸凌者 | 杀 1 霸凌者 |
| 10 | 2 | 1 霸凌者 + 3 其他 | 杀 1 霸凌者 + 1 其他 |
| 25 | 5 | 3 霸凌者 + 2 其他 | 杀 3 霸凌者 + 2 其他 |
| 任意 | ≥1 | 含僵王（Boss） | 僵王**不会**被选中 |

## 现有业务上下文

### 相关现有功能
| 功能 | 模块 | 关系 | 说明 |
|------|------|------|------|
| `ConsumePlant` + `ConsumablesSkill` | Chess / Skill | **直接复用** | 仅可种在已有 `stander` 格 |
| `PassiveSkill_Clinger` | Skill/School | 联动 | 霸凌者名 `"霸凌者"` |
| `Buff_StressBuff_Death` | Buff/GBC | 读数 + 清除 | `context["stress"]` |
| `Buff_Zombie_BullyBuff` | Skill/School | 清除对象 | `buffName = "霸凌目标"` |
| `LevelController_Boss` | LevelSystem | Boss 免疫 | `boss` 字段引用主 Boss |
| 剧情奖励发植物 | LevelSystem / Story | 获得途径 | Outcome 发卡 |

### 集成点
- **调用**: `TryGet<int>("stress")`、`max(1, stress/5)`、`GetEnemyTeam`、Boss 过滤、`Death()`、`TryOverBuff`
- **建议新增**: `ZombieEliminationRules.IsBoss` — `plantTags` 含 `"Boss"`
- **触发**: 消耗品入场 → `UseSkill` → `SkillEffect_Takopi` → `ConsumablesSkill.SkillOver`

### 对现有功能的影响
- **接口变更**: 建议新增 Boss 过滤工具类（小范围公共静态方法），无破坏性改动
- **数据变更**: Boss 类 `ChessData` 需补 `plantTags: Boss`（僵王、息事宁人者等）；新增章鱼噼 Asset/Prefab

## 技术要求
- **Unity 版本**: 与项目一致
- **依赖模块**: Chess、Skill、Buff、Map、LevelSystem（Boss 关）
- **性能要求**: 单次 O(敌方数量)，可接受
- **兼容性**: `ISkillEffect` + 配置；Boss 规则可复用

## 功能清单

### 核心功能（必须）
- [ ] `SkillEffect_Takopi`：`max(1, floor(stress/5))` 击杀，霸凌者优先，**排除 Boss**
- [ ] `stress` 读失败视为 0，仍触发保底 1 次击杀尝试
- [ ] 清除宿主 `压力` / `霸凌目标` Buff，`stress` 归零
- [ ] `ZombieEliminationRules`（或等价）封装 Boss 不可击杀判定
- [ ] Prefab + `PropertyCreator`（`chessName = 章鱼噼`，`ConsumePlant`）
- [ ] Boss 相关 `ChessData` 补 `plantTags: Boss`

### 扩展功能（可选）
- [ ] 种植 VFX / 音效
- [ ] 僵尸侧 `霸凌目标` Context 同步清理
- [ ] 剧情关赠送配置

## 验收标准
- [ ] 只能种在已有己方植物上
- [ ] 宿主无压力 Buff、`TryGet` 失败：仍尝试杀 **1** 只（优先霸凌者）
- [ ] 压力 10：杀 2 只（霸凌者优先）
- [ ] 场上仅有 Boss：不击杀，仍清 Buff
- [ ] 僵王 / 息事宁人者（Boss 标）永不被选为目标
- [ ] 消耗品本体离场

## 风险评估
| 风险点 | 概率 | 影响 | 应对措施 |
|--------|------|------|----------|
| 先清 Buff 再读压力 | 中 | 高 | **必须先读 stress 再清 Buff** |
| 项目无统一 Boss 标 | 中 | 中 | 实现时补 `plantTags: Boss` + `LevelController_Boss` 双保险 |
| 保底 1 但场上无小怪 | 低 | 低 | 无目标则 0 击杀，仍清 Buff |

## 关联 Context
- `context/modules/Skill.md`, `context/modules/Buff.md`, `context/modules/LevelSystem.md`
- `docs/剧情设计/章鱼噼篇-v1.0-修订说明.md`
- `docs/requirements/clinger-passive-skill.md`

## 配置与资源（实现期）
| 项 | 路径 |
|----|------|
| ChessData | `Assets/Resources/ChessData/Player/章鱼噼.asset` |
| 技能脚本 | `Assets/Script/Skill/ISkillEffect_Plant/Story_Skill/SkillEffect_Takopi.cs` |
| Boss 过滤 | `Assets/Script/Skill/.../ZombieEliminationRules.cs`（建议路径） |
| Prefab | `Assets/Prefab/ChessPrefab/GBC/章鱼噼/章鱼噼.prefab` |
