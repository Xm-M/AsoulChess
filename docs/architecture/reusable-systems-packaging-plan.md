# 可复用系统打包规划

**需求**: [reusable-systems-packaging](../requirements/reusable-systems-packaging.md)  
**日期**: 2026-07-27  
**状态**: 规划文档（本期不改代码）  
**范围锁定**: 第一期实现 = **仅 Core**（用户 2026-07-27 确认）  
**适用项目**: AVZ / AsoulChess → 未来其他 Unity 项目

---

## 1. 一句话结论

**不要直接打包「数值 / 角色 / 关卡」整模块。**  
先抽一层不绑玩法的 **Core（事件 + 计时 + 对象池）** 做成 **UPM 包**，用一个空项目引用验证；数值与角色骨架放后续期；关卡与具体棋子内容永远留在游戏项目里。

---

## 2. 你在解决什么问题

| 想要的 | 常见错误做法 | 正确方向 |
|--------|-------------|---------|
| 下个项目少写一遍基础设施 | 复制整个 `Assets/Script` | 只复用「无玩法语义」的内核 |
| 数值 / 角色 / 关卡可迁移 | 导出 `.unitypackage` 硬塞 | 分层：通用 API vs 游戏内容 |
| 多项目长期维护 | 多份拷贝各自改 | 一个包仓库，多项目引用同一版本 |

AVZ 现状（来自 Context）：

- 架构：`GameManage` 单例中枢 + Chess 多 Controller + `EventController`
- 风险：**几乎处处 `GameManage.instance`**；`ObjectPool` 等已混入 Chess/Tile 语义
- 工程内几乎 **没有游戏脚本的 `.asmdef`**，模块边界只靠文件夹，尚未具备「可独立成包」的物理隔离

因此：打包工作的本质是 **先划边界、再搬家**，不是「选几个文件夹导出」。

---

## 3. 推荐交付形态

### 3.1 主形态：Unity UPM 包（推荐）

```
你的包仓库（可独立 Git）
└── com.yourstudio.game.core/
    ├── package.json
    ├── Runtime/
    │   ├── YourStudio.Game.Core.asmdef
    │   ├── Events/
    │   ├── Timing/
    │   └── Pooling/
    ├── Editor/          （可选）
    └── Samples~/        （最小示例场景）
```

其他项目引用方式（任选）：

1. **Git URL**（推荐长期）：Package Manager → Add package from git URL  
2. **本地路径**：开发期最方便（`file:../GameCore`）  
3. **Embedded**：拷进目标项目的 `Packages/`（简单，但多项目同步较差）

### 3.2 可选副产物：空壳模板工程

在 Core 包稳定后，另存一个「已引用 Core + 基础场景」的模板仓库，开新项目时 clone。  
**模板跟包走，不要把业务代码只放在模板里。**

### 3.3 不推荐作为主方案

| 方式 | 问题 |
|------|------|
| 裸 `.unitypackage` | 依赖与 GUID 易乱，无版本语义 |
| 整项目 Duplicate | 分叉后无法合并改进 |
| 直接把 Chess/Level 文件夹拷走 | 会拖入半个 AVZ，编译与运行都会炸 |

---

## 4. 分层模型（什么该进包）

```
┌─────────────────────────────────────────┐
│  GameContent（永远留在具体游戏项目）        │
│  棋子皮肤/技能/羁绊/关卡脚本/商店/肉鸽…     │
├─────────────────────────────────────────┤
│  Gameplay Framework（可选后期包）          │
│  实体生命周期骨架、关卡状态机抽象、地图格子抽象 │
├─────────────────────────────────────────┤
│  Stats（第二期候选包）                     │
│  属性定义、修正器、伤害结算接口、SO 基类     │
├─────────────────────────────────────────┤
│  Core（第一期必须包）⭐                     │
│  事件总线、计时器、对象池、服务定位接口       │
└─────────────────────────────────────────┘
```

### 4.1 进包 / 不进包对照（对照 AVZ 现状）

| AVZ 现状 | 建议归属 | 理由 |
|----------|---------|------|
| `EventController` 总线能力 | **Core** | 通用 |
| `EventName` 枚举（GameStart、WhenPlantChess…） | **GameContent** | 玩法语义，禁止进通用包 |
| `TimerManage` 的计时/倍速/池化 | **Core** | 通用；离开关卡清理用回调/接口，不硬绑 EventName |
| `ObjectPool` 的 Create/Recycle 通用 API | **Core** | 通用 |
| `ObjectPool` 里 Chess/Tile 专用逻辑 | **GameContent** | 抽离时必须拆开 |
| `PropertyController` / `PropertyCreator` | **Stats（二期）** | 需先去 Chess/UI 硬依赖 |
| `Chess` + 各 Controller | **骨架→Framework；实例→Content** | 不能整类搬走 |
| `LevelController` 子类与出怪 | **GameContent** | 高度特化 |
| `LevelController` 基类状态流程 | **Framework（三期）** | 可抽象为「局内阶段机」 |
| `GameManage` | **GameContent 组装根** | 包内不应出现「必须有 GameManage」 |

