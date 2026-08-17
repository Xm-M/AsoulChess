# 架构设计与影响面分析报告：Core UPM 包

### 基本信息
- **需求名称**: 实现 Core UPM 包（事件/计时/对象池）
- **所属模块**: `com.asoulchess.game.core`
- **分析日期**: 2026-07-27
- **复杂度**: L2
- **选型**: Monorepo + 选项 A（不改 AVZ 玩法）

---

### 一、系统定位

**模块归属**: 新建独立 UPM 包 / 程序集，位于依赖图最底层。

**模块关系**:
- **上游依赖**: Unity Engine 模块
- **下游影响（本期）**: 无强制；Sample 为包内自用
- **下游影响（远期）**: AVZ Manage/Event 可逐步依赖 Core
- **横向关联**: 概念对齐现有 EventController / TimerManage / ObjectPool

```
UnityEngine → AsoulChess.Game.Core → (远期) AVZ Scripts
                      ↓
                 Samples~（包内）
```

---

### 二、设计决策

**推荐方案**: 精简重写 + 命名空间隔离 + 接口服务入口

| 组件 | 建议类型名（可微调） | 说明 |
|------|---------------------|------|
| 事件 | `EventBus` 实现 `IEventBus` | string 事件名；无 EventName 枚举 |
| 计时 | `TimerService` + `TimerHandle` | Update 驱动需 MonoBehaviour 宿主或由 Sample 挂载 |
| 对象池 | `GameObjectPool` | 按 prefab/name 分栈；无场景强绑也可简化 |
| 入口 | `GameServices` 静态注册 | 可选；避免强制单例上帝类 |

**计时器宿主**: 提供 `TimerServiceBehaviour`（MonoBehaviour）在场景中驱动 Tick；纯 C# 服务持有逻辑。

**备选（不采用）**:
- 剪切粘贴三个旧类进包 — 会拖入 EventName / Chess
- 本期替换 AVZ 引用 — 选项 A 明确排除

**数据流（Sample）**:
```
按键 → EventBus.Trigger("demo.ping")
     → 监听回调里 TimerService.Delay(1s)
     → 到时 GameObjectPool.Get(prefab) → 再 Delay → Release
```

---

### 三、影响面评估

**本期改动范围**:
- 新增: `Packages/com.asoulchess.game.core/**`
- 可能: `Packages/manifest.json`（若需显式依赖；嵌入式包通常自动可见）
- **不改**: `Assets/Script/**` 玩法与旧管理器

**性能**: 新建路径，不影响现网局内性能。  
**兼容性**: AVZ 旧代码并行存在；注意类型名勿放入全局无命名空间（必须带 `AsoulChess.Game.Core`）。

---

### 四、风险识别

| 风险点 | 概率 | 影响 | 应对 |
|-------|------|------|------|
| 与旧 `EventController` 名冲突 | 低 | 中 | 新名 `EventBus` + 命名空间 |
| Timer 无人 Update | 中 | 高 | Sample 必须挂 Behaviour |
| 开发者误以为 AVZ 已切换 | 中 | 低 | README 醒目标注「AVZ 未迁移」 |

风险可接受（选项 A 下）。

---

### 五、结论

按 Monorepo 新建 `com.asoulchess.game.core`，精简实现三件套 + Sample + README；零玩法迁移。通过后可进入开发。
