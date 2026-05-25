using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 肉鸽选路地图 UI。预制体：<c>Resources/UIPrefab/RoguelikeMapPanel</c>。
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

    [Header("2. 布局（每层一行 + 格内随机）")]
    [SerializeField] float layerRowWidth = 1250f;
    [SerializeField] float layerRowHeight = 200f;
    [SerializeField] Vector2 nodeSize = new Vector2(100f, 100f);
    [Tooltip("在各自格子 (行宽/列数 × 行高) 内随机偏移，避免地图过于整齐；同一 Run 种子下位置稳定")]
    [SerializeField] bool randomizeNodePlacement = true;
    [SerializeField] Vector2 mapOffset;
    [SerializeField] float mapScale = 1f;

    [Header("2b. 纵向滚动（起点在底、Boss 在顶，滚轮/拖拽向上查看）")]
    [SerializeField] float mapPaddingTop = 48f;
    [SerializeField] float mapPaddingBottom = 48f;
    [Tooltip("Refresh 后滚到当前节点；无当前则滚到底部（起点）")]
    [SerializeField] bool scrollToFocusOnRefresh = true;
    [SerializeField] float scrollFocusViewportFraction = 0.35f;

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
    float _lastContentWidth;
    float _lastContentHeight;
    Coroutine _scrollFocusCoroutine;

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
    }

    void OnDisable()
    {
        RoguelikeRunService.OnActMapGenerated -= OnMapChanged;
        RoguelikeRunService.OnNodeResolved -= OnNodeResolved;
        RoguelikeRunService.OnRunCompleted -= OnRunEnded;
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

    void OnRunEnded() => RefreshHeader("通关！");

    public void ShowAndRefresh()
    {
        Show();
        Refresh();
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
        ClearVisuals();
        base.Hide();
    }

    public static void OpenRun(RunMapConfig config, int? seed = null)
    {
        if (config == null)
        {
            Debug.LogError("[RoguelikeMapPanel] RunMapConfig 为空");
            return;
        }
        RoguelikeRunService.StartNewRun(config, seed);
        UIManage.GetView<RoguelikeMapPanel>()?.ShowAndRefresh();
    }

    /// <summary>继续未完成的 Run（需存在 active 存档且 config 能解析）。</summary>
    public static bool OpenContinuedRun(RunMapConfig config)
    {
        if (config == null)
        {
            Debug.LogError("[RoguelikeMapPanel] RunMapConfig 为空");
            return false;
        }
        if (!RoguelikeRunService.TryContinueRun(config))
            return false;
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
        float contentWidth = layerRowWidth * mapScale;
        float contentHeight = ComputeContentHeight(maxLayer);
        ApplyMapContentSize(contentWidth, contentHeight);
        SyncLinesRootWithNodesRoot();

        BuildLayerRowsAndPlaceNodes(state, map, maxLayer);
        Canvas.ForceUpdateCanvases();
        DrawLinesUnderLayers(map, maxLayer);

        if (scrollToFocusOnRefresh)
            ScheduleScrollToFocus(map, state, maxLayer, contentHeight);
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

    float LayerRowStride => layerRowHeight * mapScale;

    float ComputeContentHeight(int maxLayer)
    {
        return mapPaddingTop + mapPaddingBottom + (maxLayer + 1) * LayerRowStride;
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

    /// <summary>Lines 与 Nodes 必须用同一套锚点/位置，否则连线会整体偏移。</summary>
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

        linesRoot.SetSiblingIndex(nodesRoot.GetSiblingIndex());
        CopyRectTransformLayout(linesRoot, nodesRoot);
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
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
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

    /// <summary>每层 1250×200 行父物体；房间 100×100 在行内，仅 X 方向格内随机（local Y=0，锚点行中心）。</summary>
    void BuildLayerRowsAndPlaceNodes(RoguelikeRunState state, GeneratedRoguelikeMap map, int maxLayer)
    {
        if (nodesRoot == null)
            return;

        _layerRowsByLayer.Clear();
        _widgetByNodeId.Clear();

        int gridWidth = ResolveMapGridWidth(map);
        int layoutSeed = state.runSeed ^ (map.seed * 486187739);
        float rowW = layerRowWidth * mapScale;
        float rowH = layerRowHeight * mapScale;
        Vector2 scaledNodeSize = nodeSize * mapScale;

        var selectable = new HashSet<int>();
        foreach (var n in state.GetSelectableNextNodes())
            selectable.Add(n.id);
        var stateStyle = EffectiveNodeStateStyle ?? new RoguelikeMapNodeStateStyle();

        for (int layer = 0; layer <= maxLayer; layer++)
        {
            float rowTopY = mapPaddingTop + (maxLayer - layer) * LayerRowStride;
            var layerRow = CreateLayerRow(layer, rowW, rowH, rowTopY);

            var rowNodes = map.GetNodesOnLayer(layer);
            for (int i = 0; i < rowNodes.Count; i++)
            {
                var node = rowNodes[i];
                Vector2 localInRow = ComputeNodeLocalInRow(node, gridWidth, rowW, scaledNodeSize, layoutSeed);

                var visual = ResolveVisualState(state, node.id, selectable);
                var widget = SpawnNodeWidget(layerRow);
                var rt = widget.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(localInRow.x, 0f);
                widget.SetSize(scaledNodeSize);
                widget.Bind(node.id, node.roomType, visual, OnNodeClicked, GetTypeStyle(node.roomType), stateStyle,
                    showLabelWhenIconPresent);
                _nodeWidgets.Add(widget);
                _widgetByNodeId[node.id] = widget;
            }
        }
    }

    RectTransform CreateLayerRow(int layer, float rowW, float rowH, float rowTopY)
    {
        var go = new GameObject($"Layer_{layer}", typeof(RectTransform));
        go.transform.SetParent(nodesRoot, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(rowW, rowH);
        rt.anchoredPosition = new Vector2(0f, -rowTopY);
        _layerRowObjects.Add(go);
        _layerRowsByLayer[layer] = rt;
        return rt;
    }

    /// <summary>连线挂在源节点所在层（Boss 层不挂）；起点为房间中心，终点为下一层房间。</summary>
    void DrawLinesUnderLayers(GeneratedRoguelikeMap map, int bossLayer)
    {
        var style = EffectiveLineStyle ?? new RoguelikeMapLineStyle();
        bool prefabOnly = style.usePrefabAppearanceOnly && linePrefab != null;

        for (int i = 0; i < map.nodes.Count; i++)
        {
            var from = map.nodes[i];
            if (from.layer >= bossLayer)
                continue;
            if (!_layerRowsByLayer.TryGetValue(from.layer, out var layerRow))
                continue;
            if (!_widgetByNodeId.TryGetValue(from.id, out var fromWidget))
                continue;

            var fromRt = fromWidget.transform as RectTransform;
            if (fromRt == null)
                continue;

            Vector2 startInRow = fromRt.anchoredPosition;

            for (int j = 0; j < from.nextNodeIds.Count; j++)
            {
                if (!_widgetByNodeId.TryGetValue(from.nextNodeIds[j], out var toWidget))
                    continue;
                var toRt = toWidget.transform as RectTransform;
                if (toRt == null)
                    continue;

                Vector3 toWorld = toRt.TransformPoint(toRt.rect.center);
                Vector2 endInRow = layerRow.InverseTransformPoint(toWorld);
                CreateLine(layerRow, startInRow, endInRow, style, prefabOnly);
            }
        }
    }

    Vector2 ComputeNodeLocalInRow(
        RoguelikeMapNode node,
        int gridWidth,
        float rowW,
        Vector2 scaledNodeSize,
        int layoutSeed)
    {
        float cellW = rowW / gridWidth;
        float centerX = (node.slot + 0.5f) * cellW - rowW * 0.5f;
        float maxDx = Mathf.Max(0f, (cellW - scaledNodeSize.x) * 0.5f);

        float rx = 0f;
        if (randomizeNodePlacement && maxDx > 0f)
        {
            var rng = new System.Random(layoutSeed + node.id * 73856093 + node.layer * 19349663 + node.slot * 83492791);
            rx = (float)(rng.NextDouble() * 2.0 - 1.0) * maxDx;
        }

        return new Vector2(centerX + rx, 0f);
    }

    void ScheduleScrollToFocus(GeneratedRoguelikeMap map, RoguelikeRunState state, int maxLayer, float contentHeight)
    {
        if (_scrollFocusCoroutine != null)
            StopCoroutine(_scrollFocusCoroutine);
        _scrollFocusCoroutine = StartCoroutine(ScrollToFocusNextFrame(map, state, maxLayer, contentHeight));
    }

    System.Collections.IEnumerator ScrollToFocusNextFrame(
        GeneratedRoguelikeMap map,
        RoguelikeRunState state,
        int maxLayer,
        float contentHeight)
    {
        yield return null;
        Canvas.ForceUpdateCanvases();

        var scroll = mapScrollRect != null ? mapScrollRect : GetComponentInChildren<ScrollRect>(true);
        if (scroll == null)
            yield break;

        var viewport = scroll.viewport != null ? scroll.viewport : scroll.GetComponent<RectTransform>();
        if (viewport == null)
            yield break;

        float viewportH = viewport.rect.height;
        if (viewportH <= 0f || contentHeight <= viewportH)
        {
            scroll.verticalNormalizedPosition = 0f;
            yield break;
        }

        int focusLayer = 0;
        if (state.currentNodeId >= 0)
        {
            var cur = map.GetNode(state.currentNodeId);
            if (cur != null)
                focusLayer = cur.layer;
        }

        float focusYFromTop = mapPaddingTop + (maxLayer - focusLayer) * LayerRowStride;
        float scrollRange = contentHeight - viewportH;
        float offsetFromTop = focusYFromTop - viewportH * scrollFocusViewportFraction;
        offsetFromTop = Mathf.Clamp(offsetFromTop, 0f, scrollRange);
        scroll.verticalNormalizedPosition = 1f - offsetFromTop / scrollRange;
        _scrollFocusCoroutine = null;
    }

    RoguelikeMapNodeWidget SpawnNodeWidget(Transform parent)
    {
        if (nodePrefab != null)
            return Instantiate(nodePrefab, parent);
        return RoguelikeMapNodeWidget.CreateRuntime(parent, nodeSize, runtimeNodeLabelFontSize);
    }

    static RoguelikeNodeVisualState ResolveVisualState(RoguelikeRunState state, int nodeId, HashSet<int> selectable)
    {
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

    void OnNodeClicked(int nodeId)
    {
        if (!RoguelikeRunService.TrySelectNextNode(nodeId))
            return;

        var node = RoguelikeRunService.State?.currentMap?.GetNode(nodeId);
        if (node == null) return;

        switch (node.roomType)
        {
            case MapRoomType.Start:
                Refresh();
                return;
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
        Hide();
        UIManage.GetView<StartUI>()?.Show();
    }

    void CreateLine(RectTransform layerRow, Vector2 a, Vector2 b, RoguelikeMapLineStyle style, bool usePrefabAppearanceOnly)
    {
        if (layerRow == null)
            return;

        GameObject go;
        if (linePrefab != null)
        {
            var line = Instantiate(linePrefab, layerRow);
            line.ApplyBetween(a, b, style, usePrefabAppearanceOnly);
            go = line.gameObject;
        }
        else
        {
            go = new GameObject("Line", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(layerRow, false);
            var lineWidget = go.AddComponent<RoguelikeMapLineWidget>();
            lineWidget.ApplyBetween(a, b, style, false);
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
    }

    static string BuildHeaderText(RoguelikeRunState state)
    {
        var act = RoguelikeRunService.ActiveRunConfig?.GetAct(state.currentActIndex);
        string actName = act != null ? act.displayName : $"Act {state.currentActIndex + 1}";
        return $"{actName}  |  种子 {state.runSeed}";
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

        if (_lastContentHeight > 0f)
        {
            float w = _lastContentWidth > 0f ? _lastContentWidth : layerRowWidth;
            ApplyMapContentSize(w, _lastContentHeight);
        }
        else if (content != null)
        {
            ApplyDrawRootSize(nodesRoot, layerRowWidth, content.sizeDelta.y);
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
            mapScrollRect.horizontal = false;
            mapScrollRect.vertical = true;
            mapScrollRect.movementType = ScrollRect.MovementType.Elastic;
            mapScrollRect.scrollSensitivity = 24f;

            if (mapScrollRect.content != null)
            {
                if (mapContent == null && mapViewport == null)
                    mapContent = mapScrollRect.content;
                else if (mapContent == null)
                    mapContent = mapViewport;

                var content = mapScrollRect.content;
                content.anchorMin = new Vector2(0f, 1f);
                content.anchorMax = new Vector2(1f, 1f);
                content.pivot = new Vector2(0.5f, 1f);
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
