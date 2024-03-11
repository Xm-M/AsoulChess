using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextPanel : View
{
    public Animator animator;
    public AudioManage manage;
    public GameObject zombieWave;
    public GameObject lastWave;
    public override void Init()
    {
        //throw new System.NotImplementedException();
    }
    public void FirstZombieCom()
    {
        manage.PlayAudio("firstZombie");
    }
    public void ZombieWave()
    {
         zombieWave.SetActive(true);
    }
    public void LastWave()
    {
         lastWave.SetActive(true);
    }
}
