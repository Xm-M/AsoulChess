# 架构设计: 灯石头 · MyGO 成员贴图

**需求**: `docs/requirements/deng-tomori-chess-member-sprites.md`  
**日期**: 2026-07-24  
**复杂度**: L1

## 1. 系统定位

- **所属模块**: Skill（`PassiveSkillEffect_TomoriHost`）
- **上游**: `PassiveSkill_Mygo.nearChess`、`CreateChess`
- **下游**: 石头 `SpriteRenderer` / `AnimatorController.sprite`
- **横向**: 无商店 / 爆炸逻辑改动

模块归属：Skill 宿主扩展（正确）。

## 2. 设计决策（推荐）

**推荐方案**: 宿主序列化映射表 + 额外部署队列带 `memberId` + 种完写 Sprite

```csharp
[Serializable]
public class TomoriMemberStoneSprite {
    public string memberId; // 高松灯 / 千早爱音 / …
    public Sprite sprite;
}
// PassiveSkillEffect_TomoriHost:
public List<TomoriMemberStoneSprite> memberStoneSprites;
// 队列: (Tile tile, string memberId)
```

| 方案 | 优点 | 缺点 |
|------|------|------|
| **A. 映射表 + 写 Sprite（推荐）** | 与现架构一致；你自行拖切片 | 须处理 Animator 刷 Sprite |
| B. 每成员一个 Prefab | 动画完全独立 | 资源膨胀；额外部署换 Creator 复杂 |
| C. AnimatorOverride 每成员一套 | 动画友好 | 配置重；5 套 Override |

**Animator**: 用户确认 idle 仅有 sprite **运动/位移曲线**、无换帧 → 种完赋一次 `SpriteRenderer.sprite` 即可，不必去曲线或强制写回。

数据流:
```
买卡种主棋 → 默认外观
→ 复制 nearChess 顺序
→ 可种邻格排队 (tile, memberId)
→ CreateChess → 查表设 sprite
```

## 3. 影响面（预估）

| 文件 | 改动 |
|------|------|
| `PassiveSkillEffect_TomoriHost.cs` | 映射表、队列、ApplySprite |
| 可选 `TomoriChessKeys.cs` | 小结构体 / Apply 工具 |
| `石头/动画/idle*.anim` | 若选方案 A1：去 sprite 曲线 |
| 灯 Prefab 上 Host 序列化 | 你拖 5 张 `棋子_0`～`棋子_4` |

爆炸、Cap、Support、商店：无改。

## 4. 风险

| 风险 | 概率 | 影响 | 应对 |
|------|------|------|------|
| 动画刷掉换图 | 高 | 中 | 去曲线或强制写回 |
| id 拼写不一致 | 中 | 低 | 回退默认；文档列标准 id |
| 第 6 切片未用 | — | — | 忽略或留作日后默认 |

## 5. 影响面分析（impact）

- **改动类型**: 修改现有功能（表现）
- **直接影响**: TomoriHost 额外部署路径
- **间接**: 石头 idle 若去曲线，默认外观完全依赖 Prefab SR（当前已是 `棋子_0`）
- **回归**: 买卡默认图；邻格 1～4 额外图；未配置回退；爆炸仍正常

回归清单:
1. 无映射表 → 全默认  
2. 配齐 5 人 → 额外棋对应成员图且不被 idle 刷回  
3. 空位不足 → 少种不崩  
4. 灯死亡清棋 / 商店还原  
