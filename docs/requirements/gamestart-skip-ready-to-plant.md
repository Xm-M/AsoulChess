# 功能需求卡片: 关卡插件 · 跳过「准备种植」开战文字

## 基本信息
- **功能名称**: GameStartPlugin_SkipReadyToPlant
- **所属模块**: LevelSystem / UI（TextPanel）
- **需求类型**: 功能扩展
- **优先级**: P1
- **预估复杂度**: L1
- **预估耗时**: 0.3 小时
- **提出日期**: 2026-07-27

## 功能描述
### 详细描述
GameStart 插件：开战时跳过 `TextPanel.GameStart()` 的「准备种植 / Ready to Plant」**动画与音效**。仍 `Show` TextPanel，以便后续波次/失败等文字可用。

### 用户故事
作为关卡配置者，我希望部分关（如已预放置植物）跳过开战种植提示横幅，减少打断。

## 现有业务上下文

### 相关现有功能
| 功能 | 模块 | 关系 | 说明 |
|------|------|------|------|
| `TextPanel.GameStart()` | UI | 修改 | 播「准备种植」+ `gamestart` 动画 |
| `LevelController.GameStart` | LevelSystem | 依赖 | 先跑 GameStartPlugin，再 Show+GameStart |
| `LevelController_Endless.GameStart` | LevelSystem | 依赖 | 同样调用 TextPanel.GameStart |

### 已锁定规则
| 项 | 结论 |
|----|------|
| 跳过范围 | **动画 + 音效** 都跳 |
| TextPanel | **仍 Show** |
| 挂载 | **GameStartPlugin** |
| 实现 | 插件置位 → `TextPanel.GameStart` 检测后 early return 并清标志 |

### 集成点
- 插件 `StadgeEffect` 置 `SkipReadyToPlant`
- `TextPanel.GameStart` 消费标志

### 对现有功能的影响
- 未挂插件关卡：行为不变
- 挂插件关卡：无开战横幅/音效；波次字等仍可用

## 功能清单
- [ ] `GameStartPlugin_SkipReadyToPlant`
- [ ] `TextPanel` 支持跳过 GameStart 展示
- [ ] Endless 路径一并生效（走同一 TextPanel API）

## 验收标准
- [ ] 挂插件：开战无 gamestart / 无「准备种植」音
- [ ] 仍可显示大波/失败等 TextPanel 内容
- [ ] 未挂插件：与现网一致

## 关联 Context
- LevelSystem、UI
