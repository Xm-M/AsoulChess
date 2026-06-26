# 功能需求卡片: 肉鸽地图首次卷动 Intro（Boss 端 → 起点）

## 基本信息
- **功能名称**: RoguelikeMapPanel 地图 Intro 滚动
- **所属模块**: Roguelike / UI
- **需求类型**: 功能优化（体验 / Polish）
- **优先级**: P1（RG-P02）
- **预估复杂度**: L1
- **提出日期**: 2026-06-11

## 功能描述

首次进入本 Act 的 `RoguelikeMapPanel` 时，**不要瞬间定位**到起点，而是：

1. 卷轴先停在 **Boss 端（最右侧）**
2. 在约 **1.5～2.5s** 内平滑滚到 **起点（layer 0）** 附近
3. 滚动期间 **禁止拖拽 / 滚轮**，节点 **不可点击**
4. 结束后与现有 `ScrollToFocus` 落点一致，再开放交互

与乐队选择 `leave` 动画衔接：leave 结束 → `OpenRun` → 地图面板 Show → 播放本 Intro。

### 用户故事

作为玩家，我希望新开 Run 进地图时镜头从远处 Boss 端滑到起点，像 STS 一样建立「一路打到右边」的空间感。

## 现有业务上下文

| 功能 | 关系 |
|------|------|
| `RoguelikeMapPanel.ScrollToFocusNextFrame` | 当前 Refresh 后 **一帧内直接设** `horizontalNormalizedPosition` |
| 横向布局 | layer 0 = 左（起点），`maxLayer` = 右（Boss） |
| `RoguelikeBandSelectPanel` leave | 上游：进地图前的离场动画 |
| `OpenContinuedRun` / 战斗返回 | **不应**播 Intro，只瞬时滚到当前节点 |

### 坐标语义（Unity `ScrollRect`）

- `horizontalNormalizedPosition = 0` → 视口看 Content **左侧**
- `horizontalNormalizedPosition = 1` → 视口看 Content **右侧（Boss 端）**
- Intro 方向：**1 → 目标 normalized（layer 0 的焦点）**

焦点 X 计算（已有逻辑，应抽成共用方法）：

```text
focusXFromLeft = mapPaddingBottom + focusLayer * LayerColumnStride
offsetFromLeft = clamp(focusXFromLeft - viewportW * scrollFocusViewportFraction, 0, scrollRange)
normalized = offsetFromLeft / scrollRange
```

## 推荐实现方案（代码 Tween，不用 Animator）

**原因**：Content 宽度随层数变化，Animator 绑 `anchoredPosition` 难维护；项目无 DOTween，用 **协程 + AnimationCurve** 即可。

### 1. 抽取滚动位置

在 `RoguelikeMapPanel` 增加：

```csharp
bool TryGetHorizontalNormalizedForLayer(int layer, float contentWidth, out float normalized);
```

把 `ScrollToFocusNextFrame` 内 569–581 行逻辑迁入此处。

### 2. Intro 协程

```csharp
IEnumerator PlayMapIntroScrollCoroutine(float contentWidth, int maxLayer, float targetLayer0Norm)
{
    var scroll = mapScrollRect;
    scroll.enabled = false;           // 禁止拖拽
    _mapInputLocked = true;           // 禁止节点 OnNodeClicked

    float from = TryGetHorizontalNormalizedForLayer(maxLayer, contentWidth, out var bossNorm)
        ? bossNorm : 1f;
    scroll.horizontalNormalizedPosition = from;

    float t = 0f;
    while (t < mapIntroDuration)
    {
        t += Time.unscaledDeltaTime;
        float u = mapIntroCurve.Evaluate(t / mapIntroDuration);
        scroll.horizontalNormalizedPosition = Mathf.Lerp(from, targetLayer0Norm, u);
        yield return null;
    }
    scroll.horizontalNormalizedPosition = targetLayer0Norm;
    scroll.enabled = true;
    _mapInputLocked = false;
}
```

Inspector 建议字段：

| 字段 | 默认 | 说明 |
|------|------|------|
| `mapIntroEnabled` | true | 总开关 |
| `mapIntroDuration` | 2f | 秒，`unscaledDeltaTime` 以免暂停影响 |
| `mapIntroCurve` | EaseInOut | `AnimationCurve` |

### 3. 何时播放（触发条件）

**推荐（无需改存档）**：

| 场景 | Intro |
|------|-------|
| `OpenRun`（新开局，乐队 leave 之后） | ✅ 播 |
| `OpenContinuedRun` | ❌ |
| 战斗 / 商店 / 休息返回 `ShowAndRefresh` | ❌，沿用现有 `ScrollToFocus` |
| 通关换 Act、`OnActMapGenerated` 后首次进图 | ✅ 可选 P1：每 Act 播一次 |

实现：在 `RoguelikeMapPanel` 设静态/实例标志：

```csharp
// OpenRun 末尾：RequestMapIntroOnNextRefresh();
// Refresh 内：若 Requested && scrollToFocusOnRefresh → Intro 协程，否则原 ScrollToFocus
```

或 **Act 级存档字段**（若希望读档后也不播）：

```csharp
// RoguelikeRunState.introPlayedActIndices : List<int>
```

### 4. 与 `ScrollToFocus` 的关系

```
Refresh()
  ├─ Build nodes / lines
  └─ if (ShouldPlayMapIntro())
        PlayMapIntroScrollCoroutine → layer 0
     else if (scrollToFocusOnRefresh)
        ScrollToFocusNextFrame → currentNode layer
```

`ShouldPlayMapIntro()` 为 false 时，行为与 **现在完全一致**。

### 5. 不建议的方案

| 方案 | 原因 |
|------|------|
| Animator 直接 K 帧 Content | 宽度动态，每层 Act 不同 |
| 移动 `mapBackgroundImage` 而不动 ScrollRect | 节点与背景不同步 |
| Timeline | 过重，且无复用收益 |

## 技术要求

- **依赖**: 现有 `ScrollRect`、`RoguelikeMapPanel.Refresh`
- **Skills**: `@unity-ui-system`（ScrollRect）、`@unity-coroutine-system`
- **性能**: 单协程每帧改一个 float，可忽略

## 验收标准

- [ ] 新开局：leave 后进地图，镜头从 Boss 端滚到起点，约 2s，过程不能拖地图、不能点节点
- [ ] 继续冒险：无 Intro，直接定位当前节点
- [ ] 战斗胜利回地图：无 Intro，滚到当前节点
- [ ] 换 Act（若启用）：新地图播一次 Intro
- [ ] Content 很窄（≤ viewport）时不播或 instant 0

## 测试建议

1. 新游戏 → 选乐队 → leave → 观察 Intro
2. 中途退出主菜单 → 继续冒险 → 无 Intro
3. 赢一场回地图 → 无 Intro，焦点在当前格
4. Test 模式开地图 → 按配置决定是否 Intro

## 关联文档

- [roguelike-map-horizontal-layout.md](./roguelike-map-horizontal-layout.md)
- [roguelike-band-select.md](./roguelike-band-select.md)（leave 衔接）
- [roguelike-known-issues.md](../roguelike-known-issues.md) RG-P02
