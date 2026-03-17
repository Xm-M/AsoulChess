# Prefab System 基础概念

## 什么是预制体（Prefab）？

预制体是Unity中可重复使用的游戏对象模板，存储了GameObject及其所有组件、子对象的配置信息。预制体允许在场景中创建多个实例，且修改预制体会自动同步到所有实例。

---

## 核心概念

### 1. 预制体实例（Prefab Instance）

从预制体创建的场景对象称为实例。实例保持与源预制体的链接关系。

**特点**：
- 修改预制体 → 所有实例自动更新
- 实例可以有自己的覆盖（Override）属性
- 断开链接后变为普通GameObject

### 2. 预制体覆盖（Prefab Override）

实例相对于预制体的修改称为覆盖，包括：
- 属性覆盖：修改组件属性值
- 添加组件：实例添加额外组件
- 删除组件：实例移除组件
- 子对象变更：添加或删除子对象

### 3. 嵌套预制体（Nested Prefab）

预制体可以包含其他预制体作为子对象，形成层级结构。

**优点**：
- 模块化设计
- 复杂对象的分层管理
- 独立修改各层预制体

### 4. 预制体变体（Prefab Variant）

基于现有预制体创建的变体，继承原预制体的所有属性，可以添加额外修改。

**应用场景**：
- 基础敌人 → 不同类型敌人变体
- 基础武器 → 不同等级武器变体
- 基础道具 → 不同属性道具变体

---

## 预制体工作流

### 创建预制体

```
1. 在场景中配置GameObject
2. 将GameObject从Hierarchy拖到Project窗口
3. 自动创建.prefab文件
4. 场景中的对象变为实例
```

### 编辑预制体

```
方式1：双击预制体进入Prefab Mode
方式2：修改实例后应用更改到预制体
方式3：在Project窗口Inspector中直接编辑
```

### 实例化预制体

```csharp
// 运行时实例化
GameObject instance = Instantiate(prefab, position, rotation);

// 编辑器实例化
GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
```

---

## 预制体 vs 普通GameObject

| 特性 | 预制体实例 | 普通GameObject |
|------|-----------|---------------|
| 批量修改 | ✅ 支持 | ❌ 不支持 |
| 变体继承 | ✅ 支持 | ❌ 不支持 |
| 覆盖管理 | ✅ 可视化 | ❌ 无 |
| 运行时实例化 | ✅ Instantiate | ❌ 需手动创建 |
| 编辑器预览 | ✅ Prefab Mode | ❌ 仅场景 |

---

## 常见使用场景

### 1. 游戏对象模板

```
- 角色、敌人、NPC
- 子弹、道具、特效
- UI面板、按钮
```

### 2. 场景模块化

```
- 房屋、建筑物
- 地形区块
- 环境装饰
```

### 3. 配置数据容器

```
- ScriptableObject数据对象
- 配置文件模板
- 资源引用容器
```

---

## 预制体文件结构

### .prefab文件格式

Unity 2018.3+使用新预制体系统，文件格式包含：

```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!1 &1234567890
GameObject:
  m_Name: Player
  m_Component:
    - component: {fileID: 1234567891}
    - component: {fileID: 1234567892}
--- !u!4 &1234567891
Transform:
  m_LocalPosition: {x: 0, y: 0, z: 0}
```

### 文件位置建议

```
Assets/
├── Prefabs/
│   ├── Characters/
│   │   ├── Player.prefab
│   │   └── Enemies/
│   │       ├── BasicEnemy.prefab
│   │       └── BossEnemy.prefab
│   ├── Props/
│   │   ├── Weapons/
│   │   └── Items/
│   └── UI/
│       ├── Panels/
│       └── Buttons/
```

---

## 性能基础

### 实例化性能

```
Instantiate()操作：
- 分配内存
- 复制组件数据
- 初始化脚本
- 触发Awake()

优化方案：
- 对象池预加载
- 异步实例化
- 批量实例化
```

### 内存占用

```
预制体本身：仅存储模板数据
实例对象：复制数据 + 覆盖数据

内存公式：
总内存 = 预制体数据 + (实例数 × 实例数据)
```

---

## 最佳实践基础

### 命名规范

```
✅ 推荐：Player, Enemy_Basic, UI_Panel_MainMenu
❌ 避免：New Prefab, Copy of Player, prefab1
```

### 目录组织

```
按功能分类，使用变体而非复制：
Assets/Prefabs/Enemies/BasicEnemy.prefab
Assets/Prefabs/Enemies/FastEnemy.prefab (变体)
Assets/Prefabs/Enemies/TankEnemy.prefab (变体)
```

### 变体使用

```
基础预制体 → 定义通用属性
变体预制体 → 定义特定差异

示例：
  Weapon_Base.prefab (攻击力、射速)
    ├── Weapon_Pistol.prefab (低攻击、快射速)
    ├── Weapon_Rifle.prefab (中攻击、中射速)
    └── Weapon_Shotgun.prefab (高攻击、慢射速)
```

---

## 常见问题基础

### Q: 修改预制体后实例没有更新？

```
检查：
1. 实例是否断开连接（Prefab > Disconnect）
2. 实例是否有覆盖属性
3. 是否正确保存预制体
```

### Q: 如何批量修改多个预制体？

```
方法1：使用变体系统
方法2：编写编辑器脚本
方法3：使用PrefabUtility API
```

### Q: 预制体丢失引用怎么办？

```
原因：
- 预制体文件被删除
- 文件移动后meta文件丢失
- GUID变更

解决：
- 使用Project窗口查找Missing引用
- 重新指定预制体引用
```

---

## 扩展学习

### 官方文档
- [Unity Prefabs Manual](https://docs.unity3d.com/Manual/Prefabs.html)
- [Nested Prefabs](https://docs.unity3d.com/Manual/NestedPrefabs.html)
- [Prefab Variants](https://docs.unity3d.com/Manual/PrefabVariants.html)

### 进阶主题
- 预制体覆盖系统详解
- PrefabUtility编辑器扩展
- 预制体性能优化
- 运行时动态加载预制体
