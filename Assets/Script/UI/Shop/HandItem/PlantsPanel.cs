 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlantsPanel : BaseHandPanel
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
                    // 禁止在 continue 中跳过本循环末尾的 yield，否则同一帧内 GetMouseButtonDown 仍为 true 会死循环卡死/崩溃
                    // BlocksPlayerPlantAt：true=该点被冰等阻挡，不可种；只有未阻挡时才尝试落子
                    if (!IceCell.BlocksPlayerPlantAt(rayPos))
                    {
                        Tile t = FindTileAtWorld(rayPos);
                        if (t != null
                            && MapManage.instance.IsPlantColumnAllowed(t.mapPos.x)
                            && data.creator.IfCanPlant(t))
                        {
                            if (data.creator.plantFunction is LevelUpPlant) t.stander?.Death();
                            Chess c = ChessTeamManage.Instance.CreateChess(data.creator, t, data.tag);
                            if (data.tag == "Player")
                                t.PlantChess(c);

                            Plant?.Invoke(c);
                            break;
                        }
                    }
                }
            }
            yield return null;
        }
        //gameObject.SetActive(false);
        MapManage.instance.SleepTile();
    }
    //public override void TryToPlant(UnityAction CancelPlant, UnityAction<Chess> Plant, PrePlantImage_Data data)
    //{
    //    transform.position = Input.mousePosition;
    //    gameObject.SetActive(true);
    //    //image.sprite = creator.chessSprite;
        
    //    base.TryToPlant(CancelPlant, Plant, data);
    //}
}
