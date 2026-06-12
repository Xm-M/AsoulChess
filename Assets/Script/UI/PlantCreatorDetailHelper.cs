using System.Collections.Generic;
using System.Text;

/// <summary>
/// 植物 PropertyCreator 详情文本构建，供图鉴与商店详情面板复用。
/// </summary>
public static class PlantCreatorDetailHelper
{
    public static readonly Dictionary<PlantType, string> PlantTypeNames = new Dictionary<PlantType, string>
    {
        { PlantType.MainPlant, "主力" },
        { PlantType.SupportPlant, "辅助" },
        { PlantType.PotPlant, "地形" },
        { PlantType.Consume, "消耗品" },
        { PlantType.LimitType, "周期限制" },
    };

    public static string GetPlantTypeName(PlantType plantType)
    {
        return PlantTypeNames.TryGetValue(plantType, out string name) ? name : plantType.ToString();
    }

    public static string BuildAttributeText(Property prop)
    {
        if (prop == null) return "";
        return $"生命值: {(int)prop.HpMax}        攻击力: {prop.attack}\n" +
               $"护甲: {prop.AR}            价格: {prop.price}\n" +
               $"冷却: {prop.CD}s           攻击距离: {prop.attackRange}\n" +
               $"移速: {prop.speed}";
    }

    public static string BuildDescriptionText(PropertyCreator creator)
    {
        if (creator == null) return "";
        var sb = new StringBuilder();
        if (!string.IsNullOrEmpty(creator.chessDescription))
            sb.AppendLine(creator.chessDescription);
        if (!string.IsNullOrEmpty(creator.chessEffect))
        {
            if (sb.Length > 0) sb.AppendLine();
            sb.AppendLine("【效果】");
            sb.AppendLine(creator.chessEffect);
        }
        if (!string.IsNullOrEmpty(creator.chessShortDescription))
        {
            if (sb.Length > 0) sb.AppendLine();
            sb.AppendLine("【简介】");
            sb.AppendLine(creator.chessShortDescription);
        }
        return sb.ToString();
    }

    public static string BuildTagsText(PropertyCreator creator)
    {
        if (creator?.plantTags == null || creator.plantTags.Count == 0)
            return "无";
        return string.Join("  ", creator.plantTags);
    }
}
