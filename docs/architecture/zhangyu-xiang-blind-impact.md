# 影响面分析: 章鱼祥致盲被动

## 改动类型

功能扩展（新被动 + 新 Buff + Property 轻量钩子）

## 改动范围

| 文件 | 类型 | 说明 |
|------|------|------|
| `Assets/Script/Chess/Property.cs` | 修改 | `onBeforeTakeDamage`；Miss 飘字 |
| `Assets/Script/Skill/ISkillPassive/Mygo/PassiveSkillEffect_ZhangyuXiang.cs` | 新增 | 25% 致盲 |
| `Assets/Script/Buff/Buff_Blind.cs`（或并入合适 Buff 文件） | 新增 | 致盲 TimeBuff |
| `Assets/Resources/ChessData/Player/章鱼祥.asset` | 修改 | 对齐八九寺 |
| `Assets/Prefab/ChessPrefab/Mygo/章鱼祥/章鱼祥.prefab` | 修改 | 武器/被动/状态图 |
| 棒球相关（条件） | 小改 | Miss 飘字抑制 |

## 依赖关系

```
PassiveSkillEffect_ZhangyuXiang
  → PropertyController.onTakeDamage
  → BuffController.AddBuff(Buff_Blind)
Buff_Blind
  → PropertyController.onBeforeTakeDamage
  → DamageType.Miss → GetDamage / DamagePanel.ShowMiss
章鱼祥 Prefab
  → 同八九寺：StraightLaser + ShootBullet + stateGraph
```

## 影响评估

| 影响项 | 程度 | 说明 |
|--------|------|------|
| 八九寺 | 无 | 不改其 Prefab/数据 |
| 全体 `TakeDamage` | 低 | 多一次空事件 Invoke（无监听时成本极低） |
| 显式 Miss 伤害飘字 | 低 | 行为对齐闪避；喂球需核对 |
| 存档 | 低 | BuffFactory 按类名反射，新 Buff 自动可创建 |

## 高风险点

1. Prefab SerializeReference 配置错误 → 被动/武器不生效  
2. `onBeforeTakeDamage` 未在所有伤害入口调用（仅 `TakeDamage`；直接调 `GetDamage` 的路径不致盲——与「攻击」语义一致）

## 回归测试建议

- [ ] 章鱼祥射击与八九寺手感/射程一致
- [ ] 连续攻击观察致盲触发与 2s 刷新
- [ ] 致盲僵尸啃咬/射击植物：飘 Miss、植物 HP 不变
- [ ] 致盲期间治疗（若有）仍正常
- [ ] 八九寺关卡抽测无回归
- [ ] 棒球喂球无异常 Miss 飘字（若有则 suppress）
