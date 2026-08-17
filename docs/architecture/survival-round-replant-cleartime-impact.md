# 影响面分析: survival-round-replant-cleartime

**日期**: 2026-08-14  
**改动类型**: 修改现有功能（生存轮界 + 存档）

## 1. 改动范围（预估）

| 文件 | 改动 |
|------|------|
| `LevelController_Endless.cs` | 轮末 ClearTime；GameStart 末尾灭→重种；EnterMap 展示种 |
| `SaveSystem.cs` / `CapturePlayerPlants` | 生存跳过 buffs（或 Capture 参数） |
| `LevelController.cs` `RestorePlayerPlants` | `restoreBuffs` 开关；生存默认 false |
| `Buff_Mujica_Doloris.cs`（可选） | `buffFrom==null` 防护 |
| 可能：`ChessTeamManage` / 静默移除植物工具 | 避免 Death 副作用 |

## 2. 依赖

- **被依赖**: Map Timeline EnterMap/GameStart、PlantsShop、羁绊 GameStart 插件
- **依赖**: TimerManage、Chess 进战/离场、Tile.PlantChess

## 3. 影响评估

| 影响项 | 程度 | 说明 |
|--------|------|------|
| 生存轮间植物实例身份 | 高 | 每轮开战重建，外部若缓存 Chess 引用会失效（插件应在重种后不再持有旧引用） |
| 生存存档 | 中 | 不再恢复战斗 Buff |
| 冒险存档 | 无（约定） | 保持原逻辑 |
| 阳光 Timer | 低 | ClearTime + 已有清阳光 |

## 4. 风险

| 风险 | 概率 | 影响 | 应对 |
|------|------|------|------|
| 静默移除不彻底导致格子占用 | 中 | 高 | 测 PlantChess 前 stander 清空 |
| 羁绊插件跑在展示种上、重种后丢失 | 中 | 高 | 重种后补 Trigger 或插件改为监听重种完成 |
| ClearTime 清掉选卡阶段仍需的 Timer | 低 | 中 | ClearTime 放在轮末 Pause 后；选卡阶段勿依赖局内 Timer |

## 5. 回归测试建议

- [ ] 生存第 1→2 轮：选卡可见植物；开战无 MissingReference；植物会攻击
- [ ] 含 Mujica/Doloris 阵容：轮末存档、读档进轮不 NRE；开战后协战 Buff 由被动重挂
- [ ] 羁绊/GameStart 插件：加成在开战后仍存在
- [ ] 读档：场上阵容与存档一致；阳光/手牌仍按既有生存存档逻辑
- [ ] 冒险关读档中途（若有）：Buff 仍恢复（确认未被生存改动误伤）
- [ ] 轮末清阳光 + ClearTime 后无报错
