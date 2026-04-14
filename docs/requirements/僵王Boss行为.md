# 功能需求卡片: 僵王 Boss 行为（阶段、站立/俯身、技能编排）

## 基本信息
- **功能名称**: 僵王（原版僵王）Boss 阶段、召唤池、站立/俯身循环与技能释放
- **所属模块**: Skill（`Skill_ZombieKingBoss` + `ISkillEffect`）+ Chess（选中、Buff）+ `AnimatorController_Zombieking` + Map
- **需求类型**: 功能扩展（在现有 `SkillContext` / 动画分发 / 僵王效果脚本上落地策划案）
- **优先级**: P0（Boss 战核心）
- **预估复杂度**: L2 / A 级边缘（状态多、与动画帧强绑定）
- **预估耗时**: 16–40 小时（含 Prefab、状态图、动画事件、联调）
- **提出日期**: 2026-03-30
- **约定修订**: 与策划口头对齐 — **蹦极仅血量 &lt; 80% 可用**；**俯身次数**在**每次从俯身回到站立**时 +1；**随机行**上界用 **`Mathf.Min`**；**召唤**每次一只、站立阶段共 **7 或 14 次** cast；砸车 **y=1** 固定；**不修改** `UnSelectable`/`IfSelectable` 命名，调用侧注释区分

## 功能描述

### 详细描述（与 `僵王.md` 对齐 + 已定稿澄清）

1. **血量阶段（外观与能力）**  
   - **&gt;80%**：全身完整；站立阶段**仅**召唤僵尸（无蹦极）。  
   - **50%–80%**：头/下巴/脚破损贴图；站立增加**召唤蹦极**（与 §3 一致时，蹦极仍受 **&lt;80%** 规则约束则本档已可用）。  
   - **10%–50%**：破损加重 + **冒烟** `SetActive(true)`；站立增加**砸车、脚踩**；召唤更密（与 §3「低于 50%」14 次等一致）。  
   - **&lt;10%**：无新贴图档；全身**闪烁**（材质/颜色协程等）。

2. **蹦极门槛（已定稿）**  
   - **仅当** `GetHpPerCent() < 0.8f` 时允许将「召唤蹦极」排入站立队列或可选项。

3. **五类技能与动画**  
   - 进入 `SkillState` 前写入 `ZombieKingContextKeys` + `ZombieKingSkillAnimKind`；`PlaySkill()` 播对应 `anim_*`（见 `AnimatorController_Zombieking`）。  
   - **召唤**：`x = standTile.x + (int)right.x * 2`，`y = Random.Range(0, Mathf.Min(5, MapManage.instance.mapSize.y))`（**上界用 Min**；`Random.Range(0, N)` 为 **0..N-1**，若需 0..`mapSize.y-1` 则取 `N = mapSize.y` 并文档说明）。每次 cast 生成 **1** 只，站立阶段共 **7 次（高血档）** 或 **14 次（低血档）** 由 `僵王.md` §3 与 HP 档合并实现。  
   - **召唤池**：`List<PropertyCreator> zombieTypes`；维护 `ZombieCanSummons`：当 **`bendCount * 2 > zombieTypes[n].baseProperty.waveLimit`** 时将第 n 种加入池；生成时在 `ZombieCanSummons` 随机。  
   - **蹦极**：三处随机植物格召唤蹦极（`ISkillEffect` 实现）。  
   - **脚踩**：按 `僵王.md` 六格；`y` 随机上界同 **Min** 逻辑；**有植物**（`IFindTarget`）才排入本次站立；`StompBand` 与 `anim_stomp_1~4` 映射。  
   - **砸车**：`x = standTile.x + right.x * 6`，**`y = 1` 固定**（全地图一致，单动画）。  
   - **俯身吐球**：`x = standTile.x + right.x * 3`，`y` 随机；冰/火随机；`Row` + `BallVisual`；眼嘴贴图在 Effect 中处理。

4. **站立 / 俯身与选中、Buff**  
   - **进入站立**：`UnSelectable()`、**清除所有 Buff**（如 `buffController.ResetList()`）。  
   - **进入俯身**：`ResumeSelectable()`。  
   - **站立内节奏**（§3）：高血 4.5s 间隔召唤、低血 3s；队列含 7 召唤 +1 蹦极（随机顺序）等；**全部用完后 2s** 切俯身。  
   - **俯身**：**10s** 后吐火球（走技能+动画事件）；**20s** 回站立；时间累加为 **`t += Time.deltaTime * propertyController.GetAccelerate()`**（冰冻/减速延长）。

5. **俯身次数 `bendCount`**（已定稿）  
   - 在**每次从俯身切换回站立**时 `bendCount++`，用于召唤池解锁条件。

