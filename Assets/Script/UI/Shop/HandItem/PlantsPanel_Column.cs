using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>排山倒海：点击格必须可种，同列其余格逐格判定。</summary>
public class PlantsPanel_Column : BaseHandPanel
{
    static Tile FindTileAtWorld(Vector2 worldPos)
    {
        foreach (Collider2D col in Physics2D.OverlapPointAll(worldPos))
        {
            Tile tile = col.GetComponentInParent<Tile>();
            if (tile != null) return tile;
        }
        return null;
    }

    static Chess PlantOnTile(Tile t, PrePlantImage_Data data)
    {
        if (data.creator.plantFunction is LevelUpPlant) t.stander?.Death();
        Chess c = ChessTeamManage.Instance.CreateChess(data.creator, t, data.tag);
        if (data.tag == "Player")
            t.PlantChess(c);
        return c;
    }

    public override IEnumerator Plants(UnityAction CancelPlant, UnityAction<Chess> Plant, PrePlantImage_Data data)
    {
        MapManage.instance.AwakeTile();
        while (true)
        {
            if (Input.GetMouseButtonDown(1))
            {
                CancelPlant?.Invoke();
                break;
            }
            if (Input.GetMouseButtonDown(0))
            {
                Camera cam = Camera.main;
                if (cam != null)
                {
                    Vector2 rayPos = cam.ScreenToWorldPoint(Input.mousePosition);
                    if (!IceCell.BlocksPlayerPlantAt(rayPos))
                    {
                        Tile anchor = FindTileAtWorld(rayPos);
                        if (anchor != null && data.creator.IfCanPlant(anchor))
                        {
                            int colX = anchor.mapPos.x;
                            Chess first = null;
                            Vector2Int mapSize = MapManage.instance.mapSize;
                            for (int y = 0; y < mapSize.y; y++)
                            {
                                Tile t = MapManage.instance.tiles[colX, y];
                                if (t == null || !data.creator.IfCanPlant(t)) continue;
                                Chess c = PlantOnTile(t, data);
                                if (first == null) first = c;
                            }
                            if (first != null)
                                Plant?.Invoke(first);
                            break;
                        }
                    }
                }
            }
            yield return null;
        }
        MapManage.instance.SleepTile();
    }
}