### 4.2 核心设计规则（抽包铁律）

1. **包不依赖游戏**：`YourStudio.Game.Core` 不得 `using` 或引用 Chess、Level、UI、Fetter 等  
2. **游戏可以依赖包**：AVZ 引用 Core，逐步把调用改过去  
3. **用接口替代单例直取**：例如 `ITimerService`、`IEventBus`、`IObjectPool`，由游戏在启动时注入  
4. **事件 ID 分层**：Core 提供 `string` / 泛型总线；AVZ 继续用自己的 `EventName`  
5. **一次只抽一层**：先 Core 跑通，再谈 Stats

---

## 5. 分期路线图

### 第 0 期：规划（本期 ✅）

**产出**：本文件 + 需求卡片  
**不做**：改代码、建包仓库  

**验收**：你能向自己说清「先抽什么、不抽什么、用 UPM」。

---

### 第 1 期：Core 包（建议第一次真正动手就做这个）

**目标**：其他空 Unity 项目能引用包，并跑通「发事件 / 开计时器 / 池化预制体」。

**建议包含**：

| 能力 | 从 AVZ 借鉴 | 改造要点 |
|------|------------|---------|
| 事件总线 | `EventController` | 去掉对游戏枚举的编译依赖；保留 Add/Remove/Trigger |
| 计时服务 | `TimerManage` | 倍速、循环、回收队列可保留；清理钩子改为接口事件 |
| 对象池 | `ObjectPool` | 只保留 GameObject/泛型池；删除 Chess 专用成员 |
| 服务入口 | 新建 | `GameServices` 或简单 ServiceLocator，避免新包再造 `GameManage` 上帝类 |

**工程步骤（实现时按序）**：

1. 建独立 Git 仓库或本仓库 `Packages/com.xxx.game.core`  
2. 写 `package.json` + Runtime `asmdef`  
3. **干净实现**一版 Core API（推荐：参考 AVZ 重写精简版，而不是直接剪切粘贴后修几百个报错）  
4. 在 AVZ 中改为引用包（可先双轨：新代码走包，旧代码暂留）  
5. 新建空项目 → Add package → 跑 `Samples~`  
6. 文档：README（如何引用、API 一页纸）

**第 1 期验收**：

- [ ] 空项目仅依赖 Core 即可编译  
- [ ] Sample 演示事件 + Timer + Pool  
- [ ] Core 的 asmdef 引用列表中 **没有** AVZ 游戏程序集  
- [ ] AVZ 主流程仍可玩（若已接入）；或至少 AVZ 未因建包而损坏

**粗估**：熟悉 UPM 的情况下约 1～3 天；若边学边做且要开始替换 AVZ 调用，约 3～7 天。

---

### 第 2 期：Stats 包（数值）

**何时做**：Core 已被至少一个外部项目（或空项目）验证成功之后。

**目标**：可配置属性 + 伤害/治疗结算管道，不绑定「棋子」「格子」。

**建议抽象**：

```
IStatOwner          // 任意实体可有属性
StatModifier        // 加成/倍率
DamageRequest       // 替代强耦合的局内消息结构（逐步对齐 DamageMessage）
IDamagePipeline     // 结算步骤可插拔
ScriptableObject    // 属性模板基类（对应 PropertyCreator 的通用部分）
```

**不要塞进 Stats 的**：

- 行伤、植物阳光、羁绊、商店价格等玩法字段  
- 直接调用 `UIManage` / `DamagePanel`

**粗估**：视解耦深度，约 1～2 周量级（含 AVZ 适配）。

---

### 第 3 期：Entity / Level 骨架（角色与关卡「框架」）

**目标**：复用「实体有组件 Controllers + 状态机」「局有阶段」的模式，而不是复用某只具体棋子或某关脚本。

**可复用**：

- Controller 基类模式  
- 状态机（Idle/Attack/Death 这类通用态，名称可配置）  
- 关卡插件接口思路（`ILevelplugIn` 一类）

**不可复用（应留在 AVZ）**：

