---
name: feature-decomposer
description: 功能拆分器。将审批通过的需求拆分为细粒度的功能列表（JSON格式），分析功能依赖关系，标记可并行开发的功能，为增量开发提供基础。
---

# 功能拆分器

## 目的

将审批通过的需求拆分为细粒度的、可独立开发的功能单元，并分析依赖关系，支持并行开发和进度跟踪。

**关键设计**：功能列表在**开发前**生成，而非需求分析阶段，确保更贴近技术实现。

## 使用场景

**触发时机**：
- 需求审批通过后，开始开发前
- 需要细化开发计划时
- 用户主动要求拆分功能

**输入**：
- 需求卡片（`docs/requirements/[需求ID].md`）
- 架构设计文档（`docs/architecture/[需求ID]/`）
- 审批报告（`docs/approvals/[需求ID].md`）

**输出**：
- 功能列表（`features/[需求ID].json`）

## 触发方式

```
@feature-decomposer
需求: [需求ID或需求卡片路径]
```

简写：
```
@feature-decomposer inventory-system
```

## 工作流程

```
读取需求卡片
    ↓
读取架构设计
    ↓
识别功能边界
    ↓
创建功能条目
    ↓
分析依赖关系
    ↓
标记并行开发机会
    ↓
生成开发计划
    ↓
输出 JSON 功能列表
```

---

## 阶段1：需求理解

**目标**：充分理解需求范围和目标。

**操作**：
1. 读取需求卡片
2. 提取核心功能描述
3. 理解验收标准
4. 识别技术要求

**输出**：
```
📋 需求摘要

需求名称: [名称]
所属模块: [模块]
需求类型: [新增/扩展/优化/重构]

核心功能:
1. [功能点1]
2. [功能点2]
3. [功能点3]

技术要求:
- [技术1]
- [技术2]

预估复杂度: [L1/L2/A级]
```

---

## 阶段2：功能边界识别

**目标**：识别功能的层次边界。

**分层策略**：

| 层次 | 类型 | 说明 | 示例 |
|------|------|------|------|
| **UI层** | ui | 用户界面相关 | 背包界面、按钮、网格 |
| **逻辑层** | logic | 业务逻辑 | 拖拽逻辑、堆叠判断 |
| **数据层** | data | 数据模型 | 物品定义、库存数据 |
| **集成层** | integration | 系统集成 | 存档集成、事件通知 |
| **测试层** | test | 测试验证 | 单元测试、集成测试 |

**操作**：
1. 逐条分析需求功能点
2. 判定每个功能所属的层次
3. 识别层次间的依赖关系

---

## 阶段3：功能条目创建

**目标**：为每个功能创建详细的条目。

**功能条目结构**：

```json
{
  "id": "[前缀]-[序号]",
  "title": "简短标题",
  "description": "详细描述",
  "type": "ui|logic|data|integration|test",
  "priority": "P0|P1|P2",
  "status": "not_started"
}
```

**创建原则**：

1. **单一职责**：每个功能只做一件事
2. **可验证**：有明确的验收标准
3. **可估算**：能预估代码行数和开发时间
4. **可独立**：尽量减少与其他功能的耦合

**示例**：

```json
{
  "id": "INV-001",
  "title": "背包 UI 基础框架",
  "description": "创建背包界面，显示 8x5 网格布局，支持响应式适配",
  "type": "ui",
  "priority": "P0",
  "status": "not_started",
  
  "technical_details": {
    "component": "InventoryUI",
    "prefab": "UI/InventoryPanel",
    "scripts": ["InventoryPanel.cs", "InventorySlot.cs"],
    "dependencies": ["@unity-ui-system"],
    "estimated_lines": 150,
    "estimated_hours": 3
  },
  
  "acceptance_criteria": [
    "显示 8x5 网格布局",
    "每个格子有独立标识",
    "适配 16:9、4:3、21:9 等常见比例"
  ]
}
```

---

## 阶段4：依赖分析

**目标**：分析功能间的依赖关系。

**依赖类型**：

| 类型 | 符号 | 说明 |
|------|------|------|
| **强依赖** | → | 必须先完成 A 才能开始 B |
| **弱依赖** | ⇢ | B 可以开始，但功能不完整直到 A 完成 |
| **无依赖** | ⊘ | 可完全独立开发 |

**分析维度**：

1. **数据依赖**
   - 功能 B 需要使用功能 A 定义的数据结构
   - 示例：物品拖拽需要使用物品数据模型

