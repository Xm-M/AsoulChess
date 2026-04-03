# 功能需求卡片: 植物图鉴面板

## 基本信息
- **功能名称**: 图鉴（已拥有棋子展示）
- **所属模块**: UI / 养成展示
- **需求类型**: 新增功能（只读展示，不改存档结构）
- **优先级**: P1
- **预估复杂度**: L2
- **预估耗时**: 4–8 小时（含预制体与布局）
- **提出日期**: 2026-04-02

## 功能描述
### 详细描述
在主界面可打开图鉴面板。左半区列出玩家**当前已拥有**的植物棋子（与存档 `ownedCreatorIds` / `GameManage.playerOwnedCreators` 一致），每项显示 `chessSprite` 与 `chessName`。点击某项后，右半区显示棋子名称、`Property.baseProperty` 中的主要属性、以及 `chessDescription` / `chessEffect` 等介绍文本；介绍区域使用 `ScrollRect` 支持长文滑动。

### 用户故事
作为玩家，我希望在图鉴中查看我已获得的植物及其数值与背景介绍，以便理解养成进度与战术定位。

## 现有业务上下文

### 相关现有功能
| 功能 | 模块 | 关系 |
|------|------|------|
| 拥有列表 | `PlayerSaveData.ownedCreatorIds` | 数据源 |
| 同步 | `PlayerSaveContext.ApplyPlayerChessToGame` | 进入游戏后填充 `playerOwnedCreators` |
| 配置 | `PropertyCreator` | 名称、图标、描述、`baseProperty` |
| UI 模式 | `View` + `UIManage` | 与 `CoinShopPanel` 一致 |

### 集成点
- **读取**: `GameManage.instance.playerOwnedCreators`
- **入口**: `StartUI.codexButton`（可选，需在预制体中绑定）

### 对现有功能的影响
- **存档**: 无变更
- **行为**: 仅新增 UI 与可选按钮

## 技术要求
- **Unity**: 与工程一致；uGUI + TMP
- **依赖**: `GameManage`、`PropertyCreator`、`Tile`/`PlantType` 枚举
- **性能**: 列表项数量等于拥有植物数，通常很小

## 功能清单
### 核心功能（必须）
- [x] `CodexPanel` 脚本与 `View` 集成
- [ ] Unity 中创建 `Resources/UIPrefab/CodexPanel` 预制体（布局、ScrollRect、选项卡预制体）
- [ ] 主界面图鉴按钮绑定（可选）

### 扩展功能（可选）
- [ ] 未拥有棋子的锁定展示
- [ ] 本地化/枚举中文展示优化

## 验收标准
- [ ] 仅显示已拥有植物，信息与 `PropertyCreator` 一致
- [ ] 右侧介绍过长时可滚动
- [ ] 关闭面板可返回主界面

## 关联 Context
- `workflow_v2/context/modules/chess-data.md`
- `workflow_v2/context/modules/ui.md`
- `workflow_v2/context/modules/save-player.md`
