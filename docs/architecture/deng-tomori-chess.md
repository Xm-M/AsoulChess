# 架构设计: 灯（高松灯升级 · 棋子模式）

## 系统定位
- **所属模块**: Skill（主动 + 被动）+ Chess（灯 / 棋子配置）+ UI（`ShopIcon.RefreshGood` 换卡）
- **上游**: Map 邻格、`PassiveSkill_Mygo` / `SkillReady_Mygo`、`LevelUpPlant`、`AnimatorController`、`SunLightPanel` / `PlantsShop`
- **下游**: 场上棋子实例、商店槽 Creator、灯死亡清场
- **横向**: MyGO 羁绊计数、土豆雷式触发、Wisadel 3×3 格伤

**归属确认**: 主逻辑在 Skill；商店换卡为薄 UI 调用，不新建商店子系统。

## 设计决策（推荐）

### 方案 A（推荐）：灯主动技变身 + 棋子独立被动 + 商店 `RefreshGood`
1. **灯**（`LevelUpPlant` → 常服高松灯）：挂 MyGO 邻格计数被动（复用/小改 `PassiveSkill_Mygo`，去重对齐 `fetterMemberId`，**不含自身**）+ `SkillReady_Mygo`（配置使邻格唯一数 ≥4）
2. **主动技** `SkillEffect_TomoriChessMode`：
   - `AttackAble=false` + `StopAttack`
   - `animatorController.ChangeFloat(1)`
   - 快照 ATK 写入 `skillController.context`
   - 在 `PlantsShop.currentShopIcons` 找到灯卡 → `RefreshGood(棋子Creator)`（参考注释中的 SliverAsh）
3. **棋子**（普通种植，价 25）：`PassiveSkillEffect_TomoriChessPiece` 检测敌人进入自身格/触发范围 → `WisadelGridHelper` 3×3 伤（灯 ATK×22.5）→ `Death()`
4. **买卡种植**：监听 `WhenPlantChess`（仅买卡主棋，非额外）：`PlayAttack()` 一次；按灯邻格 MyGO 数在主棋邻空**免费** `CreateChess`；写入灯 context 追踪列表
5. **上限**：自定义 `IfCanBuyCard`：场上棋子数 < 全场 MyGO `fetterMemberId` 去重数
6. **灯 OnRemove/Death**：清追踪棋子；`RefreshGood` 还原灯 Creator

**选择理由**: 全走 `ISkillEffect` / 既有 ShopIcon API；无技能专用运行时 MonoBehaviour；先例齐全（Wisadel Blend、MyGO Ready、RefreshGood）。

### 备选 B：棋子挂 TileEffect 碰撞组件
- 优点: 触发即时
- 缺点: 易滑向技能专用 MonoBehaviour；与项目技能规范冲突 → **放弃**

### 备选 C：全局 Event 驱动商店换卡，无灯侧持有
- 优点: 解耦
- 缺点: 多事件名、死亡还原难定位槽位 → 不优先

## 数据流

```
邻格 MyGO 去重计数 → context["mygo"]
        │
        ▼ SkillReady ≥4
主动技 → Blend=1 / 停攻 / 快照ATK / RefreshGood(棋子)
        │
玩家买棋子(25) → CreateChess(主) → PlayAttack(灯)
        │              └→ 免费邻空额外 CreateChess × 邻格MyGO数
        ▼
棋子被动: 敌人进范围 → 3×3 伤(快照ATK×22.5) → Death
灯死亡 → 清全部棋子 + RefreshGood(灯)
```

## 主要新增 / 修改

| 路径 | 类型 |
|------|------|
| `Assets/Script/Skill/ISkillEffect_Plant/Mygo_Skill/SkillEffect_TomoriChessMode.cs` | 新增主动 |
| `Assets/Script/Skill/ISkillPassive/Mygo/PassiveSkillEffect_TomoriChessPiece.cs` | 新增棋子被动 |
| `Assets/Script/Skill/ISkillPassive/Mygo/` 邻格计数（复用或薄封装 fetterMemberId） | 新增/小改 |
| `Assets/Script/Chess/PropertyCreator/IfCanBuyCard.cs`（或旁路新类）动态上限 | 新增 |
| `Resources/ChessData/Player/灯*.asset` + 棋子.asset | 配置 |
| Prefab：灯 / 棋子 | 资源 |
| `ShopIcon.RefreshGood` | **已有，调用即可** |

## 扩展点
- 爆炸倍率 / 范围可配
- 若多张灯卡：按 Creator 匹配换对应槽（本需求按单槽）

## 风险
| 风险 | 等级 | 缓解 |
|------|------|------|
| 商店换卡无活跃先例（仅注释） | 中 | 跟 `RefreshGood`；测 CD/价刷新 |
| MyGO 去重口径（chessName vs fetterMemberId） | 中 | 灯侧统一 fetterMemberId；与现网 Passive_Mygo 行为对齐文档 |
| 免费额外超上限 | 中 | 额外部署前检查上限；满则跳过 |
| 灯已死棋子仍读 ATK | 低 | 种下/开技时快照 |
| 额外 CreateChess 误触发买卡回调 | 中 | 上下文标记 `isExtraDeploy` 跳过 attack/再部署 |

## 复杂度
L2（接近 A：跨 Skill + Shop + 动态上限）。
