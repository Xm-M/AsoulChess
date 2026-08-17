# 功能需求卡片: 奶龙僵尸配置

## 基本信息
- **功能名称**: 奶龙僵尸
- **所属模块**: Chess（粉色奶龙转化单位）
- **需求类型**: 功能扩展（换皮单位）
- **优先级**: P1
- **预估复杂度**: L1
- **提出日期**: 2026-08-12
- **需求 ID**: nailong-zombie

## 功能描述
补齐粉色奶龙主动转化目标：独立 `PropertyCreator` + Prefab，**数值/武器/移动/SampleZombie 与普通僵尸相同**；动画由制作人自行配置。

## 已锁定
| # | 结论 |
|---|------|
| 1 | baseProperty / 武器 / HorMove / CloseAttack / SampleZombie 对齐普通僵尸 |
| 2 | 动画自制；Prefab 暂可沿用普通僵尸 Controller 作占位，可随时替换 |
| 3 | 粉色奶龙 `nailongZombieCreator` 指向本单位（不再占位普通僵尸） |

## 实现
- `Assets/Resources/ChessData/Enemy/奶龙僵尸.asset`
- `Assets/Prefab/ChessPrefab/Zombie/奶龙僵尸/奶龙僵尸.prefab`
- `粉色奶龙.prefab` → Creator 已切换

## 验收
- [ ] 转化生成我方奶龙僵尸，血攻速等同普通僵尸
- [ ] 替换 Animator/贴图后无需改技能代码
