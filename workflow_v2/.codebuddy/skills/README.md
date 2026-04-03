# Unity 2D独立游戏AI Coding Skills

## 概述

本目录包含27个为Unity 2D独立游戏开发定制的AI Coding Skills，采用**层级化结构**组织：
- **统一大Skill**：unity-2d-game-development（Unity 2D游戏开发知识体系）
- **三级子Skills**：L1基础、L2功能、L3高级
- **辅助工具**：A级生成器、B级分析器、开发编排器、进度管理、C级审查器

---

## 目录结构

```
.codebuddy/skills/
├── unity-2d-game-development/      # 🔥 大Skill：Unity 2D游戏开发
│   ├── SKILL.md                    # 知识体系总览
│   │
│   ├── l1-basics/                  # L1级：核心基础（7个）
│   │   ├── unity-2d-physics/       # 2D物理系统
│   │   ├── unity-2d-sprite/        # Sprite精灵系统
│   │   ├── unity-2d-animation/     # 2D动画系统
│   │   ├── unity-tilemap/          # 瓦片地图系统
│   │   ├── unity-input-system/     # 输入系统
│   │   ├── unity-ui-system/        # UI系统
│   │   └── unity-audio-system/     # 音频系统
│   │
│   ├── l2-features/                # L2级：常用功能（8个）
│   │   ├── unity-scene-management/ # 场景管理
│   │   ├── unity-prefab-system/    # 预制体系统
│   │   ├── unity-state-machine/    # 状态机
│   │   ├── unity-coroutine-system/ # 协程系统
│   │   ├── unity-object-pool/      # 对象池
│   │   ├── unity-save-system/      # 存档系统
│   │   ├── unity-2d-character-controller/  # 角色控制器
│   │   └── unity-scriptableobject-config/  # 数据配置
│   │
│   ├── l3-advanced/                # L3级：高级功能（待创建）
│   │   └── .gitkeep                # 占位
│   │
│   └── shared/                     # 共享资源
│       └── .gitkeep                # 占位
│
├── a-generators/                   # A级：代码生成器（2个）
│   ├── unity-singleton-manager/    # 单例管理器生成
│   └── unity-ui-panel/             # UI面板生成
│
├── b-analyzers/                    # B级：分析工具（3个）
│   ├── project-context-analyzer/   # 项目框架分析器 ⭐ 新增
│   ├── architecture-design-analyzer/  # 架构设计分析
│   └── impact-scope-analyzer/         # 影响面分析
│
├── requirement-workflow/            # 需求开发流程编排器（独立入口）
│   └── SKILL.md                    # 串联需求理解→审批的全流程
│
├── development-workflow/            # 开发执行流程编排器 ⭐ 新增
│   └── SKILL.md                    # 增量开发，一次一个功能
│
├── feature-decomposer/              # 功能拆分器 ⭐ 新增
│   └── SKILL.md                    # 需求拆分为功能列表
│
├── progress-tracker/                # 进度跟踪器 ⭐ 新增
│   └── SKILL.md                    # 记录和更新开发进度
│
├── progress-recovery/               # 进度恢复器 ⭐ 新增
│   └── SKILL.md                    # 恢复开发状态，推荐功能
│
├── session-manager/                 # 会话管理器 ⭐ 新增
│   └── SKILL.md                    # 管理开发会话生命周期
│
└── c-reviewers/                    # C级：审查工具（1个）
    └── unity-code-review/          # 代码审查
```

---

## Skills分类概览

| 分类 | Skills数量 | 特点 |
|------|-----------|------|
| **L1级（核心基础）** | 7个 | Unity 2D开发必备核心系统 |
| **L2级（常用功能）** | 8个 | 游戏开发常用功能模块 |
| **L3级（高级功能）** | 待创建 | 复杂系统集成方案 |
| **A级（代码生成）** | 2个 | 完全自动化，渐进式披露 |
| **B级（分析工具）** | 3个 | 半自动化，强制规范 |
| **需求编排器** | 1个 | 独立入口，串联全流程，自动检查Context |
| **开发编排器** | 1个 | 增量开发，功能拆分，进度跟踪 |
| **进度管理** | 3个 | 进度跟踪、恢复、会话管理 |
| **C级（代码审查）** | 1个 | 辅助工具，质量保障 |

---

## L1级：核心基础（7个）

