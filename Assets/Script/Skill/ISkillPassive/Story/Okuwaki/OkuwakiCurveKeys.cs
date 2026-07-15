using System.Collections.Generic;
using UnityEngine;

/// <summary>老仓育公式弹道：SkillContext 键与格子/压力工具。</summary>
public static class OkuwakiCurveKeys
{
    public const string AmplitudeRow = "okuwaki_A";
    public const string AmplitudeCol = "okuwaki_B";
    public const string RatioA = "okuwaki_a";
    public const string RatioB = "okuwaki_b";

    public const int RatioDenominator = 30;
    public const int MinStress = 30;
    public const int DefaultMaxAmplitude = 9;
    public const int DefaultRatioMax = 9;
}

public static class OkuwakiCurveState
{
    public static void ApplyInitial(SkillContext context)
    {
        if (context == null) return;
        context.Set(OkuwakiCurveKeys.AmplitudeRow, 1);
        context.Set(OkuwakiCurveKeys.AmplitudeCol, 1);
        context.Set(OkuwakiCurveKeys.RatioA, 1);
        context.Set(OkuwakiCurveKeys.RatioB, 1);
    }

    public static void RefreshFromSkill(Chess user, int maxAmplitude = OkuwakiCurveKeys.DefaultMaxAmplitude, int ratioMax = OkuwakiCurveKeys.DefaultRatioMax)
    {
        if (user?.skillController?.context == null) return;
        var ctx = user.skillController.context;
        int row = OkuwakiGridHelper.CountAlliesInRow(user, maxAmplitude);
        int col = OkuwakiGridHelper.CountAlliesInColumn(user, maxAmplitude);
        ctx.Set(OkuwakiCurveKeys.AmplitudeRow, row);
        ctx.Set(OkuwakiCurveKeys.AmplitudeCol, col);

        user.skillController.context.TryGet<int>("stress", out int stress);
        OkuwakiGridHelper.StressToRatio(stress, OkuwakiCurveKeys.RatioDenominator, out int a, out int b, ratioMax);
        ctx.Set(OkuwakiCurveKeys.RatioA, a);
        ctx.Set(OkuwakiCurveKeys.RatioB, b);
    }
}

public static class OkuwakiGridHelper
{
    static readonly List<Tile> TileBuffer = new List<Tile>(8);

    public static int CountAlliesInRow(Chess user, int maxCount = OkuwakiCurveKeys.DefaultMaxAmplitude)
    {
        return CountAlliesOnLine(user, horizontal: true, maxCount);
    }

    public static int CountAlliesInColumn(Chess user, int maxCount = OkuwakiCurveKeys.DefaultMaxAmplitude)
    {
        return CountAlliesOnLine(user, horizontal: false, maxCount);
    }

    static int CountAlliesOnLine(Chess user, bool horizontal, int maxCount)
    {
        var stand = user?.moveController?.standTile;
        var map = MapManage.instance;
        if (stand == null || map == null) return 1;

        int count = 0;
        if (horizontal)
        {
            int y = stand.mapPos.y;
            for (int x = 0; x < map.mapSize.x; x++)
            {
                if (!map.IfInMapRange(x, y)) continue;
                count += CountLivingAlliesOnTile(map.tiles[x, y], user.tag);
            }
        }
        else
        {
            int x = stand.mapPos.x;
            for (int y = 0; y < map.mapSize.y; y++)
            {
                if (!map.IfInMapRange(x, y)) continue;
                count += CountLivingAlliesOnTile(map.tiles[x, y], user.tag);
            }
        }

        return Mathf.Clamp(Mathf.Max(1, count), 1, Mathf.Max(1, maxCount));
    }

    static int CountLivingAlliesOnTile(Tile tile, string allyTag)
    {
        if (tile?.chessesIntile == null) return 0;
        int n = 0;
        for (int i = 0; i < tile.chessesIntile.Count; i++)
        {
            var c = tile.chessesIntile[i];
            if (c == null || c.IfDeath || !c.CompareTag(allyTag)) continue;
            n++;
        }
        return n;
    }

    public static void CollectNeighbor8Tiles(Tile center, List<Tile> buffer)
    {
        buffer.Clear();
        if (center == null || MapManage.instance == null) return;

        var p = center.mapPos;
        var map = MapManage.instance;
        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0) continue;
                int x = p.x + dx;
                int y = p.y + dy;
                if (!map.IfInMapRange(x, y)) continue;
                var t = map.tiles[x, y];
                if (t != null) buffer.Add(t);
            }
        }
    }

    public static void StressToRatio(int stress, int denominator, out int a, out int b, int ratioMax = OkuwakiCurveKeys.DefaultRatioMax)
    {
        int safeStress = Mathf.Max(1, stress);
        int safeDen = Mathf.Max(1, denominator);
        int g = Gcd(safeStress, safeDen);
        a = Mathf.Clamp(safeStress / g, 1, ratioMax);
        b = Mathf.Clamp(safeDen / g, 1, ratioMax);
    }

    public static int Gcd(int x, int y)
    {
        x = Mathf.Abs(x);
        y = Mathf.Abs(y);
        while (y != 0)
        {
            int t = x % y;
            x = y;
            y = t;
        }
        return Mathf.Max(1, x);
    }

    public static Vector2 GetAnchorWorldPos(Chess user)
    {
        if (user?.moveController?.standTile != null)
            return user.moveController.standTile.transform.position;
        return user != null ? (Vector2)user.transform.position : Vector2.zero;
    }
}

public static class OkuwakiStressSpread
{
    static bool _applying;
    static readonly List<Tile> TileBuffer = new List<Tile>(8);

    public static void ApplyDelta(Chess source, int delta, Buff_StressBuff_Death guestTemplate)
    {
        if (_applying || delta <= 0 || source == null) return;
        var stand = source.moveController?.standTile;
        if (stand == null) return;

        _applying = true;
        try
        {
            OkuwakiGridHelper.CollectNeighbor8Tiles(stand, TileBuffer);
            for (int ti = 0; ti < TileBuffer.Count; ti++)
            {
                var tile = TileBuffer[ti];
                if (tile?.chessesIntile == null) continue;
                for (int ci = 0; ci < tile.chessesIntile.Count; ci++)
                {
                    var chess = tile.chessesIntile[ci];
                    if (chess == null || chess.IfDeath || chess == source) continue;
                    ApplyStressDelta(chess, delta, guestTemplate);
                }
            }
        }
        finally
        {
            _applying = false;
        }
    }

    public static void ApplyStressDelta(Chess chess, int delta, Buff_StressBuff_Death guestTemplate)
    {
        if (delta <= 0 || chess?.skillController?.context == null) return;

        EnsureStressBuffIfMissing(chess, guestTemplate);
        if (chess.buffController?.buffDic == null
            || !chess.buffController.buffDic.TryGetValue("压力", out Buff buff)
            || buff is not Buff_StressBuff_Death stressBuff)
            return;

        stressBuff.BuffReset(new Buff_StressBuff_Death { extraStress = delta });
    }

    static void EnsureStressBuffIfMissing(Chess target, Buff_StressBuff_Death guestTemplate)
    {
        if (target?.buffController == null || target.buffController.buffDic.ContainsKey("压力"))
            return;

        Buff_StressBuff_Death template = guestTemplate != null
            ? (Buff_StressBuff_Death)guestTemplate.Clone()
            : new Buff_StressBuff_Death();
        target.buffController.AddBuff(template);
    }
}
