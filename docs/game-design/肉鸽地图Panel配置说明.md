# RoguelikeMapPanel 配置说明（手把手）

## 先弄清：哪些东西「挂在场景里」，哪些是「拖引用」

| 东西 | 放哪 | 说明 |
|------|------|------|
| **RoguelikeMapPanel** | `Resources/UIPrefab/RoguelikeMapPanel.prefab` | 游戏启动时由 UIRoot 自动加载，**不用**拖进场景 |
| **nodePrefab**（节点） | Project 里任意文件夹的 `.prefab` | 拖到面板的 **Node Prefab** 槽，**不要**放进 Hierarchy |
| **linePrefab**（连线） | Project 里任意文件夹的 `.prefab` | 拖到面板的 **Line Prefab** 槽，**不要**放进 Hierarchy |
| **visualSettingsAsset** | Project 里的 `.asset` | 拖到面板的 **Visual Settings Asset** 槽（可选） |
| **RunMapConfig / ActMapConfig** | Project 里的 `.asset` | 在 StartUI 或 debug 槽配置，与地图外观无关 |

**RoguelikeMapVisualSettings 不是 MonoBehaviour，不能挂到 GameObject 上。**

---

## 为什么 Inspector 只有「布局」和「可选」？

说明 Unity 还在用**旧脚本**或预制体未刷新。请：

1. 等 Unity 编译完成（Console 无报错）。
2. 选中 `Resources/UIPrefab/RoguelikeMapPanel`。
3. 看 **Roguelike Map Panel** 组件是否出现 **1～7** 共七组 Header。
4. 若仍只有两项：组件右上角 **⋮ → Reset**，或移除组件再重新 Add Component。

---

## 推荐配置顺序（第一次）

### 步骤 A：改 RoguelikeMapPanel 预制体层级

打开 `Assets/Resources/UIPrefab/RoguelikeMapPanel.prefab`：

```text
RoguelikeMapPanel          ← 加 Image 当全屏背景，拖到「Panel Background」
├── Header                 ← TMP，拖到 Header Text
├── Scroll View            ← ScrollRect，拖到 Map Scroll Rect
│   ├── Viewport           ← 带 Mask，可见区域
│   │   └── Content        ← 拖到 Map Content（地图实际宽度由代码设置）
│   │       ├── MapBackground
│   │       └── Nodes        ← Nodes Root
│   │           ├── Layer_0   ← 最左（起点）
│   │           │   ├── Line…（运行时，从房间中心连出）
│   │           │   └── 房间节点
│   │           └── Layer_N   ← 最右（Boss 层只有节点，无出线）
│   ├── Scrollbar Horizontal （可选，建议开启）
│   └── Scrollbar Vertical（建议关掉 ScrollRect 的 Vertical）
└── AbandonButton
```

**滚动方向（横向）：** 起点在 **Content 最左**（`Layer_0`），Boss 在 **最右**；滚轮 / 拖拽向右 = 往 Boss 方向看。`Refresh` 后会自动滚到当前节点（可关 `Scroll To Focus On Refresh`）。

**布局字段（兼容旧 prefab 命名）：**

| Inspector 字段 | 横向语义 |
|----------------|----------|
| `layerRowWidth` | **列高**（slot 纵向可用高度，默认 1250） |
| `layerRowHeight` | **列宽**（层间距，默认 200） |
| `mapPaddingBottom` | **左侧**（起点）留白 |
| `mapPaddingTop` | **右侧**（Boss 端）留白 |

把 **Auto Generate Missing Ui** 取消勾选（搭好后）。若已有 **Scroll View**，把 **Content** 拖到 **Map Content**，**Scroll View** 拖到 **Map Scroll Rect**；**Nodes** 放在 Content 下即可。

**ScrollRect 建议：** 只勾 Horizontal；Content 锚点 **左中** `(0, 0.5)`，宽由 `(层数+1)×列宽+左右 padding` 决定；不要用 Content Size Fitter 控宽度。

**每层一列：** 运行时自动建 `Layer_0`…`Layer_N`（列宽×列高），房间 100×100 在列内按 slot **纵向**分布，格内随机偏移（`randomizeNodePlacement`，同种子位置不变）。Inspector：**Layer Row Width/Height**（语义见上表）、**Node Size**。

### 步骤 B：做一个「节点」预制体（只要 1 个）

1. 在 Project 里 `Create → UI → Image`，命名 `RoguelikeMapNode`。
2. 结构示例：

```text
RoguelikeMapNode（Image + Button + RoguelikeMapNodeWidget）
├── Icon（Image，可选）
└── Label（TextMeshPro）
```

3. 在根物体挂 **RoguelikeMapNodeWidget**，把 Background / Icon / Button / Label 拖进脚本槽。
4. 把这个 prefab 拖到 **RoguelikeMapPanel → Node Prefab**。

### 步骤 C：每种房间类型换图（不用多个 nodePrefab）

在 **Node Type Styles** 列表里会有 7 项（Start / Normal / Elite / Boss / Rest / Shop / Event），每项配：

- **Background Sprite** 或 **Icon Sprite**（图标）
- **Base Color**（着色）
- **Label Override**（可选文字）

运行时 `Refresh()` 会 **Instantiate 同一个 nodePrefab**，再按房间类型调用 `Bind()` 换图换色。

| 房间 | 列表里 Room Type |
|------|------------------|
| 普通战 | Normal |
| 精英 | Elite |
| 问号事件 | Event |
| 休息 | Rest |
| 商店 | Shop |
| Boss | Boss |
| 起点 | Start |

### 步骤 D：连线

**方式 1（简单）**：不建 linePrefab，只改 **Line Style → Width / Color / Sprite**。

**方式 2（美术条）**：

1. 建细长 Image 预制体 `RoguelikeMapLine`，挂 **RoguelikeMapLineWidget**（或脚本自动用 Image）。
2. 预制体保存在 `Assets/Prefab/UI/` 等任意处。
3. 把该 prefab 拖到 **RoguelikeMapPanel → Line Prefab**（Project 拖入，不是 Hierarchy）。

### 步骤 E：Visual Settings Asset（可选）

若想多套主题复用：

1. `Create → Roguelike → Map Visual Settings` → 得到 `.asset`。
2. 在 asset 里配 **Node Type Styles**、**Line Style**（与面板上类似）。
3. 拖到 **RoguelikeMapPanel → Visual Settings Asset**。

**指定 asset 后，节点/连线的颜色与图以 asset 为准；布局、nodePrefab、linePrefab 仍在面板上配。**

不配 asset 也完全 OK，直接在面板的 **Node Type Styles / Line Style** 里改。

---

## 和关卡的关系（别混淆）

| 配置 | 管什么 |
|------|--------|
| **ActMapConfig / RunMapConfig** | 地图有几层、几个精英、从哪个 **LevelData 池** 抽战斗关 |
| **RoguelikeMapPanel** | 地图上 **长什么样**（图、线、间距） |
| **LevelData** | 点进格子后 **怎么打**（波次、场景） |

商店/休息/事件还没有专用 LevelData 时：仍会进 **normalLevelPool** 里随机的普通关。

---

## 快速检查清单

- [ ] RoguelikeMapPanel 上能看到 1～7 组 Header
- [ ] Node Prefab 已拖（1 个就够）
- [ ] Node Type Styles 里 Elite/Boss 等已换 Sprite
- [ ] Line Style 或 Line Prefab 已设
- [ ] StartUI 上挂了 RunMapConfig，按钮调 `StartRoguelikeRun()`
- [ ] 战斗关 LevelData 的 outcome 为 `LevelOutCome_Roguelike`
