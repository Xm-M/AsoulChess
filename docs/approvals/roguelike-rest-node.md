# 需求开发审批报告：肉鸽休息节点

## 基本信息

- **需求名称**：Roguelike Rest — 地图休息房（可扩展选项）
- **所属模块**：Roguelike / UI
- **分析日期**：2026-06-11

## 需求摘要

实现 Rest 非战斗房间：一期仅内置「休整」选项（Run 金币津贴）+ 可扩展选项 API + 第二常驻位 UI 占位；不做卡组移除/锻造 MVP。

**需求类型**：功能扩展  
**与现有功能的关系**：对称扩展 `RoguelikeShopFlow`；修复 Rest 点击进战斗的断层

## 分析结果汇总

### Context 复用

- 已读取 `context/index.md`
- 涉及模块：Roguelike、LevelSystem、UI、SaveSystem

### 设计决策（架构分析摘要）

| 决策 | 选择 | 理由 |
|------|------|------|
| 流程宿主 | `RoguelikeRestFlow` 静态类 | 与 Shop/Reward 一致 |
| UI | 独立 `RoguelikeRestPanel` | 与 Shop 对称，便于美术布局 |
| 选项模型 | `RoguelikeRestOption` + `IRoguelikeRestOptionProvider` | 一期一个内置 + 事件扩展，避免 Rest 面板硬编码 |
| 休整效果 MVP | `runGold += RollRestGold` | 无战斗插件改动，EconomyConfig 可调 |
| 第二常驻位 | Prefab 占位隐藏 | 二期锻造，不阻塞一期 |
| 每节点一次 | `restUsedNodeIds` | 对齐尖塔二选一 |

### Skills 白名单检查

| 技术点 | 结论 |
|--------|------|
| UI 面板 / Button / TMP | ✅ `@unity-ui-system` |
| ScriptableObject 配置 | ✅ `@unity-scriptableobject-config` |
| 存档字段 | ✅ `@unity-save-system` |
| 场景回地图 | ✅ `@unity-scene-management` |

未覆盖：无阻塞项。

### 影响面分析

**预估修改/新增文件（约 10～12 个）**

| 文件 | 改动 |
|------|------|
| `RoguelikeRunService.cs` | `EnterRestNode` / `LeaveRestNode` / Resume |
| `RoguelikeMapPanel.cs` | Rest 分支 |
| `RoguelikeRunState.cs` / Save 克隆 | `restUsedNodeIds` |
| `RoguelikeEconomyConfig.cs` | `RoguelikeRestRule` |
| `RoguelikeRestFlow.cs` | 新增 |
| `RoguelikeRestOption.cs` | 新增 |
| `IRoguelikeRestOptionProvider.cs` | 新增 |
| `RoguelikeRestPanel.cs` | 新增 |
| `RoguelikeRestPanel.prefab` | 新增（手动） |
| `RoguelikeEconomyConfig.asset` | 配 rest 默认值 |

**回归测试项**

- [ ] Rest 不进战斗
- [ ] 休整加金币 + 存档
- [ ] 每节点只能选一次
- [ ] 商店购买 / 战斗奖励 / Boss 换 Act
- [ ] 继续冒险恢复 Rest 面板

**高风险点**：1 个 — 休整体感依赖数值，非代码风险

## 风险总评

| 维度 | 评级 | 说明 |
|------|------|------|
| 技术可行性 | 高 | 商店范式可复制 |
| 改动范围 | 中 | 核心 Run 流程少量分支 |
| 性能风险 | 低 | 打开面板时构建列表 |
| 时间评估 | 1～2 天 | 含 prefab 搭建 |

## 建议的 Skills 使用清单

- `@unity-ui-system` — RestPanel
- `@unity-scriptableobject-config` — RestRule
- `@unity-save-system` — State 字段

## 开发优先级建议

- **P0**：Flow + Service + Map 分支 + 休整金币 + Panel
- **P1**：测试 Provider、RunInfo 刷新金币
- **P2**：第二常驻位玩法、战斗向休整、事件孵化选项

## 决策审批

- [x] **通过** — 可以开始开发（用户确认 phased 方案：休整 + 扩展 API + 锻造位占位）
- [ ] 修改后通过
- [ ] 驳回

审批意见：一期不做卡组移除；第二常驻位二期；休整默认金币津贴，EconomyConfig 可调。  
日期：2026-06-11