2. **接口依赖**
   - 功能 B 需要调用功能 A 提供的接口
   - 示例：存档功能需要调用库存查询接口

3. **UI依赖**
   - 功能 B 需要在功能 A 的 UI 基础上扩展
   - 示例：物品拖拽需要在背包网格上实现

4. **事件依赖**
   - 功能 B 需要监听功能 A 触发的事件
   - 示例：UI 更新需要监听库存变化事件

**输出格式**：
```
📋 依赖关系图

INV-001 (背包 UI) 
    → INV-002 (物品拖拽)
    → INV-005 (快捷栏)

INV-003 (物品数据)
    → INV-002 (物品拖拽)
    → INV-004 (物品堆叠)
    ⇢ INV-006 (存档集成)

INV-004 (物品堆叠)
    ⊘ INV-005 (快捷栏)  [可并行]
```

---

## 阶段5：并行开发标记

**目标**：识别可并行开发的功能，提高开发效率。

**并行条件**：
1. 功能间无强依赖
2. 功能属于不同层次，互不干扰
3. 功能修改的文件不冲突

**标记策略**：

```json
{
  "dependencies": {
    "requires": [],
    "required_by": ["INV-002", "INV-005"],
    "parallelizable": true,
    "parallel_group": "ui_layer",
    "conflict_files": []
  }
}
```

**并行组划分**：

| 组名 | 说明 | 示例 |
|------|------|------|
| `ui_layer` | UI层功能 | 界面布局、动画效果 |
| `data_layer` | 数据层功能 | 数据模型、配置定义 |
| `logic_core` | 核心逻辑 | 状态管理、核心算法 |
| `integration` | 集成层 | 存档、网络、第三方 |
| `testing` | 测试层 | 单元测试、集成测试 |

**并行开发建议**：
```
📋 并行开发建议

可并行功能组:

组 A (ui_layer):
- INV-001: 背包 UI 基础框架
- INV-005: 快捷栏 UI
建议: 可同时开发，由不同开发者负责

组 B (data_layer):
- INV-003: 物品数据模型
- INV-007: 配置数据定义
建议: 可同时开发

注意: 组 A 和组 B 可完全并行，无冲突
```

---

## 阶段6：开发计划生成

**目标**：生成推荐的开发顺序和计划。

**排序原则**：

1. **依赖优先**：被依赖的功能先开发
2. **基础优先**：基础框架先于高级功能
3. **风险优先**：高风险功能先验证
4. **价值优先**：高价值功能优先交付

**分阶段策略**：

```json
{
  "development_plan": {
    "phases": [
      {
        "phase": 1,
        "name": "基础架构",
        "description": "搭建基础框架和数据模型",
        "features": ["INV-001", "INV-003"],
        "parallel": true,
        "estimated_hours": 5
      },
      {
        "phase": 2,
        "name": "核心交互",
        "description": "实现核心交互逻辑",
        "features": ["INV-002", "INV-004"],
        "parallel": false,
        "estimated_hours": 8
      },
      {
        "phase": 3,
        "name": "扩展功能",
        "description": "添加快捷栏和存档",
        "features": ["INV-005", "INV-006"],
        "parallel": true,
        "estimated_hours": 6
      }
    ],
    "critical_path": ["INV-001", "INV-002", "INV-004"],
    "parallel_opportunities": [
      {
        "group": ["INV-001", "INV-003"],
        "benefit": "节省 2 小时开发时间"
      }
    ]
  }
}
```

---

## 阶段7：输出功能列表

**输出文件**：`features/[需求ID].json`

**完整结构**：

