using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 其实是对标PlantPanel
/// </summary>
public class TestScenePanel : View
{
    public GameObject shopIconPre;
    public Transform playerIconParent;//植物列表
    public Transform enemyIconParent;//僵尸列表
    

    //public Animator anim;
    //public AudioPlayer shopAudio;
    //[FoldoutGroup("初始位置")]
    //public Vector2 startPos1, startPos2;
    //[FoldoutGroup("初始位置")]
    //public RectTransform p1, p2;
 
    public override void Init()
    {
        var playerChess = Resources.LoadAll<PropertyCreator>("ChessData/Player");//加载UIPrefab文件夹下的所有UI预制体
        foreach (PropertyCreator view in playerChess)
        {
            if (view.GetPre()!=null)
            {
                GameObject chessIcon = Instantiate(shopIconPre, playerIconParent);
                TestShopIcon icon=chessIcon.GetComponent<TestShopIcon>();
                icon.InitShopIcon(view, "Player");

            }
        }
        var enemyChess = Resources.LoadAll<PropertyCreator>("ChessData/Enemy");//加载UIPrefab文件夹下的所有UI预制体
        foreach (PropertyCreator view in enemyChess)
        {
            if (view.GetPre() != null)
            {
                GameObject chessIcon = Instantiate(shopIconPre, enemyIconParent);
                TestShopIcon icon = chessIcon.GetComponent<TestShopIcon>();
                icon.InitShopIcon(view, "Enemy");

            }
        }
    }
    public void SetGamePause()
    {
        if(LevelManage.instance.IfGameStart)
            LevelManage.instance.GamePause();
        else LevelManage.instance.GameContinue();
    }
}
