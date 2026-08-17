# 功能需求卡片: survival-round-more-zombies-banner

## 基本信息
- **功能名称**: 生存轮末「更多僵尸要来了」横幅
- **所属模块**: UI / TextPanel / LevelController_Endless
- **需求类型**: 功能扩展
- **优先级**: P1
- **预估复杂度**: L1
- **预估耗时**: 1–2 小时（含 Prefab 挂点；若无现成美术另加）
- **提出日期**: 2026-08-14

## 功能描述
### 详细描述
生存模式每轮结束（**非通关**）时，在清场 / `ClearTime` 之后、等待 `roundTransitionDelay` **之前立刻**显示「更多僵尸要来了」横幅。

实现方式对齐现有大波提示：`TextPanel` 新增独立 GO + API（**不**复用 `ZombieWave`），例如 `MoreZombiesComing()` → `moreZombiesComing.SetActive(true)`。

通关最后一轮（`CheckSurvivalWin` 成立）**不播**该横幅。

### 用户故事
作为生存玩家，我希望轮间过渡有原版式提示，知道还有更多僵尸，而不是干等几秒。

## 现有业务上下文

### 相关现有功能
| 功能 | 模块 | 关系 | 说明 |
|------|------|------|------|
| `TextPanel.ZombieWave` / `LastWave` | UI | 相似 | `SetActive` 驱动横幅（一大波僵尸…） |
| `roundTransitionDelay` | EndlessSpawnConfig | 依赖 | 已有 4s 等待，注释对齐本提示间隔 |
| `OnRoundCompleteCoroutine` | Endless | 修改 | 播横幅后等待再 Timeline |

### 已确认决策
| 项 | 结论 |
|----|------|
| Q1 表现 | **新增** `MoreZombiesComing()` + 独立 GO/动画 |
| Q2 时机 | ClearTime **后立刻播**，再 `WaitForSeconds(roundTransitionDelay)` |
| Q3 通关轮 | **不播** |

### 关于通关奖励（调研结论，非本需求范围除非加做）
当前 `CheckSurvivalWin` → `GameOver(true)` → `base.GameOver(true)` 只做 `ChangeLevel(next)` / `ReturnMenu()`，**不会**调用 `SpawnVictoryReward` / `outcome.HandleOutcome`。  
冒险通关奖励是在最后一波 `CheckZombieHp` 全灭时 `SpawnVictoryReward`。  
→ **生存按 `survivalMaxWave` 通关时，目前不会正常掉落奖杯/关卡 outcome。** 若需要，建议另开需求：通关时调 `SpawnVictoryReward`（取场上最后僵尸位或地图中心）。

## 功能清单
### 核心
- [ ] `TextPanel.MoreZombiesComing()` + 序列化 `moreZombiesComing` GO
- [ ] Prefab `文字显示面板` 挂子物体（动画可仿 zombieWave；文案「更多僵尸要来了」）
- [ ] `OnRoundCompleteCoroutine`：非通关路径播横幅 → delay → 后续流程
- [ ] 通关路径不播

### 可选 / 另单
- [ ] 通关掉落奖励修复（见上）
- [ ] 横幅专用音效

## 验收标准
- [ ] 轮 1→2：清怪后出现横幅，再进入选卡 Timeline
- [ ] 达成 `survivalMaxWave`：无横幅，走通关逻辑
- [ ] 大波 `ZombieWave` 行为不变

## 风险评估
| 风险 | 概率 | 影响 | 应对 |
|------|------|------|------|
| Prefab 尚无「更多僵尸」资源 | 中 | 中 | 先克隆 zombieWave 改文案，或占位 TMP |
| SetActive 重复不重播动画 | 低 | 低 | 先 false 再 true，或 Play 状态名 |
