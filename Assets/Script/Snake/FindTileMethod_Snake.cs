using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
/// <summary>
/// 蛇头沿网格移动：默认向右；WASD / 方向键缓冲；手机可用 <see cref="TryQueueDirection"/> 接 UI 按钮或滑动层。禁止相对<b>当前这一段的实际移动方向</b>（<see cref="_segmentMoveDirection"/>）180° 反转，
/// 而非相对 <see cref="_facing"/>（预输入会改 _facing，但身体尚未走完当前一格时不能按「意图朝向」判反向）。
/// <see cref="FindNextTile"/> 返回 <c>standTile.mapPos + 当前朝向</c> 上的格子；越界时返回 null，
/// 并通知 <see cref="LevelController_Snake.NotifySnakeDefeat(Chess)"/>（每局仅触发一次，避免 MoveController 每帧重试时重复调用）。
/// </summary>
/// <para>
/// 蛇身可用 <see cref="LineRenderer"/>：路径为「尾→头」的格子中心点；每到达一格追加一点，
/// 长度由 <see cref="NotifyFoodEatenGrow"/> 与 <see cref="AfterFrameSyncPath"/>（由 <see cref="SnakeGridController.LateUpdate"/> 调用）裁剪。
/// </para>
[Serializable]
public class FindTileMethod_Snake : FindTileMethod
{
    [SerializeField] Vector2Int _initialFacing = Vector2Int.right;

    [Tooltip("开局身体节数（含头在末端）。>1 时沿 -初始朝向 在地图上占位。")]
    [Min(1)]
    [SerializeField]
    int initialSegmentCount = 1;

    [Tooltip("可选。子物体上挂 LineRenderer，材质用 Sprite/Default 等即可。")]
    [SerializeField]
    LineRenderer snakeBodyLine;

    [Tooltip("可选。蛇头的子物体：LineRenderer 最后一节顶点跟随此 Transform，用于修正美术相对根节点的偏移；空则使用蛇头 Chess 根 Transform。")]
    [SerializeField]
    Transform lineHeadAnchor;

    [Tooltip("拐弯处插入细分顶点，减轻折叠/尖角感（0 则关闭）。")]
    [Min(0)]
    [SerializeField]
    int cornerVertices = 4;

    [Tooltip("线两端圆帽细分（0 则关闭）。")]
    [Min(0)]
    [SerializeField]
    int capVertices = 2;

    [Tooltip("尾部被路径裁剪前移时，仅首顶点从旧格心滑到新格心的时长（秒）。")]
    [Min(0.02f)]
    [SerializeField]
    float tailSlideDuration = 0.12f;

    [Tooltip("蛇身 Shader 中 Tiling And Offset 的 Vector2 属性名（Shader Graph 引用名）。")]
    [SerializeField]
    string bodySizeShaderProperty = "_BodySize";

    [Tooltip("沿蛇身：_BodySize.x ≈ 折线世界长度 × 该系数；可在材质里再微调 Shader。")]
    [Min(0.0001f)]
    [SerializeField]
    float bodyTilingAlongPerWorldUnit = 1f;

    [Tooltip("传给 _BodySize 的 Y 分量（沿线为 X 时，Y 常为 1）。")]
    [SerializeField]
    float bodySizeY = 1f;

    [Tooltip("做法 A：Shader 里银/金分界（0~1，沿 Line 的归一化弧长，与 UV.x 同域）。在 SnakeBody Shader Graph 里建同名 Float。")]
    [SerializeField]
    string shaderTierSilverGoldNormalizedProperty = "_TierSilverGold";

    [Tooltip("做法 A：Shader 里金/钻分界（0~1）。钻为 UV.x 大于该值的一段。")]
    [SerializeField]
    string shaderTierGoldDiamondNormalizedProperty = "_TierGoldDiamond";

    [Tooltip("可选。蛇尾独立物体（精灵/装饰）；与 LineRenderer 分离，末顶点接在此 Transform 上，避免尾端被当成「身体贴图的一段」。")]
    [SerializeField]
    Transform snakeTrail;

    [Tooltip("蛇尾每帧追向当前尾格中心的速度（仅当已指定 SnakeTrail 且非尾滑移时）。")]
    [Min(0f)]
    [SerializeField]
    float snakeTailFollowSpeed = 14f;

    [Tooltip("蛇尾精灵默认朝向为 +X 时填 0；若美术朝 +Y，可试 -90 或 90。")]
    [SerializeField]
    float snakeTailRotationOffsetDegrees = 0f;

