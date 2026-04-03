#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json.Linq;

/// <summary>
/// 根据 JSON 为图集贴图写入 Multiple Sprite 导入数据。
/// JSON 格式：根为数组，每项 <c>{ "name": "xxx", "rect": { "x", "y", "width", "height" } }</c>，
/// 其中 <c>rect</c> 为 Unity 纹理空间（左下为原点），与 Sprite Editor 一致。
/// </summary>
public static class SpriteSheetJsonImporterUtility
{
    public struct ImportOptions
    {
        public float PixelsPerUnit;
        public FilterMode FilterMode;
        /// <summary>
        /// 子 Sprite 在自身矩形内的归一化 Pivot（Custom 对齐）。
        /// 默认 (1,0)=右下：侧视、面朝左时脚底常在包围盒右下，挥武器时比底中/中心更不易整段平移。
        /// </summary>
        public Vector2 Pivot;

        public static ImportOptions Default => new ImportOptions
        {
            PixelsPerUnit = 16f,
            FilterMode = FilterMode.Point,
            Pivot = new Vector2(1f, 0f)
        };
    }

    /// <summary>
    /// <paramref name="textureAssetPath"/> 须为 Project 内贴图路径（如 Assets/.../a.png）。
    /// </summary>
    public static bool Apply(string textureAssetPath, string jsonContent, ImportOptions options, out string errorMessage)
    {
        errorMessage = null;
        if (string.IsNullOrEmpty(textureAssetPath))
        {
            errorMessage = "贴图路径为空。";
            return false;
        }

        var ti = AssetImporter.GetAtPath(textureAssetPath) as TextureImporter;
        if (ti == null)
        {
            errorMessage = "无法作为 TextureImporter 打开: " + textureAssetPath;
            return false;
        }

        JArray arr;
        try
        {
            arr = JArray.Parse(jsonContent);
        }
        catch (System.Exception e)
        {
            errorMessage = "JSON 解析失败: " + e.Message;
            return false;
        }

        if (arr == null || arr.Count == 0)
        {
            errorMessage = "JSON 中没有任何 Sprite 条目。";
            return false;
        }

        var metas = new SpriteMetaData[arr.Count];
        for (int i = 0; i < arr.Count; i++)
        {
            var item = arr[i];
            if (item["name"] == null || item["rect"] == null)
            {
                errorMessage = $"第 {i} 项缺少 name 或 rect。";
                return false;
            }

            var r = item["rect"];
            int x = r["x"].Value<int>();
            int y = r["y"].Value<int>();
            int w = r["width"].Value<int>();
            int h = r["height"].Value<int>();

            metas[i] = new SpriteMetaData
            {
                name = item["name"].Value<string>(),
                rect = new Rect(x, y, w, h),
                alignment = (int)SpriteAlignment.Custom,
                pivot = options.Pivot,
                border = Vector4.zero
            };
        }

        ti.spriteImportMode = SpriteImportMode.Multiple;
        ti.spritePixelsToUnits = options.PixelsPerUnit;
        ti.filterMode = options.FilterMode;
        ti.spritesheet = metas;
        EditorUtility.SetDirty(ti);
        ti.SaveAndReimport();
        return true;
    }
}
#endif
