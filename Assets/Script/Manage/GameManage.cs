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
    public AudioManage audioManage;
    public CheckObjectPoolManage checObjectPoolManage;
    public BuffManage buffManage;
    public ChessFactory chessFactory;//这个要在最后的时候销毁
    public ChessManage playerManage;
    public EnemyManage enemyManage;
    public static GameManage instance;
    public Camera mainCamera;
    public Chess HandChess;
    public List<PropertyCreator> allChess;
    public List<GameObject> PlayerChess;
    public UnityEvent WhenGameOver,WhenGameStart;
    public bool ifGameStart{get;private set;}
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
        audioManage = new AudioManage();
        checObjectPoolManage = new CheckObjectPoolManage();
        buffManage = new BuffManage();
        chessFactory = new ChessFactory();
        //playerManage = new ChessManage();
        //enemyManage = new ChessManage();
    }
    private void Start()
    {
        timerManage.InitManage();
        //checObjectPoolManage.
        buffManage.InitManage();
        chessFactory.InitManage();
        playerManage.InitManage();
        enemyManage.InitManage(); 
        UIManage=new UIManage();
    }
    public void RecycleChess(Chess chess)
    {
        if (chess.CompareTag(playerManage.playerTag))
        {
            playerManage.RecycleChess(chess);
        }
        else
        {
            enemyManage.RecycleChess(chess);
        }
    }
    private void Update()
    {
        timerManage.Update();
    }

    public void GameStart()
    {
        if (!ifGameStart)
        {
            
            EventController.Instance.TriggerEvent(EventName.GameStart.ToString());
            ifGameStart = true;
            WhenGameStart?.Invoke();
        }
    }
    

    public void GameOver(string tag)
    {
        if (ifGameStart)
        {
            WhenGameOver?.Invoke();
            if (tag == "Player")
            {
                ifGameStart = false;
                EventController.Instance.TriggerEvent(EventName.GameOver.ToString());
                Debug.Log("GameOver");
            }
            else
            {
                ifGameStart = false;
                EventController.Instance.TriggerEvent(EventName.GameSuccess.ToString());
                Debug.Log("GameWin");
            }
        }
    }
    public void RestartGame()
    {
        EventController.Instance.TriggerEvent(EventName.RestartGame.ToString());
         
    }
    public LayerMask GetEnemyLayer(GameObject gameObject)
    {
        if (gameObject.CompareTag(playerManage.playerTag))
        {
            return LayerMask.GetMask( enemyManage.playerMask);
        }
        else
        {
            return LayerMask.GetMask(playerManage.playerMask);
        }
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
