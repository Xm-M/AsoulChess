# 架构 / 影响面: plant-jacinta（嘉辛塔）

## 系统定位
- **模块**: Skill（主动 ColdSkill）+ PlantUmbrella 复用 + PropertyCreator 配置
- **归属**: 撑伞体系角色；产阳节奏对齐桃金娘，挡弹仅技能窗内

## 推荐方案
**单一 `ISkillEffect`（如 `SkillEffect_Jacinta`）**：
1. `SkillEffect`（动画 UseSkill）每次：产阳（复用 `SunLight` / 同 CreateSunLight 逻辑，`baseDamage[0]`）
2. 首次进入本段技能：启动与 `PassiveSkill_LettuceUmbrella` 同款挡弹 Tick；订阅 `onSkillOver` + `OnRemove` 清理
3. Prefab：`ColdSkill` + `DurationFinish` + `SkillRuntimeInfo_Duration.maxTime=25`；`SkillConfig_Cold.baseCd=15`

不新增运行时 MonoBehaviour。

### 备选（不推荐本期）
- 常驻被动挡弹 + 主动只产阳 → 与「仅技能期挡弹」不符
- 双 active MultySkill → 过重

## 数据流
```
CD ready → 播 skill → Duration 25s
  → Anim UseSkill → SkillEffect：产阳(+首次开挡弹)
  → SkillOver → 停挡弹 → ColdSkill t=0 → CD 15s
```

## 主要改动
| 文件 | 动作 |
|------|------|
| `Assets/Script/Skill/.../SkillEffect_Jacinta.cs`（路径按现有 PlantUmbrella/产阳目录） | 新建 |
| `Assets/SO/Skill/.../嘉辛塔.asset` | 新建 Cold 配置 |
| `Assets/Resources/ChessData/Player/嘉辛塔.asset` | 补全 |
| `Assets/Prefab/ChessPrefab/明日方舟/嘉辛塔/嘉辛塔.prefab` | 接线技能 |

## 影响面
- **改动类型**: 新增为主
- **影响文件**: ~4（1 新脚本 + SO/Prefab/Creator）
- **高风险**: 低（独立角色）
- **回归**: 桃金娘产阳、薇薇安挡弹、ColdSkill 持续技通用路径

## 测试建议
1. 种植 → 确认不普攻  
2. 技能中产阳次数/数值  
3. 技能中丢敌弹进 3×3 → 被弹开；结束后不再弹  
4. 技能结束后计时约 15s 再放  
5. 嘉辛塔死亡/铲除时挡弹 Timer 已停

## Skills
- `@unity-scriptableobject-config`
- 技能：现有 `ISkillEffect` + PlantUmbrella 白名单内 API
