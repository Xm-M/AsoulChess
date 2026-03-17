# 示例案例1：背包系统开发

## 项目背景

**游戏类型**：2D像素风格RPG游戏
**开发团队**：3人独立团队（1程序+1美术+1策划）
**开发周期**：3天

## 需求描述

开发一个网格背包系统，支持：
- 8x5网格布局
- 物品拖拽移动
- 物品堆叠合并
- 物品详情提示
- 存档功能

## 完整开发流程

### 第一步：填写需求卡片

**创建文件**：`docs/requirements/inventory-system.md`

```markdown
## 功能需求卡片

### 基本信息
- 功能名称: 背包系统
- 所属模块: 物品系统
- 任务等级: B级
- 预估耗时: 3天

### 功能描述
网格背包系统，支持物品存储、拖拽移动、堆叠合并、详情提示、存档功能

### 接口定义
**公共方法**:
- `bool AddItem(int itemId, int count)`: 添加物品
- `bool RemoveItem(int itemId, int count)`: 移除物品
- `int GetItemCount(int itemId)`: 获取物品数量

**可配置参数**:
- gridWidth: int - 8（网格宽度）
- gridHeight: int - 5（网格高度）
- maxStack: int - 99（最大堆叠数）

### 验收标准
- [ ] 网格布局正确显示
- [ ] 拖拽功能流畅
- [ ] 堆叠逻辑正确
- [ ] 详情提示清晰
- [ ] 存档功能可用
```

---

### 第二步：架构设计与影响分析

**使用Skill**：
```
@architecture-design-analyzer
需求: 网格背包系统，支持拖拽、堆叠、详情、存档
模块: 物品系统
```

**AI分析过程**：

#### 1. 系统定位分析

```
AI: 开始架构分析...

**系统定位**：
- 所属模块: 物品系统
- 上游依赖: 数据管理、UI系统
- 下游影响: 存档系统、战斗系统
- 横向关联: 任务系统、商店系统

模块归属是否正确？
```

**用户确认**：正确

#### 2. 设计决策建议

```
AI: 基于需求分析，建议设计方案：

**推荐模式**: MVC + 观察者模式
**理由**:
- 数据与展示分离，便于维护
- 观察者模式实现物品变化通知
- 符合项目现有架构风格

**备选方案**:
1. **MVVM模式**:
   - 优点: 数据绑定自动化
   - 缺点: Unity支持不完善
   - 放弃: 技术复杂度高

请选择设计方案：
```

**用户选择**：采用推荐方案

#### 3. 影响面评估

```
AI: 影响面分析结果：

**需要修改的文件**:
- ItemData.cs: 新增物品数据类（新建）
- InventoryManager.cs: 背包数据管理（新建）
- InventoryPanel.cs: UI面板脚本（新建）
- UIManager.cs: 添加背包面板管理（修改）
- SaveManager.cs: 集成背包存档（修改）

**依赖的系统**:
- ScriptableObject数据系统
- UI面板管理系统
- JSON序列化工具

**会影响到的系统**:
- 战斗系统: 物品使用接口
- 任务系统: 物品收集判定

**性能影响**:
- 内存: 每个物品约100字节，100个物品≈10KB
- CPU: 拖拽计算，每帧约0.1ms
- 风险: 低

是否继续？
```

**用户确认**：继续

#### 4. 风险识别

```
AI: 风险识别：

| 风险点 | 概率 | 影响 | 应对措施 |
|-------|------|------|---------|
| 拖拽性能 | 中 | 中 | 使用对象池优化 |
| 存档数据丢失 | 低 | 高 | 自动备份机制 |
| UI层级冲突 | 低 | 低 | 统一UI管理器 |

**技术债务预警**:
- ☑️ 需要更新架构文档

风险是否可接受？
```

**用户确认**：可接受

#### 5. 生成分析报告

```
AI: 生成架构设计与影响面分析报告...

✓ 报告已生成
✓ 已存档至: docs/architecture/analysis/inventory-system-2026-03-16.md
```

**报告内容包括**：
- 系统定位
- 设计决策
- 影响面分析
- 风险识别
- 实施建议
- 审批签名区

**审批流程**：
- 提交给架构师审核 → 通过
- 提交给技术负责人审核 → 通过
- 开始开发

---

### 第三步：渐进式披露开发

#### 3.1 生成物品数据配置

**使用Skill**：
```
@unity-scriptableobject-config
```

**AI渐进式披露**：

```
AI: 好的！我来帮你创建配置类。

请选择配置类型：
1. 角色（character）
2. 武器（weapon）
3. 物品（item）
4. 关卡（level）
```

**用户选择**：物品

```
AI: 明白！物品配置。

请确认字段：

**基础字段**（预设）：
- itemId: int - 物品ID
- itemName: string - 物品名称
- itemType: enum - 物品类型
- description: string - 物品描述
- icon: Sprite - 物品图标
- maxStack: int - 最大堆叠数

是否需要自定义字段？
```