这些是Unity 2D游戏开发的**必备核心系统**，每个Skill包含完整文档：

| Skill名称 | 核心功能 | 关键优化点 |
|----------|---------|-----------|
| **unity-2d-physics** | 2D物理系统，Rigidbody2D/Collider2D | 刚体优化、碰撞检测、触发器 |
| **unity-2d-sprite** | Sprite精灵系统，SpriteRenderer | SpriteAtlas图集优化、减少DrawCall |
| **unity-2d-animation** | 2D动画系统，Animator | 精灵动画、帧动画、骨骼动画 |
| **unity-tilemap** | 瓦片地图系统 | SetTiles批量API（快50倍） |
| **unity-input-system** | 新Input System | 事件订阅节省99%性能 |
| **unity-ui-system** | UI系统 | Canvas分离策略（提升5-10倍性能） |
| **unity-audio-system** | 音频系统 | 音频池实现、限制并发<32 |

### L1级Skill结构
每个L1级Skill包含：
- `SKILL.md` - 主文档（功能边界、API白名单、学习路径）
- `references/api-reference.md` - 完整API参考（8000+字）
- `references/basics.md` - 基础概念详解（6000+字）
- `scripts/examples/` - 实战示例代码（3-5个）

---

## L2级：常用功能（8个）

这些是游戏开发的**常用功能模块**，可以直接集成到项目中：

| Skill名称 | 核心功能 | 使用场景 |
|----------|---------|---------|
| **unity-scene-management** | 场景加载、卸载、切换 | 关卡切换、场景状态管理 |
| **unity-prefab-system** | 预制体创建、实例化 | 动态生成、对象复用 |
| **unity-state-machine** | 状态机实现 | 角色AI、游戏流程 |
| **unity-coroutine-system** | 协程系统 | 异步逻辑、分帧执行 |
| **unity-object-pool** | 对象池 | 减少Instantiate/Destroy开销 |
| **unity-save-system** | 存档系统 | 数据持久化 |
| **unity-2d-character-controller** | 角色移动控制器 | 平台跳跃、俯视移动 |
| **unity-scriptableobject-config** | 数据配置类 | 角色、武器、物品数据 |

### L2级Skill结构
每个L2级Skill包含：
- `SKILL.md` - 主文档
- `references/api-reference.md` - 完整API参考
- `references/basics.md` - 基础概念
- `scripts/examples/` - 实战示例

---

## A级：代码生成器（2个）

完全自动化生成，遵循**渐进式披露原则**：

| Skill名称 | 功能说明 | 效率提升 |
|----------|---------|---------|
| **unity-singleton-manager** | 单例管理器生成 | 78%↑ |
| **unity-ui-panel** | UI面板控制器生成 | 75%↑ |

---

## B级：分析工具（3个）

**强制规范**，开发前后必须执行：

| Skill名称 | 功能说明 | 使用场景 |
|----------|---------|---------|
| **project-context-analyzer** | 项目框架分析，沉淀Context | **首次使用/Context缺失时必须** |
| **architecture-design-analyzer** | 架构设计分析 | 开发新功能前强制执行 |
| **impact-scope-analyzer** | 影响面分析 | 修改现有代码前执行 |

**Context 机制**：
- @project-context-analyzer 分析项目框架，输出到 `context/` 目录
- @requirement-workflow 自动检查 Context，缺失时提示沉淀
- 后续分析（架构/影响面）读取 Context 作为参考基础

---

## 需求编排器（1个）

**独立入口**，负责需求分析到审批的完整流程：

| Skill名称 | 功能说明 | 使用场景 |
|----------|---------|---------|
| **requirement-workflow** | 需求开发流程编排器 | **开发前必须**，基于业务上下文理解需求，输出审批通过的需求卡片 |

---

## 开发编排器（2个）⭐ 新增

**独立入口**，负责增量式开发和功能拆分：

| Skill名称 | 功能说明 | 使用场景 |
|----------|---------|---------|
| **development-workflow** | 开发执行流程编排器 | **审批通过后调用**，增量开发，一次一个功能 |
| **feature-decomposer** | 功能拆分器 | 将需求拆分为细粒度功能列表（JSON），分析依赖，标记可并行功能 |

**增量开发流程**：
```
@development-workflow start
    ↓
调用 @feature-decomposer 生成功能列表
    ↓
分析依赖关系，识别可并行开发的功能
    ↓
增量开发循环（一次一个功能）
    ↓
完成所有功能
```