```json
{
  "version": "1.0",
  "schema": "feature-list-v1",
  
  "metadata": {
    "requirement_id": "inventory-system",
    "requirement_title": "背包系统",
    "generated_at": "2026-04-02T10:00:00Z",
    "generated_by": "feature-decomposer",
    "estimated_total_hours": 19
  },
  
  "features": [
    {
      "id": "INV-001",
      "title": "背包 UI 基础框架",
      "description": "创建背包界面，显示 8x5 网格布局",
      "type": "ui",
      "priority": "P0",
      "status": "not_started",
      
      "technical_details": {
        "component": "InventoryUI",
        "prefab": "UI/InventoryPanel",
        "scripts": ["InventoryPanel.cs", "InventorySlot.cs"],
        "dependencies": ["@unity-ui-system"],
        "estimated_lines": 150,
        "estimated_hours": 3
      },
      
      "dependencies": {
        "requires": [],
        "required_by": ["INV-002", "INV-005"],
        "parallelizable": true,
        "parallel_group": "ui_layer",
        "conflict_files": []
      },
      
      "acceptance_criteria": [
        "显示 8x5 网格布局",
        "每个格子有独立标识 (0-39)",
        "适配 16:9、4:3、21:9 等常见比例"
      ],
      
      "verification": {
        "test_type": "visual",
        "test_steps": [
          "打开背包界面 (按 B 键)",
          "验证网格显示为 8x5",
          "调整窗口大小，验证适配"
        ],
        "test_data": null
      },
      
      "notes": "优先实现，其他功能依赖此功能"
    }
  ],
  
  "development_plan": {
    "phases": [
      {
        "phase": 1,
        "name": "基础架构",
        "features": ["INV-001", "INV-003"],
        "parallel": true,
        "estimated_hours": 5
      }
    ],
    "critical_path": ["INV-001", "INV-002", "INV-004"],
    "total_features": 6,
    "completed_features": 0
  },
  
  "statistics": {
    "by_type": {
      "ui": 2,
      "logic": 2,
      "data": 1,
      "integration": 1
    },
    "by_priority": {
      "P0": 4,
      "P1": 2
    }
  }
}
```

---

## 使用示例

### 示例1：背包系统功能拆分

```
用户: @feature-decomposer inventory-system

AI: 开始功能拆分...

📋 阶段1: 需求理解
需求: 背包系统 (inventory-system)
类型: 功能扩展
核心功能: 8x5网格、物品拖拽、堆叠合并、快捷栏、存档

📋 阶段2: 功能边界识别
UI层: 背包界面、快捷栏、拖拽视觉反馈
逻辑层: 拖拽逻辑、堆叠判断、使用逻辑
数据层: 物品定义、库存数据
集成层: 存档集成、事件系统

📋 阶段3: 创建功能条目
- INV-001: 背包 UI 基础框架 (UI, P0)
- INV-002: 物品拖拽功能 (Logic, P0)
- INV-003: 物品数据模型 (Data, P0)
- INV-004: 物品堆叠合并 (Logic, P0)
- INV-005: 快捷使用栏 (UI, P1)
- INV-006: 存档集成 (Integration, P1)

📋 阶段4: 依赖分析
依赖链:
INV-001 → INV-002 → INV-004
INV-003 → INV-002
INV-001 → INV-005

📋 阶段5: 并行开发标记
可并行组:
- 组A: INV-001 (UI), INV-003 (Data)
- 组B: INV-005 (UI) [需等待INV-001]

📋 阶段6: 开发计划
阶段1: 基础架构 (INV-001, INV-003) - 5小时 - 可并行
阶段2: 核心交互 (INV-002, INV-004) - 8小时
阶段3: 扩展功能 (INV-005, INV-006) - 6小时

📋 阶段7: 生成功能列表
输出: features/inventory-system.json

✅ 功能拆分完成!

功能统计:
- 总功能数: 6
- 预估总工时: 19小时
- 可并行节省: 2小时
- 关键路径: 13小时

查看功能列表: features/inventory-system.json
开始开发: @development-workflow start inventory-system
```

---

## 最佳实践

### 1. 功能粒度控制

**推荐粒度**：
- 代码行数：50-300 行
- 开发时间：1-4 小时
- 验收标准：2-5 条

**避免**：
- ❌ 功能过大（超过 500 行）
- ❌ 功能过小（不到 30 行）
- ❌ 功能间高度耦合

### 2. 依赖管理

**原则**：
- 尽量减少强依赖
- 通过接口抽象解耦
- 使用事件机制降低耦合

### 3. 并行开发

**适用场景**：
- UI层和数据层通常可并行
- 独立的功能模块可并行
- 测试代码可与功能代码并行

**注意事项**：
- 避免同时修改同一文件
- 提前定义好接口契约
- 定期进行集成测试

---

## 与其他 Skill 的关系

```
feature-decomposer
    ├── 输入 ← requirement-workflow (审批通过的需求)
    ├── 输入 ← architecture-design-analyzer (架构设计)
    ├── 输出 → features/[需求ID].json
    │             ↓
    │       被 development-workflow 读取
    │             ↓
    │       被 progress-tracker 更新状态
    │             ↓
    │       被 progress-recovery 恢复状态
```

---

## 版本信息

- **版本**: 1.0.0
- **创建日期**: 2026-04-02
- **适用场景**: Unity 2D 游戏开发功能拆分
