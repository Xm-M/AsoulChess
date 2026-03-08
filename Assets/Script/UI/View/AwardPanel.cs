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
        //这里还要调用一个把奖励加到池子里的语句 就是获得了这个植物嘛
        image.sprite = property.chessSprite;
        price.text = property.baseProperty.price.ToString();
        plantName.text = property.chessName;
        plantdescription.text = property.chessShortDescription;
    }
    /// <summary>
    /// 这个是按钮用的
    /// </summary>
    public void Win()
    {
        LevelManage.instance.GameOver(true);
        Hide();
    }
}
