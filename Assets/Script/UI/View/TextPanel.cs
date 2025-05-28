using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextPanel : View
{
    public Animator animator;
    public AudioPlayer manage;
    public GameObject zombieWave;
    public GameObject lastWave;
    public GameObject gameOver;
    public override void Init()
    {
        EventController.Instance.AddListener(EventName.WhenLeaveLevel.ToString(), Hide);
    }
    public override void Show()
    { 
        base.Show();
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
    public void GameOver()
    {
        animator.Play("gameover");
        ((MapManage_PVZ.instance) as MapManage_PVZ).au.PlayAudio("游戏失败");
        gameOver.SetActive(true);   
    }
    public void GameStart()
    {
        manage.PlayAudio("准备种植");
        animator.Play("gamestart");
    }
    public void RestartGame()
    {
        LevelManage.instance.RestartLevel();
    }
}
