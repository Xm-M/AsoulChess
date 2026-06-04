# 功能需求卡片: 道具体系（Prop）

## 基本信息
- **提出日期**: 2026-05-19
- **状态**: P0 已落地（代码）

## 数据流
- 存档：`PlayerSaveData.ownedPropIds`
- 运行时：`GameManage.playerOwnedProps`（`PlayerSaveSystem.ApplyPlayerPropsToGame`）
- 全表：`GameManage.allProps`
- 本局：`PropController.ActiveProps`（`owned` 为空时回退 `allProps`，与 PlantsShop 一致）
- Test：同样走 `PlayerSaveContext.CurrentData.ownedPropIds`（`GameManage.Start` 会 `CreateNew` + Apply）

## Unity 配置清单（需手动）
1. **GameManage**（场景）：`propManage` 可留空（运行时 `new PropController()`）；`allProps` 填入所有 `PropItemData`。
2. **Resources/UIPrefab/PropPanel.prefab**：复制 `FetterPanel`，脚本换 `PropPanel`，字段绑定 `propIconPrefab`（可用 FetterIcon 预制体改 `PropIcon`）。
3. **关卡 LevelData**：`GameStartPlugin` 列表增加 `GameStartPlugin_Prop`（与 `GameStartPlugin_Fetter` 并列）。
4. **钉耙**：创建 `PropItemData`，`effect` 选 `PropEffect_Rake`，指定 `rakePrefab`（带 `TileEffect_Rake` + Trigger2D）；`propId` 写入 `ownedPropIds` 测试。

## 脚本目录
- `Assets/Script/Prop/`
- `Assets/Script/UI/View/PropPanel.cs`
- `Assets/Script/UI/Icon/PropIcon.cs`
