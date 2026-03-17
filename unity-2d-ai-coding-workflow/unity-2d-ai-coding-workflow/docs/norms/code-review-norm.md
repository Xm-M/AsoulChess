# 代码审查规范

## 规范目的

确保所有AI生成的代码符合质量标准，遵循Unity最佳实践，保证项目代码质量。

## 审查范围

### 强制审查

**必须审查的代码**：
- ✅ 所有AI生成的代码
- ✅ 修改核心系统的代码
- ✅ 性能关键路径代码
- ✅ 跨模块影响的代码

### 可选审查

**可灵活处理**：
- ⚠️ 简单工具函数
- ⚠️ 纯UI调整
- ⚠️ 配置脚本

## 审查维度

### 1. 功能正确性

**基础功能检查**：
- [ ] 功能实现符合需求描述
- [ ] 边界条件处理正确
- [ ] 异常情况有合理处理
- [ ] 返回值类型正确
- [ ] 无逻辑错误

**数据验证检查**：
- [ ] 输入参数验证
- [ ] 空值检查完备
- [ ] 数组边界检查
- [ ] 除零检查

### 2. Unity最佳实践

#### MonoBehaviour生命周期
- [ ] 正确使用Awake、Start、Update、FixedUpdate
- [ ] Awake用于组件引用初始化
- [ ] Start用于游戏逻辑初始化
- [ ] Update用于每帧逻辑
- [ ] FixedUpdate用于物理操作
- [ ] OnEnable/OnDisable配对使用
- [ ] OnDestroy正确清理资源

#### 性能优化
- [ ] 组件引用已缓存（无频繁GetComponent）
- [ ] 避免在Update中使用GameObject.Find
- [ ] 避免在Update中分配内存
- [ ] 字符串拼接使用StringBuilder
- [ ] 合理使用对象池
- [ ] 协程使用正确
- [ ] 减少不必要的序列化字段
- [ ] 使用CompareTag替代string比较

#### 资源管理
- [ ] 正确处理资源加载
- [ ] 及时释放资源
- [ ] 无内存泄漏风险
- [ ] 事件订阅正确注销
- [ ] 协程正确停止
- [ ] 静态事件订阅管理

### 3. 代码质量

#### 命名规范
- [ ] 类名使用PascalCase
- [ ] 方法名使用PascalCase
- [ ] 公共变量使用PascalCase
- [ ] 私有变量使用_camelCase
- [ ] 常量使用UPPER_CASE
- [ ] 参数使用camelCase
- [ ] 命名清晰表意，无缩写歧义

#### 代码结构
- [ ] 使用#region划分代码区域
- [ ] 类职责单一（SRP原则）
- [ ] 方法职责单一
- [ ] 方法长度适中（<50行）
- [ ] 类长度适中（<500行）
- [ ] 无重复代码（DRY原则）
- [ ] 缩进和格式统一

#### 注释文档
- [ ] 类有XML注释说明用途
- [ ] 公共方法有XML注释
- [ ] 参数有说明
- [ ] 返回值有说明
- [ ] 复杂逻辑有行内注释
- [ ] 注释准确有意义
- [ ] 无冗余注释

### 4. 安全性

#### 空值安全
- [ ] 组件引用有空值检查
- [ ] 数组访问前检查长度
- [ ] 字典访问前检查Key存在
- [ ] 事件触发前检查null

#### 类型安全
- [ ] 类型转换安全
- [ ] 泛型约束正确
- [ ] 避免装箱拆箱

#### 数据安全
- [ ] 敏感数据加密
- [ ] 输入数据验证
- [ ] 文件路径验证

### 5. Unity特定

#### SerializeField使用
- [ ] 私有字段使用[SerializeField]
- [ ] 添加[Tooltip]说明
- [ ] 添加[Header]分组
- [ ] 数值字段添加[Range]
- [ ] 避免public字段（除非必要）

