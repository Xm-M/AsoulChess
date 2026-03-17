# Unity 2D AI Coding Workflow v3.0 - 增强版工作流

## 📋 版本说明

**版本**: v3.0.0  
**更新日期**: 2026-03-16  
**核心改进**: 层级化Skills重组（L1/L2/A/B/C），强制架构分析环节，渐进式披露原则

---

## 🎯 工作流核心原则

### 1. 强制性前置环节

**开发前必须完成（不可跳过）：**

```
需求输入 → 架构设计分析 → 影响面分析 → 审批通过 → 开始开发
    │              │              │            │
    │              │              │            └─ 审批通过才可继续
    │              │              └─ 识别改动范围和风险
    │              └─ 架构设计、技术选型、性能预估
    └─ 标准需求卡片格式
```

### 2. 渐进式披露开发流程

**参考Superpowers原则：AI在未知背景下主动确认，不提前开发**

```
开发阶段：
├─ Step 1: AI理解需求 → 确认理解（不生成代码）
├─ Step 2: AI确认技术细节 → 用户确认
├─ Step 3: AI生成代码框架 → 用户审查
├─ Step 4: AI填充实现细节 → 用户测试
└─ Step 5: AI优化和完善 → 最终交付

关键原则：
- 每一步都需要用户确认，AI不擅自行动
- 逐步披露需求细节，避免过度开发
- 用户可以随时调整方向
```

### 3. 层级化Skills体系

```
Skills层级化结构：
├─ unity-2d-game-development/      # 大Skill：Unity 2D游戏开发
│   ├─ l1-basics/                  # L1级：核心基础（7个技术文档）
│   │   ├─ unity-2d-physics        # 2D物理系统
│   │   ├─ unity-2d-sprite         # Sprite精灵系统
│   │   ├─ unity-2d-animation      # 2D动画系统
│   │   ├─ unity-tilemap           # 瓦片地图系统
│   │   ├─ unity-input-system      # 输入系统
│   │   ├─ unity-ui-system         # UI系统
│   │   └─ unity-audio-system      # 音频系统
│   │
│   ├─ l2-features/                # L2级：常用功能（9个可直接集成）
│   │   ├─ unity-scene-management  # 场景管理
│   │   ├─ unity-prefab-system     # 预制体系统
│   │   ├─ unity-state-machine     # 状态机
│   │   ├─ unity-coroutine-system  # 协程系统
│   │   ├─ unity-object-pool       # 对象池
│   │   ├─ unity-save-system       # 存档系统
│   │   ├─ unity-2d-character-controller  # 角色控制器
│   │   ├─ unity-scriptableobject-config  # 数据配置
│   │   └─ unity-timeline          # 时间轴系统
│   │
│   └─ l3-advanced/                # L3级：高级功能（待创建）
│
├─ unity-design-patterns/          # 大Skill：Unity设计模式体系
│   ├─ l1-theory/                  # L1级：理论认知（7个）
│   ├─ l2-practice/                # L2级：常用实战（8个）
│   └─ l3-advanced/                # L3级：高级应用（6个）
│   │
│   └─ l3-advanced/                # L3级：高级功能（待创建）
│
├─ a-generators/                   # A级：代码生成器（2个自动化）
│   ├─ unity-singleton-manager     # 单例管理器
│   └─ unity-ui-panel              # UI面板
│
├─ b-analyzers/                    # B级：分析工具（2个，强制执行）
│   ├─ architecture-design-analyzer  # 架构设计分析
│   └─ impact-scope-analyzer         # 影响面分析
│
└─ c-reviewers/                    # C级：审查工具（1个，强制执行）
    └─ unity-code-review           # 代码审查
```

---

## 🔄 完整工作流程

### 阶段一：需求分析与架构设计（强制）

#### Step 1.1: 创建需求卡片

```markdown
## 功能需求卡片

### 基本信息
- **功能名称**: [简短描述]
- **所属模块**: [角色系统/UI系统/游戏管理等]
- **优先级**: [P0/P1/P2]
- **预估复杂度**: [L1级/L2级/A级/B级/C级]
- **预估耗时**: [X小时]

### 功能描述
[详细描述功能需求，用自然语言说明]

### 技术约束
- **Unity版本**: [如2022.3 LTS]
- **目标平台**: [PC/移动端/主机]
- **性能要求**: [帧率/内存等约束]
- **依赖模块**: [需要引用的其他脚本或组件]

### 验收标准
- [ ] 功能实现完整
- [ ] 性能达标
- [ ] 无编译警告
- [ ] 代码通过审查
- [ ] 文档完整
```

#### Step 1.2: 架构设计分析（强制，使用B级Skill）

**触发方式：**
```
@architecture-design-analyzer
需求: [需求描述]
模块: [所属模块]
复杂度: [L1级/L2级/A级]
```

**输出：架构分析报告**

#### Step 1.3: 影响面分析（强制，使用B级Skill）

**触发方式：**
```
@impact-scope-analyzer
改动类型: [新增功能/修改功能/重构]
涉及文件: [预估的文件列表]
```

**输出：影响面分析报告**

#### Step 1.4: 审批决策

**审批标准：**
- ✅ 架构设计合理
- ✅ 技术方案可行
- ✅ 风险可控
- ✅ 性能预估达标

---

### 阶段二：渐进式开发流程

#### Step 2.1: 需求理解确认（AI主动确认）

**AI行为：** 分析需求，提出澄清问题，不生成代码

#### Step 2.2: 选择合适的Skill

