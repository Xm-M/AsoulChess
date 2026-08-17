using System.Collections.Generic;
using UnityEngine;

/// <summary>多首的怪物：SkillContext 键与行序 / 行世界坐标工具。</summary>
public static class MultiHeadMutsumiKeys
{
    public const string CloneCount = "MultiHeadCloneCount";
    public const string CloneHeads = "MultiHeadCloneHeads";
    public const string StressThreshold = "stress";
    /// <summary>主动技终局锁：不再出头、不再降低 stressLimit。</summary>
    public const string FinaleLocked = "MultiHeadFinaleLocked";
    public const string GhostList = "MultiHeadGhosts";
    public const string StressTickTimer = "MultiHeadStressTickTimer";
    public const string GhostMaster = "MultiHeadGhostMaster";

    public const int DefaultMaxClones = 20;
    public const int DefaultStressPerClone = 50;
    /// <summary>入场压力死亡阈值；每分身 -5 → 20 头后为 45，再涨压紫砂。</summary>
    public const int DefaultInitialStressLimit = 145;
    public const int DefaultStressLimitReducePerClone = 5;
    /// <summary>分身槽位数：n=(initialLimit-本值)/每次减 limit；默认 (145-45)/5=20。</summary>
    public const int DefaultHeadSlotLimitFloor = 45;

    /// <summary>
    /// 运行时幽灵表（按主人 InstanceID）。
    /// 主人 Death 时 <see cref="SkillController.WhenControllerLeaveWar"/> 会先 <c>context.Clear()</c>，
    /// 再触发 OnRemove；仅靠 SkillContext 存列表会导致清幽灵失败。
    /// </summary>
    static readonly Dictionary<int, List<Chess>> RuntimeGhostLists = new Dictionary<int, List<Chess>>();

    /// <summary>同理：加压 Timer 在 context.Clear 后仍须能 Stop。</summary>
    static readonly Dictionary<int, Timer> RuntimeStressTimers = new Dictionary<int, Timer>();

    public static bool IsFinaleLocked(Chess user)
    {
        return user?.skillController?.context != null
               && user.skillController.context.TryGet(FinaleLocked, out bool locked)
               && locked;
    }

    public static void SetFinaleLocked(Chess user, bool locked)
    {
        user?.skillController?.context?.Set(FinaleLocked, locked);
    }

    public static List<Chess> GetOrCreateGhostList(Chess user)
    {
        if (user == null)
            return null;
        int id = user.GetInstanceID();
        if (!RuntimeGhostLists.TryGetValue(id, out List<Chess> list) || list == null)
        {
            list = new List<Chess>(8);
            RuntimeGhostLists[id] = list;
        }
        // 存活期间仍写入 context，便于其他逻辑 TryGet；Clear 后以 Runtime 为准
        user.skillController?.context?.Set(GhostList, list);
        return list;
    }

    /// <summary>只读取，不创建；用于幽灵自身死亡时从主人表摘除。</summary>
    public static bool TryGetGhostList(Chess user, out List<Chess> list)
    {
        list = null;
        if (user == null)
            return false;
        if (RuntimeGhostLists.TryGetValue(user.GetInstanceID(), out list) && list != null)
            return true;
        return user.skillController?.context != null
               && user.skillController.context.TryGet(GhostList, out list)
               && list != null;
    }

    public static void ClearAllHeadVisuals(Chess user)
    {
        List<Transform> heads = GetOrCreateHeadList(user);
        if (heads != null)
        {
            for (int i = heads.Count - 1; i >= 0; i--)
            {
                if (heads[i] != null)
                    Object.Destroy(heads[i].gameObject);
            }
            heads.Clear();
        }
        SetCloneCount(user, 0);
    }

    public static void DestroyAllGhosts(Chess user)
    {
        if (user == null)
            return;
        int id = user.GetInstanceID();
        List<Chess> list = null;
        if (RuntimeGhostLists.TryGetValue(id, out list))
            RuntimeGhostLists.Remove(id);
        else if (user.skillController?.context != null)
            user.skillController.context.TryGet(GhostList, out list);

        if (list == null)
            return;
        Chess[] snapshot = list.ToArray();
        list.Clear();
        for (int i = 0; i < snapshot.Length; i++)
        {
            Chess g = snapshot[i];
            if (g != null && !g.IfDeath)
                g.Death();
        }
    }

