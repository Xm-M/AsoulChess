#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 为 <see cref="IGridFindTarget.relativeCells"/> 提供点阵编辑；中心格为相对偏移 (0,0)。
/// 列 → 相对 X = col - 10（范围 -10～10）；行（自上而下）→ 相对 Y = row - 4（范围 -4～5）。
/// </summary>
[CustomPropertyDrawer(typeof(IGridFindTarget))]
public class IGridFindTargetDrawer : PropertyDrawer
{
    const int Cols = 21;
    const int Rows = 10;
    const int RelXMin = -10;
    const float CellW = 20f;
    const float CellH = 22f;
    const float RowLabelW = 22f;
    const float Spacing = 2f;

    static readonly Dictionary<string, Vector2> s_GridScroll = new Dictionary<string, Vector2>();

    static int RelXFromCol(int col) => col + RelXMin;
    static int RelYFromRow(int guiRowTopToBottom) => guiRowTopToBottom - 4;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float line = EditorGUIUtility.singleLineHeight;
        float h = line;
        if (property == null || !property.isExpanded)
            return h;

        h += Spacing;
        var boxProp = property.FindPropertyRelative("boxHalfExtents");
        if (boxProp != null)
            h += EditorGUI.GetPropertyHeight(boxProp, true) + Spacing;

        h += line * 2f + Spacing;
        h += line + Spacing;
        float headerH = line * 0.85f + Spacing;
        h += headerH + Rows * (CellH + Spacing) + Spacing;
        h += line + Spacing;

        if (HasOutOfGridCells(property))
            h += line * 2f + Spacing;

