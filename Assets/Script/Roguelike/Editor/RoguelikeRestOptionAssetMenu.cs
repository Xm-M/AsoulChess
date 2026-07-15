#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>一键生成默认休息房选项资产（休息 + 扩容）。</summary>
public static class RoguelikeRestOptionAssetMenu
{
    const string Folder = "Assets/SO/Rogue/Rest";

    [MenuItem("Roguelike/Create Default Rest Option Catalog")]
    public static void CreateDefaultCatalog()
    {
        if (!AssetDatabase.IsValidFolder("Assets/SO/Rogue"))
            AssetDatabase.CreateFolder("Assets/SO", "Rogue");
        if (!AssetDatabase.IsValidFolder(Folder))
            AssetDatabase.CreateFolder("Assets/SO/Rogue", "Rest");

        var rest = CreateDefinition(
            $"{Folder}/RestOption_LawnMower.asset",
            RoguelikeRestOption.BuiltinRestId,
            "休息",
            RoguelikeRestEffectKind.LawnMowerBonus);

        var expand = CreateDefinition(
            $"{Folder}/RestOption_ExpandLoadout.asset",
            RoguelikeRestOption.BuiltinExpandId,
            "扩容",
            RoguelikeRestEffectKind.LoadoutSlotBonus);

        var catalogPath = $"{Folder}/RoguelikeRestOptionCatalog.asset";
        var catalog = AssetDatabase.LoadAssetAtPath<RoguelikeRestOptionCatalog>(catalogPath);
        if (catalog == null)
        {
            catalog = ScriptableObject.CreateInstance<RoguelikeRestOptionCatalog>();
            AssetDatabase.CreateAsset(catalog, catalogPath);
        }

        catalog.options = new System.Collections.Generic.List<RoguelikeRestOptionDefinition> { rest, expand };
        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            $"[Roguelike] 已生成休息房 Catalog：{catalogPath}。请在 RoguelikeEconomyConfig → 休息房选项卡池 中绑定，并为 RestOption 资产设置配图。");
        Selection.activeObject = catalog;
    }

    static RoguelikeRestOptionDefinition CreateDefinition(
        string path,
        string optionId,
        string title,
        RoguelikeRestEffectKind kind)
    {
        var existing = AssetDatabase.LoadAssetAtPath<RoguelikeRestOptionDefinition>(path);
        if (existing != null)
            return existing;

        var def = ScriptableObject.CreateInstance<RoguelikeRestOptionDefinition>();
        def.optionId = optionId;
        def.title = title;
        def.effectKind = kind;
        def.effectValue = 0;
        AssetDatabase.CreateAsset(def, path);
        return def;
    }
}
#endif
