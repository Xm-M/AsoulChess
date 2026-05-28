using System.Collections.Generic;
using UnityEngine;

/// <summary>所有可选开局乐队；可被多个 <see cref="RunMapConfig"/> 引用。</summary>
[CreateAssetMenu(fileName = "RoguelikeBandCatalog", menuName = "Roguelike/Band Catalog")]
public class RoguelikeBandCatalog : ScriptableObject
{
    public List<BandMes> bands = new List<BandMes>();

    public int Count => bands != null ? bands.Count : 0;

    public BandMes GetBand(int index)
    {
        if (bands == null || index < 0 || index >= bands.Count)
            return null;
        return bands[index];
    }
}
