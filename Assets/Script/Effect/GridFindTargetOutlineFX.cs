using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 挂在 <see cref="Chess"/> 上，按当前 <see cref="Weapon_Sample"/> 的 <see cref="IGridFindTarget"/> 占用格生成描边 FX。
/// 预制：<c>Resources/Effect/OutLineFX</c>，Animator Float 参数 <c>Blend</c>：左上0、上1、右上2、左3、右4、左下5、下6、右下7（左下/右下当前规则不生成，预留编号）。
/// 刷新：<see cref="RebuildOutline"/>；<see cref="createAwake"/> 为 true 时在 <see cref="Start"/> 生成。死亡时 <see cref="Chess.OnRemove"/> 自动回收实例。
/// 颜色：<see cref="outlineColor"/> 作用于生成实例上的 <see cref="SpriteRenderer"/> / <see cref="LineRenderer"/>。
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Chess))]
public class GridFindTargetOutlineFX : MonoBehaviour
{
    const string ResourcePath = "Effect/OutLineFX";
    const string BlendParamName = "Blend";

    /// <summary>左上0、上1、右上2、左3、右4、左下5、下6、右下7</summary>
    public enum OutlineBlend : int
    {
        UpperLeft = 0,
        Up = 1,
        UpperRight = 2,
        Left = 3,
        Right = 4,
        LowerLeft = 5,
        Down = 6,
        LowerRight = 7
    }

    [Tooltip("为 true 时 Start 自动 Rebuild；否则仅在外部调用 RebuildOutline 时生成。")]
    public bool createAwake = true;

    [Tooltip("描边实例的颜色（会写入子物体 SpriteRenderer / LineRenderer；改色后需 RebuildOutline 才影响已生成的 FX）。")]
    public Color outlineColor = Color.white;

    Chess _chess;
    GameObject _prefab;
    readonly List<GameObject> _instances = new List<GameObject>(32);

    void Awake()
    {
        _chess = GetComponent<Chess>();
    }

    void Start()
    {
        if (createAwake)
            RebuildOutline();
    }

    void OnEnable()
    {
        if (_chess != null)
            _chess.OnRemove.AddListener(OnChessRemove);
    }

    void OnDisable()
    {
        if (_chess != null)
            _chess.OnRemove.RemoveListener(OnChessRemove);
    }

    void OnChessRemove(Chess _)
    {
        ClearOutlineInstances();
    }

    /// <summary>清除并重新根据当前 Weapon 的 IGridFindTarget 生成描边 FX。</summary>
    public void RebuildOutline()
    {
        ClearOutlineInstances();

        if (_chess == null || _chess.IfDeath)
            return;

        if (MapManage.instance == null)
            return;

        if (!(_chess.equipWeapon != null && _chess.equipWeapon.weapon is Weapon_Sample ws))
            return;

        if (!(ws.findTarget is IGridFindTarget grid) || grid.relativeCells == null)
            return;

        MapManage map = MapManage.instance;
        if (!GridFindTargetGeometry.TryGetBaseMapPos(_chess, map, out Vector2Int basePos))
            return;

        int forwardX = GridFindTargetGeometry.GetForwardX(_chess);
        Vector2 tileSize = map.tileSize;

        var occupied = new HashSet<Vector2Int>();
        foreach (Vector2Int rel in grid.relativeCells)
        {
            int ax = basePos.x + rel.x * forwardX;
            int ay = basePos.y + rel.y;
            if (!GridFindTargetGeometry.IsDetectableCell(ax, ay, map.mapSize))
                continue;
            if (map.tiles[ax, ay] == null)
                continue;
            occupied.Add(new Vector2Int(ax, ay));
        }

        if (occupied.Count == 0)
            return;

        if (_prefab == null)
        {
            _prefab = Resources.Load<GameObject>(ResourcePath);
            if (_prefab == null)
            {
                Debug.LogWarning($"GridFindTargetOutlineFX: 未找到 Resources/{ResourcePath}.prefab", this);
                return;
            }
        }

        foreach (Vector2Int cell in occupied)
        {
            bool upOut = EdgeExposedToward(cell, 0, 1, occupied, map);
            bool downOut = EdgeExposedToward(cell, 0, -1, occupied, map);
            bool leftOut = EdgeExposedToward(cell, -1, 0, occupied, map);
            bool rightOut = EdgeExposedToward(cell, 1, 0, occupied, map);

            bool ul = upOut && leftOut;
            bool ur = upOut && rightOut;

            if (ul)
                SpawnFx(cell, OutlineBlend.UpperLeft, map, tileSize);
            if (ur)
                SpawnFx(cell, OutlineBlend.UpperRight, map, tileSize);

            if (upOut && !ul && !ur)
                SpawnFx(cell, OutlineBlend.Up, map, tileSize);

            if (downOut)
                SpawnFx(cell, OutlineBlend.Down, map, tileSize);

            if (leftOut && !ul)
                SpawnFx(cell, OutlineBlend.Left, map, tileSize);

            if (rightOut && !ur)
                SpawnFx(cell, OutlineBlend.Right, map, tileSize);
        }
    }

    /// <summary>仅销毁全部已生成的描边实例（不立即重建）。</summary>
    public void ClearOutlineInstances()
    {
        for (int i = 0; i < _instances.Count; i++)
        {
            if (_instances[i] != null)
                Destroy(_instances[i]);
        }
        _instances.Clear();
    }

    static bool EdgeExposedToward(
        Vector2Int cell,
        int dax,
        int day,
        HashSet<Vector2Int> occupied,
        MapManage map)
    {
        int nx = cell.x + dax;
        int ny = cell.y + day;

        if (!map.IfInMapRange(nx, ny))
            return false;

        if (!GridFindTargetGeometry.IsDetectableCell(nx, ny, map.mapSize))
            return false;

        return !occupied.Contains(new Vector2Int(nx, ny));
    }

    void SpawnFx(Vector2Int cell, OutlineBlend blend, MapManage map, Vector2 tileSize)
    {
        Tile tile = map.tiles[cell.x, cell.y];
        if (tile == null)
            return;

        Vector2 center = GridFindTargetGeometry.GetCellOverlapCenter(tile, tileSize);
        var go = Instantiate(_prefab, new Vector3(center.x, center.y, transform.position.z), Quaternion.identity, transform);
        _instances.Add(go);

        var animators = go.GetComponentsInChildren<Animator>(true);
        float v = (float)(int)blend;
        for (int i = 0; i < animators.Length; i++)
        {
            if (animators[i] != null)
                animators[i].SetFloat(BlendParamName, v);
        }

        ApplyOutlineColorToInstance(go);
    }

    void ApplyOutlineColorToInstance(GameObject root)
    {
        var sprites = root.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i] != null)
                sprites[i].color = outlineColor;
        }

        var lines = root.GetComponentsInChildren<LineRenderer>(true);
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i] == null)
                continue;
            lines[i].startColor = outlineColor;
            lines[i].endColor = outlineColor;
        }
    }
}
