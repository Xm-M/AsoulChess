using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class AwardPanel : View
{
    public Image image;
    public TMP_Text price;
    public TMP_Text plantName;
    public TMP_Text plantdescription;
    public AudioPlayer AudioPlayer;
    PropertyCreator currentRewardCreator;
    public override void Init()
    {
        //throw new System.NotImplementedException();
        if(AudioPlayer==null)AudioPlayer = GetComponent<AudioPlayer>();
        EventController.Instance.AddListener(EventName.WhenLeaveLevel.ToString(), Hide);
    }
    public override void Show()
    {
        base.Show();
        AudioPlayer.RandomPlay();
    }
    public void ShowReward(PropertyCreator property)
    {
        currentRewardCreator = property;
        image.sprite = property.chessSprite;
        price.text = property.baseProperty.price.ToString();
        plantName.text = property.chessName;
        plantdescription.text = property.chessShortDescription;
    }
    /// <summary>
    /// 这个是按钮用的（植物奖励点「下一关」后调用）
    /// </summary>
    public void Win()
    {
        if (currentRewardCreator != null)
        {
            var data = PlayerSaveContext.CurrentData;
            if (data != null && data.ownedCreatorIds != null && !data.ownedCreatorIds.Contains(currentRewardCreator.chessName))
            {
                data.ownedCreatorIds.Add(currentRewardCreator.chessName);
                PlayerSaveContext.SaveCurrent();
                if (GameManage.instance != null)
                {
                    if (GameManage.instance.playerOwnedCreators == null)
                        GameManage.instance.playerOwnedCreators = new List<PropertyCreator>();
                    GameManage.instance.playerOwnedCreators.Add(currentRewardCreator);
                }
            }
            currentRewardCreator = null;
        }
        var level = LevelManage.instance?.currentLevel;
        if (level != null && !string.IsNullOrEmpty(level.levelName))
            PlayerSaveSystem.MarkLevelCompleted(level.levelName);
        LevelManage.instance.GameOver(true);
        Hide();
    }
}
