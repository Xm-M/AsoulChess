# 功能需求卡片: 休息房选项卡配置化（配图 + Economy 卡池）

## 基本信息
- **功能名称**: Roguelike 休息房选项卡 ScriptableObject 配置
- **所属模块**: Roguelike / UI
- **需求类型**: 功能扩展
- **优先级**: P1
- **提出日期**: 2026-05-20
- **关联**: `docs/requirements/roguelike-rest-node.md`

## 用户确认（2026-05-20）

1. **全部休息房卡牌**由策划自制（含休息、扩容），因自动生成无配图
2. **效果**：先支持现有两种（小推车+、携带格+）
3. **卡池位置**：`RoguelikeEconomyConfig.restOptionCatalog`（全局）

## 制作流程（策划）

### 1. 生成默认资产（首次）

Unity 菜单：**Roguelike → Create Default Rest Option Catalog**

生成路径：

```text
Assets/SO/Rogue/Rest/
├── RestOption_LawnMower.asset      (builtin.rest)
├── RestOption_ExpandLoadout.asset  (builtin.expand)
└── RoguelikeRestOptionCatalog.asset
```

### 2. 绑定 Economy

打开 `RoguelikeEconomyConfig` → **休息房** → **休息房选项卡池** → 拖入 `RoguelikeRestOptionCatalog`

### 3. 给每张卡配图

选中 `RestOption_*.asset`：

| 字段 | 说明 |
|------|------|
| optionId | 唯一 id，勿重复 |
| title | 卡标题 |
| description | 留空=自动生成「当前→变化」文案 |
| icon | **配图 Sprite** |
| effectKind | `LawnMowerBonus` / `LoadoutSlotBonus` |
| effectValue | ≤0 用 Economy 休息加成 |

### 4. 做 Card Prefab（外观）

1. 复制/新建 `RoguelikeRestOptionWidget` Prefab（可参考搜刮 `RoguelikeRewardEntryWidget`）
2. 绑 `iconImage`、`titleText`、`descriptionText`、`button`
3. `RoguelikeRestPanel` → **Option Entry Prefab** 拖入

未绑 Prefab 时仍会用运行时简易布局（有 icon 槽位）。

### 5. 新增事件卡

1. `Create → Roguelike → Rest Option` 新建资产
2. 设 id / 标题 / 配图 / 效果类型
3. 加入 `RoguelikeRestOptionCatalog.options` 列表（顺序=展示顺序）

## 技术结构

```text
RoguelikeEconomyConfig.restOptionCatalog
  └── RoguelikeRestOptionCatalog.options[]
        └── RoguelikeRestOptionDefinition (SO)
              → RoguelikeRestOptionFactory → RoguelikeRestOption
                    → RoguelikeRestOptionWidget.Bind
```

`IRoguelikeRestOptionProvider` 仍可追加动态选项（与 Catalog 并存）。

## 验收标准

- [ ] Economy 已绑 Catalog，休息房显示 Catalog 内全部选项
- [ ] 每张 RestOption 可设 icon，面板显示配图
- [ ] 休息 +2 / 扩容 +1 效果与改前一致
- [ ] 扩容满 15 时扩容卡 disabled
- [ ] 未绑 Catalog 时 Console 警告 + 无图 fallback

## 实现状态

- [x] `RoguelikeRestOptionDefinition` / `RoguelikeRestOptionCatalog`
- [x] `RoguelikeRestOptionFactory` / `RoguelikeRestEffectKind`
- [x] `RoguelikeEconomyConfig.restOptionCatalog`
- [x] `RoguelikeRestOptionWidget` + Panel 使用 `optionEntryPrefab`
- [x] Editor 菜单生成默认 Catalog