**用户选择**：使用预设字段

```
AI: 完美！最后确认：

- 配置名称: ItemData
- 字段验证: 是

生成ItemData配置类？
```

**用户确认**：是

**AI生成代码**：
```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    [Tooltip("物品唯一ID")]
    [SerializeField] private int itemId;
    
    [Tooltip("物品名称")]
    [SerializeField] private string itemName;
    
    [Header("Item Properties")]
    [Tooltip("物品类型")]
    [SerializeField] private ItemType itemType;
    
    [Tooltip("物品描述")]
    [TextArea(3, 5)]
    [SerializeField] private string description;
    
    [Tooltip("物品图标")]
    [SerializeField] private Sprite icon;
    
    [Tooltip("最大堆叠数量")]
    [Range(1, 99)]
    [SerializeField] private int maxStack = 99;
    
    // 属性访问器
    public int ItemId => itemId;
    public string ItemName => itemName;
    public ItemType ItemType => itemType;
    public string Description => description;
    public Sprite Icon => icon;
    public int MaxStack => maxStack;
    
    private void OnValidate()
    {
        if (itemId <= 0)
        {
            Debug.LogWarning($"ItemData: itemId must be greater than 0. Current: {itemId}");
            itemId = 1;
        }
    }
}

public enum ItemType
{
    Consumable,
    Equipment,
    Material,
    Quest
}
```

#### 3.2 生成背包UI面板

**使用Skill**：
```
@unity-ui-panel
```

**AI渐进式披露**：

```
AI: 好的！创建UI面板。

请选择面板类型：
1. 弹出窗口（popup）
2. 常驻面板（persistent）
3. 全屏界面（fullscreen）
```

**用户选择**：全屏界面

```
AI: 明白！全屏界面。

请确认UI元素：
- closeButton: Button（必须）
- 其他元素？

请描述需要的UI元素，或告诉我具体需求。
```

**用户输入**：
- 物品网格容器
- 物品详情面板

```
AI: 收到！

UI元素确认：
- closeButton: Button - 关闭按钮
- itemGrid: GameObject - 物品网格容器
- detailPanel: GameObject - 物品详情面板

动画方案：
1. DOTween（推荐）
2. Animation
3. 无动画
```

**用户选择**：DOTween

```
AI: 完美！生成背包面板：

✓ 类型：全屏界面
✓ 动画：DOTween
✓ 元素：关闭按钮、物品网格、详情面板

开始生成代码...
```

**AI生成代码**：
```csharp
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class InventoryPanel : BasePanel
{
    [Header("UI References")]
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject itemGrid;
    [SerializeField] private GameObject detailPanel;
    
    protected override void Awake()
    {
        base.Awake();
        
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Close);
        }
    }
    
    public override void Open()
    {
        base.Open();
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        
        RefreshInventory();
    }
    
    public override void Close()
    {
        transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack)
            .OnComplete(() => base.Close());
    }
    
    private void RefreshInventory()
    {
        // 刷新背包显示
    }
    
    protected override void OnDestroy()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Close);
        }
        base.OnDestroy();
    }
}
```

#### 3.3 生成背包管理器

**使用Skill**：
```
@unity-singleton-manager
```

**AI渐进式披露**：

```
AI: 创建管理器。

请选择管理器类型：
1. 游戏管理（game）
2. 关卡管理（level）
3. 音频管理（audio）
4. 存档管理（save）
5. 自定义（custom）
```

**用户选择**：自定义

```
AI: 明白！自定义管理器。

管理器名称？
```

**用户输入**：InventoryManager

```
AI: 收到！InventoryManager

需要哪些功能？
- 物品添加/移除
- 物品查询
- 存档功能
- 事件通知

请确认功能列表。
```

**用户确认**：全部需要

```
AI: 完美！生成InventoryManager：

✓ 单例模式
✓ 物品管理
✓ 存档功能
✓ 事件系统

开始生成代码...
```

