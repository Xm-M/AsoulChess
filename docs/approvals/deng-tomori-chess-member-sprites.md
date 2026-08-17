# 需求开发审批报告

## 基本信息
- **需求名称**: 灯石头 · MyGO 成员贴图
- **所属模块**: Skill / Chess
- **分析日期**: 2026-07-24

## 需求摘要
买卡主棋保持 Prefab 默认外观；额外部署棋按灯邻格 `nearChess` 1:1 换成员 Sprite（Inspector 配置）。

**需求类型**: 功能扩展  
**与现有功能的关系**: 基于 `PassiveSkillEffect_TomoriHost` 额外部署扩展表现

## 分析结果汇总

### Context 复用
✅ 已读取项目 Context（2026-03-30）
- 涉及模块: Skill, Chess
- 需求卡: `docs/requirements/deng-tomori-chess-member-sprites.md`
- 架构: `docs/architecture/deng-tomori-chess-member-sprites.md`

### Skills白名单检查
- 已覆盖: Sprite、2D Animation
- 未覆盖: 0
- 结论: 通过

### 架构分析
- 设计: 宿主 `memberId → Sprite` 映射 + 队列带成员 id + 种完写 `SpriteRenderer`
- Animator: **用户确认** idle 仅有 sprite **运动曲线**、无换帧 → **无需去曲线 / 强制写回**
- 风险等级: 低

### 影响面分析
- 影响文件: 主要为 `PassiveSkillEffect_TomoriHost.cs`（+ 可选 Keys 小工具）
- 高风险点: 0
- 回归: 默认外观、成员换图、空位不足、爆炸/上限/Support

## 风险总评

| 维度 | 评级 | 说明 |
|------|------|------|
| 技术可行性 | 高 | 单次赋 Sprite 即可 |
| 改动范围 | 小 | 宿主扩展 |
| 性能风险 | 低 | 每额外棋一次赋值 |
| 时间评估 | ~0.5h | 含接线说明 |

## 建议的Skills使用清单
- @unity-2d-sprite — SpriteRenderer 赋值

## 开发优先级建议
- P0: 映射表 + 额外部署绑定成员换图
- P1: 缺配置回退默认
- P2: 无

## 决策审批
☑ 通过 - 可以开始开发

审批意见: Animator 无 sprite 换帧，不影响运行时赋图  
日期: 2026-07-24