    public static void RegisterStressTickTimer(Chess user, Timer timer)
    {
        if (user == null)
            return;
        int id = user.GetInstanceID();
        if (RuntimeStressTimers.TryGetValue(id, out Timer old) && old != null && old != timer)
            old.Stop();
        if (timer != null)
            RuntimeStressTimers[id] = timer;
        else
            RuntimeStressTimers.Remove(id);
        user.skillController?.context?.Set(StressTickTimer, timer);
    }

    public static void StopStressTickTimer(Chess user)
    {
        if (user == null)
            return;
        int id = user.GetInstanceID();
        Timer timer = null;
        if (RuntimeStressTimers.TryGetValue(id, out timer))
            RuntimeStressTimers.Remove(id);
        else if (user.skillController?.context != null)
            user.skillController.context.TryGet(StressTickTimer, out timer);

        if (timer != null)
            timer.Stop();
        user.skillController?.context?.Remove(StressTickTimer);
    }

    /// <summary>
    /// 本行 → 邻行 → 更远行（先 home-d 再 home+d），越界跳过。
    /// </summary>
    public static void BuildRowOrder(int homeY, int mapHeight, List<int> into)
    {
        into.Clear();
        if (mapHeight <= 0) return;
        if (homeY >= 0 && homeY < mapHeight)
            into.Add(homeY);
        for (int d = 1; d < mapHeight; d++)
        {
            int down = homeY - d;
            int up = homeY + d;
            if (down >= 0 && down < mapHeight)
                into.Add(down);
            if (up >= 0 && up < mapHeight)
                into.Add(up);
        }
    }

    public static int GetAssignedRow(int homeY, int mapHeight, int cloneIndex, List<int> scratch)
    {
        BuildRowOrder(homeY, mapHeight, scratch);
        if (scratch.Count == 0) return homeY;
        int i = cloneIndex % scratch.Count;
        if (i < 0) i += scratch.Count;
        return scratch[i];
    }

    public static bool TryGetHomeRow(Chess user, out int homeY)
    {
        homeY = 0;
        MapManage map = MapManage.instance;
        if (map == null || user == null)
            return false;
        if (!GridFindTargetGeometry.TryGetBaseMapPos(user, map, out Vector2Int basePos))
            return false;
        homeY = basePos.y;
        return true;
    }

    public static bool TryGetRowWorldY(Chess user, int rowY, out float worldY)
    {
        worldY = 0f;
        MapManage map = MapManage.instance;
        if (map == null || user == null)
            return false;
        if (!GridFindTargetGeometry.TryGetBaseMapPos(user, map, out Vector2Int basePos))
            return false;
        if (!GridFindTargetGeometry.TryResolveTileAt(basePos.x, rowY, map, out Tile tile) || tile == null)
            return false;
        worldY = GridFindTargetGeometry.GetCellOverlapCenter(tile, map.tileSize).y;
        return true;
    }

    public static List<Transform> GetOrCreateHeadList(Chess user)
    {
        if (user?.skillController?.context == null)
            return null;
        if (!user.skillController.context.TryGet(CloneHeads, out List<Transform> list) || list == null)
        {
            list = new List<Transform>(8);
            user.skillController.context.Set(CloneHeads, list);
        }
        return list;
    }

    public static int GetCloneCount(Chess user)
    {
        if (user?.skillController?.context == null)
            return 0;
        return user.skillController.context.TryGet(CloneCount, out int n) ? Mathf.Max(0, n) : 0;
    }

    public static void SetCloneCount(Chess user, int n)
    {
        user?.skillController?.context?.Set(CloneCount, Mathf.Max(0, n));
    }

    /// <summary>主人 SkillContext 中的当前压力值（&lt;see cref="StressThreshold"/&gt;）；无则 0。</summary>
    public static int GetCurrentStress(Chess user)
    {
        if (user?.skillController?.context == null)
            return 0;
        return user.skillController.context.TryGet(StressThreshold, out int stress)
            ? Mathf.Max(0, stress)
            : 0;
    }
}
