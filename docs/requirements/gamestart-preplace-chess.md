# 功能需求卡片: 关卡插件 · 提前放置单位

## 基本信息
- **功能名称**: GameStartPlugin_PrePlaceChess（提前放置植物/单位）
- **所属模块**: LevelSystem（`ILevelPlugin` / GameStart）
- **需求类型**: 新增功能
- **优先级**: P1
- **预估复杂度**: L1
- **预估耗时**: 0.5 小时
- **提出日期**: 2026-07-27

## 功能描述
### 详细描述
在 `LevelData.GameStartPlugin` 中挂载插件，维护可序列化队列：每条为 `(PropertyCreator creator, Vector2Int pos, string tag)`。`StadgeEffect` 时按队列顺序在对应格子生成对应阵营单位（`CreateChess` + `PlantChess`）。格子无效或无法放置则**跳过**该条继续。**读档时跳过**整插件，避免与植物存档叠种。

### 用户故事
作为关卡配置者，我希望在 LevelData 里配好开局已存在的植物/单位位置与阵营，以便做教程关、固定开局阵容等。

## 现有业务上下文

### 相关现有功能
| 功能 | 模块 | 关系 | 说明 |
|------|------|------|------|
| `ILevelPlugin` / `LevelData.GameStartPlugin` | LevelSystem | 依赖 | 插件挂载点 |
| `GameStartPlugin_Tombstone` | LevelSystem | 相似 | 开局按格 `CreateChess` + `PlantChess`；读档跳过生成 |
| `ChessTeamManage.CreateChess` | Manage | 依赖 | 按 tag 进 Player/Enemy 队 |
| `Tile.PlantChess` | Map | 依赖 | MainPlant 写 stander |

### 需求类型判定理由
全新 GameStart 插件；不改现有插件接口，仅新增一类可配置插件。

### 已锁定规则
| 项 | 结论 |
|----|------|
| 阶段 | **GameStartPlugin** |
| 配置类型 | **PropertyCreator**（非运行时 Chess） |
| 失败策略 | **跳过**该条，继续下一条 |
| 读档 | **整插件跳过**（`SaveLoadContext.IsLoadFromSave`） |
| 成功路径 | `CreateChess(creator, tile, tag)` → `tile.PlantChess(chess)` |

### 集成点
- **调用**: `MapManage.tiles` / `IfInMapRange`、`ChessTeamManage.CreateChess`、`Tile.PlantChess`
- **触发**: `CreateChess` 内部会发 `WhenPlantChess`（非 forRestore）
- **新接口**: 无；新增插件类即可

### 对现有功能的影响
- **接口变更**: 无
- **行为变更**: 仅配置了该插件的关卡开局多一批单位
- **数据变更**: LevelData 序列化列表新增条目

## 技术要求
- **依赖模块**: LevelSystem、Map、Chess/Manage
- **性能要求**: 队列通常很小（个位数～几十），可接受
- **兼容性要求**: 向后兼容；未挂插件的关卡无影响
- **存档**: 读档跳过；场上植物由现有存档恢复

## 功能清单
### 核心功能（必须）
- [ ] 可序列化条目：`creator` / `pos` / `tag`
- [ ] `StadgeEffect` 按队列生成
- [ ] 越界 / null tile / creator 空 → 跳过
- [ ] 读档跳过
- [ ] `PlantChess` 写入格子

### 扩展功能（可选）
- [ ] 可选：生成前 `IfCanPlant` 检查（默认不做强制要求；失败跳过已覆盖占格冲突时可再加）
- [ ] Odin Label 中文提示

## 验收标准
- [ ] LevelData 配 2 条不同 pos/tag，开战可见对应单位
- [ ] 错误坐标不中断后续条目
- [ ] 读档后不重复生成插件配置单位（由存档恢复）
- [ ] 无关卡挂载时行为不变

## 风险评估
| 风险点 | 概率 | 影响 | 应对措施 |
|-------|------|------|---------|
| 与选卡阶段已种植物冲突 | 低 | 低 | 跳过；或关卡设计避开 |
| tag 拼写错误 | 中 | 中 | 默认 `"Player"`；文档注明 Player/Enemy |
| 未 PlantChess 导致不占格 | — | — | 强制调用 PlantChess |

## 关联 Context
- 涉及模块: LevelSystem, Map, Manage
- 参考: `context/modules/LevelSystem.md`
