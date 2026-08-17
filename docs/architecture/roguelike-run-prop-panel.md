# 架构 / 影响面: roguelike-run-prop-panel（RG-007）

## 系统定位
- **模块**: Roguelike UI（`RoguelikeRunInfoPanel`）+ Prop 数据只读
- **模式**: 与植物卡组 **共用** 弹层骨架（左列表 + 右详情），按 OverlayMode 切换内容

## 数据流
```
ownedPropIds (Run State)
    → RoguelikeRunPropPool.ResolveProps
    → 左侧 PropIcon（点击）
    → 选中 → 右侧 displayName / PropRarity / icon / effectDescription
```

## 稀有度
- 枚举 `PropRarity`：`Common / Uncommon / Rare / Legendary`（普通 / 稀有 / 史诗 / 传说）
- **不**复用 `Property.rarity`（int 刷怪权重）

## 主要改动
| 文件 | 动作 |
|------|------|
| `PropRarity.cs` / `PropItemData.cs` | 四档 + `rarity`；钉耙 Common、闪电 Uncommon |
| `PropIcon.cs` | 可选 `onClicked`（列表选中） |
| `RoguelikeRunInfoPanel.cs` | openProps / RefreshPropsList / ShowPropDetail |
| `肉鸽数据面板.prefab` | 道具按钮 + propIconPre 绑定 |
| `RoguelikeRunInfoFormatter` | Props HUD tooltip |

## 影响面
- 低：扩展 HUD；战斗 PropPanel / 领取流不改
- 回归：植物卡组开关、地图/商店/休息 HUD

## Skills
- `@unity-ui-system`、`@unity-prefab-system`
