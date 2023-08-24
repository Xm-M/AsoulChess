using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;
public class GameManage : MonoBehaviour
{
    public ItemManage itemManage;
    public UIManage UIManage;
    public FetterManage EnemyFetterM,PlayerFetterM;
    public TimerManage timerManage;
    public AudioManage audioManage;
    public static GameManage instance;

    public Camera mainCamera;
    public Chess HandChess;
    Queue<Chess> moveQueue;
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
        itemManage=GetComponent<ItemManage>();
        timerManage = new TimerManage();
        audioManage = new AudioManage();
        EnemyFetterM=new FetterManage("Enemy");
        PlayerFetterM=new FetterManage("Player");
        
        moveQueue = new Queue<Chess>();

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
    

    public void AddMoveChess(Chess chess)
    {
        if (!moveQueue.Contains(chess))
        {
            moveQueue.Enqueue(chess);
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
     
    public void QuitGame()
    {
        Application.Quit();
    }
}
