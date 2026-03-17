# 版本控制与协作规范

## 规范目的

建立统一的版本控制流程和团队协作机制，确保代码管理规范、团队协作顺畅。

## Git工作流

### 分支策略

**标准分支**：
```
main               # 稳定版本，仅通过合并release/hotfix更新
├─ develop         # 开发主分支，日常开发基于此分支
├─ feature/[name]  # 功能分支，从develop分出，完成后合并回develop
├─ hotfix/[name]   # 紧急修复分支，从main分出，修复后合并回main和develop
└─ release/[ver]   # 发布分支，从develop分出，测试通过后合并到main
```

### 分支命名规范

**命名格式**：
```
类型/简短描述

类型：
- feature: 新功能
- fix: Bug修复
- refactor: 重构
- hotfix: 紧急修复
- release: 发布版本

示例：
feature/inventory-system
fix/player-jump-height
refactor/object-pool-optimization
hotfix/save-data-corruption
release/v1.0.0
```

### 提交规范

**提交信息格式**：
```
[类型] 简短描述

[详细说明]（可选）

[相关Issue]（可选）

类型：
- feat: 新功能
- fix: Bug修复
- refactor: 重构
- docs: 文档更新
- style: 代码格式（不影响功能）
- test: 测试相关
- chore: 构建/工具/依赖

示例：
feat: 添加背包系统网格布局功能

- 实现8x5网格布局
- 支持拖拽移动
- 支持堆叠合并

Closes #123
```

### AI生成代码标记

**必须标记**：
```csharp
// AI-Generated: [功能名称]
// Prompt: [Prompt摘要，不超过100字]
// Generated-Date: [YYYY-MM-DD]
// Reviewed-By: [审查人姓名]

using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // ...
}
```

**标记位置**：文件顶部，using语句之前

## 协作流程

### 开发流程

```
1. 从develop创建feature分支
   ↓
2. 开发新功能（遵循渐进式披露）
   ↓
3. 完成架构分析（如需要）
   ↓
4. AI生成代码
   ↓
5. 代码审查
   ↓
6. 测试验证
   ↓
7. 提交Pull Request
   ↓
8. 审核通过后合并到develop
```

### Pull Request规范

**PR标题格式**：
```
[类型] 功能描述

示例：
[feat] 背包系统网格布局
[fix] 修复角色跳跃高度不一致
[refactor] 优化对象池内存管理
```

**PR描述模板**：
```markdown
## 功能描述
[简要描述此PR实现的功能]

## 改动内容
- [文件1]: [改动说明]
- [文件2]: [改动说明]

## AI生成记录
- ☐ 此PR包含AI生成的代码
- ☐ 已完成架构分析：[链接]
- ☐ 已通过代码审查

## 测试情况
- [ ] 编译测试通过
- [ ] 功能测试通过
- [ ] 性能测试通过（如需要）

## 截图/视频
[如有UI改动，附上截图或视频]

## 相关Issue
Closes #[Issue号]
```

### 代码合并要求

**必须满足**：
- [ ] 通过架构分析（如需要）
- [ ] 通过代码审查
- [ ] 通过编译测试
- [ ] 通过功能测试
- [ ] 更新相关文档
- [ ] 无合并冲突

## .gitignore配置

**Unity项目标准配置**：
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

# Build
*.apk
*.aab
*.ipa
*.exe

# AI Generated Logs
ai-generated-logs/
```

## Git LFS配置

**大文件管理**：
```gitattributes
# Image files
*.png filter=lfs diff=lfs merge=lfs -text
*.jpg filter=lfs diff=lfs merge=lfs -text
*.psd filter=lfs diff=lfs merge=lfs -text

# Audio files
*.wav filter=lfs diff=lfs merge=lfs -text
*.mp3 filter=lfs diff=lfs merge=lfs -text
*.ogg filter=lfs diff=lfs merge=lfs -text

# Video files
*.mp4 filter=lfs diff=lfs merge=lfs -text
*.mov filter=lfs diff=lfs merge=lfs -text

# 3D models
*.fbx filter=lfs diff=lfs merge=lfs -text
*.obj filter=lfs diff=lfs merge=lfs -text
```

## 知识沉淀规范

### 文档结构

**必须创建的目录**：
```
docs/
├── architecture/           # 架构设计文档
│   ├── decisions/         # 架构决策记录(ADR)
│   └── analysis/          # 影响面分析报告
├── prompts/               # Prompt模板库
│   ├── basic/
│   ├── intermediate/
│   └── advanced/
├── skills/                # Skills使用手册
├── best-practices/        # 最佳实践
└── case-studies/          # 实战案例
```

### 沉淀要求

**每次开发完成后必须**：

1. **更新Skills使用手册**
   - 记录Skills使用心得
   - 补充最佳实践案例

2. **补充最佳实践文档**
   - 记录踩坑经验
   - 总结优化技巧

3. **更新Prompt模板**
   - 优化Prompt措辞
   - 补充新的场景

4. **记录架构决策**
   - 创建ADR文档
   - 说明决策背景和理由

### ADR文档格式

**架构决策记录模板**：
```markdown
# ADR-[编号]: [决策标题]

## 状态
[提议/已接受/已废弃/已替代]

## 背景
[描述导致此决策的背景和问题]

## 决策
[描述决策内容]

## 理由
[解释为什么选择此方案]

## 备选方案
[列举其他可行方案及放弃理由]

## 影响
[说明此决策的影响范围]

## 参考
[相关文档和资源]
```

## 团队协作规范

### 代码审查制度

**审查人职责**：
- 及时响应PR请求（24小时内）
- 严格按审查Checklist执行
- 提供建设性反馈
- 确认AI生成代码质量

**开发者职责**：
- 代码提交前自审
- 提供完整的PR描述
- 及时响应审查意见
- 确保测试通过

### 知识分享机制

**每周回顾会议**：
- 分享成功的Prompt设计
- 讨论遇到的问题
- 更新最佳实践
- 培训团队成员

**文档维护**：
- 定期更新文档（每月）
- 清理过时信息
- 补充新知识点
- 优化文档结构

### 新人培训

**培训内容**：
1. 理解规范体系
2. 熟悉工作流程
3. 掌握Skills使用
4. 学习最佳实践

**培训材料**：
- 快速开始指南
- Skills使用手册
- 示例案例库
- 最佳实践文档

## 相关规范

- 架构设计与影响面分析规范
- 渐进式披露开发规范
- 代码审查规范

## 更新历史

- 2026-03-16: 初始版本发布
