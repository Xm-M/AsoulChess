# 架构设计: 铲子圆圈检测与优先级铲除

## 系统定位
- **所属模块**: UI 手持物（`ShovelPanel` / `PrePlantImage`）
- **上游**: `PlantsShop.DigPlant`、`ConveyorPanel.DigPlant`
- **下游**: `Chess.Death()`、音效回调
- **横向**: `PlantType`、Layer 选中语义

## 推荐方案
在 `ShovelPanel` 内实现「每帧解析目标 + 左键执行」；名称展示由 `PrePlantImage` 提供 TMP/Text 引用，铲子协程写入。

### 数据流
```
鼠标世界坐标
  → OverlapCircle(radius)
  → 过滤候选（存活 / 非 Consume / 可选 Layer / 花盆规则）
  → 排序（Support > Main > 仅 Pot；同级距离）
  → 更新名称文本
  → 左键：Death(当前目标) 或 Cancel
```

### 设计要点
1. **选中逻辑内聚**于 `ShovelPanel`（或同目录静态 helper），避免改 `Chess` / `Tile` API。
2. **半径**序列化在 `ShovelPanel` 或经 `PrePlantImage` 配置（默认 1）。因 `ShovelPanel` 当前为纯 C# 非 MonoBehaviour，半径建议放在 `PrePlantImage` 序列化字段，传入/读取；或把半径做成 `ShovelPanel` 静态/构造默认值 + `PrePlantImage` 可覆盖。
3. **名称 UI**：`PrePlantImage` 子节点 TMP_Text，无目标时 `text=""` 或 `SetActive(false)`。
4. **花盆规则**：候选集中若存在非 Pot 的合法目标，则从候选中移除所有 Pot；若仅剩 Pot，则可选 Pot。
5. **Consume**：过滤掉，不进入候选。
6. **UnSelectable**：继续只检测原植物 Layer（`1<<7`），自然排除；后续若要可铲，再扩 mask。

### 备选（不采用）
- 先找 Tile 再读 `chessesIntile`：同格更准，但与「圆圈检测」确认方案 A 不符。

## 风险等级
**低–中**（局部 UI 逻辑；行为对玩家可见）

## 建议 Skills
- `@unity-ui-system`（TMP 文本挂载）
- 物理检测用现有 `Physics2D.OverlapCircle`（项目内多处已用）
