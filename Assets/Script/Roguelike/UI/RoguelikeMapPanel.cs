using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 肉鸽选路地图 UI。预制体：<c>Resources/UIPrefab/RoguelikeMapPanel</c>。
/// 横向布局：起点在左（layer 0）、Boss 在右；每层一列，slot 在列内纵向分布。
/// 配置说明见 <c>docs/game-design/肉鸽地图Panel配置说明.md</c>。
/// </summary>
public class RoguelikeMapPanel : View
{
    [Header("1. 层级引用（在预制体里搭好 UI 再拖进来）")]
    [Tooltip("全屏背景；拖 RoguelikeMapPanel 根物体上的 Image，或单独子物体")]
    [SerializeField] Image panelBackground;

    [Tooltip("地图区域内的装饰底图（卷轴/山等），拖 MapViewport 下的子 Image")]
    [SerializeField] Image mapBackgroundImage;

    [Tooltip("地图 ScrollRect（预制体 Scroll View）。为空则自动查找子物体")]
    [SerializeField] ScrollRect mapScrollRect;

    [Tooltip("ScrollRect 的 Content，节点/连线画在这里。为空则用 ScrollRect.content")]
    [SerializeField] RectTransform mapContent;

    [Tooltip("兼容旧名：与 Map Content 相同，拖 Content 即可")]
    [SerializeField] RectTransform mapViewport;

    [Tooltip("连线的父节点（Content 下 Lines）")]
    [SerializeField] RectTransform linesRoot;

    [Tooltip("节点的父节点（Content 下 Nodes）")]
    [SerializeField] RectTransform nodesRoot;

    [Header("2. 布局（每层一列 + 格内随机）")]
    [Tooltip("列高（slot 纵向可用高度）；Inspector 字段名保留 layerRowWidth 以兼容旧 prefab")]
    [SerializeField] float layerRowWidth = 1250f;
    [Tooltip("列宽（层与层之间的横向间距）；Inspector 字段名保留 layerRowHeight 以兼容旧 prefab")]
    [SerializeField] float layerRowHeight = 200f;
    [SerializeField] Vector2 nodeSize = new Vector2(100f, 100f);
    [Tooltip("在各自格子 (列高/行数 × 列宽) 内随机偏移，避免地图过于整齐；同一 Run 种子下位置稳定")]
    [SerializeField] bool randomizeNodePlacement = true;
    [SerializeField] Vector2 mapOffset;
    [SerializeField] float mapScale = 1f;

    [Header("2b. 横向滚动（起点在左、Boss 在右，滚轮/拖拽向右查看）")]
    [Tooltip("Content 右侧（Boss 端）留白；字段名保留 mapPaddingTop")]
    [SerializeField] float mapPaddingTop = 48f;
    [Tooltip("Content 左侧（起点）留白；字段名保留 mapPaddingBottom")]
    [SerializeField] float mapPaddingBottom = 48f;
    [Tooltip("Refresh 后滚到当前节点；无当前则滚到最左（起点）")]
    [SerializeField] bool scrollToFocusOnRefresh = true;
    [SerializeField] float scrollFocusViewportFraction = 0.35f;

