# 架构设计: 推车列铺冰 — 小推车冰块占位

## 系统定位
- **所属模块**: `Effect_Snow`（冰块生命周期中枢）
- **上游**: `PlaceOrRefreshIce` / BFS 铺冰、`GridFindTargetGeometry` 推车列解析
- **下游**: `ChessTeamManage` 棋子创建、`Tile.PlantChess`
- **横向**: `EnterWarPlugin_CarCreate`（小推车实体，只读识别）

## 设计决策

### 推荐方案（已采用）
在 `Effect_Snow` 内集中管理 **冰格 → 占位植物** 映射，铺冰成功后尝试生成，冰注销时销毁。

**理由**:
- 与冰块生命周期一致，融化路径已有 `UnregisterIce`
- 不修改 `IceCell` 预制体或 `CarArmor`
- 占位资源通过 `PropertyCreator` 可配置，便于日后换冰块 SO

### 备选方案（未采用）
| 方案 | 优点 | 缺点 |
|------|------|------|
| 在 `IceCell` 上挂子物体 Sprite | 无棋子副作用 | 需新预制体变体，与种植系统脱节 |
| 扩展南瓜罩被动保护 `CarArmor` | 玩法统一 | 超出当前「纯视觉占位」范围 |

## 数据流

```
PlaceOrRefreshIce(mapPos)
    → mapPos.x == -1 ?
    → TryResolveTileAt → roomTile[row]
    → FindLawnMowerOnTile (CarArmor)
    → ResolveIceCoverCreator (南瓜罩)
    → CreateChess + PlantChess
    → _iceCoverByMapPos[mapPos] = cover

IceCell.Melt → UnregisterIce(mapPos) → RemoveIceCoverAt → cover.Death()
```

## 改动文件
| 文件 | 变更 |
|------|------|
| `Assets/Script/Effect/Effect_Snow.cs` | 占位生成/回收逻辑 |
| `Effect_Snow` 预制体（可选） | 绑定 `iceCoverOnMowerCreator` |

## 风险等级
**低** — 局部扩展，不改铺冰/伤害主路径。
