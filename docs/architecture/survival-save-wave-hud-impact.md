# 影响面分析: survival-save-wave-hud

**日期**: 2026-08-14  
**改动类型**: 修改现有功能（存档/重开/HUD）

## 1. 改动范围

| 文件 | 方法/点 | 类型 |
|------|---------|------|
| `PreParePlugun_ShowPlantShop.cs` | ClearLockedHand, StadgeEffect Prefill 条件 | 逻辑 |
| `LevelManage.RestartLevel` | 删档后清插件缓存 | 逻辑 |
| `LevelController_Endless.cs` | RunRoundEnter 读档波次；轮间/离场补存 | 逻辑 |
| `SaveSystem.cs` | 生存暂停态可存（可选窄接口） | 逻辑 |
| `ProgressBar.cs` | stadgeName 生存格式 | UI |

无 Prefab 强制改动；无存档 version bump（字段沿用）。

## 2. 依赖关系

```
LevelManage.RestartLevel
  → SaveSystem.DeleteSave
  → ClearLockedHand (新)
  → LoadScene

ChangeLevel (有档)
  → SaveLoadContext
  → MapManage_PVZ 读档流
  → Endless.EnterMap.RestorePlayerPlants

ProgressBar.Show/MoveBar
  → LevelManage.currentLevel / currentController 波次
```

## 3. 影响评估

| 影响项 | 程度 | 说明 |
|--------|------|------|
| 生存重开选卡 | 高 | 预期变正确（空选卡） |
| 生存读档草坪 | 高 | 必须恢复植物 |
| 冒险存档门闸 | 中 | 若改 SaveSystem 必须分支隔离 |
| 肉鸽 ProgressBar 标题 | 低 | 保持 Roguelike 格式优先 |
| 生存波次显示 | 中 | 仅文案 |

## 4. 回归测试建议

1. 生存：种植物 → 暂停重开 → 无预选卡、草坪空、阳光初始  
2. 生存：种植物 → 打完一轮到选卡 → 回主菜单再进 → 植物在  
3. 生存：战斗中途退出再进 → 植物在、波次尽量正确  
4. 冒险：有档续关 / 重开 — 行为与改前一致  
5. 肉鸽：ProgressBar 仍为 `Act · Ln · 关卡名`  
6. 生存：ProgressBar 为 `关卡名 · 第X/Y波`，进波数字更新  

## 5. 风险小结

- 高风险点: 1（SaveSystem 门闸若改错）  
- 回归项: 6  
- 结论: 可改；Save 放宽必须 Survival-only