    [Tooltip("银/金档节数超过该值时进位：银→金清零+1、金→钻清零+1（总长 = 银+金+钻）。")]
    [Min(1)]
    [SerializeField]
    int maxSegmentLengthBeforeReset = 10;

    Vector2Int _facing = Vector2Int.right;
    /// <summary>当前「这一小段位移」的格子方向（上一格 → 正在走向的下一格），仅当 <see cref="FindNextTile"/> 成功选定下一格时更新；预输入判反向用此值。</summary>
    Vector2Int _segmentMoveDirection = Vector2Int.right;
    Vector2Int? _queuedFacing;
    bool _defeatNotified;

    readonly List<Vector3> _pathTailToHead = new List<Vector3>();
    readonly List<Vector2Int> _pathCells = new List<Vector2Int>();
    /// <summary>实际用于路径裁剪的节数，恒为 <see cref="_segmentCountSilver"/> + <see cref="_segmentCountGold"/> + <see cref="_segmentCountDiamond"/>。</summary>
    [FoldoutGroup("蛇身"),ShowInInspector]
    int _segmentCount = 1;
    /// <summary>银档节数（吃食物先增加）；开局与身体长度一致。</summary>
    [FoldoutGroup("蛇身"),ShowInInspector]
    int _segmentCountSilver = 1;
    /// <summary>金档（银档满一轮后进位）。</summary>
    [FoldoutGroup("蛇身"),ShowInInspector]
    int _segmentCountGold = 0;
    /// <summary>钻档（金档满一轮后进位）。</summary>
    [FoldoutGroup("蛇身"), ShowInInspector]
    int _segmentCountDiamond = 0;
    Chess _boundChess;

    Vector3 _tailDisplayPos;
    bool _tailSliding;
    Vector3 _tailSlideFrom;
    Vector3 _tailSlideTo;
    float _tailSlideProgress;

    public int SegmentCountSilver => _segmentCountSilver;

    public int SegmentCountGold => _segmentCountGold;

    public int SegmentCountDiamond => _segmentCountDiamond;

    public override void StartMoving(Chess c)
    {
        base.StartMoving(c);
    }

    public override void WhenMoving(Chess c)
    {
        base.WhenMoving(c);
        Chess chess = c;

        // 仅 Player：僵尸等非玩家 Y 移速倍率与 X 不同，delta.x 易在目标附近来回变号，会误触发每帧翻面导致徘徊
        Vector3 pos = chess.transform.position;
        Vector3 target = c.moveController.nextTile? c.moveController.nextTile.transform.position :pos;
        chess.UpdateFacingFromHorizontalMove((Vector2)(target - pos));
        PollKeyboardToQueue();
        ApplyQueuedDirection();
        UpdateSnakeTailFollow();
        UpdateTailSlide();
        UpdateLineHeadToTransform(c);
        UpdateSnakeTrailRotation();
    }

    /// <summary>新开局或重新登记蛇头时由 <see cref="SnakeGridController"/> 调用。</summary>
    public void ResetForNewRun(Chess headChess)
    {
        _facing = _initialFacing;
        _segmentMoveDirection = _initialFacing;
        _queuedFacing = null;
        _defeatNotified = false;
        _boundChess = headChess;
        _segmentCount = Mathf.Max(1, initialSegmentCount);
        _segmentCountGold = 0;
        _segmentCountDiamond = 0;
        _segmentCountSilver = _segmentCount;
        _pathTailToHead.Clear();
        _pathCells.Clear();
        if (headChess != null && headChess.moveController != null && headChess.moveController.standTile != null)
            BuildInitialPath(headChess.moveController.standTile);
        _tailSliding = false;
        if (_pathTailToHead.Count > 0)
        {
            _tailDisplayPos = _pathTailToHead[0];
            if (snakeTrail != null)
                snakeTrail.position = LineVertexZ0(_pathTailToHead[0]);
        }

        ApplyLineRenderer();
        UpdateSnakeTrailRotation();
    }

