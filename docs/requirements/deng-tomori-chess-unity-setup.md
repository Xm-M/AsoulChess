# 灯 / 棋子 — Unity 接线清单

脚本已落地，Prefab 技能槽需在编辑器内 SerializeReference 配置（Unity MCP 当前不可用）。

## 已有资源
| 资源 | 路径 |
|------|------|
| 灯 PropertyCreator | `Assets/Resources/ChessData/Player/灯.asset`（LevelUp→常服高松灯，价 125） |
| 棋子 PropertyCreator | `Assets/Resources/ChessData/Player/棋子.asset`（价 25，`TomoriChessCap_Limit`） |
| 灯 Prefab | `Assets/Prefab/ChessPrefab/Mygo/灯/灯.prefab` |

## 灯 Prefab · 已接线（`Assets/Prefab/ChessPrefab/Mygo/灯/灯.prefab`）

- **武器**: `Weapon_Sample` + `StraightFindTarget` + `ShootBullet`（常服灯同款子弹）
- **被动**: `PassiveSkill` → `PassiveSkillEffect_TomoriHost`（`棋子.asset`）
- **主动**: `ColdSkill`（config=`SO/Skill/Mygo/高松灯.asset`）
  - Ready: `MouseDown` + `SkillReady_Mygo(min=3)` + `SkillReady_TomoriNotChessMode`
  - Effect: `SkillEffect_TomoriChessMode`（Blend=1，同 `棋子.asset`）
  - Finish: `AnimFinish`
- **状态图**: 与常服高松灯相同 MyGO 植物图
- **点击盒**: 根节点 `BoxCollider2D`（兼作技能点击）

## 仍需你做的

### 棋子 Prefab（需新建）
1. 复制简易植物 Prefab，命名 `棋子`
2. `棋子.asset` 的 `chessPre` 指到该 Prefab 的 `Chess`
3. **passiveSkill** → `PassiveSkill`，effect：`PassiveSkillEffect_TomoriChessPiece`（`damageMultiplier=22.5`）
4. **不要**加 Mygo tag

### Animator
灯 Controller 需有 **Blend** Float 与 **attack** / **skill** 状态；skill 动画帧事件调用 `UseSkill`。