---

## 进度管理（3个）⭐ 新增

**开发支持**，负责进度跟踪、恢复和会话管理：

| Skill名称 | 功能说明 | 使用场景 |
|----------|---------|---------|
| **progress-tracker** | 进度跟踪器 | 记录功能状态变更，生成进度统计报告 |
| **progress-recovery** | 进度恢复器 | 恢复开发状态，推荐下一个开发的功能 |
| **session-manager** | 会话管理器 | 管理开发会话生命周期，开始/结束会话 |

**使用示例**：
```bash
# 开始会话
@session-manager start
需求: inventory-system

# 更新进度
@progress-tracker update
需求: inventory-system
功能: INV-002
状态: completed

# 恢复状态
@progress-recovery
需求: inventory-system
```

---

## C级：代码审查工具（1个）

代码生成后**强制审查**：

| Skill名称 | 功能说明 |
|----------|---------|
| **unity-code-review** | 完整审查清单（性能、安全、最佳实践） |

---

## 开发工作流

```
需求输入
  ↓
阶段1：架构设计与影响分析 ⚠️ B级Skills强制执行
  ↓
阶段2：需求渐进式披露 ⚠️ 强制规范
  ↓
阶段3：任务分级
  ↓
阶段4：AI代码生成
  ├── L1级任务：参考L1级Skills文档
  ├── L2级任务：使用L2级Skills模板
  └── A级任务：自动生成（A级Skills）
  ↓
阶段5：代码审查 ⚠️ C级Skills强制执行
  ↓
阶段6：测试验证
  ↓
阶段7：文档沉淀 ⚠️ 强制规范
```

---

## 核心原则

### 1. 层级化学习路径
- **L1 → L2 → L3**：先掌握核心基础，再学习功能模块，最后集成高级方案
- 每个Skill独立可用，也可以组合使用

### 2. 渐进式披露原则
- 不假设用户需求
- 主动确认技术细节
- 分步展示复杂度

### 3. 强制规范
- 架构设计分析：开发前必做
- 代码审查：代码生成后必做
- API白名单：三档分类（✅推荐/⚠️警告/❌禁止）

---

## 预期效率提升

| 任务类型 | 传统方式 | AI Coding方式 | 效率提升 |
|---------|---------|--------------|---------|
| 基础脚本编写 | 30分钟 | 5分钟 | 83%↑ |
| 数据类定义 | 20分钟 | 3分钟 | 85%↑ |
| UI面板框架 | 60分钟 | 15分钟 | 75%↑ |
| 游戏管理器 | 45分钟 | 10分钟 | 78%↑ |
| 架构分析 | 60分钟 | 15分钟 | 75%↑ |
| 代码审查 | 30分钟 | 10分钟 | 67%↑ |
| 技术文档查询 | 30分钟 | 2分钟 | 93%↑ |

**整体效率提升**：60-85%

---

## 版本信息

- **版本**：3.0.0
- **创建日期**：2026-03-16
- **更新日期**：2026-03-16
- **适用项目**：Unity 2D独立游戏开发
- **团队规模**：3-10人
- **Skills总数**：27个（L1级7个 + L2级8个 + A级2个 + B级3个 + 编排器2个 + 进度管理3个 + C级1个）

---

## 更新日志

### v3.0.0 (2026-03-16)
- **重大更新**：层级化重组，统一到unity-2d-game-development大Skill下
- 新增L1级目录（7个核心基础Skills）
- 新增L2级目录（8个常用功能Skills）
- 补全unity-2d-character-controller完整文档
- 补全unity-scriptableobject-config完整文档
- 创建A级/B级/C级独立目录
- 更新README.md反映新结构

### v2.0.0 (2026-03-16)
- 新增7个D级Skills（技术文档库）
- 所有D级Skills包含API白名单三档分类

### v1.0.0 (2026-03-16)
- 初始版本发布
- 包含7个核心Skills

---

## 技术支持

如遇到问题，请检查：
1. Skills目录结构是否完整
2. SKILL.md文件格式是否正确
3. YAML前置元数据是否存在
4. CodeBuddy是否正确加载Skills

---

**总计**：27个Skills | 3个层级 | 120,000+字文档 | 30+完整代码示例
