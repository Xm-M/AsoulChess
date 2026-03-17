# Skills快速参考卡片

## 📊 层级化结构

```
unity-2d-game-development/      # 大Skill：Unity 2D游戏开发
├── l1-basics/                  # L1级：核心基础（7个技术文档库）
├── l2-features/                # L2级：常用功能（9个可直接集成）
└── l3-advanced/                # L3级：高级功能（待创建）

unity-design-patterns/          # 大Skill：Unity设计模式体系
├── l1-theory/                  # L1级：理论认知（7个）
├── l2-practice/                # L2级：常用实战（8个）
└── l3-advanced/                # L3级：高级应用（6个）

a-generators/                   # A级：代码生成器（2个，自动化）
b-analyzers/                    # B级：分析工具（2个，强制）
c-reviewers/                    # C级：审查工具（1个，强制）
```

---

## 🎯 L1级Skills（核心基础 - 技术文档库）

### unity-2d-physics
**功能：** 2D物理系统文档
```
阅读场景：Rigidbody2D、碰撞检测、射线检测、物理材质
关键优化：刚体睡眠、碰撞层优化、批量检测
API白名单：✅推荐 / ⚠️警告 / ❌禁止
```

### unity-2d-sprite
**功能：** Sprite精灵系统文档
```
阅读场景：SpriteRenderer、图集优化、图层排序、遮罩交互
关键优化：SpriteAtlas减少DrawCall、批量渲染
API白名单：✅推荐 / ⚠️警告 / ❌禁止
```

### unity-2d-animation
**功能：** 2D动画系统文档
```
阅读场景：Animator、精灵动画、帧动画、骨骼动画
关键优化：动画状态机设计、参数优化
API白名单：✅推荐 / ⚠️警告 / ❌禁止
```

### unity-tilemap
**功能：** 瓦片地图系统文档
```
阅读场景：Tilemap、瓦片绘制、碰撞检测、地图生成
关键优化：SetTiles批量API（比SetTile快50倍）
API白名单：✅推荐 / ⚠️警告 / ❌禁止
```

### unity-input-system
**功能：** 新Input System文档
```
阅读场景：InputAction、事件驱动、多设备支持
关键优化：事件订阅节省99%性能、禁止旧Input
API白名单：✅推荐 / ⚠️警告 / ❌禁止
```

### unity-ui-system
**功能：** UI系统文档
```
阅读场景：Canvas、TextMeshPro、Button、Layout Group
关键优化：Canvas分离策略（提升5-10倍性能）
API白名单：✅推荐 / ⚠️警告 / ❌禁止
```

### unity-audio-system
**功能：** 音频系统文档
```
阅读场景：AudioSource、AudioMixer、3D音效
关键优化：音频池实现、限制并发<32
API白名单：✅推荐 / ⚠️警告 / ❌禁止
```

---

## 🚀 L2级Skills（常用功能 - 可直接集成）

### unity-scene-management
**功能：** 场景管理系统
```
@unity-scene-management
场景列表: ["MainMenu", "Level1", "Level2"]
加载模式: [single/additive]
加载动画: [yes/no]
过渡效果: [fade/wipe/none]
```

### unity-prefab-system
**功能：** 预制体系统
```
@unity-prefab-system
预制体名: [名称]
池化: [yes/no]
初始化参数: [参数列表]
```

### unity-state-machine
**功能：** 状态机实现
```
@unity-state-machine
状态列表: [Idle, Walk, Run, Jump]
初始状态: [Idle]
过渡条件: [详细描述]
```

### unity-coroutine-system
**功能：** 协程系统
```
@unity-coroutine-system
用途: [异步加载/分帧执行/延迟调用]
优化: [对象池/取消机制]
```

### unity-object-pool
**功能：** 对象池系统
```
@unity-object-pool
对象类型: [Bullet/Enemy/Effect]
初始容量: [数值]
扩容策略: [double/linear]
```

### unity-save-system
**功能：** 存档系统
```
@unity-save-system
存档类型: [local/cloud]
数据格式: [json/binary]
加密: [yes/no]
```

### unity-2d-character-controller
**功能：** 2D角色移动控制器
```
@unity-2d-character-controller
模式: [platform/topdown/fighting]
跳跃: [yes/no]
攀爬: [yes/no]
动画: [yes/no]
输入: [keyboard/touch/gamepad]
移动速度: [数值]
```