    /// <summary>吃食物：银档 +1；总长 = 银+金+钻；银/金超 <see cref="maxSegmentLengthBeforeReset"/> 按档进位；路径裁剪见 <see cref="AfterFrameSyncPath"/>。</summary>
    public void NotifyFoodEatenGrow()
    {
        _segmentCountSilver++;
        _segmentCount = _segmentCountSilver + _segmentCountGold + _segmentCountDiamond;
        bool silverToGold = false;
        if (_segmentCountSilver > maxSegmentLengthBeforeReset)
        {
            _segmentCountSilver = 0;
            _segmentCountGold++;
            _segmentCount = _segmentCountSilver + _segmentCountGold + _segmentCountDiamond;
            silverToGold = true;
        }

        bool goldToDiamond = false;
        if (_segmentCountGold > maxSegmentLengthBeforeReset)
        {
            _segmentCountGold = 0;
            _segmentCountDiamond++;
            _segmentCount = _segmentCountSilver + _segmentCountGold + _segmentCountDiamond;
            goldToDiamond = true;
        }

        if (silverToGold || goldToDiamond)
            ShowSnakeTierUpgradeFloatingText(silverToGold, goldToDiamond);

        AfterFrameSyncPath();
    }

    static readonly Color SnakeTierGoldTextColor = new Color(1f, 0.78f, 0.15f);
    static readonly Color SnakeTierDiamondTextColor = new Color(0.28f, 0.62f, 1f);

    /// <summary>银→金（金色）/ 金→钻（蓝色）进位时在蛇头飘「升级」二字。</summary>
    void ShowSnakeTierUpgradeFloatingText(bool silverToGold, bool goldToDiamond)
    {
        if (_boundChess == null) return;
        var panel = UIManage.GetView<DamagePanel>();
        if (panel == null) return;

        var dm = new DamageMessege(_boundChess, _boundChess, 0f);
        if (silverToGold)
            panel.ShowText(dm, "升级", SnakeTierGoldTextColor);
        if (goldToDiamond)
            panel.ShowText(dm, "升级", SnakeTierDiamondTextColor);
    }

    /// <summary>蛇头到达新格子中心（由 <see cref="SnakeGridController"/> 订阅 <see cref="MoveController.OnReachTile"/>）。</summary>
    public void OnReachTile(Tile stand)
    {
        if (stand == null) return;
        Vector2Int p = stand.mapPos;
        if (CheckSelfCollision(p))
        {
            NotifyDefeatOnce(SnakeGameEventId.StumbleSelfBite);
            return;
        }

        _pathTailToHead.Add(stand.transform.position);
        _pathCells.Add(p);
    }

    /// <summary>当前蛇身占用的地图格（尾→头），供食物生成等查询。</summary>
    public void CopyOccupiedCellsTo(HashSet<Vector2Int> into)
    {
        if (into == null) return;
        for (int i = 0; i < _pathCells.Count; i++)
            into.Add(_pathCells[i]);
    }

    /// <summary>每帧末尾裁剪路径长度，避免「到达格子 / 吃豆」同帧顺序问题。</summary>
    public void AfterFrameSyncPath()
    {
        int before = _pathTailToHead.Count;
        while (_pathTailToHead.Count > _segmentCount)
        {
            _pathTailToHead.RemoveAt(0);
            if (_pathCells.Count > 0)
                _pathCells.RemoveAt(0);
        }

        if (_pathTailToHead.Count == 0)
        {
            ApplyLineRenderer();
            return;
        }

        if (before > _pathTailToHead.Count)
        {
            _tailSlideFrom = snakeTrail != null ? snakeTrail.position : _tailDisplayPos;
            _tailSlideTo = LineVertexZ0(_pathTailToHead[0]);
            _tailSlideProgress = 0f;
            _tailSliding = true;
        }

        ApplyLineRenderer();
    }

    void BuildInitialPath(Tile headStand)
    {
        MapManage map = MapManage.instance;
        for (int i = 0; i < initialSegmentCount; i++)
        {
            Vector2Int cell = headStand.mapPos - _initialFacing * i;
            if (map == null || !map.IfInMapRange(cell.x, cell.y)) break;
            Tile t = map.tiles[cell.x, cell.y];
            _pathTailToHead.Insert(0, t.transform.position);
            _pathCells.Insert(0, cell);
        }

        if (_pathTailToHead.Count == 0)
        {
            _pathTailToHead.Add(headStand.transform.position);
            _pathCells.Add(headStand.mapPos);
        }
    }

