 using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;
/// <summary>
/// 铲子面板 关于铲子 这个肯定也是要重新做的 因为可能一格内有多种植物
/// </summary>
public class ShovelPanel : BaseHandPanel
{
    public override IEnumerator Plants(UnityAction CancelPlant, UnityAction<Chess> Plant, PrePlantImage_Data data)
    {
        while(true)
        {
            if (Input.GetMouseButtonDown(1))
            {
                CancelPlant?.Invoke();
                break;
            }
            else if (Input.GetMouseButtonDown(0))
            {
                Vector2 rayPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(rayPos, Vector2.zero, 0, 1 << 7);
                if (hit.collider != null)
                {
                    Debug.Log(hit.collider.gameObject.name+" 被铲掉啦");
                    //au.PlayAudio(dig);
                    Chess chess = hit.collider.GetComponent<Chess>();
                    chess.GetComponent<Chess>().Death();
                    Plant?.Invoke(chess);
                    break;
                }
                else
                {
                    CancelPlant?.Invoke();
                    break;
                }
            }
            yield return null;
        }
    }
}