| 需求类型 | 推荐Skill | 级别 | 说明 |
|---------|----------|------|------|
| 核心系统问题 | 查阅L1级文档 | L1级 | 物理系统、精灵、动画、输入等 |
| 角色移动控制 | unity-2d-character-controller | L2级 | 平台/俯视/横版角色移动 |
| 数据配置类 | unity-scriptableobject-config | L2级 | 角色/武器/物品数据配置 |
| 场景管理 | unity-scene-management | L2级 | 场景加载、切换、状态管理 |
| 对象池 | unity-object-pool | L2级 | 减少Instantiate/Destroy开销 |
| 存档系统 | unity-save-system | L2级 | 数据持久化 |
| 时间轴序列 | unity-timeline | L2级 | 过场动画、教程编排、事件序列 |
| 全局管理器 | unity-singleton-manager | A级 | GameManager、AudioManager等 |
| UI面板控制器 | unity-ui-panel | A级 | 菜单、背包、设置面板 |

#### Step 2.3: 查阅L1级技术文档

**开发前先阅读相关L1级Skills文档：**
- 物理问题 → `@unity-2d-physics`
- 精灵问题 → `@unity-2d-sprite`
- 动画问题 → `@unity-2d-animation`
- 地图问题 → `@unity-tilemap`
- 输入问题 → `@unity-input-system`
- UI问题 → `@unity-ui-system`
- 音频问题 → `@unity-audio-system`

#### Step 2.4-2.5: 渐进式生成
- 生成代码框架 → 用户确认
- 填充实现细节 → 用户测试
- 优化和完善 → 最终交付

---

### 阶段三：代码审查（强制）

**触发方式：**
```
@unity-code-review
代码: [粘贴生成的代码]
类型: [monobehaviour/scriptableobject/ui/manager]
```

**审查维度：** 功能正确性、Unity最佳实践、性能优化、代码质量、安全性

---

### 阶段四：测试验证

- 编译测试
- 功能测试
- 性能测试

---

### 阶段五：文档沉淀

- 更新Skills库
- 更新最佳实践
- 更新项目文档

---

## 📊 效率提升统计

| 任务类型 | 传统开发 | AI辅助 | 效率提升 |
|---------|---------|--------|---------|
| 架构分析 | 60分钟 | 15分钟 | 75% ↑ |
| 技术文档查询 | 30分钟 | 2分钟 | 93% ↑ |
| 角色控制器 | 30分钟 | 5分钟 | 83% ↑ |
| 数据配置类 | 20分钟 | 3分钟 | 85% ↑ |
| UI面板 | 60分钟 | 15分钟 | 75% ↑ |
| 代码审查 | 30分钟 | 10分钟 | 67% ↑ |
| **整体平均** | - | - | **60-85% ↑** |

---

## 🔧 Skills快速参考

### L1级Skills（核心基础，7个技术文档库）

| Skill名称 | 触发词 | 核心内容 | 字数 |
|----------|--------|---------|------|
| 2D物理系统 | `@unity-2d-physics` | Rigidbody2D、碰撞检测、射线检测 | 5000+ |
| 精灵系统 | `@unity-2d-sprite` | SpriteRenderer、图集优化、图层排序 | 6000+ |
| 2D动画系统 | `@unity-2d-animation` | Animator、精灵动画、帧动画、骨骼动画 | 6000+ |
| 瓦片地图 | `@unity-tilemap` | Tilemap、瓦片绘制、碰撞检测 | 8000+ |
| 输入系统 | `@unity-input-system` | InputAction、事件驱动、多设备 | 9000+ |
| UI系统 | `@unity-ui-system` | Canvas、TextMeshPro、Layout | 9000+ |
| 音频系统 | `@unity-audio-system` | AudioSource、AudioMixer、音频池 | 9000+ |

### L2级Skills（常用功能，9个可直接集成）

| Skill名称 | 触发词 | 适用场景 | 效率提升 |
|----------|--------|---------|---------|
| 场景管理 | `@unity-scene-management` | 场景加载、切换、状态管理 | 70% ↑ |
| 预制体系统 | `@unity-prefab-system` | 动态实例化、对象复用 | 75% ↑ |
| 状态机 | `@unity-state-machine` | 角色AI、游戏流程控制 | 80% ↑ |
| 协程系统 | `@unity-coroutine-system` | 异步逻辑、分帧执行 | 75% ↑ |
| 对象池 | `@unity-object-pool` | 减少Instantiate/Destroy开销 | 85% ↑ |
| 存档系统 | `@unity-save-system` | 数据持久化、存档管理 | 70% ↑ |
| 角色2D控制器 | `@unity-2d-character-controller` | 平台/俯视/横版角色移动 | 83% ↑ |
| 数据配置类 | `@unity-scriptableobject-config` | 角色/武器/物品数据配置 | 85% ↑ |
| 时间轴系统 | `@unity-timeline` | 过场动画、教程编排、事件序列 | 80% ↑ |

### A级Skills（代码生成器，2个自动化）

| Skill名称 | 触发词 | 适用场景 | 效率提升 |
|----------|--------|---------|---------|
| 单例管理器 | `@unity-singleton-manager` | GameManager、AudioManager等 | 78% ↑ |
| UI面板控制器 | `@unity-ui-panel` | 菜单、背包、设置面板 | 75% ↑ |

### B级Skills（分析工具，2个）- ⚠️ 强制执行

| Skill名称 | 触发词 | 使用时机 |
|----------|--------|---------|
| 架构设计分析 | `@architecture-design-analyzer` | 开发前 |
| 影响面分析 | `@impact-scope-analyzer` | 开发前 |

### C级Skills（审查工具，1个）- ⚠️ 强制执行

| Skill名称 | 触发词 | 使用时机 |
|----------|--------|---------|
| 代码审查 | `@unity-code-review` | AI生成代码后 |

---

**详细Skills使用指南请参考：** `skills-quick-reference.md`