    /// <summary>下一步头将进入 <paramref name="nextHead"/> 时是否与身体重叠（允许吃进即将离开的尾格）。</summary>
    bool CheckSelfCollision(Vector2Int nextHead)
    {
        int n = _pathCells.Count;
        if (n <= 1) return false;
        if (nextHead == _pathCells[0]) return false;
        for (int i = 1; i < n - 1; i++)
        {
            if (_pathCells[i] == nextHead) return true;
        }

        return false;
    }

    void ApplyLineRenderer()
    {
        if (snakeBodyLine == null) return;
        if (_pathTailToHead.Count == 0)
        {
            snakeBodyLine.positionCount = 0;
            return;
        }

        snakeBodyLine.numCornerVertices = cornerVertices;
        snakeBodyLine.numCapVertices = capVertices;

        if (!_tailSliding)
            _tailDisplayPos = _pathTailToHead[0];

        int count = _pathTailToHead.Count;

        // 有独立蛇尾：与下方相同的顶点顺序（尾 → … → 路径末格 → 蛇头），仅将首顶点从 _tailDisplayPos 换为 SnakeTrail。
        if (snakeTrail != null)
        {
            int N = count + 1;
            snakeBodyLine.positionCount = N;
            snakeBodyLine.SetPosition(0, LineVertexZ0(snakeTrail.position));
            for (int i = 1; i < count - 1; i++)
                snakeBodyLine.SetPosition(i, _pathTailToHead[i]);
            if (count >= 2)
                snakeBodyLine.SetPosition(count - 1, LineVertexZ0(_pathTailToHead[count - 1]));

            Vector3 headEnd = _boundChess != null ? GetLineHeadWorldPosition() : _pathTailToHead[count - 1];
            snakeBodyLine.SetPosition(count, LineVertexZ0(headEnd));

            UpdateSnakeBodyMaterialBodySize();
            return;
        }

        // 无蛇尾物体：沿用「尾显示点 → … → 路径 → 蛇头 transform」。
        int n = count + 1;
        snakeBodyLine.positionCount = n;
        snakeBodyLine.SetPosition(0, _tailDisplayPos);
        for (int i = 1; i < count - 1; i++)
            snakeBodyLine.SetPosition(i, _pathTailToHead[i]);
        if (count >= 2)
            snakeBodyLine.SetPosition(count - 1, LineVertexZ0(_pathTailToHead[count - 1]));

        Vector3 headEndLegacy = _boundChess != null ? GetLineHeadWorldPosition() : _pathTailToHead[count - 1];
        snakeBodyLine.SetPosition(count, LineVertexZ0(headEndLegacy));

        UpdateSnakeBodyMaterialBodySize();
    }

    void UpdateSnakeTailFollow()
    {
        if (snakeTrail == null || _pathTailToHead.Count == 0 || _tailSliding) return;
        Vector3 target = LineVertexZ0(_pathTailToHead[0]);
        snakeTrail.position = Vector3.MoveTowards(snakeTrail.position, target, snakeTailFollowSpeed * Time.deltaTime);
    }

    /// <summary>蛇尾「朝前」方向：沿脊柱从尾格指向下一格；仅一节时指向蛇头；无有效向量时退回 <see cref="_segmentMoveDirection"/>。</summary>
    Vector2 GetSnakeTailForwardDir()
    {
        int count = _pathTailToHead.Count;
        if (count >= 2)
        {
            Vector2 a = LineVertexZ0(_pathTailToHead[0]);
            Vector2 b = LineVertexZ0(_pathTailToHead[1]);
            Vector2 d = b - a;
            if (d.sqrMagnitude > 1e-6f) return d.normalized;
        }
        else if (count == 1 && _boundChess != null)
        {
            Vector2 a = LineVertexZ0(_pathTailToHead[0]);
            Vector2 b = LineVertexZ0(GetLineHeadWorldPosition());
            Vector2 d = b - a;
            if (d.sqrMagnitude > 1e-6f) return d.normalized;
        }

        Vector2 fallback = new Vector2(_segmentMoveDirection.x, _segmentMoveDirection.y);
        return fallback.sqrMagnitude > 1e-6f ? fallback.normalized : Vector2.right;
    }

