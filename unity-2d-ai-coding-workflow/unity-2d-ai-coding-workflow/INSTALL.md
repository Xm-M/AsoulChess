# 安装说明

## 前置要求

- Unity 2022.3 LTS 或更高版本
- 支持CodeBuddy的IDE（如Cursor、VS Code等）
- 基础的Unity和C#知识

## 安装步骤

### 方法1：直接复制（推荐）

#### 步骤1：复制核心文件

将以下目录复制到您的Unity项目根目录：

```
your-unity-project/
├── .codebuddy/              ← 复制整个目录
│   └── rules/               ← Skills规则文件（8个.mdc文件）
└── docs/                    ← 复制整个目录
    ├── norms/               ← 规范文档（5个.md文件）
    ├── workflow/            ← 工作流文档（2个.md文件）
    ├── templates/           ← 模板文件（2个.md文件）
    ├── quick-start-guide.md ← 快速开始指南
    └── skills-manual.md     ← Skills使用手册
```

#### 步骤2：创建项目目录结构

在Unity项目中创建标准目录：

```
Assets/
└── _Project/
    ├── Scripts/
    │   ├── Core/
    │   │   ├── Managers/
    │   │   ├── Events/
    │   │   ├── Pool/
    │   │   └── Utilities/
    │   ├── Game/
    │   │   ├── Character/
    │   │   ├── UI/
    │   │   ├── Item/
    │   │   ├── AI/
    │   │   └── Level/
    │   └── Editor/
    ├── Art/
    │   ├── Sprites/
    │   ├── Animations/
    │   ├── Materials/
    │   └── UI/
    ├── Prefabs/
    │   ├── Character/
    │   ├── UI/
    │   └── Environment/
    ├── Scenes/
    │   ├── Levels/
    │   ├── MainMenu.unity
    │   └── Bootstrap.unity
    └── ScriptableObjects/
        ├── Character/
        ├── Item/
        └── Level/
```

#### 步骤3：创建Assembly Definition

**Core.asmdef**（路径：Assets/_Project/Scripts/Core/）
```json
{
  "name": "Core",
  "rootNamespace": "",
  "references": [],
  "includePlatforms": [],
  "excludePlatforms": [],
  "allowUnsafeCode": false,
  "overrideReferences": false,
  "precompiledReferences": [],
  "autoReferenced": true,
  "defineConstraints": [],
  "versionDefines": [],
  "noEngineReferences": false
}
```

**Game.asmdef**（路径：Assets/_Project/Scripts/Game/）
```json
{
  "name": "Game",
  "rootNamespace": "",
  "references": [
    "GUID:xxx"  // Core的GUID
  ],
  "includePlatforms": [],
  "excludePlatforms": [],
  "allowUnsafeCode": false,
  "overrideReferences": false,
  "precompiledReferences": [],
  "autoReferenced": true,
  "defineConstraints": [],
  "versionDefines": [],
  "noEngineReferences": false
}
```

#### 步骤4：配置Git（如使用版本控制）

创建 `.gitignore` 文件：

```gitignore
# Unity generated
[Ll]ibrary/
[Tt]emp/
[Oo]bj/
[Bb]uild/
[Bb]uilds/
[Ll]ogs/
[Mm]emoryCaptures/

# IDE
.vs/
.idea/
*.csproj
*.sln
*.userprefs

# OS
.DS_Store
Thumbs.db
```

创建 `.gitattributes` 文件（用于Git LFS）：

```gitattributes
# Image files
*.png filter=lfs diff=lfs merge=lfs -text
*.jpg filter=lfs diff=lfs merge=lfs -text

# Audio files
*.wav filter=lfs diff=lfs merge=lfs -text
*.mp3 filter=lfs diff=lfs merge=lfs -text
```

---

### 方法2：选择性复制

如果您只想使用部分功能：

#### 只使用Skills

仅复制 `.codebuddy/rules/` 目录到项目根目录

#### 只使用规范文档

仅复制 `docs/norms/` 目录到项目文档目录

#### 只使用模板

仅复制 `docs/templates/` 目录

---

## 验证安装

### 验证Skills

在对话中输入：
```
@unity-2d-character
```

如果AI正确响应并开始渐进式披露流程，说明Skills安装成功。

### 验证规范

检查 `docs/norms/` 目录下是否有5个规范文档：
- architecture-analysis-norm.md
- progressive-disclosure-norm.md
- code-review-norm.md
- version-control-norm.md
- project-structure-norm.md

### 验证目录结构

确认已创建 `_Project` 目录及其子目录。

---

## 快速开始

### 第一次使用

1. 阅读 `docs/quick-start-guide.md`
2. 查看 `examples/example-01-inventory-system.md`
3. 尝试开发第一个简单功能

### 日常工作流

```
需求 → 架构分析 → 渐进式披露 → AI生成 → 审查 → 测试 → 文档沉淀
```

---

## 常见问题

### Q1: Skills无法触发？

**解决方案**：
1. 确认 `.codebuddy/rules/` 目录在项目根目录
2. 确认文件是 `.mdc` 格式
3. 重启IDE

### Q2: 找不到docs目录？

**解决方案**：
1. 确认复制了整个 `docs/` 目录
2. 检查路径是否正确

### Q3: Assembly Definition报错？

**解决方案**：
1. 确认Core.asmdef不依赖任何项目代码
2. 确认Game.asmdef正确引用Core
3. 在Unity中重新生成GUID

### Q4: Git LFS未配置？

**解决方案**：
1. 安装Git LFS：`git lfs install`
2. 添加 `.gitattributes` 文件
3. 提交更改

---

## 更新

### 更新Skills

将新版本的 `.codebuddy/rules/` 目录覆盖旧版本

### 更新文档

将新版本的 `docs/` 目录覆盖旧版本

---

## 卸载

### 完全卸载

删除以下目录：
- `.codebuddy/`
- `docs/`（如不需要）

### 保留项目文件

仅删除 `.codebuddy/` 和 `docs/norms/`

---

## 技术支持

### 获取帮助

1. 查阅 `docs/quick-start-guide.md`
2. 查看 `examples/` 目录下的示例案例
3. 阅读 `docs/norms/` 目录下的规范文档

### 反馈问题

如发现问题或有改进建议：
1. 记录问题描述
2. 附上相关日志或截图
3. 提交给团队负责人

---

## 版本信息

**当前版本**：v1.0.0

**发布日期**：2026-03-16

**包含内容**：
- 5个核心规范
- 7个核心Skills
- 完整文档体系
- 2个示例案例

---

## 下一步

安装完成后：

1. ✅ 阅读快速开始指南
2. ✅ 熟悉规范体系
3. ✅ 尝试第一个功能开发
4. ✅ 积累最佳实践

祝开发顺利！🎮
