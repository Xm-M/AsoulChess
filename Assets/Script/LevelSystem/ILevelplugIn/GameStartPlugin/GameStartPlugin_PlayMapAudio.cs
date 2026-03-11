using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 播放音乐插件
/// </summary>
public class GameStartPlugin_PlayAudio : ILevelPlugin
{
    public string audioName;
    public float deleyPlayTime;
    Timer timer;
    public void StadgeEffect(LevelController levelController)
    {
        MapManage.instance.BGMPlayer.ChangeAudio(audioName);
        MapManage.instance.BGMPlayer.SetLoop(true);
        if (SaveLoadContext.IsLoadFromSave && SaveLoadContext.CurrentSaveData?.levelData != null)
        {
            float gameTime = SaveLoadContext.CurrentSaveData.levelData.gameTime;
            MapManage.instance.BGMPlayer.Play();
            float elapsed = gameTime - deleyPlayTime;
            if (elapsed > 0)
                MapManage.instance.BGMPlayer.Seek(elapsed);
        }
        else if (deleyPlayTime > 0)
            timer = GameManage.instance.timerManage.AddTimer(() => MapManage.instance.BGMPlayer.Play(), deleyPlayTime);
        else
            MapManage.instance.BGMPlayer.Play();
    }
    public void OverPlugin(LevelController levelController)
    {
        MapManage.instance.BGMPlayer.Stop();
        timer?.Stop();
        timer = null;
    }
}