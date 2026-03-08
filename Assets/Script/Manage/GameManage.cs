using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;

/// <summary>
/// GameManage就更像是一个用来保存各种Manage数据的地方了
/// 
/// </summary>
public class GameManage : MonoBehaviour
{
    public GameMode mode;
    public SceneManage sceneManage;
    public UIManage UIManage;
    public TimerManage timerManage;
    public AudioManage audioManage;
    public CheckObjectPoolManage checObjectPoolManage;
    //public BuffManage buffManage;
    public ChessFactory chessFactory;//这个要在最后的时候销毁
    public ChessTeamManage chessTeamManage;
    [SerializeReference]
    public FetterController fetterManage;
    [SerializeReference]
    public GameCameraManage cameraManage;
    public Camera mainCamera;
    public static GameManage instance;
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
        audioManage = new AudioManage();
        timerManage = new TimerManage();
        checObjectPoolManage = new CheckObjectPoolManage();
        chessFactory = new ChessFactory();
        chessTeamManage = new ChessTeamManage();
        cameraManage = new GameCameraManage();
        cameraManage.SetCamera(mainCamera);

    }
    private void Start()
    {
        timerManage.InitManage();
        chessFactory.InitManage();
        fetterManage.InitController();
        cameraManage.InitManage();
        UIManage=new UIManage();
    }
    
    private void Update()
    {
        timerManage.Update();
        cameraManage.Tick(Time.deltaTime);
        if (mode == GameMode.Test && Input.GetKeyUp(KeyCode.T))
        {
            UIManage.Show<TestScenePanel>();
        }else if (mode == GameMode.Test && Input.GetKeyUp(KeyCode.Y))
        {
            UIManage.Close<TestScenePanel>();
        }
        if (mode == GameMode.Test && Input.GetKeyUp(KeyCode.X)&&LevelManage.instance.IfGameStart)
        {
            LevelManage.instance.GamePause();
            Debug.Log("暂停");
        }else if (mode == GameMode.Test && Input.GetKeyUp(KeyCode.X) && !LevelManage.instance.IfGameStart)
        {
            LevelManage.instance.GameContinue();
            Debug.Log("继续");
        }
        if(mode == GameMode.Test && Input.GetKeyUp(KeyCode.S) && LevelManage.instance.IfGameStart)
        {
            SunLightPanel.instance.ChangeSunLight(10000);
        }
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
public enum GameMode
{
    Test,
    Game,
}
