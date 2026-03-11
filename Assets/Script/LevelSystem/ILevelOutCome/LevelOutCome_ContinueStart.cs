using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelOutCome_ContinueStart : ILevelOutcome
{
    public void HandleOutcome(bool win, Vector3 lastZombiePos)
    {
        //throw new System.NotImplementedException();
        if (win)
        {
            LevelManage.instance.currentController.OverPlugin();
            LevelManage.instance.GamePause();
            (MapManage.instance as MapManage_PVZ).dir.Play();
        }
    }
}
