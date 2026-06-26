# 需求开发审批报告：魂灵之影 + 四邻隐匿

## 基本信息
- **需求名称**: 魂灵之影落地 + 维什戴尔四邻隐匿
- **所属模块**: Skill / Chess / Buff
- **分析日期**: 2026-06-25

## 需求摘要
补全魂灵之影召唤物（武器 3s 周期挂标）并修正资产引用；维什戴尔在**四邻格存在己方魂灵之影**时进入 `Unselectable` 隐匿，影离开后解除。

**需求类型**: 功能扩展  
**与现有功能的关系**: 基于已实现的 `WisadelShadowPlacer` / `PassiveSkillEffect_Wisadel` 扩展

## 分析结果汇总

### Context 复用
✅ 已读取 `context/index.md`、`context/modules/Skill.md`

### 用户确认项
| 项 | 决策 |
|----|------|
| 影攻击 | 标准 Weapon，`interval=3`，攻击动画 |
| 影数值 | 1000 HP，20 双抗 |
| 影索敌 | 主人攻击格内、无标记、距主人最近 |
| 隐匿范围 | **四邻**（`MapManage.NearTile`，非八邻） |
| 隐匿效果 | `Chess.UnSelectable()`（无法选中 + 非真实伤害免疫） |

### 需求卡片
✅ `docs/requirements/wisadel-shadow.md`

### Skills 白名单
✅ 2D 物理、状态机、预制体、Buff — 全部覆盖

### 架构要点
- 新建 `FindTarget_WisadelShadowUnmarked`、`WisadelShadowCloseAttack`
- 新建 `Buff_WisadelCamouflage` + `WisadelStealthHelper.Refresh`
- 隐匿刷新挂接：召影成功 / 影死亡 / 入场

### 影响面
- **新增脚本**: 4–5 个（Wisadel 目录内）
- **修改**: `PassiveSkillEffect_WisadelShadow`、`WisadelShadowPlacer`、`PassiveSkillEffect_Wisadel`（各 1–3 行 Refresh 调用）
- **资产**: `魂灵之影.prefab`、`魂灵之影.asset`、`维什戴尔.prefab`（shadowCreator）
- **零侵入**其他角色

## 风险总评

| 维度 | 评级 |
|------|------|
| 技术可行性 | 高 |
| 改动范围 | 小 |
| 性能风险 | 低（事件驱动 Refresh） |

## 开发优先级
- **P0**: 影 Prefab/资产、武器挂标、隐匿、shadowCreator 修正
- **P1**: 召唤/命中特效、隐匿视觉（半透明）
- **P2**: 同格啃食兜底、多株维什戴尔视觉区分

## 决策审批
✅ **通过** — 用户已确认魂灵之影方案并补充隐匿需求（2026-06-25）

审批意见: 四邻有影则隐匿；标准武器 interval=3；1000HP/20双抗  
日期: 2026-06-25
