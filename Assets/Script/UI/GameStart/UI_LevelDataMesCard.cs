using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UI_LevelDataMesCard : MonoBehaviour
{
    public Image levelImage;
    public TMP_Text levelName;
    public LevelData LevelData;
    
    public void InitCard(LevelData levelData)
    {
        this.LevelData = levelData;
        if (levelData != null)
        {
            if (levelData.levelSprit != null)
                levelImage.sprite = levelData.levelSprit;
            levelName.text = levelData.levelName;
        }
        var btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(LoadLevel);
        }
    }
    public void LoadLevel()
    {
        LevelManage.instance.ChangeLevel(LevelData);
        UIManage.GetView<StartUI>().Hide();
    }
}
