using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(BuffCreator))]
public class BuffEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("refresh"))
        {
            (target as BuffCreator).Refresh();
        }
    }
}
