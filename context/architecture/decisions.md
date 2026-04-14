# 架构决策记录

## 设计模式

| 模式 | 场景 | 代表类型 |
|------|------|----------|
| 单例 | 全局入口 | `GameManage`, `LevelManage`, `EventController`, `MapManage`（等） |
| 组件组合 | 棋子能力拆分 | `Chess` + 多个 `*Controller` |
| 状态机 | 棋子行为切换 | `StateController`, `State`, `Transition` |
| 观察者 | 跨系统通知 | `EventController`, `Chess` 上部分 `UnityEvent` |
| 对象池 | 频繁创建/销毁 | `ObjectPool`（与管理器配合） |
| 策略/插件 | 关卡差异 | `LevelController` 派生、`ILevelplugIn` 等 |

## 架构风格

**组件化棋子 + 单例管理器 + 事件辅助**，数据侧大量使用 **ScriptableObject**（`PropertyCreator`、技能配置等）。

**优点**: 扩展棋子可换 Controller / 换 `ISkill` 实现；关卡可插拔 Controller。  
**局限**: 单例与静态使单元测试成本高；全局状态需小心时序与读档。

## 技术选型

| 领域 | 选型 | 备注 |
|------|------|------|
| 物理 | Physics **2D** | `Rigidbody2D` / `Collider2D` |
| 动画 | Mecanim `Animator` | 各 `AnimatorController_*` 子类 `Play`/`IfAnimPlayOver` |
| UI | uGUI（+ 第三方皮肤/组件） | 见 UI 模块 |
| 序列化 | Odin + Unity `SerializeReference` | 技能多态、Inspector 分组 |
| 存档 | 项目内 `SaveSystem/` | 与 `SkillContext` 等键过滤配合 |

## 项目规则（强制）

- **技能目录** `Assets/Script/Skill/`：禁止为技能逻辑再挂**运行时专用** MonoBehaviour；用 `ISkillEffect` + `Chess` 生命周期 / 事件；协程用 `Chess.StartCoroutine`。详见 `.cursor/rules/unity-skill-no-runtime-monobehaviour.mdc`。  
- **新 MonoBehaviour**：`.cursor/rules/unity-2d-game-development-workflow.mdc` 要求优先扩展现有组件；新增需在架构分析中说明理由。

## 编码约定（常见）

- 类名 **PascalCase**；公共 API 多为 PascalCase。  
- 目录 **`Assets/Script`**（非 `Scripts`）。  
- `SkillContext` 中战斗临时键需在 `SkillController.ShouldSkipKey` 等处排除存档（如 `stand`、僵王动画键等）。

## 演进建议

- 新模块补充 `context/modules/[模块].md` 并更新 `index.md` 表格。  
- 输入系统若升级至 **Input System**，在本文档与 overview 中同步版本与包名。
