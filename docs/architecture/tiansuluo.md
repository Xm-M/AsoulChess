# 架构设计: 天素罗

## 系统定位
- **所属模块**: Skill（被动计时转化）+ Chess（配置/Prefab）
- **上游**: `TimerManage`、`ChessTeamManage.CreateChess`、`Tile` / `PlantChess`
- **下游**: 同格生成「长崎素世」；MyGO `fetterMemberId` 与素世共享成员位
- **横向**: 南瓜罩（`OnPlant` 换绑 Main）、气球僵尸落地（死→同格生成先例）

```
TimerManage ──► PassiveSkillEffect_TianSuoLuo ──► Death
                         │
                         └─ OnRemove(转化标志) ──► CreateChess(长崎素世, tile)
                                                          │
Tile.OnPlant ◄────────────────────────────────────────────┘
      │
      └─ PassiveSkillEffect_PumpkinShell.ProtectChess（若有罩）
```

## 设计决策（推荐）

### 方案 A（推荐）：被动 + 一次性 Timer + OnRemove 生成
`PassiveSkillEffect_TianSuoLuo`（名可微调）：
1. `SkillEffect` 入场：缓存 `standTile`；`AddTimer(OnMature, delaySeconds, false)`
2. `OnMature`：置 `_shouldTransform = true`，调用 `user.Death()`
3. `user.OnRemove`：若 `_shouldTransform` 且 tile 有效 → `CreateChess(soyorinCreator, tile, tag)` 并 `ResumeState`（对齐气球落地）
4. 任意离场路径：`Stop` Timer；非成熟死亡则 `_shouldTransform == false`，不生成

**理由**：与 `PassiveSkillEffect_BalloonZombiePop` 同构；先清 Main/`stander` 再种素世，南瓜靠现有 `Tile.OnPlant` 换绑。

### 备选 B：Timer 回调内先 Create 再 Death
- 优点：同帧内立刻有新棋
- 缺点：短暂双 Main / `stander` 覆盖顺序难推理；放弃

### 备选 C：独立 MonoBehaviour 倒计时
- 违反技能勿 `AddComponent` 规范；放弃

### 身份与数值
| 字段 | 值 |
|------|-----|
| `chessName` | 天素罗（长崎素世） |
| `fetterMemberId` | 长崎素世 |
| `plantTags` | Mygo（+ 可选贝斯） |
| `plantType` | MainPlant |
| 战斗 | 高防、0 攻、挡路（对齐「小坚果」定位；具体 Hp 策划调） |
| `soyorinCreator` / `delaySeconds` | Prefab 序列化可配 |

## 主要新增 / 修改
| 路径 | 动作 |
|------|------|
| `Assets/Script/Skill/ISkillPassive/Mygo/PassiveSkillEffect_TianSuoLuo.cs` | 新增 |
| `Assets/Resources/ChessData/Player/天素罗.asset` | 补全字段 + MainPlant |
| `Assets/Prefab/ChessPrefab/Mygo/天素罗/天素罗.prefab` | 挂被动、引用素世 Creator |
| （可选）allChess / 图鉴注册 | 按现有单位入库习惯 |

## 风险
| 风险 | 等级 | 缓解 |
|------|------|------|
| Death→Create 与南瓜换绑 | 中 | 测：罩在 → 到期 → 罩仍护素世；确认旧 Main 监听随回收失效 |
| Timer 泄漏 | 低 | OnRemove / LeaveWar 必 Stop |
| asset 空壳漏填 | 中 | 对照压力希/素世 checklist |
| 提前死亡误刷 | 低 | 仅成熟标志为真时生成 |

## 复杂度
L1–L2（单被动 + 配置接线）。
