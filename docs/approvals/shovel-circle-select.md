# 需求开发审批报告

## 基本信息
- **需求名称**: 铲子圆圈检测与优先级铲除
- **所属模块**: UI / Chess / Map
- **分析日期**: 2026-07-10

## 需求摘要
铲子改为半径可调（默认 1）的圆圈检测；`PrePlantImage` 下显示将被铲植物名；多目标时 Support > Main、同级最近；有其他植物时不可铲 Pot；Consume 不参与；无目标左键取消；UnSelectable 暂排除。

**需求类型**: 功能扩展  
**与现有功能的关系**: 扩展 `ShovelPanel` / `PrePlantImage`，入口不变

## 分析结果汇总

### Context 复用
✅ 已读取 `context/index.md`  
- 涉及模块: UI, Chess, Map

### 现有业务分析
- 相关: `ShovelPanel` 射线铲除、`PlantType`、`UnSelectable`
- 集成: 仍 `Death()` + dig/cancel 音效

### 需求卡片
✅ `docs/requirements/shovel-circle-select.md`

### Skills白名单检查
| 技术点 | 结论 |
|--------|------|
| UI / TMP 文本 | ✅ `@unity-ui-system` |
| 协程手持物 | ✅ 现有 `BaseHandPanel` 模式 |
| `Physics2D.OverlapCircle` | ✅ 项目内已广泛使用（音频 Skill 非必须） |
| 未覆盖 | 无阻塞项 |

**结论**: 通过

### 架构分析
✅ `docs/architecture/shovel-circle-select.md`  
- 模式: Panel 内聚选中 + PrePlantImage 展示  
- 风险: 低–中

### 影响面分析
✅ `docs/architecture/shovel-circle-select-impact.md`  
- 主要改 2 个脚本 + Prefab  
- 回归约 9 项

## 风险总评

| 维度 | 评级 | 说明 |
|------|------|------|
| 技术可行性 | 高 | 局部改动 |
| 改动范围 | 小 | UI 手持物 |
| 性能风险 | 低 | 每帧小圆检测 |
| 时间评估 | 2–4h | 含 Prefab 与调参 |

## 建议的 Skills 使用清单
- `@unity-ui-system` — 名称文本
- 实现时对齐现有 `ShovelPanel` / `PlantsPanel` 输入与取消模式

## 开发优先级建议
- P0: 圆检测 + 过滤/优先级 + 铲除/取消
- P0: 名称文本绑定与刷新
- P1: 半径 Inspector 可调、离开关卡清理
- P2: 调试 Gizmo；UnSelectable 可铲（另开需求）

## 决策审批
✅ 通过 - 可以开始开发  
⬜ 修改后通过 - 需要调整方案  
⬜ 驳回 - 暂不开发

审批意见: 用户确认通过，已进入开发并完成实现  
日期: 2026-07-10
