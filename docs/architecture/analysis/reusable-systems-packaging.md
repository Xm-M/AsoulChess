# 架构设计与影响面分析报告

### 基本信息
- **需求名称**: 可复用系统打包规划（文档先行；第一期范围锁定 Core）
- **所属模块**: 工程基建 / Framework
- **分析日期**: 2026-07-27
- **复杂度**: L1（本期文档）／远期 Core 实现约 L2～A

---

### 一、系统定位

**模块归属**: 新建「Game Foundation / Core」层（不属于现有玩法模块）

**模块关系**:
- **上游依赖（远期实现时）**: Unity 引擎 API；不依赖 AVZ 游戏模块
- **下游影响**: 未来 AVZ 的 Event / Timer / ObjectPool 可改为依赖 Core；新项目直接引用 Core
- **横向关联**: 与 Manage、Event、Chess、LevelSystem 有「概念对应」，但 Core **不得反向依赖**它们

**模块关系图**:
```
[Unity Engine] → [Core UPM] → [AVZ GameContent / 未来项目]
                     ↑
              本期仅文档描述此层
```

**定位确认**: 第一期只定义/日后实现 Core；数值、角色、关卡不在本期与第一期实现范围内。

---

### 二、设计决策

**推荐模式**: 分层 + UPM 包 + 接口注入（Service / 接口替代到处 `GameManage.instance`）

**理由**:
- 符合 Context 中「单例过度集中」的改进建议
- 个人开发者可先 Monorepo 本地包，再拆独立仓库
- Core 精简重写比剪切粘贴更可控

**备选方案**:
1. 仅模板工程拷贝 — 上手快，多项目易分叉（不推荐作主方案）
2. 裸 `.unitypackage` — 最快，版本与依赖差（不推荐）

**已选决策（用户确认）**:
- 交付：UPM
- 第一期：仅 Core（Event + Timer + Pool + 服务接口）
- 本期：只出规划文档，零代码

---

### 三、影响面评估（本期）

| 维度 | 结论 |
|------|------|
| 运行时代码 | **无改动** |
| 资源 / 场景 | 无 |
| 存档 | 无 |
| 文档 | 新增 requirements + architecture 规划 |

**远期 Core 实现时的预估影响**（非本期）:
- 可能触及：`EventController.cs`、`TimerManage.cs`、`ObjectPool.cs`、`GameManage` 组装处
- 风险点：ObjectPool 内 Chess/Tile 专用逻辑、EventName 枚举污染
- 策略：包内精简实现 + AVZ 渐进替换，避免大爆炸重构

---

### 四、风险识别

| 风险点 | 概率 | 影响 | 应对措施 |
|-------|------|------|---------|
| 把 Core 理解成「整个游戏内核」而塞入玩法 | 中 | 高 | 文档明确边界；审批锁定范围 |
| 文档通过后过早开动 Stats/Chess | 中 | 高 | 第一期验收不过不做二期 |
| UPM / asmdef 经验不足 | 高 | 中 | 规划内含最小验证里程碑；动手前另开实现需求 |
| Skills 未覆盖 UPM 专项 | 中 | 低 | 实现前查 Unity Package Manager 官方文档 |

**风险是否可接受**: 本期文档风险极低；实现期需另开需求重评。

---

### 五、结论

架构方案与「先 Core」范围一致，本期以文档冻结决策即可。  
完整路线见：`docs/architecture/reusable-systems-packaging-plan.md`
