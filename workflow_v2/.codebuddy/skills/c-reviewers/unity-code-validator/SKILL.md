---
name: unity-code-validator
description: Unity AI编码编译验证器，提供预防+检测+自动修复的完整解决方案，在代码进入Unity前发现并修复编译错误。
---

# Unity代码编译验证器

## 目的

解决Unity AI编码中的编译错误延迟发现问题，提供**预防+检测+自动修复**的三层防护体系，让AI生成的代码在Unity外部就能验证通过。

## 核心问题

AI编码完成后，经常有编译问题需要等到进入Unity才能发现，影响开发效率。

## 解决方案架构

```
┌─────────────────────────────────────────────────────────────────┐
│                    Unity AI编码验证体系                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐      │
│  │   预防层     │───▶│   检测层     │───▶│   修复层     │      │
│  │  (AI生成前)  │    │ (代码生成后) │    │ (发现问题后) │      │
│  └──────────────┘    └──────────────┘    └──────────────┘      │
│         │                   │                   │              │
│         ▼                   ▼                   ▼              │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐      │
│  │ Prompt约束   │    │ Roslyn分析器 │    │ AI自动修复   │      │
│  │ 规范检查清单 │    │ dotnet build │    │ 编译错误修复 │      │
│  │ API白名单    │    │ SARIF输出    │    │ 迭代验证     │      │
│  └──────────────┘    └──────────────┘    └──────────────┘      │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## 第一层：预防机制（AI生成前）

### 1.1 Unity C#代码规范检查清单

在AI生成代码前，将以下约束添加到Prompt中：

#### 基础结构检查
- [ ] 类名与文件名完全一致（区分大小写）
- [ ] 文件扩展名为.cs
- [ ] 类声明使用public访问修饰符
- [ ] 组件脚本继承自MonoBehaviour（`: MonoBehaviour`）

#### 命名空间检查
- [ ] 包含 `using UnityEngine;`
- [ ] UI相关代码包含 `using UnityEngine.UI;` 或 `using UnityEngine.UIElements;`
- [ ] 物理相关代码包含 `using UnityEngine.Physics;`
- [ ] 场景管理包含 `using UnityEngine.SceneManagement;`
- [ ] Editor代码放在Editor文件夹并包含 `using UnityEditor;`

#### 生命周期方法检查
- [ ] Awake() - 方法签名：`private void Awake()`
- [ ] Start() - 方法签名：`private void Start()`
- [ ] Update() - 方法签名：`private void Update()`
- [ ] FixedUpdate() - 方法签名：`private void FixedUpdate()`

#### Unity特定约束
- [ ] 所有组件脚本必须显式继承MonoBehaviour
- [ ] 禁止使用new创建MonoBehaviour（使用AddComponent）
- [ ] 避免在构造函数中使用Unity API
- [ ] 类名必须与文件名一致

### 1.2 AI Prompt约束模板

```markdown
你是一位专业的Unity C#开发专家。请根据以下要求生成代码：

## 项目上下文
- Unity版本：2022.3 LTS
- 目标平台：PC/Mac/移动端
- 使用URP渲染管线

## 代码要求
1. **必须遵循Unity C#代码规范**
2. **所有组件脚本必须继承MonoBehaviour**
3. **类名必须与功能描述一致**
4. **包含必要的using指令**

## 约束检查清单
生成代码前请自检：
- [ ] 命名空间声明完整
- [ ] MonoBehaviour继承正确
- [ ] 生命周期方法签名正确
- [ ] 访问修饰符使用正确
- [ ] 命名规范符合标准
- [ ] 类型安全无隐患

## 功能需求
[在此处描述具体功能需求]

## 输出格式
1. 完整的C#代码
2. 关键代码段的注释说明
3. 使用示例
```

---

## 第二层：检测机制（代码生成后）

### 2.1 推荐工具组合

| 工具 | 功能 | 使用场景 |
|------|------|----------|
| **Roslynator CLI** | 代码分析 | 快速检查代码规范 |
| **dotnet build** | 完整编译 | 验证代码可编译性 |
| **SARIF输出** | 标准化报告 | 机器可读错误格式 |

### 2.2 快速配置步骤

#### 步骤1：安装工具
```bash
# 安装.NET SDK 8.0或9.0
# 下载地址: https://dotnet.microsoft.com/download

# 安装Roslynator CLI
dotnet tool install -g roslynator.dotnet.cli

# 验证安装
dotnet --version
roslynator --version
```

#### 步骤2：在Unity项目中添加分析器
```bash
# 添加Microsoft.Unity.Analyzers包
dotnet add package Microsoft.Unity.Analyzers --version 1.26.0
```

#### 步骤3：修改.csproj文件
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
    <!-- 启用SARIF输出 -->
    <ErrorLog>analysis.sarif,version=2.1</ErrorLog>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="Microsoft.Unity.Analyzers" Version="1.26.0">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
    </PackageReference>
  </ItemGroup>
</Project>
```