#### Inspector友好
- [ ] 字段有默认值
- [ ] 复杂类型可编辑
- [ ] 枚举类型友好
- [ ] 引用拖拽方便

## 审查流程

### 开发者提交

**提交内容**：
1. 代码文件
2. AI生成记录（Prompt）
3. 架构分析报告（如需要）

**提交格式**：
```
提交信息：
[feat/fix/refactor] 功能描述

AI生成记录：
- Prompt: [Prompt摘要]
- 架构分析: [报告链接]（如需要）

文件列表：
- [文件1]
- [文件2]
```

### 审查者执行

**第一步：使用Skill审查**
```bash
@unity-review
代码: [粘贴代码]
类型: [monobehaviour/scriptableobject/ui/editor/utility]
```

**第二步：填写审查报告**

**审查报告格式**：
```markdown
# 代码审查报告

## 基本信息
- 文件名: [文件名]
- 审查类型: [类型]
- 审查日期: [日期]
- 审查人: [姓名]

## 审查结果

### ✅ 通过项 ([数量]/[总数])
- [列出通过的项]

### ⚠️ 警告项 ([数量])
- [列出需要改进但不影响功能的项]

### ❌ 错误项 ([数量])
- [列出必须修复的问题]

## 改进建议
1. [具体建议1]
2. [具体建议2]

## 总体评价
[优秀/良好/需改进/不合格]

## 审查人签名
[签名]
```

### 结果处理

**通过**：
- ✅ 代码可合并
- ✅ 更新文档
- ✅ 进入测试阶段

**警告**：
- ⚠️ 确认警告项可接受
- ⚠️ 记录待优化点
- ⚠️ 可合并代码

**错误**：
- ❌ 必须修改错误项
- ❌ 重新提交审查
- ❌ 不得合并代码

## 常见问题清单

### 性能问题

**高频问题**：
- ❌ 在Update中使用GameObject.Find
- ❌ 在Update中调用GetComponent
- ❌ 在Update中使用字符串拼接
- ❌ 频繁创建临时数组/List
- ❌ 未使用对象池管理频繁创建的对象

**修复建议**：
```csharp
// 错误
private void Update()
{
    GameObject player = GameObject.Find("Player");
    player.transform.position += Vector3.right * speed;
}

// 正确
private Transform _playerTransform;

private void Awake()
{
    _playerTransform = GameObject.Find("Player").transform;
}

private void Update()
{
    _playerTransform.position += Vector3.right * speed;
}
```

### 内存泄漏问题

**高频问题**：
- ❌ 事件订阅未注销
- ❌ 静态事件订阅管理不当
- ❌ 协程未正确停止
- ❌ 资源未释放

**修复建议**：
```csharp
// 错误
private void OnEnable()
{
    GameEvents.OnScoreChanged += UpdateScore;
}

// 正确
private void OnEnable()
{
    GameEvents.OnScoreChanged += UpdateScore;
}

private void OnDisable()
{
    GameEvents.OnScoreChanged -= UpdateScore;
}
```

### 逻辑错误

**高频问题**：
- ❌ 生命周期方法签名错误
- ❌ 协程返回类型错误
- ❌ 物理操作放在Update中
- ❌ 缺少空值检查

**修复建议**：
```csharp
// 错误：Update处理物理
private void Update()
{
    rigidbody.velocity = velocity;
}

// 正确：FixedUpdate处理物理
private void FixedUpdate()
{
    rigidbody.velocity = velocity;
}
```

## 审查工具

### 使用 @unity-review Skill

**触发方式**：
```
@unity-review
代码: [代码内容]
类型: monobehaviour
```

**输出内容**：
- 自动检查所有维度
- 生成详细审查报告
- 标记问题等级
- 提供修复建议

## 相关规范

- 架构设计与影响面分析规范
- 渐进式披露开发规范
- 项目目录结构规范

## 更新历史

- 2026-03-16: 初始版本发布
