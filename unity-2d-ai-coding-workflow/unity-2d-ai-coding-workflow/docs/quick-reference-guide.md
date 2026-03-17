# Unity 2D AI Coding Workflow v3.0 - 快速参考指南

## 🎯 核心流程（5步强制）

```
需求卡片 → 架构分析 → 影响面分析 → 审批 → 开发 → 审查 → 测试 → 文档
   ↓           ↓            ↓          ↓      ↓      ↓      ↓      ↓
 标准      @arch-      @impact-    人工   L2/A级 @code  功能   Skills
 格式      analyzer    analyzer   决策   Skill  review  测试   沉淀
```

---

## 📋 需求卡片模板

```markdown
## 功能需求卡片

### 基本信息
- 功能名称: [简短描述]
- 所属模块: [角色系统/UI系统/游戏管理等]
- 优先级: [P0/P1/P2]
- 预估复杂度: [L1级/L2级/A级/B级/C级]
- 预估耗时: [X小时]

### 功能描述
[详细描述功能需求]

### 技术约束
- Unity版本: [如2022.3 LTS]
- 目标平台: [PC/移动端/主机]
- 性能要求: [帧率/内存等约束]
- 依赖模块: [需要引用的其他脚本或组件]

### 验收标准
- [ ] 功能实现完整
- [ ] 性能达标
- [ ] 无编译警告
- [ ] 代码通过审查
- [ ] 文档完整
```

---

## 🔧 Skills速查表

### 层级化结构

```
unity-2d-game-development/      # 大Skill：Unity 2D游戏开发
├── l1-basics/                  # L1级：核心基础（7个）
├── l2-features/                # L2级：常用功能（9个）
└── l3-advanced/                # L3级：高级功能（待创建）

unity-design-patterns/          # 大Skill：Unity设计模式体系
├── l1-theory/                  # L1级：理论认知（7个）
├── l2-practice/                # L2级：常用实战（8个）
└── l3-advanced/                # L3级：高级应用（6个）

a-generators/                   # A级：代码生成器（2个）
b-analyzers/                    # B级：分析工具（2个，强制）
c-reviewers/                    # C级：审查工具（1个，强制）
```

---

### L1级Skills（核心基础，技术文档库）

| Skill名称 | 触发词 | 核心内容 | 字数 |
|----------|--------|---------|------|
| 2D物理系统 | `@unity-2d-physics` | Rigidbody2D、碰撞检测、射线检测 | 5000+ |
| 精灵系统 | `@unity-2d-sprite` | SpriteRenderer、图集优化、图层排序 | 6000+ |
| 2D动画系统 | `@unity-2d-animation` | Animator、精灵动画、帧动画、骨骼动画 | 6000+ |
| 瓦片地图 | `@unity-tilemap` | Tilemap、瓦片绘制、碰撞检测 | 8000+ |
| 输入系统 | `@unity-input-system` | InputAction、事件驱动、多设备 | 9000+ |
| UI系统 | `@unity-ui-system` | Canvas、TextMeshPro、Layout | 9000+ |
| 音频系统 | `@unity-audio-system` | AudioSource、AudioMixer、音频池 | 9000+ |

### L2级Skills（常用功能，可直接集成）

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

### A级Skills（代码生成器，完全自动化）

| Skill名称 | 触发词 | 适用场景 | 效率提升 |
|----------|--------|---------|---------|
| 单例管理器 | `@unity-singleton-manager` | GameManager、AudioManager等 | 78% ↑ |
| UI面板控制器 | `@unity-ui-panel` | 菜单、背包、设置面板 | 75% ↑ |

### B级Skills（分析类，⚠️ 强制执行）

| Skill名称 | 触发词 | 使用时机 | 强制性 |
|----------|--------|---------|--------|
| 架构设计分析 | `@architecture-design-analyzer` | 开发前 | ✅ 强制 |
| 影响面分析 | `@impact-scope-analyzer` | 开发前 | ✅ 强制 |

### C级Skills（审查类，⚠️ 强制执行）

| Skill名称 | 触发词 | 使用时机 | 强制性 |
|----------|--------|---------|--------|
| 代码审查 | `@unity-code-review` | AI生成代码后 | ✅ 强制 |

---

## 🚀 快速使用示例

### 1. 创建角色控制器
```
@unity-2d-character-controller
模式: platform
跳跃: yes
攀爬: no
动画: yes
输入: keyboard
移动速度: 6
跳跃力度: 12
```

### 2. 创建数据配置
```
@unity-scriptableobject-config
配置名: PlayerData
类型: character
字段列表: [
  {"name": "playerId", "type": "int", "validation": ">0"},
  {"name": "playerName", "type": "string", "validation": "required"},
  {"name": "maxHealth", "type": "int", "validation": "10-1000"},
  {"name": "moveSpeed", "type": "float", "validation": "1-10"}
]
```

### 3. 创建UI面板
```
@unity-ui-panel
面板名: InventoryPanel
类型: fullscreen
动画: dotween
遮罩: yes
元素: [
  {"name": "closeButton", "type": "Button", "action": "Close"},
  {"name": "itemGrid", "type": "GameObject", "tooltip": "物品网格容器"}
]
```

### 4. 创建管理器
```
@unity-singleton-manager
管理器名: GameManager
功能: game
持久化: yes
事件列表: [
  {"name": "OnScoreChanged", "type": "Action<int>", "tooltip": "分数改变时触发"}
]
```

### 5. 架构分析
```
@architecture-design-analyze
需求: 实现敌人AI系统，包含巡逻、追击、攻击三种状态
模块: 敌人系统
复杂度: B级
```

### 6. 影响面分析
```
@impact-scope-analyze
改动类型: 新增功能
涉及文件: Scripts/Enemies/（新建目录）
```

### 7. 代码审查
```
@unity-code-review
代码: [粘贴生成的代码]
类型: monobehaviour
```

---

## ⚠️ 强制规范提醒

### 开发前（不可跳过）
- ✅ 创建标准需求卡片
- ✅ 运行架构分析（`@architecture-design-analyze`）
- ✅ 运行影响面分析（`@impact-scope-analyze`）
- ✅ 审批通过后才能开始开发

### 开发中（渐进式披露）
- ✅ AI先理解需求，提出澄清问题
- ✅ AI生成代码框架，等待用户确认
- ✅ 逐步填充实现细节
- ✅ 用户可随时调整方向

### 开发后（不可跳过）
- ✅ 运行代码审查（`@unity-code-review`）
- ✅ 修复审查发现的问题
- ✅ 编译、功能、性能测试
- ✅ 文档沉淀，更新Skills库

---

## 📊 效率提升统计

| 环节 | 传统开发 | AI辅助 | 效率提升 |
|-----|---------|--------|---------|
| 架构分析 | 60分钟 | 15分钟 | 75% ↑ |
| 代码生成 | 60分钟 | 10分钟 | 83% ↑ |
| 代码审查 | 30分钟 | 10分钟 | 67% ↑ |
| **整体平均** | - | - | **60-80% ↑** |

---

**版本：** v2.0.0  
**更新日期：** 2026-03-16  
**核心改进：** 14个Skills整合，强制架构分析，渐进式披露原则
