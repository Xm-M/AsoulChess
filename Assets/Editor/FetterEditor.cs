using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;

[CustomEditor(typeof(FetterMessege))]
public class FetterEditor : Editor
{
    string messege;
    List<object> sendMessege=new List<object>();
    public override void OnInspectorGUI()
    {
        FetterMessege fm= this.target as FetterMessege;
        base.OnInspectorGUI();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("reflesh"))
        {
            fm.ShowMessage();
        }
        GUILayout.EndHorizontal();
    }

}
