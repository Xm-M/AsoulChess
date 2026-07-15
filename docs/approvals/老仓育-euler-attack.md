# 需求开发审批报告：老仓育 — 欧拉公式常驻弹道

## 基本信息
- **需求名称**: 老仓育 — 公式常驻弹道 + 压力传导被动
- **需求 ID**: OKU-EULER-001
- **所属模块**: Skill / Bullet / Buff / Map
- **分析日期**: 2026-07-09
- **需求卡片**: `docs/requirements/老仓育-euler-attack.md`
- **架构报告**: `docs/architecture/老仓育-euler-attack.md`

## 需求摘要

**老仓育**为主 C：一颗**常驻**弹道沿李萨如曲线循环（中心在老仓格），触碰造成伤害。

| 参数 | 规则 |
|------|------|
| A, B | 行/列友方数（含自己）；种下与**主动技能**快照 |
| ω | `k × 攻速`，实时 |
| a:b | 种下 **1:1**；技能时 **`stress : 30` 最简比** |
| 被动 | stress 增 Δ → **八邻格**（不含自己）所有单位 +Δ；老仓 stress **≥ 30**；Tomo 式挂 Buff；**一层、不连锁** |
| 主动技能 | 刷新 A/B/a:b；**不改压力** |

**需求类型**: 新增功能

## 用户定稿记录（2026-07-09）

| 项 | 决定 |
|----|------|
| a:b 映射 | **a:b = 当前压力值 : 30**（gcd 最简） |
| 传导范围 | 八邻格，**不含中心/自己** |
| 传导目标 | **友方 + 敌方** |
| 无 Buff | 与 **Tomo** 一致，先挂再叠 |
| 连锁 | **不传回老仓、只一层** |
| 需求卡片 | **确认** |

## 分析结果汇总

### Context 复用
- ✅ 已读取 `context/index.md`（2026-03-30）
- 涉及模块: Skill, Buff, Chess, Map
- 参考: `context/modules/Skill.md`

### Skills 白名单
| 技术点 | Skill | 结论 |
|--------|-------|------|
| 2D 碰撞/移动 | unity-2d-physics | ✅ |
| Sprite/VFX | unity-2d-sprite | ✅ |
| 对象池 | unity-object-pool | ✅ |
| 协程/Timer | unity-coroutine-system | ✅ |
| ScriptableObject 配置 | unity-scriptableobject-config | ✅ |

**结论**: 通过（无大量未覆盖项）

### 架构定稿

| 决策 | 选择 |
|------|------|
| 弹道 | `Bullet_OkuwakiOrbit` + `BulletMove_Lissajous` |
| 参数存储 | `SkillContext` 快照 A/B/a/b；ω 实时读攻速 |
| 攻击入口 | `OkuwakiOrbitAttack` 空实现；伤害由常驻弹承担 |
| 被动 | `PassiveSkillEffect_Okuwaki` + `Buff_StressBuff_Okuwaki` |
| 主动 | `SkillEffect_OkuwakiRefresh` |
| 扩散防递归 | `_isApplyingSpread` 守卫 + 排除 self |
| 压力施加 | 复用 `Buff_StressBuff_Death.BuffReset` / Tomo `EnsureStressBuff` 模式 |

### 影响面

| 类型 | 数量 | 说明 |
|------|------|------|
| **新增** | ~8 个 C# 文件 | 见架构报告 |
| **修改** | 2 个资源 | 老仓 Prefab + Asset |
| **回归** | 0 破坏性 | 不改全局 Bullet/Buff 基类 |

**高风险点**: 1（ω 漏判 — 用 ωMax + TouchCd 缓解）

### 回归测试建议

- [ ] 种下：A=1,B=1,a:b=1:1，弹道循环
- [ ] 补种不点技能：A/B 不变
- [ ] 技能：A/B=行列数；P=45→3:2，P=60→2:1
- [ ] 攻速变化：ω 变，A/B/a:b 不变
- [ ] 老仓 +5 压力：八邻格 +5；老仓不二次吃传导
- [ ] 邻格单位压力上升不触发二次扩散
- [ ] stress 试图 <30：clamp 30
- [ ] 老仓死亡/离场：弹道回收

## 风险总评

| 维度 | 评级 | 说明 |
|------|------|------|
| 技术可行性 | **高** | 现有 Bullet/ISkillEffect/压力 Buff 可复用 |
| 改动范围 | **中** | 新增为主，约 8–12h |
| 性能风险 | **低** | 单弹 + 事件驱动扩散 |
| 平衡风险 | **中** | 压力传导含敌方，需分关数值 |

## 建议 Skills 使用清单

- `@unity-2d-physics` — 触碰判定
- `@unity-object-pool` — Bullet 池化
- `@unity-scriptableobject-config` — 老仓 Asset
- `@unity-code-review` — 开发完成后审查

## 开发优先级

- **P0**: Lissajous 移动 + 常驻弹 + 被动扩散 + 主动刷新 + Prefab
- **P1**: 轨迹残影 / 公式飘字
- **P2**: 仅前方半弧裁剪

## 决策审批

- ✅ **通过** — 可开始开发
- ⬜ 修改后通过
- ⬜ 驳回

**审批意见**: 用户已确认需求卡片与 a:b 映射（stress:30）；架构方案 SkillContext + IBulletMove 策略。  
**日期**: 2026-07-09
