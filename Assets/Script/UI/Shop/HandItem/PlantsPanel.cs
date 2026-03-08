using Language.Lua;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlantsPanel : BaseHandPanel
{

    public override IEnumerator Plants(UnityAction CancelPlant, UnityAction<Chess> Plant, PrePlantImage_Data data)
    {
        MapManage.instance.AwakeTile();
        while (true)
        {
            //transform.position = Input.mousePosition;
            if (Input.GetMouseButtonDown(1))
            {
                CancelPlant?.Invoke();
                break;
            }
            else if (Input.GetMouseButtonDown(0))
            {
                Vector2 rayPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(rayPos, Vector2.zero, 0, 1 << 9);
                if (hit.collider != null)
                {
                    Tile t = hit.collider.GetComponent<Tile>();
                    if (data.creator.IfCanPlant(t))
                    {
                        if (data.creator.plantFunction is LevelUpPlant) t.stander.Death();
                        Chess c = ChessTeamManage.Instance.CreateChess(data.creator, t, data.tag);
                        if (data.tag == "Player")
                        {
                            t.PlantChess(c);

                        }

                        Plant?.Invoke(c);
                        break;
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
