using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;

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
    /// <summary>读档后玩家拥有的植物 creator 列表，为 null 时 PlantsShop 使用 allChess</summary>
    public List<PropertyCreator> playerOwnedCreators;
    public UnityEvent WhenGameOver,WhenGameStart;
    public LevelData TestLevel;
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
        if (mode == GameMode.Test && allChess != null) playerOwnedCreators = new List<PropertyCreator>(allChess);
        timerManage.InitManage();
        chessFactory.InitManage();
        fetterManage.InitController();
        cameraManage.InitManage();
        UIManage=new UIManage();
        if (mode == GameMode.Test) UIManage.GetView<StartUI>().startLevelData=TestLevel;
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
        if (mode == GameMode.Test && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            if (Input.GetKeyUp(KeyCode.Alpha1)) SetDifficulty(0);
            else if (Input.GetKeyUp(KeyCode.Alpha2)) SetDifficulty(1);
            else if (Input.GetKeyUp(KeyCode.Alpha3)) SetDifficulty(2);
            else if (Input.GetKeyUp(KeyCode.Alpha4)) SetDifficulty(3);
        }
    }

    /// <summary>Test 模式下设置难度：0=简单 1=普通 2=困难 3=噩梦。Inspector 中可用 Odin 按钮调用，或 Ctrl+1/2/3/4</summary>
    [Button("Set Difficulty (0-3)", ButtonSizes.Medium)]
    public void SetDifficulty(int difficulty)
    {
        if (mode != GameMode.Test) return;
        int d = Mathf.Clamp(difficulty, 0, 3);
        DifficultyManager.TestModeDifficulty = d;
        string[] names = { "简单", "普通", "困难", "噩梦" };
        Debug.Log($"[Test] 难度已设为: {names[d]} ({d})");
    }
    public void QuitGame()
    {
        Application.Quit();
    }

    void OnApplicationQuit()
    {
        PlayerSaveContext.SaveCurrent();
        SaveSystem.SaveCurrentLevel();
    }
}
public enum GameMode
{
    Test,
    Game,
    Phone,
}
