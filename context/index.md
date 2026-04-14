# AVZ（AsoulChess）项目 Context

**生成日期**: 2026-03-30  
**版本**: 2.0.0  
**分析工具**: project-context-analyzer（按 `.cursor/skills/b-analyzers/project-context-analyzer/SKILL.md` 沉淀）  
**工作流对齐**: `unity-2d-ai-coding-workflow/unity-2d-ai-coding-workflow/docs/workflow/ai-coding-workflow-guide.md`（v3）、`.cursor/rules/unity-2d-game-development-workflow.mdc`

## 与本仓库其它文档的关系

| 路径 | 说明 |
|------|------|
| `context/`（本目录） | **需求工作流 / AI 默认读取** 的 Context 根目录（`@requirement-workflow` 查 `context/index.md`） |
| `workflow_v2/context/` | 历史或并行沉淀，**以本目录为准**；重大变更请同步或废弃旧副本 |

## 快速导航

### 整体概览
- [项目概览](./overview.md) — 目录、统计、数据流
- [依赖图谱](./architecture/dependency-graph.md) — 模块依赖与 Mermaid 图
- [架构决策](./architecture/decisions.md) — 模式、选型、规范、风险

### 模块详情
| 模块 | 复杂度 | 文档 |
|------|--------|------|
| Manage（GameManage 等） | 高 | [Manage](./modules/Manage.md) |
| Chess | 高 | [Chess](./modules/Chess.md) |
| LevelSystem | 高 | [LevelSystem](./modules/LevelSystem.md) |
| Skill | 高 | [Skill](./modules/Skill.md) |
| State | 中 | [State](./modules/State.md) |
| Buff | 中 | [Buff](./modules/Buff.md) |
| Map | 中 | [Map](./modules/Map.md) |
| UI | 中 | [UI](./modules/UI.md) |
| Event | 低 | [Event](./modules/Event.md) |

## 关键信息

### 核心模块（重要性）
1. **Manage** — `GameManage.instance` 为中枢；关卡、队伍、UI、计时器等由此挂载或获取  
2. **Chess** — 棋子；多 Controller 组合；与技能、状态、Buff、动画强绑定  
3. **LevelSystem** — `LevelManage` / `LevelController` 驱动单局流程  
4. **Skill** — `ISkill` + `ISkillEffect` + `SkillContext`；技能勿挂运行时专用 MonoBehaviour（见规则）  
5. **Event** — `EventController` 字符串事件解耦  

### 高风险 / 注意
- 单例集中（测试与替换成本高）  
- `UnityEvent` 与订阅须在离场/死亡时解除，避免泄漏  
- 脚本目录名为 **`Assets/Script`**（单数），非 `Scripts`  

### 后续开发建议
- 新功能优先经 `@requirement-workflow`：有 Context 后再写需求卡片与审批  
- 跨模块通信用 `EventController`；棋子侧扩展优先加 Controller / ISkillEffect  
- 技能协程用 `Chess.StartCoroutine`，勿为技能单独 `AddComponent` 运行时行为脚本  

---

*本 Context 供 AI 与协作者阅读；架构变更后请更新对应 `modules/` 与 `architecture/`。*
