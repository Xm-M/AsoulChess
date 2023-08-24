using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="AllFunction",fileName ="AllFunction")]
public class FunctionManage : ScriptableObject
{
     public void ShowShop(){
        PlantsShop.instance.director.Play();
     }
     public void GameStart(){
        GameManage.instance.GameStart();
     }
     public void MapDirResume(){
        MapManage.instance.dir.Resume();
     }
}
