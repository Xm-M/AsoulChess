# 影响面分析: hanekawa-tsubasa-black-form

**日期**: 2026-08-17  
**改动类型**: 新增功能（半成品接线）+ 局部 Tile 占格约定

## 1. 改动范围

### 新增（预估）
| 文件 | 说明 |
|------|------|
| `PassiveSkillEffect_Hanekawa.cs`（建议路径 `Skill/ISkillPassive/Story/`） | 挂压、变身、降压、索敌移动、回格、Cleanup |
| （可选）小工具方法同文件内 | 最近邻格选取、预订 API |

### 修改
| 文件 | 说明 |
|------|------|
| `羽川翼.asset` | 名称、属性、描述、chessPre |
| `羽川翼.prefab` | 被动、产阳 ColdSkill、近战武器、Animator、stateGraph |
| Animator Override | 黑/白形态状态 |

### 原则上不改
- `MainPlant` / `Tile` 公共 API（用 stander 预订约定）
- 全局压力 Buff 基类行为（只读/BuffReset）
- 其他产阳植物

## 2. 依赖关系
```
PassiveSkillEffect_Hanekawa
  → Buff_StressBuff_Death
  → MoveController.MoveToTarget
  → IGridFindTarget / Weapon CloseAttack
  → PropertyController HP / LifeSteal
  → Tile.stander（预订）
  → AnimatorController
  ← ColdSkill CreateSunLight（Prefab 配置，黑形态需暂停）
```

## 3. 影响面评估

### 直接影响
| 项 | 程度 | 说明 |
|----|------|------|
| 羽川翼战斗行为 | 高 | 全新逻辑 |
| 原格种植占用 | 中 | 离格期间 stander 预订 |
| 压力体系交互 | 中 | 吃友希那等加压 |

### 间接影响
| 项 | 验证 |
|----|------|
| 南瓜/保护类读 stander | 原格仍显示有主植物？需确认是否合理 |
| 铲除原格 | 若只铲 stander 位置而棋子在外 → OnRemove 清理 |
| 存档中途 | 若关卡可存盘，形态/预订是否入档（建议首版不入档或随棋子状态重建） |

### 潜在影响
- 多只羽川翼同时出击：互不影响（各订各 home）
- 黑形态占邻格 standTile 但不占 stander：僵尸吃法/碾压是否只认 stander — 需玩法回归

## 4. 风险与回归测试

| 风险 | 等级 | 回归项 |
|------|------|--------|
| 预订泄漏导致永不可种 | 高 | 死亡/铲除/回格后原格可种 |
| 回格 Death 误触 | 中 | 空原格正常回家；被占则死 |
| 产阳与近战双开 | 中 | 黑形态不产阳；白形态不近战 |
| 移动中断 | 中 | 眩晕/死亡停 Timer |

### 建议测试清单
1. 种下 → 产阳间隔与阳光量对齐向日葵  
2. 外部加压到 90 → 自动变身、血量约 ×6、有吸血  
3. 有怪：走向最近目标邻格并近战  
4. 无怪：原地待机、压力持续降  
5. 压力到 0：回原格、恢复产阳与外观  
6. 离格时尝试在原格种植 → 失败  
7. 人为占用原格后压力归 0 → 羽川翼 Death  
8. 黑形态中铲除 / 被吃 → 无残留 Timer、原格可种  

## 5. 结论
影响面 **中等偏可控**：以新增被动 + Prefab 为主；stander 预订是最大交叉风险，需重点测种植/清理。
