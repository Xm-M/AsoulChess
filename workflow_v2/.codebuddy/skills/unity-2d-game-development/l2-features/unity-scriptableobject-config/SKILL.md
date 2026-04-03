---
name: unity-scriptableobject-config
description: 用于快速生成Unity游戏数据配置类，如角色属性、武器数据、物品信息、关卡配置等。支持字段验证和编辑器特性配置。
---

# Unity ScriptableObject配置生成器

## 目的

快速生成Unity游戏数据配置类（ScriptableObject），支持多种预设类型和自定义字段，提高数据配置效率。

## 使用场景

适用于以下情况：
- 创建游戏数据配置类（角色、武器、物品、关卡等）
- 需要数据驱动的设计
- 需要字段验证逻辑
- 需要编辑器友好的配置界面

## 工作流程

### 步骤1：确认配置类型

**操作**：询问用户配置类型：
- **character** - 角色配置（ID、名称、生命值、移动速度等）
- **weapon** - 武器配置（ID、名称、伤害、攻击范围等）
- **item** - 物品配置（ID、名称、类型、描述、堆叠数等）
- **level** - 关卡配置（ID、名称、难度、时间限制等）
- **skill** - 技能配置（ID、名称、冷却时间、魔法消耗等）
- **custom** - 自定义配置（用户指定字段）

**确认点**：等待用户选择配置类型。

### 步骤2：确认字段定义

**操作**：根据选择的类型展示预设字段，或让用户自定义字段。

**预设类型示例（weapon）**：
```
预设字段：
- weaponId: int - 武器ID
- weaponName: string - 武器名称
- attackDamage: int - 攻击伤害
- attackRange: float - 攻击范围
- attackSpeed: float - 攻击速度
- icon: Sprite - 武器图标

是否使用预设字段，或需要自定义？
```

**自定义字段格式**：
```
[
  {
    "name": "fieldName",
    "type": "int/string/float/bool/Sprite/GameObject",
    "tooltip": "字段说明",
    "range": [min, max],
    "required": true/false,
    "defaultValue": "value"
  }
]
```

**确认点**：等待用户确认字段列表。

### 步骤3：确认是否启用验证

**操作**：询问是否需要OnValidate验证逻辑：
```
是否启用字段验证？
- 验证必填字段非空
- 验证数值范围
- 验证引用完整性
```

**确认点**：等待用户选择。

### 步骤4：生成配置类

**操作**：生成完整的ScriptableObject配置类，包含：
- ✅ CreateAssetMenu特性配置
- ✅ 字段定义及特性标签
- ✅ 属性访问器（只读）
- ✅ OnValidate验证逻辑（如启用）
- ✅ 使用示例代码

## 代码规范

生成的代码遵循：
- 所有字段使用`[SerializeField] private`
- 添加`[Header]`分组
- 添加`[Tooltip]`说明
- 数值字段添加`[Range]`限制
- 属性访问器只读
- OnValidate验证必填字段和范围

## 验收标准

- [ ] 编译无错误无警告
- [ ] CreateAssetMenu菜单项正确
- [ ] Inspector显示友好
- [ ] 验证逻辑正确（如启用）
- [ ] 属性访问器正常工作
- [ ] 包含XML注释

## 使用示例

### 示例1：使用预设类型

```
用户：我需要生成武器配置类

AI：好的！请选择配置类型：
1. character - 角色配置
2. weapon - 武器配置
3. item - 物品配置
4. level - 关卡配置
5. skill - 技能配置
6. custom - 自定义配置

[用户选择：weapon]

AI：收到！武器配置类。

预设字段：
- weaponId: int
- weaponName: string
- attackDamage: int
- attackRange: float
- attackSpeed: float
- icon: Sprite

使用预设字段，还是自定义？

[用户确认：使用预设]

AI：是否启用字段验证？

[用户确认：是]

AI：正在生成WeaponData.cs...
[生成完整代码]
```

### 示例2：自定义配置

```
用户：我需要技能配置类

AI：请选择配置类型...

[用户选择：custom]

AI：请提供自定义字段列表（JSON格式）：

[用户提供字段列表]

AI：收到字段定义。是否启用验证？

[用户确认]

AI：正在生成SkillData.cs...
[生成完整代码]
```

## 注意事项

- ScriptableObject适合存储静态游戏数据
- 避免在运行时修改ScriptableObject数据（会导致修改持久化）
- 配置类生成后可在Unity中通过Create菜单创建实例
- 适合数据驱动的设计模式