### unity-scriptableobject-config
**功能：** 数据配置类
```
@unity-scriptableobject-config
配置名: [名称]
类型: [character/weapon/item/level]
字段列表: [
  {"name": "字段名", "type": "类型", "validation": "验证规则"}
]
```

### unity-timeline
**功能：** 时间轴系统
```
@unity-timeline
用途: [过场动画/教程编排/事件序列/Boss战斗]
轨道类型: [animation/audio/activation/signal]
播放控制: [play/pause/stop/speed]
信号事件: [事件名称列表]
```

---

## ⚡ A级Skills（代码生成器 - 自动化）

### unity-singleton-manager
**功能：** 全局管理器生成
```
@unity-singleton-manager
管理器名: [GameManager/AudioManager]
功能: [game/level/audio/save/pool/custom]
持久化: [yes/no]
事件列表: [
  {"name": "事件名", "type": "Action<T>", "tooltip": "说明"}
]
```

### unity-ui-panel
**功能：** UI面板控制器生成
```
@unity-ui-panel
面板名: [InventoryPanel]
类型: [popup/fullscreen/overlay]
动画: [dotween/animation/none]
遮罩: [yes/no]
元素: [
  {"name": "元素名", "type": "Button", "action": "Close"}
]
```

---

## ⚠️ B级Skills（分析工具 - 强制执行）

### architecture-design-analyzer
**功能：** 架构设计分析
```
@architecture-design-analyzer
需求: [功能描述]
模块: [所属模块]
复杂度: [A级/B级/C级]
```
**触发时机：** 开发新功能前必须执行

### impact-scope-analyzer
**功能：** 影响面分析
```
@impact-scope-analyzer
改动类型: [新增功能/修改功能/删除功能]
涉及文件: [文件路径列表]
```
**触发时机：** 修改现有代码前必须执行

---

## ⚠️ C级Skills（审查工具 - 强制执行）

### unity-code-review
**功能：** 代码审查
```
@unity-code-review
代码: [粘贴生成的代码]
类型: [monobehaviour/scriptableobject/static]
```
**触发时机：** AI生成代码后必须执行

---

## 📋 使用流程

### 开发新功能时
```
1. 创建需求卡片（标准格式）
2. 执行架构分析 → @architecture-design-analyzer ⚠️ 必须
   【重点】架构分析时参考设计模式 → 查阅 @design-patterns-overview
3. 执行影响面分析 → @impact-scope-analyzer ⚠️ 必须
4. 阅读相关L1级Skills技术文档
5. 选择L2级Skills或A级Skills生成代码
   【设计模式】根据场景选择合适模式：
   - 状态管理 → @state-machine-pattern
   - 事件通信 → @event-system-pattern
   - 对象复用 → @object-pool-pattern
   - 输入处理 → @command-pattern
6. 代码审查 → @unity-code-review ⚠️ 必须
   【重点】审查时检查设计模式应用
7. 测试验证
8. 文档沉淀
```

### 查询技术文档时
```
直接阅读L1级Skills文档：
- 物理问题 → @unity-2d-physics
- 精灵问题 → @unity-2d-sprite
- 动画问题 → @unity-2d-animation
- 地图问题 → @unity-tilemap
- 输入问题 → @unity-input-system
- UI问题 → @unity-ui-system
- 音频问题 → @unity-audio-system
```

---

## 💡 最佳实践

### 1. 优先使用L1级Skills文档
开发前先阅读相关技术文档，了解API白名单和最佳实践

### 2. 选择合适的L2级Skills
L2级Skills提供完整实现，可直接集成到项目

### 3. 简单任务使用A级Skills
单例管理器、UI面板等标准化组件使用A级Skills自动生成

### 4. 不要跳过B级/C级Skills
架构分析、影响面分析、代码审查是强制环节

### 5. 利用API白名单
L1级Skills的API白名单帮助避免性能陷阱

---

**版本：** v3.1.0  
**更新日期：** 2026-03-16  
**Skills总数：** 22个（L1级7个 + L2级9个 + A级2个 + B级2个 + C级1个 + 设计模式体系）
