using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
[CustomEditor(typeof(AllItemMessage))]
public class AllItemEditor : Editor
{
    AllItemMessage allItemMessage;
    protected override void OnHeaderGUI()
    {
        if(allItemMessage==null)allItemMessage= target as AllItemMessage;
        base.OnHeaderGUI();
        if(GUILayout.Button("AddNewItem")){
            allItemMessage.AddItem();
        }
    }
}