        return h;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property == null)
            return;

        property.serializedObject.Update();
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty relProp = property.FindPropertyRelative("relativeCells");
        SerializedProperty boxProp = property.FindPropertyRelative("boxHalfExtents");

        float y = position.y;
        float w = position.width;
        float line = EditorGUIUtility.singleLineHeight;

        Rect foldRect = new Rect(position.x, y, w, line);
        property.isExpanded = EditorGUI.Foldout(foldRect, property.isExpanded, label, true);
        y += line + Spacing;

        if (!property.isExpanded)
        {
            EditorGUI.EndProperty();
            property.serializedObject.ApplyModifiedProperties();
            return;
        }

        float innerX = position.x + 15f;
        float innerW = w - 15f;

        if (boxProp != null)
        {
            float boxH = EditorGUI.GetPropertyHeight(boxProp, true);
            EditorGUI.PropertyField(new Rect(innerX, y, innerW, boxH), boxProp, true);
            y += boxH + Spacing;
        }

        var helpText =
            "点阵为相对自身格子的偏移 (0,0)。列 = 相对 X（运行时随面朝方向只镜像 X）；行 = 相对 Y（不镜像）。黄格为中心。";
        EditorGUI.LabelField(new Rect(innerX, y, innerW, line * 2f), helpText, EditorStyles.wordWrappedMiniLabel);
        y += line * 2f + Spacing;

        EditorGUI.LabelField(new Rect(innerX, y, innerW, line), "攻击范围（点选格子）", EditorStyles.boldLabel);
        y += line + Spacing;

        var selected = ReadSetFromList(relProp);

        float headerH = line * 0.85f + Spacing;
        float gridContentW = RowLabelW + Cols * (CellW + Spacing);
        float gridContentH = headerH + Rows * (CellH + Spacing);

        string scrollKey = property.serializedObject.targetObject.GetInstanceID() + "_" + property.propertyPath;
        if (!s_GridScroll.TryGetValue(scrollKey, out Vector2 gridScroll))
            gridScroll = Vector2.zero;

        float gridLeft = RowLabelW;
        Rect scrollViewRect = new Rect(innerX, y, innerW, gridContentH);
        Rect scrollContentRect = new Rect(0, 0, Mathf.Max(gridContentW, innerW), gridContentH);
        gridScroll = GUI.BeginScrollView(scrollViewRect, gridScroll, scrollContentRect, true, false);

        for (int c = 0; c < Cols; c++)
        {
            int rx = RelXFromCol(c);
            Rect lr = new Rect(gridLeft + c * (CellW + Spacing), 0, CellW, line * 0.85f);
            var colHeader = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleCenter };
            GUI.Label(lr, rx.ToString(), colHeader);
        }

        for (int guiRow = 0; guiRow < Rows; guiRow++)
        {
            int ry = RelYFromRow(guiRow);
            float rowY = headerH + guiRow * (CellH + Spacing);
            EditorGUI.LabelField(new Rect(0, rowY, RowLabelW - 2f, CellH), ry.ToString(), EditorStyles.miniLabel);

            for (int col = 0; col < Cols; col++)
            {
                int rx = RelXFromCol(col);
                var key = new Vector2Int(rx, ry);
                bool on = selected.Contains(key);
                bool center = rx == 0 && ry == 0;

                Rect cell = new Rect(gridLeft + col * (CellW + Spacing), rowY, CellW, CellH);
                Color bg = center ? new Color(1f, 0.92f, 0.5f, 0.35f) : (on ? new Color(0.4f, 0.75f, 0.45f, 0.5f) : new Color(0.35f, 0.35f, 0.38f, 0.25f));
                EditorGUI.DrawRect(cell, bg);

                string t = center ? "0" : (on ? "■" : "·");
                var prev = GUI.color;
                GUI.color = on || center ? Color.white : new Color(0.7f, 0.7f, 0.7f);
                if (GUI.Button(cell, t, EditorStyles.miniButton))
                {
                    Undo.RecordObject(property.serializedObject.targetObject, "Toggle IGridFindTarget Cell");
                    if (on) selected.Remove(key);
                    else selected.Add(key);
                    WriteListFromSet(relProp, selected);
                    property.serializedObject.ApplyModifiedProperties();
                }
                GUI.color = prev;
            }
        }

        GUI.EndScrollView();
        s_GridScroll[scrollKey] = gridScroll;
        y += gridContentH + Spacing;

        if (HasOutOfGridCells(property))
        {
            y += Spacing;
            EditorGUI.HelpBox(new Rect(innerX, y, innerW, line * 2f),
                "relativeCells 中存在超出上方点阵范围的偏移，仍会在运行时使用；可在列表中手动编辑或删除。",
                MessageType.Warning);
        }

        EditorGUI.EndProperty();
        property.serializedObject.ApplyModifiedProperties();
    }

    static HashSet<Vector2Int> ReadSetFromList(SerializedProperty listProp)
    {
        var set = new HashSet<Vector2Int>();
        if (listProp == null || !listProp.isArray)
            return set;
        for (int i = 0; i < listProp.arraySize; i++)
        {
            var elem = listProp.GetArrayElementAtIndex(i);
            if (elem == null) continue;
            var px = elem.FindPropertyRelative("x");
            var py = elem.FindPropertyRelative("y");
            if (px == null || py == null) continue;
            set.Add(new Vector2Int(px.intValue, py.intValue));
        }
        return set;
    }

    static void WriteListFromSet(SerializedProperty listProp, HashSet<Vector2Int> set)
    {
        if (listProp == null || !listProp.isArray)
            return;
        var sorted = new List<Vector2Int>(set);
        sorted.Sort(CompareCell);
        listProp.arraySize = sorted.Count;
        for (int i = 0; i < sorted.Count; i++)
        {
            var elem = listProp.GetArrayElementAtIndex(i);
            elem.FindPropertyRelative("x").intValue = sorted[i].x;
            elem.FindPropertyRelative("y").intValue = sorted[i].y;
        }
    }

    static int CompareCell(Vector2Int a, Vector2Int b)
    {
        int c = a.y.CompareTo(b.y);
        if (c != 0) return c;
        return a.x.CompareTo(b.x);
    }

    static bool IsInEditorGrid(Vector2Int v)
    {
        return v.x >= RelXFromCol(0) && v.x <= RelXFromCol(Cols - 1)
            && v.y >= RelYFromRow(0) && v.y <= RelYFromRow(Rows - 1);
    }

    static bool HasOutOfGridCells(SerializedProperty property)
    {
        var relProp = property.FindPropertyRelative("relativeCells");
        if (relProp == null || !relProp.isArray) return false;
        for (int i = 0; i < relProp.arraySize; i++)
        {
            var elem = relProp.GetArrayElementAtIndex(i);
            if (elem == null) continue;
            var v = new Vector2Int(
                elem.FindPropertyRelative("x").intValue,
                elem.FindPropertyRelative("y").intValue);
            if (!IsInEditorGrid(v))
                return true;
        }
        return false;
    }
}
#endif
