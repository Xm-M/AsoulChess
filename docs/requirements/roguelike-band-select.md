# 功能需求卡片: 肉鸽开局乐队选择

## 基本信息
- **功能名称**: 肉鸽开局乐队选择面板
- **所属模块**: Roguelike / UI
- **需求类型**: 功能扩展
- **优先级**: P0
- **提出日期**: 2026-05-20

## 功能描述
新开局流程改为：主菜单「开始新游戏」→ 乐队选择面板 → 确认后生成地图并进入 Run。类似杀戮尖塔选角色/初始卡组。

### 用户故事
作为玩家，我希望在开 Run 前选择乐队并预览初始植物，以便每局有不同的开局体验。

## 数据
- `BandMes`：背景图、乐队名、简介、`List<PropertyCreator> startingMembers`
- `RoguelikeBandCatalog`（SO）或 `RunMapConfig.startingBands` 保存全部乐队
- Run 存档写入 `RoguelikeRunState.selectedBandId/Name` 与 `ownedPlantCreatorIds`

## 流程
```
StartUI.StartRoguelikeRun → RoguelikeBandSelectPanel
  → 左右切换 BandMes
  → 开始挑战 → RoguelikeRunService.StartNewRun(config, band) → RoguelikeMapPanel
StartUI.ContinueRoguelikeRun → 仍直接进地图（不经过选乐队）
```

## 预制体（手动）
1. 在 `Resources/UIPrefab/` 创建 `RoguelikeBandSelectPanel.prefab`
2. 根物体挂 `RoguelikeBandSelectPanel`，name 必须为 `RoguelikeBandSelectPanel`
3. 拖 UI 引用：背景 Image、乐队名/简介 TMP、左右按钮、开始/返回、**成员站位 `memberPos`（Transform 列表，Camera 场景内）**
4. 可选：面板或 RunMapConfig 上指定 `RoguelikeBandCatalog`
5. 成员展示：`RefreshMemberPreviews` 在 each `memberPos` 下 `Instantiate(creator.chessPre)`，逻辑同 `CodexPanel` 预览

## 动画

| 状态名 | 用途 | Animation Event |
|--------|------|-----------------|
| `change` | 左右切换乐队 | 0.5s `OnBandSwitchAnimationApply`；1s `OnBandSwitchAnimationFinished` |
| `leave` | 点击「开始挑战」离场 | 末帧 `OnLeaveAnimationFinished` → 进入地图 |

观众 `livehouse (1)` Animator：开始挑战时播放 `欢呼`。

## 验收标准
- [ ] 新开局先进入选乐队，确认后地图正常生成
- [ ] 所选乐队初始植物写入 Run 植物池
- [ ] 继续冒险不经过选乐队
- [ ] 左右切换刷新背景/文案/成员 3D 预览
- [x] 开始挑战：播放 `leave` + 观众 `欢呼`，期间按钮不可点，动画结束进地图
