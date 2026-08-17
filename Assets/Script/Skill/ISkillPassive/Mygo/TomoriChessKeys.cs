using System.Collections.Generic;
using UnityEngine;

/// <summary>灯（高松灯）棋子模式：SkillContext / 静态标记键。</summary>
public static class TomoriChessKeys
{
    public const string ChessMode = "tomoriChessMode";
    public const string AtkSnapshot = "tomoriAtkSnapshot";
    public const string SpawnedPieces = "tomoriSpawnedPieces";
    public const string PieceDamageAtk = "tomoriPieceDamageAtk";

    static int _extraDeployDepth;

    public static bool IsExtraDeploy => _extraDeployDepth > 0;

    public static void BeginExtraDeploy() => _extraDeployDepth++;

    public static void EndExtraDeploy()
    {
        if (_extraDeployDepth > 0)
            _extraDeployDepth--;
    }

    public static string ResolveMemberId(PropertyCreator creator)
    {
        if (creator == null)
            return string.Empty;
        if (!string.IsNullOrEmpty(creator.fetterMemberId))
            return creator.fetterMemberId;
        string name = creator.chessName ?? string.Empty;
        if (name.Contains("高松灯"))
            return "高松灯";
        return name;
    }

    /// <summary>场上带 Mygo tag 的单位数量（不去重；含灯自身）。</summary>
    public static int CountMygoUnitsOnField()
    {
        if (ChessTeamManage.Instance == null)
            return 0;
        List<Chess> team = ChessTeamManage.Instance.GetTeam("Player");
        if (team == null)
            return 0;
        int n = 0;
        for (int i = 0; i < team.Count; i++)
        {
            Chess c = team[i];
            if (c == null || c.IfDeath)
                continue;
            PropertyCreator creator = c.propertyController?.creator;
            if (creator?.plantTags == null || !creator.plantTags.Contains("Mygo"))
                continue;
            n++;
        }
        return n;
    }

    /// <summary>兼容旧名：现为 MyGO 单位总数（不去重）。</summary>
    public static int CountUniqueMygoMembersOnField() => CountMygoUnitsOnField();

    public static int CountFieldPieces(PropertyCreator pieceCreator)
    {
        if (pieceCreator == null || ChessTeamManage.Instance == null)
            return 0;
        int n = 0;
        List<Chess> team = ChessTeamManage.Instance.GetTeam("Player");
        if (team == null)
            return 0;
        for (int i = 0; i < team.Count; i++)
        {
            Chess c = team[i];
            if (c == null || c.IfDeath)
                continue;
            if (c.propertyController?.creator == pieceCreator)
                n++;
        }
        return n;
    }

    public static List<Chess> GetOrCreatePieceList(Chess lamp)
    {
        if (lamp?.skillController?.context == null)
            return null;
        if (!lamp.skillController.context.TryGet(SpawnedPieces, out List<Chess> list) || list == null)
        {
            list = new List<Chess>();
            lamp.skillController.context.Set(SpawnedPieces, list);
        }
        return list;
    }

    public static ShopIcon FindShopIconByGood(PropertyCreator good)
    {
        if (good == null)
            return null;
        PlantsShop shop = UIManage.GetView<PlantsShop>();
        if (shop?.currentShopIcons == null)
            return null;
        for (int i = 0; i < shop.currentShopIcons.Count; i++)
        {
            ShopIcon icon = shop.currentShopIcons[i];
            if (icon != null && icon.good == good)
                return icon;
        }
        return null;
    }

    public static void CollectFourNeighborTiles(Tile center, List<Tile> buffer)
    {
        buffer.Clear();
        if (center == null || MapManage.instance == null)
            return;
        int[] dx = { 0, 0, 1, -1 };
        int[] dy = { 1, -1, 0, 0 };
        for (int i = 0; i < 4; i++)
        {
            int x = center.mapPos.x + dx[i];
            int y = center.mapPos.y + dy[i];
            if (!MapManage.instance.IfInMapRange(x, y))
                continue;
            buffer.Add(MapManage.instance.tiles[x, y]);
        }
    }
}
