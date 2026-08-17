# 功能需求卡片: 灯石头 · MyGO 成员贴图

## 基本信息
- **功能名称**: 灯石头成员贴图（额外部署换 Sprite）
- **所属模块**: Skill（灯宿主）/ Chess（石头表现）
- **需求类型**: 功能扩展
- **优先级**: P1
- **预估复杂度**: L1
- **预估耗时**: 0.5～1 小时（含 Animator 是否覆盖 Sprite 的处理）
- **提出日期**: 2026-07-24
- **父需求**: [灯（高松灯升级 · 棋子模式）](./deng-tomori-chess.md)

## 功能描述
### 详细描述
买卡种下的石头保持 Prefab **当前默认**外观。额外部署的石头按灯邻格 MyGO 成员列表（`nearChess` / `fetterMemberId`）**1:1** 更换 Sprite。贴图来源：工程内 `棋子.png` 已切 6 格，**前 5 格**对应五名 MyGO 成员图标；由策划在灯宿主 Inspector 配置 `memberId → Sprite`，代码不写死资源引用。

### 用户故事
作为玩家，我希望额外部署的石头能看出对应是哪位邻格 MyGO，买卡主棋仍是统一默认外观。

## 现有业务上下文

### 相关现有功能
| 功能 | 模块 | 关系 | 说明 |
|------|------|------|------|
| `PassiveSkillEffect_TomoriHost` | Skill | 扩展 | 额外部署队列、TrackPiece |
| `PassiveSkill_Mygo.nearChess` | Skill | 依赖 | 邻格去重成员 id 列表（宿主可直接访问） |
| `AnimatorController.sprite` | Chess | 依赖 | 石头表现入口 |
| `棋子.png`（Multiple） | 资源 | 配置 | 6 切片；前 5 = 成员图标 |

### 需求类型判定理由
不改爆炸/上限/Support 占格；仅扩展额外部署的表现与成员绑定。

### 已锁定规则
| 项 | 结论 |
|----|------|
| 对应规则 | 灯邻格 `nearChess` **1:1**：第 N 颗成功种下的额外石头 → 第 N 个成员贴图 |
| 默认外观 | **不改**：买卡主棋用 Prefab 当前 Sprite / 动画默认 |
| 资源 | 已有切片；**用户在 Inspector 配置**映射表 |
| 成员键 | `fetterMemberId`（与现有一致：高松灯 / 千早爱音 / 要乐奈 / 长崎素世 / 椎名立希） |
| 缺配置 | 该成员无 Sprite 时保持默认外观（不报错） |
| 空位不足 | 仍按现有：跳过无法种植的格；仅对**实际种出**的额外棋按顺序消耗成员列表 |

### 集成点
- **调用**: `nearChess`、`CreateChess`、现有额外部署 Timer
- **触发**: 无新事件
- **需扩展**: 额外部署队列携带 `memberId`；种完后应用 Sprite

### 对现有功能的影响
- **接口变更**: `PassiveSkillEffect_TomoriHost` 增加可序列化成员贴图表
- **行为变更**: 仅额外棋外观；战斗逻辑不变
- **数据变更**: 无存档；Inspector 配置

## 技术要求
- **依赖模块**: Skill、Chess（Animator/SpriteRenderer）
- **性能要求**: 每颗额外棋一次设 Sprite；数量受 MyGO 上限约束
- **兼容性要求**: 向后兼容；未配置表时行为与现网一致（全默认外观）
- **风险点**: 无（用户确认石头 idle 仅位移曲线、无 sprite 换帧）

## 功能清单
### 核心功能（必须）
- [ ] 宿主可配 `memberId → Sprite`（建议 5 条）
- [ ] 额外部署按 `nearChess` 顺序绑定成员并换图
- [ ] 买卡主棋保持默认
- [ ] 未配置成员 → 默认外观
- [ ] Animator 不覆盖已换成员图（或等价方案）

### 扩展功能（可选）
- [ ] 编辑器校验 memberId 拼写提示

## 验收标准
- [ ] 邻格 2 名不同 MyGO → 买卡 1 默认 + 最多 2 额外为对应成员图
- [ ] 只配部分成员时，未配的额外棋仍为默认
- [ ] 爆炸/上限/Support 占格回归通过
- [ ] 未配置映射表时与改前外观一致

## 风险评估
| 风险点 | 概率 | 影响 | 应对措施 |
|-------|------|------|---------|
| Animator 刷掉 Sprite | — | — | **不适用**：idle 仅运动曲线 |
| memberId 与表不一致 | 中 | 低 | 文档列出标准 id；缺省回退默认 |

## 关联 Context
- 涉及模块: Skill, Chess
- 父需求: `docs/requirements/deng-tomori-chess.md`
- 参考: `context/modules/Skill.md`, `context/index.md`
