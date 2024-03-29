using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;
public class GameManage : MonoBehaviour
{
    public SceneManage sceneManage;
    public UIManage UIManage;
    public TimerManage timerManage;
    public CheckObjectPoolManage checObjectPoolManage;
    public BuffManage buffManage;
    public ChessFactory chessFactory;//这个要在最后的时候销毁
    public ChessTeamManage chessTeamManage;
    public static GameManage instance;
    public Camera mainCamera;
    public Chess HandChess;
    public List<PropertyCreator> allChess;
    public List<GameObject> PlayerChess;
    public UnityEvent WhenGameOver,WhenGameStart;
    //public bool ifGameStart{get;private set;}
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        timerManage = new TimerManage();
        //audioManage = new AudioManage();
        checObjectPoolManage = new CheckObjectPoolManage();
        buffManage = new BuffManage();
        chessFactory = new ChessFactory();
        chessTeamManage = new ChessTeamManage();
        //enemyManage = new ChessManage(); 
    }
    private void Start()
    {
        timerManage.InitManage();
        buffManage.InitManage();
        chessFactory.InitManage();
        UIManage=new UIManage();
    }
    
    private void Update()
    {
        timerManage.Update();
    }
    public void RestartGame()
    {
        EventController.Instance.TriggerEvent(EventName.RestartGame.ToString());
         
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
}
