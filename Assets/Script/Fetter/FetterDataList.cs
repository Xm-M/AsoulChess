using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="羁绊列表",menuName ="Message/FetterList")]
public class FetterDataList : ScriptableObject
{
    [SerializeReference]
    public Fetter fetter;
}
