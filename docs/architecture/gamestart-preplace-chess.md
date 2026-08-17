# 架构 / 影响面: GameStartPlugin_PrePlaceChess

**日期**: 2026-07-27  
**复杂度**: L1

## 系统定位
- **模块**: LevelSystem · GameStartPlugin
- **上游**: LevelData、LevelController 调用 `StadgeEffect`
- **下游**: ChessTeamManage、MapManage、Tile

## 设计方案（推荐）

```csharp
[Serializable]
public class PrePlaceChessEntry
{
    public PropertyCreator creator;
    public Vector2Int pos;
    public string tag = "Player";
}

[Serializable]
public class GameStartPlugin_PrePlaceChess : ILevelPlugin
{
    public List<PrePlaceChessEntry> entries;
    // StadgeEffect: if load save → return; else foreach spawn
    // OverPlugin: 空（单位随队伍清理）
}
```

路径对齐 `GameStartPlugin_Tombstone`：
1. `SaveLoadContext.IsLoadFromSave` → return  
2. `IfInMapRange` / tile / creator 校验，失败 continue  
3. `CreateChess` → `PlantChess`

**是否 IfCanPlant**：首版不强制（关卡配置信任）；占格冲突时 Create/Plant 仍可能叠，可选后续加检查后 skip。首版仅校验范围与 null。

## 影响面
| 文件 | 改动 |
|------|------|
| 新建 `GameStartPlugin_PrePlaceChess.cs` | 插件实现 |
| 现有文件 | **不改**（全新插件） |

间接：配置该插件的 LevelData asset。

## 风险
| 风险 | 评级 | 说明 |
|------|------|------|
| 叠种 | 低 | 跳过策略 + 关卡设计 |
| WhenPlantChess 触发羁绊等 | 低 | 与正常种植一致，通常期望行为 |

## 回归建议
1. 挂插件关：开局位置/阵营正确  
2. 错误坐标：后续条目仍生成  
3. 读档：无双份单位  
4. 未挂插件关：无变化  