    [Header("2c. 地图 Intro 卷动（Boss 端 → 起点）")]
    [SerializeField] bool mapIntroEnabled = true;
    [Tooltip("通关 Boss 换 Act 后，下次进图是否再播 Intro")]
    [SerializeField] bool mapIntroOnActAdvance = true;
    [SerializeField] float mapIntroDuration = 2f;
    [SerializeField] AnimationCurve mapIntroCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("3. 节点（只需 1 个 nodePrefab，不同类型用下面列表换图）")]
    [Tooltip("节点 UI 预制体（挂 RoguelikeMapNodeWidget）。普通/精英/Boss 等共用这一个，不要每种做一个 prefab")]
    [SerializeField] RoguelikeMapNodeWidget nodePrefab;

    [Tooltip("有图标时是否仍显示文字")]
    [SerializeField] bool showLabelWhenIconPresent;

    [Tooltip("未配置 nodePrefab 时，代码生成节点的字号")]
    [SerializeField] float runtimeNodeLabelFontSize = 16f;

    [Tooltip("锁定/可选/当前/已通关 等状态的颜色修饰")]
    [SerializeField] RoguelikeMapNodeStateStyle nodeStateStyle = new RoguelikeMapNodeStateStyle();

    [Tooltip("每种房间类型一张什么图：Normal/Elite/Boss/Rest/Shop/Event… 展开列表逐项配")]
    [SerializeField] List<RoguelikeMapNodeTypeStyle> nodeTypeStyles = new List<RoguelikeMapNodeTypeStyle>();

    [Header("4. 连线（样式 + 可选预制体）")]
    [SerializeField] RoguelikeMapLineStyle lineStyle = new RoguelikeMapLineStyle();

    [Tooltip("连线预制体（Project 里的 .prefab 拖到这里，不要放进场景）。为空则用 lineStyle 代码画线")]
    [SerializeField] RoguelikeMapLineWidget linePrefab;

    [Header("5. 视觉资产（可选，不配就用上面 3、4 的字段）")]
    [Tooltip("Create → Roguelike → Map Visual Settings 做的 .asset，拖到这里。不是挂在场景物体上")]
    [SerializeField] RoguelikeMapVisualSettingsAsset visualSettingsAsset;

    [Header("6. 其它 UI")]
    [SerializeField] TMP_Text headerText;
    [SerializeField] Button abandonButton;
    [SerializeField] RunMapConfig debugRunConfig;

    [Header("7. 自动生成（预制体搭好后建议关掉）")]
    [SerializeField] bool autoGenerateMissingUi = true;
    [SerializeField] Color fallbackPanelBackgroundColor = new Color(0.06f, 0.08f, 0.12f, 0.92f);

    readonly List<RoguelikeMapNodeWidget> _nodeWidgets = new List<RoguelikeMapNodeWidget>();
    readonly List<GameObject> _lineObjects = new List<GameObject>();
    readonly List<GameObject> _layerRowObjects = new List<GameObject>();
    readonly Dictionary<int, RectTransform> _layerRowsByLayer = new Dictionary<int, RectTransform>();
    readonly Dictionary<int, RoguelikeMapNodeWidget> _widgetByNodeId = new Dictionary<int, RoguelikeMapNodeWidget>();
    readonly Dictionary<int, Vector2> _nodeCenterInDrawRoot = new Dictionary<int, Vector2>();
    float _lastContentWidth;
    float _lastContentHeight;
    Coroutine _scrollFocusCoroutine;
    Coroutine _mapIntroCoroutine;
    bool _mapInputLocked;

    /// <summary>下次 <see cref="Refresh"/> 且 Act 匹配时播地图 Intro；-1 表示不播。</summary>
    static int _pendingMapIntroActIndex = -1;

    public override void Init()
    {
        EnsureLayout();
        if (abandonButton != null)
            abandonButton.onClick.AddListener(OnAbandonClicked);
    }

    void OnEnable()
    {
        RoguelikeRunService.OnActMapGenerated += OnMapChanged;
        RoguelikeRunService.OnNodeResolved += OnNodeResolved;
        RoguelikeRunService.OnRunCompleted += OnRunEnded;
        RoguelikeRunService.OnActCompleted += OnActCompletedHandler;
    }

    void OnDisable()
    {
        RoguelikeRunService.OnActMapGenerated -= OnMapChanged;
        RoguelikeRunService.OnNodeResolved -= OnNodeResolved;
        RoguelikeRunService.OnRunCompleted -= OnRunEnded;
        RoguelikeRunService.OnActCompleted -= OnActCompletedHandler;
        CancelMapIntroScroll(unlockInput: true);
    }

    void OnValidate()
    {
        EnsureDefaultNodeTypeStyles();
    }

    void EnsureDefaultNodeTypeStyles()
    {
        nodeTypeStyles ??= new List<RoguelikeMapNodeTypeStyle>();
        foreach (MapRoomType t in System.Enum.GetValues(typeof(MapRoomType)))
        {
            bool found = false;
            for (int i = 0; i < nodeTypeStyles.Count; i++)
            {
                if (nodeTypeStyles[i].roomType == t)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
                nodeTypeStyles.Add(RoguelikeMapVisualSettingsAsset.CreateDefaultTypeStyle(t));
        }
    }

    void OnMapChanged(RoguelikeRunState _) => Refresh();

    void OnNodeResolved(RoguelikeMapNode node, bool win)
    {
        if (!win)
        {
            RefreshHeader("本局失败");
            return;
        }
        Refresh();
    }

    void OnRunEnded()
    {
        RefreshHeader("通关！");
        RoguelikeRunInfoPanel.HideForRunEnded();
    }

    void OnActCompletedHandler()
    {
        if (!mapIntroOnActAdvance)
            return;
        var state = RoguelikeRunService.State;
        if (state != null)
            RequestMapIntroOnNextRefresh(state.currentActIndex);
    }

    /// <summary>请求在下次 <see cref="Refresh"/> 末尾播放地图 Intro（与当前 Act 索引匹配时消费）。</summary>
    public static void RequestMapIntroOnNextRefresh(int actIndex)
    {
        _pendingMapIntroActIndex = actIndex;
    }

    public void ShowAndRefresh()
    {
        Show();
        RoguelikeRunInfoPanel.TryShowAndRefresh();
    }

    public override void Show()
    {
        EnsureLayout();
        if (!RoguelikeRunService.HasActiveRun && debugRunConfig != null)
            RoguelikeRunService.StartNewRun(debugRunConfig);
        base.Show();
        Refresh();
    }

    public override void Hide()
    {
        CancelMapIntroScroll(unlockInput: true);
        ClearVisuals();
        base.Hide();
    }

    public static void OpenRun(RunMapConfig config, int? seed = null)
    {
        OpenRun(config, null, seed);
    }

    public static void OpenRun(RunMapConfig config, BandMes band, int? seed = null)
    {
        if (config == null)
        {
            Debug.LogError("[RoguelikeMapPanel] RunMapConfig 为空");
            return;
        }
        RoguelikeRunService.StartNewRun(config, band, seed);
        RequestMapIntroOnNextRefresh(0);
        UIManage.GetView<RoguelikeMapPanel>()?.ShowAndRefresh();
    }

    /// <summary>继续未完成的 Run（磁盘 active 存档或内存中 HasActiveRun）。</summary>
    public static bool OpenContinuedRun(RunMapConfig config)
    {
        if (config == null)
        {
            Debug.LogError("[RoguelikeMapPanel] RunMapConfig 为空");
            return false;
        }

        if (RoguelikeRunService.HasContinuableRunSave && !RoguelikeRunService.TryContinueRun(config))
            return false;

        if (!RoguelikeRunService.HasActiveRun)
            return false;

        return ResumeActiveRunUi();
    }

    static bool ResumeActiveRunUi()
    {
        if (RoguelikeRunService.TryResumePendingNonCombatNode())
            return true;
        if (RoguelikeRunService.TryResumePendingCombatNode())
            return true;
        UIManage.GetView<RoguelikeMapPanel>()?.ShowAndRefresh();
        return true;
    }

    RoguelikeMapLineStyle EffectiveLineStyle =>
        visualSettingsAsset != null ? visualSettingsAsset.lineStyle : lineStyle;

    RoguelikeMapNodeStateStyle EffectiveNodeStateStyle =>
        visualSettingsAsset != null ? visualSettingsAsset.nodeStateStyle : nodeStateStyle;

    RoguelikeMapNodeTypeStyle GetTypeStyle(MapRoomType type)
    {
        if (visualSettingsAsset != null)
            return visualSettingsAsset.GetTypeStyle(type);

        if (nodeTypeStyles != null)
        {
            for (int i = 0; i < nodeTypeStyles.Count; i++)
            {
                if (nodeTypeStyles[i].roomType == type)
                    return nodeTypeStyles[i];
            }
        }
        return RoguelikeMapVisualSettingsAsset.CreateDefaultTypeStyle(type);
    }

    void Refresh()
    {
        EnsureLayout();
        ClearVisuals();

        if (!RoguelikeRunService.HasActiveRun)
        {
            RefreshHeader("无进行中的 Run");
            return;
        }

        var state = RoguelikeRunService.State;
        var map = state.currentMap;
        if (map == null || map.nodes == null || map.nodes.Count == 0)
        {
            RefreshHeader("地图数据为空");
            return;
        }

        map.RebuildIndex();
        RefreshHeader(BuildHeaderText(state));

        int maxLayer = GetMaxLayer(map);
        float contentWidth = ComputeContentWidth(maxLayer);
        float contentHeight = ComputeContentHeight();
        ApplyMapContentSize(contentWidth, contentHeight);
        SyncLinesRootWithNodesRoot();

        BuildLayerColumnsAndPlaceNodes(state, map, maxLayer);
        Canvas.ForceUpdateCanvases();
        DrawLinesUnderLayers(map, maxLayer);
        SyncLinesRootWithNodesRoot();

        if (scrollToFocusOnRefresh)
        {
            if (TryConsumeMapIntroRequest(state))
                ScheduleMapIntroScroll(maxLayer, contentWidth);
            else
                ScheduleScrollToFocus(map, state, maxLayer, contentWidth);
        }
    }

    static int GetMaxLayer(GeneratedRoguelikeMap map)
    {
        int maxLayer = 0;
        for (int i = 0; i < map.nodes.Count; i++)
        {
            if (map.nodes[i].layer > maxLayer)
                maxLayer = map.nodes[i].layer;
        }
        return maxLayer;
    }

    /// <summary>列宽（层与层横向间距）。</summary>
    float LayerColumnStride => layerRowHeight * mapScale;

    float ComputeContentWidth(int maxLayer)
    {
        return mapPaddingBottom + mapPaddingTop + (maxLayer + 1) * LayerColumnStride;
    }

    float ComputeContentHeight()
    {
        return layerRowWidth * mapScale;
    }

    void ApplyMapContentSize(float width, float height)
    {
        _lastContentWidth = width;
        _lastContentHeight = height;
        var content = ResolveMapContent();
        if (content == null)
            return;

        content.sizeDelta = new Vector2(width, height);
        ApplyDrawRootSize(nodesRoot, width, height);
        if (mapBackgroundImage != null)
            ApplyDrawRootSize(mapBackgroundImage.rectTransform, width, height);
        SyncLinesRootWithNodesRoot();
    }

    /// <summary>Lines 与 Nodes 同布局；Lines 的 sibling index 必须小于 Nodes，连线才会画在节点下层。</summary>
    void SyncLinesRootWithNodesRoot()
    {
        if (nodesRoot == null)
            return;

        if (linesRoot == null || linesRoot == nodesRoot)
        {
            linesRoot = nodesRoot;
            return;
        }

        if (linesRoot.parent != nodesRoot.parent)
            linesRoot.SetParent(nodesRoot.parent, false);

        CopyRectTransformLayout(linesRoot, nodesRoot);

        int lineIdx = linesRoot.GetSiblingIndex();
        int nodeIdx = nodesRoot.GetSiblingIndex();
        if (lineIdx > nodeIdx)
            linesRoot.SetSiblingIndex(nodeIdx);

        if (nodesRoot.GetSiblingIndex() <= linesRoot.GetSiblingIndex())
            nodesRoot.SetSiblingIndex(linesRoot.GetSiblingIndex() + 1);
    }

    static void CopyRectTransformLayout(RectTransform dst, RectTransform src)
    {
        dst.anchorMin = src.anchorMin;
        dst.anchorMax = src.anchorMax;
        dst.pivot = src.pivot;
        dst.anchoredPosition = src.anchoredPosition;
        dst.sizeDelta = src.sizeDelta;
        dst.localScale = src.localScale;
        dst.localRotation = src.localRotation;
    }

    static void ApplyDrawRootSize(RectTransform rt, float width, float height)
    {
        if (rt == null)
            return;
        rt.anchorMin = new Vector2(0f, 0.5f);
        rt.anchorMax = new Vector2(0f, 0.5f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(width, height);
    }

    static int ResolveMapGridWidth(GeneratedRoguelikeMap map)
    {
        var runCfg = RoguelikeRunService.ActiveRunConfig;
        var state = RoguelikeRunService.State;
        if (runCfg != null && state != null)
        {
            var act = runCfg.GetAct(state.currentActIndex);
            if (act != null && act.mapWidth > 0)
                return act.mapWidth;
        }

        int maxSlot = 0;
        for (int i = 0; i < map.nodes.Count; i++)
        {
            if (map.nodes[i].slot > maxSlot)
                maxSlot = map.nodes[i].slot;
        }
        return Math.Max(maxSlot + 1, 1);
    }

    /// <summary>每层一列（列宽 layerRowHeight × 列高 layerRowWidth）；房间在列内 slot 纵向分布。</summary>
    void BuildLayerColumnsAndPlaceNodes(RoguelikeRunState state, GeneratedRoguelikeMap map, int maxLayer)
    {
        if (nodesRoot == null)
            return;

        _layerRowsByLayer.Clear();
        _widgetByNodeId.Clear();

        int gridWidth = ResolveMapGridWidth(map);
        int layoutSeed = state.runSeed ^ (map.seed * 486187739);
        float colW = layerRowHeight * mapScale;
        float colH = layerRowWidth * mapScale;
        Vector2 scaledNodeSize = nodeSize * mapScale;

        var selectable = new HashSet<int>();
        foreach (var n in state.GetSelectableNextNodes())
            selectable.Add(n.id);
        var stateStyle = EffectiveNodeStateStyle ?? new RoguelikeMapNodeStateStyle();

        for (int layer = 0; layer <= maxLayer; layer++)
        {
            float colLeftX = mapPaddingBottom + layer * LayerColumnStride;
            var layerColumn = CreateLayerColumn(layer, colW, colH, colLeftX);

            var columnNodes = map.GetNodesOnLayer(layer);
            for (int i = 0; i < columnNodes.Count; i++)
            {
                var node = columnNodes[i];
                Vector2 localInColumn = ComputeNodeLocalInColumn(node, gridWidth, colH, scaledNodeSize, layoutSeed);

                var visual = ResolveVisualState(state, node.id, selectable);
                var widget = SpawnNodeWidget(layerColumn);
                var rt = widget.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(0f, localInColumn.y);
                widget.SetSize(scaledNodeSize);
                widget.Bind(node.id, node.roomType, visual, OnNodeClicked, GetTypeStyle(node.roomType), stateStyle,
                    showLabelWhenIconPresent);
                _nodeWidgets.Add(widget);
                _widgetByNodeId[node.id] = widget;
                _nodeCenterInDrawRoot[node.id] = ComputeNodeCenterInDrawRoot(layerColumn, localInColumn.y, colW);
            }
        }
    }

    /// <summary>节点中心在 nodesRoot / linesRoot 本地坐标（与列布局一致，避免 Canvas 世界坐标换算误差）。</summary>
    Vector2 ComputeNodeCenterInDrawRoot(RectTransform layerColumn, float localY, float colW)
    {
        if (layerColumn == null)
            return Vector2.zero;
        float centerX = layerColumn.anchoredPosition.x + colW * 0.5f;
        float centerY = layerColumn.anchoredPosition.y + localY;
        return new Vector2(centerX, centerY);
    }

    RectTransform CreateLayerColumn(int layer, float colW, float colH, float colLeftX)
    {
        var go = new GameObject($"Layer_{layer}", typeof(RectTransform));
        go.transform.SetParent(nodesRoot, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0.5f);
        rt.anchorMax = new Vector2(0f, 0.5f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.sizeDelta = new Vector2(colW, colH);
        rt.anchoredPosition = new Vector2(colLeftX + mapOffset.x, mapOffset.y);
        _layerRowObjects.Add(go);
        _layerRowsByLayer[layer] = rt;
        return rt;
    }

    /// <summary>连线画在 linesRoot（与 nodesRoot 同布局）；端点为各节点 rect 中心的世界坐标转本地。</summary>
    void DrawLinesUnderLayers(GeneratedRoguelikeMap map, int bossLayer)
    {
        var lineRoot = ResolveLineDrawRoot();
        if (lineRoot == null)
            return;

        var style = EffectiveLineStyle ?? new RoguelikeMapLineStyle();
        bool prefabOnly = style.usePrefabAppearanceOnly && linePrefab != null;

        for (int i = 0; i < map.nodes.Count; i++)
        {
            var from = map.nodes[i];
            if (from.layer >= bossLayer)
                continue;
            if (!_widgetByNodeId.TryGetValue(from.id, out var fromWidget))
                continue;

            Vector2 startLocal = _nodeCenterInDrawRoot.TryGetValue(from.id, out var fromCenter)
                ? fromCenter
                : WorldCenterToLocal(fromWidget.transform as RectTransform, lineRoot);

            for (int j = 0; j < from.nextNodeIds.Count; j++)
            {
                int toId = from.nextNodeIds[j];
                if (!_widgetByNodeId.TryGetValue(toId, out var toWidget))
                    continue;

                Vector2 endLocal = _nodeCenterInDrawRoot.TryGetValue(toId, out var toCenter)
                    ? toCenter
                    : WorldCenterToLocal(toWidget.transform as RectTransform, lineRoot);
                CreateLine(lineRoot, startLocal, endLocal, style, prefabOnly);
            }
        }
    }

    RectTransform ResolveLineDrawRoot()
    {
        if (linesRoot != null && linesRoot != nodesRoot)
            return linesRoot;
        return nodesRoot;
    }

    static Vector2 WorldCenterToLocal(RectTransform nodeRt, RectTransform root)
    {
        Vector3 world = nodeRt.TransformPoint(nodeRt.rect.center);
        return root.InverseTransformPoint(world);
    }

    Vector2 ComputeNodeLocalInColumn(
        RoguelikeMapNode node,
        int gridWidth,
        float colH,
        Vector2 scaledNodeSize,
        int layoutSeed)
    {
        float cellH = colH / gridWidth;
        float centerY = (node.slot + 0.5f) * cellH - colH * 0.5f;
        float maxDy = Mathf.Max(0f, (cellH - scaledNodeSize.y) * 0.5f);

        float ry = 0f;
        if (randomizeNodePlacement && maxDy > 0f)
        {
            var rng = new System.Random(layoutSeed + node.id * 73856093 + node.layer * 19349663 + node.slot * 83492791);
            ry = (float)(rng.NextDouble() * 2.0 - 1.0) * maxDy;
        }

        return new Vector2(0f, centerY + ry);
    }

    bool TryConsumeMapIntroRequest(RoguelikeRunState state)
    {
        if (!mapIntroEnabled || state == null || _pendingMapIntroActIndex < 0)
            return false;
        if (_pendingMapIntroActIndex != state.currentActIndex)
            return false;

        _pendingMapIntroActIndex = -1;
        return true;
    }

    void ScheduleScrollToFocus(GeneratedRoguelikeMap map, RoguelikeRunState state, int maxLayer, float contentWidth)
    {
        if (_mapIntroCoroutine != null)
            return;

        CancelMapIntroScroll(unlockInput: true);
        if (_scrollFocusCoroutine != null)
            StopCoroutine(_scrollFocusCoroutine);
        _scrollFocusCoroutine = StartCoroutine(ScrollToFocusNextFrame(map, state, maxLayer, contentWidth));
    }

    void ScheduleMapIntroScroll(int maxLayer, float contentWidth)
    {
        if (_scrollFocusCoroutine != null)
        {
            StopCoroutine(_scrollFocusCoroutine);
            _scrollFocusCoroutine = null;
        }
        CancelMapIntroScroll(unlockInput: true);
        _mapIntroCoroutine = StartCoroutine(PlayMapIntroScrollCoroutine(maxLayer, contentWidth));
    }

    void CancelMapIntroScroll(bool unlockInput)
    {
        if (_mapIntroCoroutine != null)
        {
            StopCoroutine(_mapIntroCoroutine);
            _mapIntroCoroutine = null;
        }

        if (!unlockInput)
            return;

        _mapInputLocked = false;
        var scroll = mapScrollRect != null ? mapScrollRect : GetComponentInChildren<ScrollRect>(true);
        if (scroll != null)
            scroll.enabled = true;
    }

    bool TryGetHorizontalNormalizedForLayer(int layer, float contentWidth, out float normalized)
    {
        normalized = 0f;
        var scroll = mapScrollRect != null ? mapScrollRect : GetComponentInChildren<ScrollRect>(true);
        if (scroll == null)
            return false;

        var viewport = scroll.viewport != null ? scroll.viewport : scroll.GetComponent<RectTransform>();
        if (viewport == null)
            return false;

        float viewportW = viewport.rect.width;
        if (viewportW <= 0f)
            return false;

        if (contentWidth <= viewportW)
        {
            normalized = 0f;
            return true;
        }

        float focusXFromLeft = mapPaddingBottom + layer * LayerColumnStride;
        float scrollRange = contentWidth - viewportW;
        float offsetFromLeft = focusXFromLeft - viewportW * scrollFocusViewportFraction;
        offsetFromLeft = Mathf.Clamp(offsetFromLeft, 0f, scrollRange);
        normalized = offsetFromLeft / scrollRange;
        return true;
    }

    IEnumerator ScrollToFocusNextFrame(
        GeneratedRoguelikeMap map,
        RoguelikeRunState state,
        int maxLayer,
        float contentWidth)
    {
        yield return null;
        Canvas.ForceUpdateCanvases();

        var scroll = mapScrollRect != null ? mapScrollRect : GetComponentInChildren<ScrollRect>(true);
        if (scroll == null)
            yield break;

        int focusLayer = 0;
        if (state.currentNodeId >= 0)
        {
            var cur = map.GetNode(state.currentNodeId);
            if (cur != null)
                focusLayer = cur.layer;
        }

        if (TryGetHorizontalNormalizedForLayer(focusLayer, contentWidth, out float normalized))
            scroll.horizontalNormalizedPosition = normalized;
        _scrollFocusCoroutine = null;
    }

    IEnumerator PlayMapIntroScrollCoroutine(int maxLayer, float contentWidth)
    {
        yield return null;
        Canvas.ForceUpdateCanvases();

        var scroll = mapScrollRect != null ? mapScrollRect : GetComponentInChildren<ScrollRect>(true);
        if (scroll == null)
        {
            _mapIntroCoroutine = null;
            yield break;
        }

        scroll.horizontal = true;
        scroll.vertical = false;

        if (!TryGetHorizontalNormalizedForLayer(0, contentWidth, out float targetNorm))
        {
            _mapIntroCoroutine = null;
            yield break;
        }

        var viewport = scroll.viewport != null ? scroll.viewport : scroll.GetComponent<RectTransform>();
        float viewportW = viewport != null ? viewport.rect.width : 0f;
        if (viewportW <= 0f || contentWidth <= viewportW)
        {
            scroll.horizontalNormalizedPosition = targetNorm;
            _mapIntroCoroutine = null;
            yield break;
        }

        float fromNorm = TryGetHorizontalNormalizedForLayer(maxLayer, contentWidth, out float bossNorm)
            ? bossNorm
            : 1f;

        scroll.horizontalNormalizedPosition = fromNorm;
        scroll.enabled = false;
        _mapInputLocked = true;

        float duration = Mathf.Max(0.01f, mapIntroDuration);
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float u = mapIntroCurve != null && mapIntroCurve.length > 0
                ? mapIntroCurve.Evaluate(Mathf.Clamp01(t / duration))
                : Mathf.Clamp01(t / duration);
            scroll.horizontalNormalizedPosition = Mathf.Lerp(fromNorm, targetNorm, u);
            yield return null;
        }

        scroll.horizontalNormalizedPosition = targetNorm;
        scroll.enabled = true;
        _mapInputLocked = false;
        _mapIntroCoroutine = null;
    }

    RoguelikeMapNodeWidget SpawnNodeWidget(Transform parent)
    {
        if (nodePrefab != null)
            return Instantiate(nodePrefab, parent);
        return RoguelikeMapNodeWidget.CreateRuntime(parent, nodeSize, runtimeNodeLabelFontSize);
    }

    static RoguelikeNodeVisualState ResolveVisualState(RoguelikeRunState state, int nodeId, HashSet<int> selectable)
    {
        if (IsRoguelikeMapTestMode())
        {
            if (state.currentNodeId == nodeId)
                return RoguelikeNodeVisualState.Current;
            return RoguelikeNodeVisualState.Selectable;
        }

        if (state.currentNodeId == nodeId)
            return RoguelikeNodeVisualState.Current;
        if (state.IsNodeCleared(nodeId))
            return RoguelikeNodeVisualState.Cleared;
        if (selectable.Contains(nodeId))
            return RoguelikeNodeVisualState.Selectable;
        if (state.IsNodeVisited(nodeId))
            return RoguelikeNodeVisualState.Visited;
        return RoguelikeNodeVisualState.Locked;
    }

    static bool IsRoguelikeMapTestMode() =>
        GameManage.instance != null && GameManage.instance.mode == GameMode.Test;

    void OnNodeClicked(int nodeId)
    {
        if (_mapInputLocked)
            return;

        bool selected = IsRoguelikeMapTestMode()
            ? RoguelikeRunService.TrySelectNodeForTest(nodeId)
            : RoguelikeRunService.TrySelectNextNode(nodeId);
        if (!selected)
            return;

        var node = RoguelikeRunService.State?.currentMap?.GetNode(nodeId);
        if (node == null) return;

        switch (node.roomType)
        {
            case MapRoomType.Start:
                Refresh();
                return;
            case MapRoomType.Shop:
                Hide();
                RoguelikeRunService.EnterShopNode();
                break;
            case MapRoomType.Rest:
                Hide();
                RoguelikeRunService.EnterRestNode();
                break;
            default:
                if (RoguelikeRunService.ResolveLevelForPendingCombat() == null)
                {
                    Debug.LogWarning("[RoguelikeMapPanel] 无法解析关卡，请检查 ActMapConfig 关卡池");
                    Refresh();
                    return;
                }
                Hide();
                RoguelikeRunService.EnterCombatNode();
                break;
        }
    }

    void OnAbandonClicked()
    {
        RoguelikeRunService.AbandonRun();
        RoguelikeRunInfoPanel.HideForRunEnded();
        Hide();
        UIManage.GetView<StartUI>()?.Show();
    }

    void CreateLine(RectTransform lineParent, Vector2 a, Vector2 b, RoguelikeMapLineStyle style, bool usePrefabAppearanceOnly)
    {
        if (lineParent == null)
            return;

        var coordinateRoot = nodesRoot != null ? nodesRoot : lineParent;
        var styleRef = style ?? new RoguelikeMapLineStyle();

        GameObject go;
        if (RoguelikeMapLineWidget.ShouldUseUiImageLine(coordinateRoot))
        {
            RoguelikeMapLineWidget.LogOverlayLineRendererWarningOnce();
            go = RoguelikeMapLineWidget.CreateUiImageLine(lineParent, a, b, styleRef);
        }
        else if (linePrefab != null)
        {
            var line = Instantiate(linePrefab, lineParent);
            line.ApplyBetween(a, b, coordinateRoot, styleRef, usePrefabAppearanceOnly);
            go = line.gameObject;
        }
        else
        {
            var line = RoguelikeMapLineWidget.CreateRuntime(lineParent);
            line.ApplyBetween(a, b, coordinateRoot, styleRef, false);
            go = line.gameObject;
        }

        go.transform.SetAsFirstSibling();
        _lineObjects.Add(go);
    }

    void ClearVisuals()
    {
        for (int i = _nodeWidgets.Count - 1; i >= 0; i--)
        {
            if (_nodeWidgets[i] != null)
                Destroy(_nodeWidgets[i].gameObject);
        }
        _nodeWidgets.Clear();

        for (int i = _lineObjects.Count - 1; i >= 0; i--)
        {
            if (_lineObjects[i] != null)
                Destroy(_lineObjects[i]);
        }
        _lineObjects.Clear();

        for (int i = _layerRowObjects.Count - 1; i >= 0; i--)
        {
            if (_layerRowObjects[i] != null)
                Destroy(_layerRowObjects[i]);
        }
        _layerRowObjects.Clear();
        _layerRowsByLayer.Clear();
        _widgetByNodeId.Clear();
        _nodeCenterInDrawRoot.Clear();
    }

    static string BuildHeaderText(RoguelikeRunState state)
    {
        var act = RoguelikeRunService.ActiveRunConfig?.GetAct(state.currentActIndex);
        if (act != null && !string.IsNullOrEmpty(act.displayName))
            return act.displayName;
        return $"Act {state.currentActIndex + 1}";
    }

    void RefreshHeader(string text)
    {
        if (headerText != null)
            headerText.text = text;
    }

    RectTransform ResolveMapContent()
    {
        if (mapContent != null)
            return mapContent;
        if (mapViewport != null)
            return mapViewport;
        if (mapScrollRect != null && mapScrollRect.content != null)
            return mapScrollRect.content;
        return null;
    }

    void EnsureLayout()
    {
        var panelRt = transform as RectTransform;
        if (panelRt == null) return;

        if (panelBackground == null)
            panelBackground = GetComponent<Image>();

        EnsureScrollMapHierarchy();

        if (!autoGenerateMissingUi)
        {
            StretchFull(panelRt);
            return;
        }

        if (ResolveMapContent() == null)
        {
            var viewportGo = new GameObject("MapViewport", typeof(RectTransform));
            viewportGo.transform.SetParent(transform, false);
            mapViewport = viewportGo.GetComponent<RectTransform>();
            mapContent = mapViewport;
            StretchFull(mapViewport);
            mapViewport.offsetMin = new Vector2(40f, 80f);
            mapViewport.offsetMax = new Vector2(-40f, -120f);
        }

        var content = ResolveMapContent();

        if (mapBackgroundImage == null && content != null)
        {
            var bgGo = new GameObject("MapBackground", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bgGo.transform.SetParent(content, false);
            mapBackgroundImage = bgGo.GetComponent<Image>();
            mapBackgroundImage.raycastTarget = false;
            mapBackgroundImage.color = new Color(1f, 1f, 1f, 0.15f);
        }

        if (linesRoot == null && content != null)
        {
            var linesGo = new GameObject("Lines", typeof(RectTransform));
            linesGo.transform.SetParent(content, false);
            linesRoot = linesGo.GetComponent<RectTransform>();
        }

        if (nodesRoot == null && content != null)
        {
            var nodesGo = new GameObject("Nodes", typeof(RectTransform));
            nodesGo.transform.SetParent(content, false);
            nodesRoot = nodesGo.GetComponent<RectTransform>();
        }

        SyncLinesRootWithNodesRoot();

        if (_lastContentWidth > 0f)
        {
            float w = _lastContentWidth;
            float h = _lastContentHeight > 0f ? _lastContentHeight : layerRowWidth;
            ApplyMapContentSize(w, h);
        }
        else if (content != null)
        {
            ApplyDrawRootSize(nodesRoot, layerRowWidth, content.sizeDelta.y > 0f ? content.sizeDelta.y : layerRowWidth);
            SyncLinesRootWithNodesRoot();
        }

        if (headerText == null)
            CreateRuntimeHeader();

        if (abandonButton == null)
            CreateRuntimeAbandonButton();

        if (panelBackground == null)
        {
            panelBackground = gameObject.AddComponent<Image>();
            panelBackground.color = fallbackPanelBackgroundColor;
            panelBackground.raycastTarget = true;
        }

        StretchFull(panelRt);
    }

    void EnsureScrollMapHierarchy()
    {
        if (mapScrollRect == null)
            mapScrollRect = GetComponentInChildren<ScrollRect>(true);

        if (mapScrollRect != null)
        {
            mapScrollRect.horizontal = true;
            mapScrollRect.vertical = false;
            mapScrollRect.movementType = ScrollRect.MovementType.Elastic;
            mapScrollRect.scrollSensitivity = 24f;

            if (mapScrollRect.content != null)
            {
                if (mapContent == null && mapViewport == null)
                    mapContent = mapScrollRect.content;
                else if (mapContent == null)
                    mapContent = mapViewport;

                var content = mapScrollRect.content;
                content.anchorMin = new Vector2(0f, 0.5f);
                content.anchorMax = new Vector2(0f, 0.5f);
                content.pivot = new Vector2(0f, 0.5f);
                content.anchoredPosition = Vector2.zero;

                var fitter = content.GetComponent<ContentSizeFitter>();
                if (fitter != null)
                    fitter.enabled = false;
            }

            EnsureMapDrawChildrenUnderContent(mapScrollRect.content);
        }

        if (mapContent == null && mapViewport != null)
            mapContent = mapViewport;
    }

    void EnsureMapDrawChildrenUnderContent(RectTransform content)
    {
        if (content == null)
            return;

        if (mapBackgroundImage == null)
        {
            var existing = content.Find("MapBackground");
            if (existing != null)
                mapBackgroundImage = existing.GetComponent<Image>();
        }

        if (mapBackgroundImage == null)
        {
            var bgGo = new GameObject("MapBackground", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bgGo.transform.SetParent(content, false);
            bgGo.transform.SetAsFirstSibling();
            mapBackgroundImage = bgGo.GetComponent<Image>();
            mapBackgroundImage.raycastTarget = false;
            mapBackgroundImage.color = new Color(1f, 1f, 1f, 0.15f);
        }

        if (linesRoot == null)
        {
            var t = content.Find("Lines");
            if (t != null)
                linesRoot = t as RectTransform;
        }

        if (linesRoot == null)
        {
            var linesGo = new GameObject("Lines", typeof(RectTransform));
            linesGo.transform.SetParent(content, false);
            linesRoot = linesGo.GetComponent<RectTransform>();
        }

        if (nodesRoot == null)
        {
            var t = content.Find("Nodes");
            if (t != null)
                nodesRoot = t as RectTransform;
        }

        if (nodesRoot == null)
        {
            var nodesGo = new GameObject("Nodes", typeof(RectTransform));
            nodesGo.transform.SetParent(content, false);
            nodesRoot = nodesGo.GetComponent<RectTransform>();
        }

        SyncLinesRootWithNodesRoot();
    }

    void CreateRuntimeHeader()
    {
        var headerGo = new GameObject("Header", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        headerGo.transform.SetParent(transform, false);
        var hrt = headerGo.GetComponent<RectTransform>();
        hrt.anchorMin = new Vector2(0f, 1f);
        hrt.anchorMax = new Vector2(1f, 1f);
        hrt.pivot = new Vector2(0.5f, 1f);
        hrt.sizeDelta = new Vector2(0f, 48f);
        hrt.anchoredPosition = new Vector2(0f, -8f);
        headerText = headerGo.GetComponent<TextMeshProUGUI>();
        headerText.fontSize = 22f;
        headerText.alignment = TextAlignmentOptions.Center;
    }

    void CreateRuntimeAbandonButton()
    {
        var btnGo = new GameObject("AbandonButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnGo.transform.SetParent(transform, false);
        var brt = btnGo.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(0.5f, 0f);
        brt.anchorMax = new Vector2(0.5f, 0f);
        brt.pivot = new Vector2(0.5f, 0f);
        brt.sizeDelta = new Vector2(160f, 40f);
        brt.anchoredPosition = new Vector2(0f, 24f);
        var img = btnGo.GetComponent<Image>();
        img.color = new Color(0.35f, 0.35f, 0.4f);
        abandonButton = btnGo.GetComponent<Button>();
        abandonButton.targetGraphic = img;

        var labelGo = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(btnGo.transform, false);
        StretchFull(labelGo.GetComponent<RectTransform>());
        var tmp = labelGo.GetComponent<TextMeshProUGUI>();
        tmp.text = "放弃本局";
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 18f;
    }

    static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.pivot = new Vector2(0.5f, 0.5f);
    }
}
