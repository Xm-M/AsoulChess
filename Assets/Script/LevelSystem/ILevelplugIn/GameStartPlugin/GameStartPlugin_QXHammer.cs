using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameStartPlugin_QXHammer : ILevelPlugin
{
    public PrePlantImage_Data data;
    [SerializeReference]
    public HammerPanel_QX hammer;
    Timer timer;
    public void OverPlugin(LevelController levelController)
    {
        timer?.Stop();
        hammer?.CancleUse();
    }

    public void StadgeEffect(LevelController levelController)
    {
        hammer = new HammerPanel_QX();
        //hammer.damage = 900;
        timer = GameManage.instance.timerManage.AddTimer(() => PrePlantImage.instance.TryToPlant(null, null,data, hammer),0.1f,true);
    }
}
public class HammerPanel_QX : BaseHandPanel
{
    public float damage=900;
    public override IEnumerator Plants(UnityAction CancelPlant, UnityAction<Chess> Plant, PrePlantImage_Data data)
    {
        //damage = data.DM.damage;
        PrePlantImage.instance.PlayChildAnim("qianxia_idle");
        while (true)
        {
            if (Input.GetMouseButtonDown(1))
            {
                PrePlantImage.instance.PlayChildAnim("qianxia_idle");
            }
            else if (Input.GetMouseButtonDown(0))
            {
                Vector2 rayPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(rayPos, Vector2.zero, 0, 1 << 8);
                PrePlantImage.instance.PlayChildAnim("qianxia_Attack");
                if (hit.collider != null)
                {
                    //Debug.Log(hit.collider.gameObject.name + " 锤死你");
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
    public override void CancleUse()
    {
        base.CancleUse();
        PrePlantImage.instance.OverPlayAnim();
    }
}
