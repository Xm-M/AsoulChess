# 需求开发审批报告

## 基本信息
- **需求名称**: 凑友希那
- **所属模块**: Chess / Skill / Buff
- **分析日期**: 2026-07-13

## 需求摘要
新增远程棋子凑友希那：每次攻击对上下左右四邻格**友方**施加「压力」Buff（无则挂、有则 `BuffReset` 每次 +1）；自身不挂压力。标签/动画由配置者自填。

**需求类型**: 新增功能  
**与现有功能的关系**: 复用压力 Buff 协议与 `OnAttack` 被动模式；与 GBC/老仓育压力体系联动

## 分析结果汇总

### Context 复用
✅ 已读取项目 Context（2026-03-30）
- 涉及模块: Skill, Buff, Chess
- 参考: `context/modules/Skill.md`, `Buff.md`

### 现有业务分析
- **相关**: `Buff_StressBuff_Death`、Tomo/老仓育加压、棒球手 OnAttack、邦多利远程
- **判定**: 新增功能
- **集成**: `OnAttack` + 四邻格 + `AddBuff`/`BuffReset`
- **影响**: 不改核心压力类；仅新棋子行为

### 需求卡片
✅ `docs/requirements/minato-yukina.md`（用户已确认通过）

### Skills 白名单检查
已覆盖：
- ✅ 预制体 → `@unity-prefab-system`
- ✅ ScriptableObject 配置 → `@unity-scriptableobject-config`
- ✅ 2D Sprite（占位）→ `@unity-2d-sprite`

未覆盖专项：无（纯 C# 被动 + 既有 Buff/Weapon，不新增未知 Unity API）

**结论**: 通过

### 架构分析
✅ `docs/architecture/minato-yukina.md`
- 设计: `PassiveSkillEffect_Yukina` + Ensure/BuffReset（保证首次也 +1）
- 风险等级: 低

### 影响面分析
✅ `docs/architecture/minato-yukina-impact.md`
- 影响文件: 约 2–4（1 新脚本 + asset/prefab）
- 高风险点: 0
- 回归测试项: 7

## 风险总评

| 维度 | 评级 | 说明 |
|------|------|------|
| 技术可行性 | 高 | 全复用现有模式 |
| 改动范围 | 小 | 几乎纯新增 |
| 性能风险 | 低 | 每攻 4 格 |
| 时间评估 | 2–4 小时 | 含 Prefab 接线 |

## 建议的 Skills 使用清单
- `@unity-prefab-system` — Prefab 配置
- `@unity-scriptableobject-config` — PropertyCreator
- 开发后可选 `@unity-code-review`

## 开发优先级建议
- P0: 被动加压（四邻友方、Ensure+BuffReset +1）+ OnRemove 卸载
- P0: 远程 ShootBullet 骨架接线
- P1: PropertyCreator / allChess 注册
- P2: 独立子弹、动画、标签文案（用户自填）

## 决策审批
✅ 通过 - 可以开始开发

审批意见: 用户确认通过  
日期: 2026-07-13

## 开发状态
✅ 已实现（2026-07-13）
- `PassiveSkillEffect_Yukina`：OnAttack → 四邻友方 Ensure+BuffReset(+1)
- Prefab：远程 `StraightFindTarget` + `ShootBullet`（暂复用莉莎子弹）
- PropertyCreator：MainPlant；标签/立绘留空供自填
- `开始.unity` allChess 已注册