#### 步骤4：执行编译检查
```bash
# 方案A: 使用Roslynator进行代码分析
roslynator analyze MyProject.csproj \
  --severity warning \
  --output analysis.xml

# 方案B: 使用dotnet build生成SARIF报告
dotnet build -p:ErrorLog=build-errors.sarif,version=2.1

# 方案C: 组合使用（推荐）
dotnet build && roslynator analyze
```

### 2.3 错误输出解析

#### 标准错误格式
```
文件路径(行号,列号): 错误代码: 错误消息
```

#### 示例
```
Assets/Scripts/Player.cs(15,23): error CS0103: 名称"speed"在当前上下文中不存在
```

#### SARIF格式（机器可读）
```json
{
  "runs": [{
    "results": [{
      "ruleId": "CS0103",
      "message": { "text": "名称不存在" },
      "locations": [{
        "physicalLocation": {
          "artifactLocation": { "uri": "Player.cs" },
          "region": { "startLine": 15, "startColumn": 23 }
        }
      }]
    }]
  }]
}
```

---

## 第三层：自动修复机制（发现问题后）

### 3.1 可自动修复的错误类型

| 错误类型 | 错误代码 | 自动修复可行性 | 修复策略 |
|---------|---------|---------------|---------|
| **语法错误** | CS1001, CS1002 | ⭐⭐⭐⭐⭐ | 自动补全缺失符号 |
| **缺少分号** | CS1002 | 极高 | 在语句末尾添加`;` |
| **括号不匹配** | CS1026, CS1513 | 极高 | 自动补全`{}`或`()` |
| **类型转换错误** | CS0029, CS0266 | ⭐⭐⭐⭐ | 添加显式转换或修改类型 |
| **命名空间错误** | CS0103, CS0246 | ⭐⭐⭐⭐ | 自动添加using语句 |
| **访问修饰符错误** | CS0122, CS0501 | ⭐⭐⭐⭐ | 调整访问级别 |

### 3.2 AI修复Prompt模板

```markdown
# Unity C# 编译错误修复任务

## 错误信息
- 错误代码: {ERROR_CODE}
- 错误消息: {ERROR_MESSAGE}
- 文件路径: {FILE_PATH}
- 行号: {LINE_NUMBER}
- 列号: {COLUMN_NUMBER}

## 错误代码片段
```csharp
{ERROR_CODE_SNIPPET}
```

## 项目上下文
- Unity版本: {UNITY_VERSION}
- .NET版本: {DOTNET_VERSION}
- 目标平台: {TARGET_PLATFORM}

## 修复要求
1. 仅修复导致编译错误的代码，不要改变其他逻辑
2. 保持原有代码风格和命名规范
3. 优先使用Unity推荐的API和模式
4. 修复后代码必须能通过编译

## 输出格式
请按以下格式返回修复结果：

### 错误原因分析
[简要分析错误原因]

### 修复方案
[描述具体的修复方法]

### 修复后代码
```csharp
[完整的修复后代码]
```

### 变更说明
- 变更1: [具体变更内容]
- 变更2: [具体变更内容]
```

### 3.3 修复工作流

```
┌─────────────────────────────────────────────────────────────────┐
│                    Unity AI自动修复工作流                         │
└─────────────────────────────────────────────────────────────────┘

┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│   步骤1      │────▶│   步骤2      │────▶│   步骤3      │
│  错误检测    │     │  错误分析    │     │  AI修复生成  │
└──────────────┘     └──────────────┘     └──────────────┘
      │                                         │
      │    ┌──────────────┐     ┌──────────────┐
      └───▶│   步骤6      │◀────│   步骤4      │
           │  迭代判断     │     │  修复应用    │
           │ (循环控制)    │     └──────────────┘
           └──────┬───────┘            │
                  │              ┌──────────────┐
                  └─────────────▶│   步骤5      │
                                 │  重新验证    │
                                 └──────────────┘
```

### 3.4 安全机制

```csharp
// 安全检查清单
public class FixSafetyChecker
{
    // 1. 代码范围限制
    public bool IsWithinAllowedScope(string filePath)
    {
        var forbiddenPaths = new[] {
            "Assets/Plugins/",
            "Assets/ThirdParty/",
            "Packages/"
        };
        return !forbiddenPaths.Any(p => filePath.Contains(p));
    }
    
    // 2. 变更大小限制
    public bool IsChangeSizeAcceptable(string diff)
    {
        var linesChanged = diff.Split('\n').Length;
        return linesChanged <= 50; // 单次最多50行变更
    }
    
    // 3. 关键代码检测
    public bool ContainsCriticalCode(string code)
    {
        var criticalPatterns = new[] {
            "Destroy(", "Application.Quit",
            "PlayerPrefs.Delete", "File.Delete"
        };
        return criticalPatterns.Any(p => code.Contains(p));
    }
}
```

---

## 使用方式

### 方式1：完整验证流程

```
用户：@unity-code-validator
代码：
[粘贴AI生成的代码]

AI：开始编译验证...

【预防层检查】
✅ 命名空间检查通过
✅ MonoBehaviour继承正确
⚠️ 缺少[SerializeField]特性

【检测层检查】
执行dotnet build...
❌ 发现1个编译错误：
   - CS0103: 名称"speed"在当前上下文中不存在 (Player.cs:15)

【修复层处理】
分析错误原因...
生成修复方案...

修复后代码：
```csharp
[修复后的完整代码]
```

验证结果：✅ 编译通过
```