**AI生成代码**：
```csharp
using UnityEngine;
using System.Collections.Generic;
using System;

public class InventoryManager : Singleton<InventoryManager>
{
    private Dictionary<int, int> items = new Dictionary<int, int>();
    private const int MAX_SLOTS = 40; // 8x5
    
    public event Action<int, int> OnItemChanged;
    
    public bool AddItem(int itemId, int count)
    {
        if (!CanAddItem(itemId, count))
            return false;
        
        if (items.ContainsKey(itemId))
        {
            items[itemId] += count;
        }
        else
        {
            items[itemId] = count;
        }
        
        OnItemChanged?.Invoke(itemId, items[itemId]);
        return true;
    }
    
    public bool RemoveItem(int itemId, int count)
    {
        if (!HasItem(itemId, count))
            return false;
        
        items[itemId] -= count;
        
        if (items[itemId] <= 0)
        {
            items.Remove(itemId);
        }
        
        OnItemChanged?.Invoke(itemId, items.ContainsKey(itemId) ? items[itemId] : 0);
        return true;
    }
    
    public int GetItemCount(int itemId)
    {
        return items.ContainsKey(itemId) ? items[itemId] : 0;
    }
    
    public bool HasItem(int itemId, int count = 1)
    {
        return GetItemCount(itemId) >= count;
    }
    
    private bool CanAddItem(int itemId, int count)
    {
        return true; // 简化逻辑
    }
    
    public string SaveToJson()
    {
        return JsonUtility.ToJson(new InventoryData(items));
    }
    
    public void LoadFromJson(string json)
    {
        var data = JsonUtility.FromJson<InventoryData>(json);
        items = data.ToDictionary();
    }
    
    public void ClearInventory()
    {
        items.Clear();
        OnItemChanged?.Invoke(-1, 0);
    }
}

[Serializable]
public class InventoryData
{
    public List<int> itemIds;
    public List<int> itemCounts;
    
    public InventoryData(Dictionary<int, int> items)
    {
        itemIds = new List<int>(items.Keys);
        itemCounts = new List<int>(items.Values);
    }
    
    public Dictionary<int, int> ToDictionary()
    {
        var dict = new Dictionary<int, int>();
        for (int i = 0; i < itemIds.Count; i++)
        {
            dict[itemIds[i]] = itemCounts[i];
        }
        return dict;
    }
}
```

---

### 第四步：代码审查

**使用Skill**：
```
@unity-code-review
代码: [粘贴所有生成的代码]
类型: ui
```

**AI审查报告**：

```markdown
# 代码审查报告

## 基本信息
- 文件: ItemData.cs, InventoryPanel.cs, InventoryManager.cs
- 审查类型: ui
- 审查日期: 2026-03-16

## 审查结果

### ✅ 通过项 (45/50)
- [列出所有通过的项]

### ⚠️ 警告项 (3)
- ⚠️ InventoryPanel中itemGrid应添加空值检查
- ⚠️ InventoryManager的SaveToJson应添加异常处理
- ⚠️ 物品数量变化时应触发UI刷新

### ❌ 错误项 (2)
- ❌ InventoryPanel缺少detailPanel的初始化逻辑
- ❌ InventoryManager未限制物品最大数量

## 改进建议
1. 为InventoryPanel添加完整的UI元素初始化
2. 为InventoryManager添加物品数量上限验证
3. 添加异常处理机制

## 总体评价
良好

需修改错误项后可合并。
```

**根据审查意见修改代码**

---

### 第五步：测试验证

#### 5.1 编译测试
- [x] 无编译错误
- [x] 无编译警告

#### 5.2 功能测试
- [x] 创建ItemData资产成功
- [x] UI面板打开/关闭正常
- [x] 物品添加功能正常
- [x] 物品移除功能正常
- [x] 存档/读档功能正常

#### 5.3 性能测试
- [x] 100个物品测试：内存增长< 1MB
- [x] 拖拽流畅度：60 FPS
- [x] 存档速度：< 50ms

---

### 第六步：文档沉淀

#### 6.1 更新Skills手册
记录：
- @unity-config 使用心得
- @unity-ui-panel 最佳实践
- @unity-manager 单例模式应用

#### 6.2 补充最佳实践
- 背包系统开发经验
- UI面板动画优化技巧
- 数据序列化最佳实践

#### 6.3 更新Prompt模板
优化：
- 物品配置Prompt
- UI面板Prompt
- 管理器Prompt

#### 6.4 记录架构决策
创建ADR：
- 为什么选择MVC模式
- 为什么使用观察者模式
- 存档方案选择理由

---

## 效率对比

| 任务 | 传统方式 | AI辅助 | 提升 |
|------|---------|--------|------|
| 架构分析 | 60分钟 | 15分钟 | 75%↑ |
| 数据配置 | 20分钟 | 3分钟 | 85%↑ |
| UI面板 | 60分钟 | 15分钟 | 75%↑ |
| 管理器 | 45分钟 | 10分钟 | 78%↑ |
| 代码审查 | 30分钟 | 10分钟 | 67%↑ |
| **总计** | **3小时** | **45分钟** | **75%↑** |

---

## 经验总结

### 成功要点
1. ✅ 架构分析前置，避免返工
2. ✅ 渐进式披露，需求精准
3. ✅ Skills组合使用，效率最大化
4. ✅ 严格审查，质量保障

### 踩坑经验
1. ⚠️ UI元素命名要一致
2. ⚠️ 事件订阅要记得注销
3. ⚠️ 数据序列化要考虑版本兼容

### 改进方向
1. 添加拖拽预览功能
2. 优化大量物品时的性能
3. 增加物品分类筛选
