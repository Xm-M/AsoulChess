# 功能需求卡片: 推车列铺冰 — 小推车冰块占位

## 基本信息
- **功能名称**: 推车列有冰且格上有小推车时生成冰块占位植物
- **所属模块**: Effect（`Effect_Snow`）+ Map（`roomTile`）+ LevelSystem（小推车）
- **需求类型**: 功能扩展
- **优先级**: P1
- **预估复杂度**: L1
- **预估耗时**: 2–4 小时
- **提出日期**: 2026-07-06
- **关联需求**: `ice-hatsuyuki-enter-damage`（推车列铺冰）

## 功能描述

### 详细描述
当 **推车列**（`MapManage_PVZ.roomTile`，逻辑坐标 `x = -1`）成功铺冰，且该格上存在 **存活的小推车**（带 `CarArmor`）时，在小推车所在格 **系统生成一株占位植物**，表现「小推车被冻在冰里」。

- **目标美术**：冰块罩住小推车（尚未有资源）
- **当前占位**：使用 **南瓜罩**（`南瓜罩.asset` / Support 预制体）配置，仅作视觉占位
- **不启用**南瓜罩对 Main 的承伤逻辑（小推车不是 `PlantType.MainPlant`）

### 用户故事
作为玩家，当圣聆初雪把推车列冻住时，我希望看到小推车像被冰块包住，以便感知冰冻范围覆盖到推车列。

## 现有业务上下文

### 相关现有功能
| 功能 | 模块 | 关系 | 说明 |
|------|------|------|------|
| 推车列铺冰 | `Effect_Snow` | 扩展 | 已实现 `LawnMowerColumnMapX = -1` |
| 小推车生成 | `EnterWarPlugin_CarCreate` | 依赖 | `roomTile` 上 `CreateChess`，移出 Player 队伍 |
| 南瓜罩 | Support 植物 | 占位资源 | `SupportPlant` 可与同格非 Support 重复类型共存 |
| 冰块融化 | `IceCell.Melt` | 生命周期 | 融化时注销 `Effect_Snow` 登记 |

### 需求类型判定理由
在已有推车列铺冰能力上增加 **视觉反馈**，不新建模块。

### 集成点
- **调用**: `PlaceOrRefreshIce` 成功后 → `TrySpawnIceCoverOnLawnMower`
- **识别小推车**: 格上 `chessesIntile` 中含 `CarArmor` 子组件
- **生成**: `ChessTeamManage.CreateChess` + `Tile.PlantChess`
- **回收**: `UnregisterIce` / `ClearAllIce` → 占位植物 `Death()`

### 对现有功能的影响
- **行为**: 推车列有冰+有推车时出现额外 Support 棋子；冰融化时移除
- **接口**: `Effect_Snow` 新增可选 Inspector 字段 `iceCoverOnMowerCreator`
- **小推车移动**: 推车离场后占位可能留在原格（边缘情况，P2 可跟车销毁）

## 技术要求
- **依赖**: `Effect_Snow`、`PropertyCreator`（南瓜罩）、`CarArmor`
- **性能**: 每行至多 1 株占位，字典跟踪
- **兼容性**: 未配置 creator 时回退 `GameManage.allChess` 按名查找「南瓜罩」

## 功能清单

### 核心功能（必须）
- [x] 推车列铺冰成功且格上有存活小推车 → 生成占位植物
- [x] 同格已有占位则不重复生成
- [x] 冰融化 / 清场 → 移除占位植物
- [x] Inspector 可配置占位 `PropertyCreator`，默认南瓜罩

### 扩展功能（可选）
- [ ] 冰块专用美术资源替换南瓜罩配置
- [ ] 小推车启动离场时同步移除占位
- [ ] 占位植物替小推车承伤（需扩展 `PassiveSkillEffect_PumpkinShell`）

## 验收标准
- [ ] 初雪 BFS 铺到推车列且该行有小推车 → 出现南瓜罩视觉
- [ ] 冰融化后占位消失
- [ ] 无小推车的推车列铺冰 → 不生成占位
- [ ] 主战场铺冰行为不变

## 风险评估
| 风险点 | 概率 | 影响 | 应对措施 |
|-------|------|------|---------|
| 占位与小推车渲染层级 | 中 | 低 | 后续调 Sorting Order |
| 推车离场占位残留 | 低 | 低 | P2 监听 `ChessLeave` |
| 南瓜罩进 Player 队伍计数 | 低 | 低 | 系统生成，非玩家种植 |

## 关联 Context
- 涉及模块: Map, Effect, LevelSystem
- 参考: `context/modules/Map.md`, `docs/requirements/ice-hatsuyuki-enter-damage.md`
