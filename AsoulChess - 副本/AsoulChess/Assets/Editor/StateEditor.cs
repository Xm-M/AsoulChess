using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(StateGraph))]
public class StateEditor : Editor
{
    string messege;
    List<object> sendMessege=new List<object>();
    public override void OnInspectorGUI()
    {
        StateGraph fm= this.target as StateGraph;
        base.OnInspectorGUI();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("reflesh"))
        {
            fm.ShowMessage();
        }
        GUILayout.EndHorizontal();
    }
}
