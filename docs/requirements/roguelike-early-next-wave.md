# 功能需求卡片: 肉鸽战斗「提前进下一波」（RG-009）

## 基本信息
- **功能名称**: 肉鸽 ProgressBar「下一波」可选按钮
- **所属模块**: LevelSystem / UI（ProgressBar）/ Roguelike
- **需求类型**: 功能扩展
- **优先级**: P1
- **预估复杂度**: L1
- **预估耗时**: 2~3 小时（代码 + Prefab 绑 Button）
- **提出日期**: 2026-05-20
- **关联**: `docs/roguelike-known-issues.md` RG-009、RG-005 ProgressBar 标题

## 功能描述

### 详细描述

肉鸽 **普通波次关卡**（基类 `LevelController`）战斗中，在 `ProgressBar` 同屏增加 **「下一波」可选按钮**。

- **保留现有自动进波**：`Update` 中 `WaveCanAdvance()` → `DoEnterNextWave()` **不改语义**。
- 按钮是 **额外选项**：满足条件时玩家可主动点进下一波；不点则仍按原逻辑自动进波。
- **仅肉鸽战斗**显示（`RoguelikeRunService.IsRoguelikeCombatLevel()`）。
- Boss / 贪吃蛇 / 无尽等 **子类 LevelController** 无此按钮。

### 按钮显隐（核心 UX）

以 **当前波** 的 `t`（波内计时）与 `mintime` 为准：

| 阶段 | 按钮 |
|------|------|
| 本波开始 → `t ≤ mintime` | **隐藏** |
| `t > mintime` 且满足下方门禁 | **显示**（可点与否见可交互条件） |
| 点击进下一波 / 自动进下一波后 | **隐藏**（新波重新从 `t ≤ mintime` 计时） |

### 可交互（可点击）条件

在 **已显示** 前提下即可点击（与显隐条件相同，**不要求清场/血量阈值**）：

1. `t > mintime`
2. `currentWave >= 0` 且 `currentWave < MaxWave - 1`
3. `waveDatas[currentWave].Wave % 10 != 9`
4. `GetType() == typeof(LevelController)`

点击后调用 `TryManualAdvanceWave()` → 复用 `DoEnterNextWave()`。**自动进波逻辑不变**（仍依赖 `WaveCanAdvance`）。

### 用户故事

作为肉鸽玩家，在最小等待时间过后，我希望在 ProgressBar 旁看到「下一波」按钮并随时主动进波；若我不点，游戏仍会在清场/maxtime 等条件满足时自动进波。

## 现有业务上下文

### 相关现有功能

| 功能 | 模块 | 关系 | 说明 |
|------|------|------|------|
| `WaveCanAdvance` / `DoEnterNextWave` | LevelController | 依赖 | 自动进波与手动共用进波入口 |
| `ProgressBar` | UI | 扩展 | RG-005 已扩展关卡标题 |
| `IsRoguelikeCombatLevel` | RoguelikeRunService | 门禁 | 仅肉鸽战斗 |
| `LevelController_Boss` 等 | LevelSystem | 排除 | 无波次概念或不同 UI |

### 需求类型判定理由

在现有波次推进上增加肉鸽 UI 入口；不改变非肉鸽、Boss、Snake 等行为。

### 集成点

- **调用**: `DoEnterNextWave()`、`WaveData.CheckZombieHp()`、`SaveSystem.SaveCurrentLevel()`
- **新增**: `LevelController.CanShowManualAdvanceWaveButton()`、`CanManualAdvanceWave()`、`TryManualAdvanceWave()`
- **UI**: `ProgressBar.RefreshEarlyNextWaveButton()`

### 对现有功能的影响

- **接口**: LevelController 新增 3 个 public 方法
- **行为**: 肉鸽普关多一个 UI 选项；自动进波 **不变**
- **数据**: 无

## 技术要求

- **依赖模块**: LevelSystem、UI、RoguelikeRunService
- **性能**: 每帧刷新按钮显隐一次（与 `Update` 同频），无额外分配
- **兼容性**: 非肉鸽 / 子类 Controller 零影响
- **Prefab**: `Assets/Resources/UIPrefab/ProgressBar.prefab` 增加 Button，文案「下一波」

## 功能清单

### 核心功能（必须）
- [x] `LevelController` 手动进波 API（显隐 / 可点 / 执行）
- [x] `ProgressBar` 按钮引用、刷新、点击
- [x] 肉鸽 + 普关 + mintime 后显隐逻辑
- [x] w9/w19 与最后一波禁用
- [x] 自动进波逻辑保持不变

### 扩展功能（可选）
- [ ] ~~未清场时按钮显示但灰态~~（已取消：出现即可点）

## 验收标准

- [ ] 仅肉鸽 **基类 LevelController** 战斗出现按钮
- [ ] 每波开始至 `mintime` 内：按钮 **不可见**
- [ ] `t > mintime` 后：满足门禁则 **可见且可点**
- [ ] 点击立即进下一波；不点则 **仍按 WaveCanAdvance 自动进波**
- [ ] 进下一波后按钮 **立即隐藏**，新波重新计时
- [ ] w9/w19…：**无按钮**
- [ ] 最后一波：**无按钮**
- [ ] Boss / Snake / Endless：**无按钮**
- [ ] 主线非肉鸽：**无按钮**

## 风险评估

| 风险点 | 概率 | 影响 | 应对措施 |
|--------|------|------|----------|
| 与自动进波同帧双调 | 低 | 中 | `TryManualAdvanceWave` 内校验 `CanManualAdvanceWave`；`DoEnterNextWave` 已有边界检查 |
| Prefab 未绑 Button | 中 | 低 | 字段 Optional，未绑则 no-op + 日志一次 |

## 关联 Context

- 涉及模块: LevelSystem, UI, Roguelike
- 参考: `context/modules/LevelSystem.md`、`context/modules/UI.md`