### 用户故事
作为关卡设计者，我希望僵王按血量阶段切换外观与技能池，并在站立/俯身循环中与动画、选中、Buff 规则一致，便于玩家理解与验收。

## 现有业务上下文

### 相关现有功能
| 功能 | 模块 | 关系 |
|------|------|------|
| `ZombieKingSkillAnimKind` / `PlaySkill` | `AnimatorController_Zombieking` | 动画分发 |
| `SkillContext`、`UseSkill`、动画事件 | `SkillController`、`SkillState` | 释放与帧触发 |
| `SkillEffect_ZombieKing_*` | `ISkillEffect_ZombieKing.cs` | 召唤/火球/砸车（部分待补） |
| `GetHpPerCent()` | `PropertyController` | 阶段判定 |
| `MapManage.mapSize`、`IfInMapRange` | Map | 随机行与越界保护 |
| `PassesWavePoolFilter` / `waveLimit` | `PropertyCreator.baseProperty` | 与僵王池条件区分用：僵王用 **`bendCount*2 > waveLimit`** 文案，不直接复用波次筛选函数除非一致 |

### 集成点
- **`IfSkillReady`**：在返回 `true` **当帧**内写好 `SkillContext`（含 `stand`、`zombieKingSkillAnimKind`、`Row`、`StompBand`、`BallVisual`），再进入 `SkillState` → `PlaySkill()`。  
- **`UseSkill`**（动画事件）：`SkillEffect` 从 **同一 `SkillContext`** 读参数执行逻辑。  
- **`IsSkillFinished`**：配置 **`AnimFinish`**（或按招式定制），与 `IfAnimPlayOver` 一致。  
- **与 `Transition_ZombieKing_ChangeIdleTransition`**：若仍挂在状态图上，须避免与技能写 `stand` **双重打架**（二选一或该 Transition 仅用于无 Boss 技能时）。

### 对现有功能的影响
- 新增 `Skill_ZombieKingBoss` 及若干 Effect / 运行时信息；僵王 Prefab 上 **`activeSkill`** 指向该实例。  
- 可能调整 `ISkillEffect_ZombieKing` 内硬编码行号，改为读 Context。

## 技术要求
- **Unity**: 与工程一致；**不新增**技能用运行时专用 MonoBehaviour（遵守 `.cursor/rules`）。  
- **协程**: 闪烁等使用 **`chess.StartCoroutine`**。

## 功能清单

### 核心功能（必须）
- [ ] `Skill_ZombieKingBoss`：站立队列 / 俯身计时 / `bendCount` / HP 档判定 / Context 写入。  
- [ ] 蹦极仅 **HP &lt; 80%** 进入队列。  
- [ ] 召唤 7/14 次单次生成、`ZombieCanSummons` 与 `waveLimit` 规则。  
- [ ] 脚踩索敌再释放；砸车六格；吐球行+冰火。  
- [ ] 站立入场 `UnSelectable` + `ResetList`；俯身 `ResumeSelectable`。  
- [ ] 随机行 `Mathf.Min` + `mapSize` 边界。  
- [ ] 存档：`WriteToSaveData` / `RestoreFromSaveData` 持久化 `bendCount` 与必要阶段字段。

### 扩展功能（可选）
- [ ] 四档破损与冒烟、低血闪烁与 `SetVisualTierPublic` 等表现对齐。

## 验收标准
- [ ] 血量 **≥80%** 站立队列中**不出现**蹦极；**&lt;80%** 可出现（且满足 §1 其它阶段条件）。  
- [ ] 站立召唤次数与间隔符合 §3 与 HP 档；每次仅 1 只僵尸。  
- [ ] 俯身 10s/20s 受 `GetAccelerate()` 缩放；回站立时 `bendCount` 增加。  
- [ ] 动画与逻辑一致（事件触发 `UseSkill` 时机正确）。  
- [ ] 编译无错误；读档后阶段与 `bendCount` 合理。

## 风险评估
| 风险点 | 概率 | 影响 | 措施 |
|--------|------|------|------|
| `stand` 与全局 Transition 冲突 | 中 | 高 | 状态图与技能约定唯一写口 |
| 动画时长与 `AnimFinish` 不一致 | 中 | 中 | 按状态名定制 `ISkillFinish` 或调 Exit Time |
| `UnSelectable` 命名误用 | 低 | 中 | 代码注释 + 与现有调用一致 |

## 关联 Context
- `context/index.md`
- `context/modules/Skill.md`、`context/modules/Chess.md`、`context/modules/State.md`
- `context/architecture/decisions.md`

## 实现骨架代码
- `Assets/Script/Skill/SkillBase/Skill_ZombieKingBoss.cs`（本需求关联提交）
