using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextPanel : View
{
    public Animator animator;
    public AudioPlayer manage;
    public GameObject zombieWave;
    public GameObject lastWave;
    public override void Init()
    {
        EventController.Instance.AddListener(EventName.WhenLeaveLevel.ToString(), Hide);
    }
    public override void Show()
    {
        
        base.Show();
        manage.PlayAudio("×¼±¸ÖÖÖ²");
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
