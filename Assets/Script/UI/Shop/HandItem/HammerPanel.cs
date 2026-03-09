using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 小千夏就放在这了
/// </summary>
public class HammerPanel : BaseHandPanel
{
    float damage;
    public override IEnumerator Plants(UnityAction CancelPlant, UnityAction<Chess> Plant, PrePlantImage_Data data)
    {
        damage = data.DM.damage;
        while (true)
        {
            if (Input.GetMouseButtonDown(1))
            {
                //CancelPlant?.Invoke();
                //break; 这里应该是播放千夏的idle 动画
            }
            else if (Input.GetMouseButtonDown(0))
            {
                Vector2 rayPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(rayPos, Vector2.zero, 0, 1 << 8);
                if (hit.collider != null)
                {
                    Debug.Log(hit.collider.gameObject.name + " 锤死你");
                    Chess chess = hit.collider.GetComponent<Chess>();
                    //chess.GetComponent<Chess>().Death();
                    data.DM.damage = damage;
                    data.DM.damageTo = chess;
                    chess.propertyController.GetDamage(data.DM);
                    //break;
                }
                else
                {
                    //CancelPlant?.Invoke();
                    //break;这里应该是播放千夏的idle 动画
                }
            }
            yield return null;
        }
    }
}
