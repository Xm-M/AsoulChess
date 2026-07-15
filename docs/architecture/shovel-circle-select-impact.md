# 影响面分析: 铲子圆圈检测与优先级铲除

## 改动范围
| 文件 | 改动类型 | 说明 |
|------|----------|------|
| `Assets/Script/UI/Shop/HandItem/ShovelPanel.cs` | 修改 | 圆检测、优先级、花盆规则、写名称 |
| `Assets/Script/UI/Shop/PrePlantImage.cs` | 修改 | 名称 Text 引用、半径配置、显隐清理 |
| `PrePlantImage` Prefab（Resources/场景） | 资源 | 增加文本子节点并绑定 |
| （可选）小 helper `.cs` | 新增 | 若选中逻辑抽出 |

**不改**: `PlantsShop.DigPlant` / `ConveyorPanel.DigPlant` 签名；`Chess.Death`；`PlantType` 枚举。

## 依赖
- 被入口调用：商店/传送带 → `PrePlantImage.TryToPlant(..., Shovel)`
- 依赖：`PlantType`、`chessName`、Layer 7、`Death()`

## 影响评估
| 影响项 | 程度 | 说明 |
|--------|------|------|
| 铲子手感/误铲邻格 | 中 | 半径默认 1，需实机调 |
| 南瓜罩+主植物同格 | 高（预期） | 改为先铲辅助 |
| 花盆+主植物 | 高（预期） | 不可铲花盆 |
| 锤子/种植手持物 | 无 | 独立 Panel |
| 存档 | 无 | |

## 风险
| 风险 | 等级 | 缓解 |
|------|------|------|
| Prefab 未绑 Text | 低 | null 安全 |
| 半径扫到邻行植物 | 中 | 可调半径 + 验收 |
| UnSelectable 暂不可铲 | 低 | 需求已确认；后续单开 |

## 回归测试建议
1. 单主植物铲除 + 名称
2. 南瓜罩+主植物 → 先铲罩
3. 两株辅助（若可同屏近距）→ 铲更近
4. 花盆+主 → 只能铲主；仅花盆 → 可铲盆
5. 消耗品种植后铲子不选中
6. 武装土豆雷暂不可铲
7. 左键空白取消；右键取消；音效 cancel/dig
8. 传送带关与普通商店入口各测一次
9. 离开关卡 `ForceHide` 后名称不残留