- 具体 `ISkillEffect_*`、Fetter、Roguelike 节点  
- 具体 `LevelController_*`  
- Map 上与 PVZ 强相关的 Tile 规则（可另做「格子地图」包，但是新项目）

**警告**：这一期最容易失控。只有当你明确下一个项目也是「战棋 / 塔防 / 自走棋类」时，才值得做；若下个项目是横版动作或纯 RPG，**停在 Core + Stats 往往更划算**。

---

## 6. 针对「数值 / 角色 / 关卡」的直接回答

| 你说的系统 | 能不能打包给别的游戏 | 怎么做才合理 |
|-----------|---------------------|-------------|
| **数值** | 能（二期） | 抽公式与修正器，不抽「植物属性表」 |
| **角色** | 只能抽骨架（三期） | 抽实体+组件模式；皮肤/技能/动画状态名留游戏 |
| **关卡** | 基本不能整包 | 抽「阶段 / 插件 / 胜负接口」；关卡内容永远项目内 |

一句话：**打包「机制」，不要打包「这款游戏的内容」。**

---

## 7. 推荐仓库与目录布局（实现期参考）

### 方案 A：独立包仓库（更干净，推荐）

```
GameFoundation/                          # 新 Git 仓库
├── Packages/
│   └── com.yourstudio.game.core/
└── README.md

AVZ/                                     # 现有游戏
└── Packages/manifest.json               # 引用 git 或 file: 路径

NewGame/                                 # 未来项目
└── Packages/manifest.json               # 同一引用
```

### 方案 B：单仓 Monorepo（省事，适合个人）

```
AVZ/
├── Packages/
│   └── com.yourstudio.game.core/        # 本地 UPM
├── Assets/Script/                       # 游戏内容继续在这
└── docs/architecture/...
```

个人开发者可先用 **方案 B** 练手，包 API 稳定后再拆成方案 A。

---

## 8. 最小验证里程碑（避免「只写了文档」）

当你决定动手时，用这个检查是否「真的可复用」：

1. 新建空白 2D 项目  
2. 只添加 Core 包  
3. 放一个 Cube，按下键：触发事件 → Timer 1 秒后从池取出特效 → 回收  
4. **全程不出现** `GameManage`、`Chess`、`LevelManage` 字样  

过了这关，再考虑把 AVZ 逐步迁过去。

---

## 9. 风险清单

| 风险 | 等级 | 缓解 |
|------|------|------|
| 想一次抽 Chess+Level | 高 | 严格按期；下项目类型不明时停在 Core |
| 剪切粘贴后修不完引用 | 高 | Core 倾向「精简重写」+ AVZ 适配，而非整文件搬迁 |
| 单例测试/替换困难 | 中 | 包内用接口；游戏组装时注入 |
| 事件泄漏 | 中 | 包 API 强调成对 Add/Remove；Sample 示范 |
| 包版本与 AVZ 分叉 | 中 | 语义化版本；破坏性改动升 major |
| Odin 等编辑器依赖渗进 Runtime | 中 | Runtime asmdef 不引用 Odin；编辑器扩展放 Editor 程序集 |

---

## 10. 与现有开发流程的衔接

1. **本期**：规划文档审批（requirement-workflow）  
2. **动手前**：再跑一遍 `@requirement-workflow`，范围锁定「第 1 期 Core 实现」  
3. **开发中**：`@architecture-design-analyzer` / `@impact-scope-analyzer` 针对具体文件列表  
4. **开发后**：`@unity-code-review`；更新 `context/`（新增 Framework 模块说明）

本期 **不** 启动 `@development-workflow` 写代码。

---

## 11. 你可以怎么用这份文档

- **现在**：只读懂第 1、4、5、6 节，建立正确预期  
- **准备动手时**：按第 5 节第 1 期 checklist 开新需求（范围=Core UPM）  
- **评估下一个游戏类型时**：用第 5 节第 3 期的「警告」决定要不要抽角色/关卡骨架  

---

## 12. 决策摘要（供审批）

| 项 | 决策 |
|----|------|
| 交付形态 | UPM 为主；模板工程为辅 |
| 第一期范围 | Core = Event + Timer + Pool + 服务接口 |
| 数值 / 角色 / 关卡 | 二期 / 三期 / 仅抽象；内容不进包 |
| 本期交付 | 仅文档，零代码改动 |
| 实现策略 | 精简重写进包 + 游戏侧渐进替换，避免一次性大搬家 |

---

*文档随实现进展更新；重大变更请同步 `docs/requirements/reusable-systems-packaging.md` 与 `context/`。*
