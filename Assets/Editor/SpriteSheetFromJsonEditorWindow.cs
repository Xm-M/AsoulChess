#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// 选择贴图 + 切片 JSON，一键写入 Multiple Sprite 导入设置。
/// JSON 约定见 <see cref="SpriteSheetJsonImporterUtility"/>。
/// </summary>
public class SpriteSheetFromJsonEditorWindow : EditorWindow
{
    const string PrefsKeyTex = "SpriteSheetFromJson_LastTextureGuid";
    const string PrefsKeyJson = "SpriteSheetFromJson_LastJsonGuid";
    const string PrefsKeyPivotX = "SpriteSheetFromJson_PivotX";
    const string PrefsKeyPivotY = "SpriteSheetFromJson_PivotY";

    Texture2D _texture;
    TextAsset _jsonAsset;
    float _pixelsPerUnit = 16f;
    FilterMode _filterMode = FilterMode.Point;
    Vector2 _pivot = new Vector2(1f, 0f);
    Vector2 _scroll;

    [MenuItem("Tools/Sprite Sheet from JSON")]
    public static void Open()
    {
        var w = GetWindow<SpriteSheetFromJsonEditorWindow>(true, "Sprite Sheet from JSON", true);
        w.minSize = new Vector2(380, 260);
        w.LoadLastSelection();
    }

    void LoadLastSelection()
    {
        var tg = EditorPrefs.GetString(PrefsKeyTex, "");
        var jg = EditorPrefs.GetString(PrefsKeyJson, "");
        if (!string.IsNullOrEmpty(tg))
            _texture = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(tg));
        if (!string.IsNullOrEmpty(jg))
            _jsonAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(jg));
        if (EditorPrefs.HasKey(PrefsKeyPivotX) && EditorPrefs.HasKey(PrefsKeyPivotY))
            _pivot = new Vector2(EditorPrefs.GetFloat(PrefsKeyPivotX, 1f), EditorPrefs.GetFloat(PrefsKeyPivotY, 0f));
    }

    void SaveSelection()
    {
        if (_texture != null)
            EditorPrefs.SetString(PrefsKeyTex, AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_texture)));
        if (_jsonAsset != null)
            EditorPrefs.SetString(PrefsKeyJson, AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_jsonAsset)));
    }

    void OnGUI()
    {
        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        EditorGUILayout.HelpBox(
            "1) 拖入 Project 中的图集贴图（PNG 等）。\n" +
            "2) 拖入切片 JSON（根为数组，每项含 name + rect）。\n" +
            "3) 应用后写入 Multiple Sprite 并重新导入。\n\n" +
            "Pivot 默认 (1, 0)=右下：侧视、面朝左时脚底常在包围盒右下，挥武器时比中心/底中更稳；\n" +
            "若需底中可改为 (0.5, 0)。改完请再点一次「应用」。",
            MessageType.Info);

        _texture = (Texture2D)EditorGUILayout.ObjectField("图集 Texture", _texture, typeof(Texture2D), false);
        _jsonAsset = (TextAsset)EditorGUILayout.ObjectField("切片 JSON", _jsonAsset, typeof(TextAsset), false);

        EditorGUILayout.Space(6);
        _pixelsPerUnit = EditorGUILayout.FloatField("Pixels Per Unit", _pixelsPerUnit);
        _filterMode = (FilterMode)EditorGUILayout.EnumPopup("Filter Mode", _filterMode);
        _pivot = EditorGUILayout.Vector2Field("Pivot 归一化 (右下=1,0 底中=0.5,0)", _pivot);

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(8);
        using (new EditorGUI.DisabledScope(_texture == null || _jsonAsset == null))
        {
            if (GUILayout.Button("应用切片到贴图", GUILayout.Height(32)))
                Apply();
        }
    }

    void Apply()
    {
        string texPath = AssetDatabase.GetAssetPath(_texture);
        if (string.IsNullOrEmpty(texPath))
        {
            EditorUtility.DisplayDialog("Sprite Sheet from JSON", "无效的贴图资源。", "OK");
            return;
        }

        if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(_jsonAsset)))
        {
            EditorUtility.DisplayDialog("Sprite Sheet from JSON", "无效的 JSON 资源。", "OK");
            return;
        }

        string json = _jsonAsset.text;
        if (string.IsNullOrWhiteSpace(json))
        {
            EditorUtility.DisplayDialog("Sprite Sheet from JSON", "JSON 内容为空。", "OK");
            return;
        }

        var opt = new SpriteSheetJsonImporterUtility.ImportOptions
        {
            PixelsPerUnit = _pixelsPerUnit,
            FilterMode = _filterMode,
            Pivot = _pivot
        };

        if (!SpriteSheetJsonImporterUtility.Apply(texPath, json, opt, out string err))
        {
            EditorUtility.DisplayDialog("Sprite Sheet from JSON", err, "OK");
            Debug.LogError("[SpriteSheetFromJSON] " + err);
            return;
        }

        SaveSelection();
        EditorPrefs.SetFloat(PrefsKeyPivotX, _pivot.x);
        EditorPrefs.SetFloat(PrefsKeyPivotY, _pivot.y);
        Debug.Log($"[SpriteSheetFromJSON] 已写入 {texPath}，子 Sprite 数: 由 JSON 决定。");
        EditorUtility.DisplayDialog("Sprite Sheet from JSON", "已应用并重新导入贴图。", "OK");
    }
}
#endif
