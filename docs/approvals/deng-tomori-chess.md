# 需求开发审批报告

## 基本信息
- **需求名称**: 灯（高松灯升级 · 棋子模式）
- **所属模块**: Chess / Skill / UI（PlantsShop）/ Map
- **分析日期**: 2026-07-24

## 需求摘要
常服高松灯升级态「灯」：邻格 4 不同 MyGO（不含自身、去重）开技后变棋子模式（Blend=1、停攻），商店灯卡切棋子卡；买棋(25)可埋雷，3×3 爆炸后自毁；按灯邻格 MyGO 数免费额外部署；上限=全场 MyGO 数；灯死清棋并还原商店卡。

**需求类型**: 新增功能  
**与现有关系**: `LevelUpPlant` + MyGO Ready + `ShopIcon.RefreshGood` + Wisadel 3×3；参考望简化，贴 MyGO

## 分析结果汇总

### Context
✅ 已读（2026-03-30）— Chess / Skill / UI / Map

### 需求卡片
✅ `docs/requirements/deng-tomori-chess.md`  
（额外部署=邻格 MyGO 触发的额外棋，**免费**；已更正）

### Skills 白名单
- ✅ `@unity-2d-animation` / `@unity-ui-system` / `@unity-prefab-system` / `@unity-object-pool` / `@unity-2d-physics` / `@unity-state-machine`
- ✅ 项目强制：`ISkillEffect`，禁技能专用运行时 MonoBehaviour

**结论**: 通过

### 架构
✅ `docs/architecture/deng-tomori-chess.md`  
- 推荐：主动变身 + 棋子被动 + `RefreshGood`  
- 风险：中（商店换卡先例少、额外部署递归）

### 影响面
✅ `docs/architecture/deng-tomori-chess-impact.md`  
- 新增为主；常服灯无逻辑改  
- 回归项见影响面文档

## 风险总评

| 维度 | 评级 | 说明 |
|------|------|------|
| 技术可行性 | 高 | 先例足够 |
| 改动范围 | 中 | 跨 Skill+Shop，文件以新增为主 |
| 性能风险 | 低 | 棋子数受 MyGO 上限 |
| 时间评估 | 1.5～3 天 | 含 Prefab/调参 |

## 建议 Skills
- `@unity-prefab-system` / `@unity-2d-animation` / `@unity-object-pool`
- 技能规范：`.cursor/rules/unity-skill-no-runtime-monobehaviour.mdc`
- 完成后 `@unity-code-review`

## 开发优先级
- P0: 灯 LevelUp 配置 + MyGO 开技条件（不含自身、去重）
- P0: 主动技变身（Blend/停攻/换卡/快照 ATK）
- P0: 棋子爆炸被动 + 动态买卡上限
- P0: 买卡额外部署（免费）+ attack 一次 + 灯死清场还原
- P1: 特效/音效/图鉴

## 决策审批
✅ 通过 - 可以开始开发  
⬜ 修改后通过 - 需要调整方案  
⬜ 驳回 - 暂不开发

审批意见: 用户确认通过  
日期: 2026-07-24