    void UpdateSnakeTrailRotation()
    {
        if (snakeTrail == null || _pathTailToHead.Count == 0) return;

        Vector2 dir;
        if (_tailSliding)
        {
            Vector2 slide = LineVertexZ0(_tailSlideTo) - LineVertexZ0(_tailSlideFrom);
            dir = slide.sqrMagnitude > 1e-8f ? slide.normalized : GetSnakeTailForwardDir();
        }
        else
        {
            dir = GetSnakeTailForwardDir();
        }

        if (dir.sqrMagnitude < 1e-6f) return;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + snakeTailRotationOffsetDegrees;
        snakeTrail.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    static Vector3 LineVertexZ0(Vector3 p) => new Vector3(p.x, p.y, 0f);

    /// <summary>线段末端世界坐标：优先 <see cref="lineHeadAnchor"/>，否则蛇头根节点。</summary>
    Vector3 GetLineHeadWorldPosition()
    {
        if (_boundChess == null) return Vector3.zero;
        if (lineHeadAnchor != null)
            return lineHeadAnchor.position;
        return _boundChess.transform.position;
    }

    /// <summary>按当前折线长度设置材质 <c>_BodySize</c>（Vector2），控制 Shader 里 Tiling 平铺。</summary>
    void UpdateSnakeBodyMaterialBodySize()
    {
        if (snakeBodyLine == null) return;
        Material mat = snakeBodyLine.material;
        if (mat == null) return;

        string prop = string.IsNullOrEmpty(bodySizeShaderProperty) ? "_BodySize" : bodySizeShaderProperty;
        int id = Shader.PropertyToID(prop);
        if (!mat.HasProperty(id)) return;

        float len = 0f;
        int pc = snakeBodyLine.positionCount;
        if (pc >= 2)
        {
            for (int i = 0; i < pc - 1; i++)
                len += Vector3.Distance(snakeBodyLine.GetPosition(i), snakeBodyLine.GetPosition(i + 1));
        }

        float x = Mathf.Max(0.01f, len * bodyTilingAlongPerWorldUnit);
        mat.SetVector(id, new Vector2(x, bodySizeY));

        TrySetShaderTierNormalizedBoundaries(mat, len);
    }

    /// <summary>
    /// 沿「尾→头」路径折线，按节数划分银/金/钻段，得到银|金、金|钻分界的世界长度，再除以 <paramref name="lineWorldLength"/> 得到与 LineRenderer UV.x 同域的 0~1 阈值。
    /// </summary>
    void TrySetShaderTierNormalizedBoundaries(Material mat, float lineWorldLength)
    {
        if (mat == null || lineWorldLength <= 1e-5f) return;

        ComputeTierWorldEndDistancesFromPath(out float silverEndWorld, out float goldEndWorld);

        // Shader 一般为：t < sn 银，sn ≤ t < gn 金，t ≥ gn 钻。金=0 时 goldEnd==silverEnd → sn==gn，中间无金带，
        // 若仍用「t > gn」当钻，则 t > sn 整段都会变成钻；且拐角细分会让 Line 总长 > 格心折线，sn/gn 被低估，尾部也会误成钻。
        float sn = Mathf.Clamp01(silverEndWorld / lineWorldLength);
        float gn = Mathf.Clamp01(goldEndWorld / lineWorldLength);
        if (_segmentCountDiamond == 0)
            gn = 1f;
        if (_segmentCountGold == 0 && _segmentCountDiamond == 0)
            sn = 1f;

        if (!string.IsNullOrEmpty(shaderTierSilverGoldNormalizedProperty))
        {
            int tid = Shader.PropertyToID(shaderTierSilverGoldNormalizedProperty);
            if (mat.HasProperty(tid))
                mat.SetFloat(tid, sn);
        }

        if (!string.IsNullOrEmpty(shaderTierGoldDiamondNormalizedProperty))
        {
            int tid = Shader.PropertyToID(shaderTierGoldDiamondNormalizedProperty);
            if (mat.HasProperty(tid))
                mat.SetFloat(tid, gn);
        }
    }

    /// <summary>
    /// 路径尾→头：格 0..S-1 银、S..S+G-1 金、余钻。边 i 连接 path[i]→path[i+1]；
    /// 银|金分界弧长 = 前 S 条边之和；金|钻 = 前 S+G 条边之和（与 <see cref="_segmentCountSilver"/> / Gold / Diamond 一致）。
    /// </summary>
    void ComputeTierWorldEndDistancesFromPath(out float silverEndWorld, out float goldEndWorld)
    {
        silverEndWorld = 0f;
        goldEndWorld = 0f;
        int n = _pathTailToHead.Count;
        if (n < 2)
            return;

        int s = _segmentCountSilver;
        int g = _segmentCountGold;

        for (int i = 0; i < s && i < n - 1; i++)
            silverEndWorld += Vector3.Distance(LineVertexZ0(_pathTailToHead[i]), LineVertexZ0(_pathTailToHead[i + 1]));

        goldEndWorld = silverEndWorld;
        for (int i = s; i < s + g && i < n - 1; i++)
            goldEndWorld += Vector3.Distance(LineVertexZ0(_pathTailToHead[i]), LineVertexZ0(_pathTailToHead[i + 1]));
    }

    void UpdateTailSlide()
    {
        if (!_tailSliding || _pathTailToHead.Count == 0 || snakeBodyLine == null) return;

        _tailSlideProgress += Time.deltaTime / Mathf.Max(0.02f, tailSlideDuration);
        float t = Mathf.Clamp01(_tailSlideProgress);
        float s = t * t * (3f - 2f * t);
        if (snakeTrail != null)
        {
            snakeTrail.position = Vector3.Lerp(_tailSlideFrom, _tailSlideTo, s);
        }
        else
        {
            _tailDisplayPos = Vector3.Lerp(_tailSlideFrom, _tailSlideTo, s);
        }

        if (t >= 1f)
        {
            _tailSliding = false;
            if (snakeTrail != null)
                snakeTrail.position = LineVertexZ0(_pathTailToHead[0]);
            else
                _tailDisplayPos = _pathTailToHead[0];
        }

        ApplyLineRenderer();
    }

    void UpdateLineHeadToTransform(Chess c)
    {
        if (snakeBodyLine == null || c == null || _pathTailToHead.Count == 0) return;
        if (snakeTrail != null)
            snakeBodyLine.SetPosition(0, LineVertexZ0(snakeTrail.position));

        int last = snakeBodyLine.positionCount - 1;
        if (last >= 0)
            snakeBodyLine.SetPosition(last, LineVertexZ0(GetLineHeadWorldPosition()));

        UpdateSnakeBodyMaterialBodySize();
    }

    public override Tile FindNextTile(Chess c)
    {
        if (c == null || c.moveController == null)
            return null;

        Tile stand = c.moveController.standTile;
        if (stand == null)
            return null;

        PollKeyboardToQueue();
        ApplyQueuedDirection();

        Vector2Int cell = stand.mapPos + _facing;
        MapManage map = MapManage.instance;
        if (map == null || !map.IfInMapRange(cell.x, cell.y))
        {
            NotifyDefeatOnce(SnakeGameEventId.StumbleOutOfBounds);
            return null;
        }

        if (CheckSelfCollision(cell))
        {
            NotifyDefeatOnce(SnakeGameEventId.StumbleSelfBite);
            return null;
        }

        _segmentMoveDirection = cell - stand.mapPos;
        return map.tiles[cell.x, cell.y];
    }

    void ApplyQueuedDirection()
    {
        if (!_queuedFacing.HasValue) return;
        Vector2Int q = _queuedFacing.Value;
        if (q != Vector2Int.zero && q != -_segmentMoveDirection)
            _facing = q;
        _queuedFacing = null;
    }

    void PollKeyboardToQueue()
    {
        Vector2Int? d = null;
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) d = Vector2Int.up;
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) d = Vector2Int.down;
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) d = Vector2Int.left;
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) d = Vector2Int.right;

        if (d.HasValue) TryQueueDirection(d.Value);
    }

    /// <summary>
    /// 与键盘预输入相同规则：写入下一格意图朝向；不可为 <see cref="Vector2Int.zero"/>，且不可与 <see cref="_segmentMoveDirection"/> 相反。
    /// 供手机端 UI 按钮、滑动桥接 <see cref="SnakeMobileInputBridge"/> 等调用。
    /// </summary>
    public void TryQueueDirection(Vector2Int direction)
    {
        if (direction == Vector2Int.zero) return;
        if (direction == -_segmentMoveDirection) return;
        _queuedFacing = direction;
    }

    void NotifyDefeatOnce(string stumbleKind)
    {
        if (_defeatNotified) return;
        _defeatNotified = true;
        if (!string.IsNullOrEmpty(stumbleKind))
            EventController.Instance.TriggerEvent(EventName.SnakeHitWall.ToString(), stumbleKind);
        GameManage.instance.timerManage.AddTimer(() => _defeatNotified = false, 5.5f);
        LevelController_Snake level = UnityEngine.Object.FindObjectOfType<LevelController_Snake>();
        level?.NotifySnakeDefeat(_boundChess);
    }
}
