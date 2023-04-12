using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManage : MonoBehaviour
{
    public ItemManage itemManage;
    public UIManage UIManage;
    public BuffManage buffManage;
    
    public FetterManage EnemyFetterM,PlayerFetterM;

    public static GameManage instance;

    public int playerHp=10;
    public Camera mainCamera;
    public Chess HandChess;
    Queue<Chess> moveQueue;
    public List<GameObject> allChess;
    public List<GameObject> PlayerChess;
    public List<State> states;
    public GameObject window;

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
        buffManage=GetComponent<BuffManage>();
        EnemyFetterM=new FetterManage("Enemy");
        PlayerFetterM=new FetterManage("Player");
        moveQueue = new Queue<Chess>();

    }
    private void Start()
    {
        playerHp = 10;
    }//

    public void GameStart()
    {
        if (!ifGameStart)
        {
            EventController.Instance.TriggerEvent(EventName.GameStart.ToString());
            ifGameStart = true;
            StartCoroutine(MoveControl());
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
        if(ChessFactory.instance.CheckAllDeath(tag)){    
            ifGameStart = false;
            EventController.Instance.TriggerEvent(EventName.GameOver.ToString());
            Debug.Log("GameOver");   
        }
    }
    public void RestartGame()
    {
        UIManage.instance.OpenWindow(window);
        EventController.Instance.TriggerEvent(EventName.RestartGame.ToString());
    }
     
    IEnumerator MoveControl()
    {
        while (ifGameStart)
        {
            if(moveQueue.Count > 0)
            {
                Chess c=moveQueue.Dequeue();
                (c.stateController.currentState.state as MoveState)?.MoveToNextTile();
            }
            yield return null;
        }
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