### 方式2：快速检查

```
用户：@unity-code-validator check
代码：
[粘贴代码]

AI：快速编译检查...

编译状态：❌ 失败
错误数量：1

错误详情：
1. CS0103: Player.cs(15,23) - 名称"speed"不存在

修复建议：
- 添加字段声明: [SerializeField] private float speed = 5f;
```

### 方式3：自动修复

```
用户：@unity-code-validator fix
代码：
[粘贴代码]
错误：
[粘贴编译错误]

AI：分析错误并生成修复...

修复方案：
[详细说明]

修复后代码：
```csharp
[完整修复代码]
```
```

---

## Unity特定错误检查清单

### MonoBehaviour相关
- ❌ 使用new创建MonoBehaviour
- ❌ 构造函数中使用Unity API
- ❌ 类名与文件名不匹配
- ❌ 缺少MonoBehaviour继承

### 命名空间相关
- ❌ UnityEditor命名空间在运行时脚本中
- ❌ 缺少UnityEngine引用
- ❌ 命名空间冲突

### 生命周期方法
- ❌ Awake/Start/Update等方法签名错误
- ❌ 方法名大小写错误（如update而非Update）
- ❌ 参数签名不匹配

### API兼容性
- ❌ 使用已废弃的API
- ❌ API参数变更未更新
- ❌ 版本不兼容的API调用

### ScriptableObject
- ❌ 文件名与类名不匹配
- ❌ 未添加CreateAssetMenu
- ❌ 运行时修改期望持久化

---

## 集成到AI工作流

### 在Development Workflow中使用

```
@development-workflow
  ↓
生成代码
  ↓
@unity-code-validator (编译验证)
  ↓
编译通过? 
  ├── 是 → 继续下一步
  └── 否 → AI自动修复 → 重新验证
  ↓
@unity-code-review (代码审查)
  ↓
完成
```

### CI/CD集成示例

```yaml
# GitHub Actions示例
- name: Unity Code Validation
  run: |
    # 编译检查
    dotnet build -p:ErrorLog=errors.sarif,version=2.1
    
    # 分析检查
    roslynator analyze --severity warning
    
    # 上传SARIF报告
    - name: Upload SARIF
      uses: github/codeql-action/upload-sarif@v2
      with:
        sarif_file: errors.sarif
```

---

## 配置示例

### .editorconfig配置

```ini
# Unity C#代码风格配置
[*.cs]
# 缩进
indent_style = space
indent_size = 4

# 命名规范
dotnet_naming_rule.unity_fields.symbols = unity_fields
dotnet_naming_rule.unity_fields.style = _camelCase
dotnet_naming_symbols.unity_fields.applicable_kinds = field
```

### 规则集配置 (Default.ruleset)

```xml
<?xml version="1.0" encoding="utf-8"?>
<RuleSet Name="Unity Rules" ToolsVersion="10.0">
  <Rules AnalyzerId="Microsoft.Unity.Analyzers">
    <Rule Id="UNT0001" Action="Error" />  <!-- 空Unity消息方法 -->
    <Rule Id="UNT0002" Action="Warning" /> <!-- 使用CompareTag -->
    <Rule Id="UNT0004" Action="Error" />   <!-- Update中使用GetComponent -->
    <Rule Id="UNT0006" Action="Error" />   <!-- 不正确的消息签名 -->
  </Rules>
</RuleSet>
```

---

## 常见问题

### Q1: 为什么AI生成的代码会有编译错误？
**A**: AI可能不了解Unity特定约束（如MonoBehaviour必须用AddComponent创建），或缺少必要的using指令。

### Q2: Roslyn分析和Unity编译有什么区别？
**A**: Roslyn分析检查代码规范和语法，dotnet build验证完整编译。Unity编译会额外检查Unity特定API和资源引用。

### Q3: 自动修复安全吗？
**A**: 建议开启安全模式，对关键代码（如Destroy、资源删除）强制人工确认，其他简单错误可自动修复。

### Q4: 如何处理Unity版本差异？
**A**: 使用Unity Versioner工具检查API兼容性范围，或在Prompt中指定目标Unity版本。

---

## 推荐工具链

| 工具 | 用途 | 链接 |
|------|------|------|
| Microsoft.Unity.Analyzers | Unity特定代码分析 | NuGet |
| Roslynator | 代码分析和修复 | dotnet tool |
| Unity Versioner | API版本兼容性检查 | ngtools.tech |
| StyleCop.Analyzers | 代码风格检查 | NuGet |

---

## 总结

通过三层防护体系，可以显著减少Unity AI编码中的编译错误：

1. **预防层**：通过Prompt约束和规范检查清单，在生成前预防错误
2. **检测层**：通过Roslyn分析和dotnet build，在Unity外部快速检测错误
3. **修复层**：通过AI自动修复和迭代验证，快速解决发现的错误

建议将此验证流程集成到AI编码工作流中，确保每次代码生成后自动执行验证，提升开发效率。
