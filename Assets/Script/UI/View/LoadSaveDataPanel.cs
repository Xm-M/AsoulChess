using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 读档面板。有存档时显示继续/新游戏；无存档时显示名字输入并创建。
/// Test 模式下不执行相关逻辑。
/// </summary>
public class LoadSaveDataPanel : View
{
    [Header("用户名显示（有存档时）")]
    public TMP_Text usernameText;

    [Header("继续游戏")]
    public Button continueButton;

    [Header("新游戏")]
    public Button newGameButton;

    [Header("无存档时：名字输入")]
    public TMP_InputField nameInputField;

    [Header("无存档时：创建存档按钮")]
    public Button createSaveButton;

    [Header("新游戏时：取消按钮（返回继续/新游戏选择），可与新游戏共用同一按钮")]
    public Button cancelNewGameButton;

    [Header("新游戏/取消共用同一按钮时的文案")]
    public string newGameButtonText = "重新开始";
    public string cancelButtonText = "取消";

    [Header("无存档时隐藏继续按钮")]
    public bool hideContinueWhenNoSave = true;

    bool isNewGameInputMode;

    public override void Init()
    {
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinue);
        if (newGameButton != null)
        {
            if (newGameButton == cancelNewGameButton)
                newGameButton.onClick.AddListener(OnNewGameOrCancel);
            else
                newGameButton.onClick.AddListener(OnNewGame);
        }
        if (createSaveButton != null)
            createSaveButton.onClick.AddListener(OnCreateSave);
        if (cancelNewGameButton != null && cancelNewGameButton != newGameButton)
            cancelNewGameButton.onClick.AddListener(OnCancelNewGame);
    }

    public override void Show()
    {
        if (IsTestMode())
        {
            Hide();
            return;
        }
        base.Show();
        isNewGameInputMode = false;
        RefreshUI();
    }

    void RefreshUI()
    {
        if (IsTestMode())
        {
            SetAllInactive();
            return;
        }

        bool hasSave = PlayerSaveSystem.HasSave();
        bool showInputMode = !hasSave || isNewGameInputMode;

        if (usernameText != null)
            usernameText.gameObject.SetActive(hasSave && !isNewGameInputMode);
        if (continueButton != null)
            continueButton.gameObject.SetActive(hasSave && !isNewGameInputMode && (hideContinueWhenNoSave == false || hasSave));
        bool sameButton = newGameButton != null && newGameButton == cancelNewGameButton;
        if (newGameButton != null)
        {
            bool showNewGame = hasSave && !isNewGameInputMode;
            bool showCancel = hasSave && isNewGameInputMode;
            newGameButton.gameObject.SetActive(sameButton ? (showNewGame || showCancel) : showNewGame);
            if (sameButton && newGameButton.gameObject.activeSelf)
                SetButtonText(newGameButton, isNewGameInputMode ? cancelButtonText : newGameButtonText);
        }
        if (cancelNewGameButton != null && !sameButton)
            cancelNewGameButton.gameObject.SetActive(hasSave && isNewGameInputMode);

        if (nameInputField != null)
            nameInputField.gameObject.SetActive(showInputMode);
        if (createSaveButton != null)
            createSaveButton.gameObject.SetActive(showInputMode);

        if (hasSave && usernameText != null && !isNewGameInputMode)
        {
            var data = PlayerSaveContext.CurrentData ?? PlayerSaveSystem.Load();
            usernameText.text = !string.IsNullOrEmpty(data?.username) ? data.username : "玩家";
        }
        if (showInputMode && nameInputField != null && isNewGameInputMode)
        {
            var data = PlayerSaveContext.CurrentData ?? PlayerSaveSystem.Load();
            nameInputField.text = !string.IsNullOrEmpty(data?.username) ? data.username : "";
        }
    }

    void SetAllInactive()
    {
        if (usernameText != null) usernameText.gameObject.SetActive(false);
        if (continueButton != null) continueButton.gameObject.SetActive(false);
        if (newGameButton != null) newGameButton.gameObject.SetActive(false);
        if (nameInputField != null) nameInputField.gameObject.SetActive(false);
        if (createSaveButton != null) createSaveButton.gameObject.SetActive(false);
        if (cancelNewGameButton != null) cancelNewGameButton.gameObject.SetActive(false);
    }

    void OnNewGame()
    {
        if (IsTestMode()) return;
        isNewGameInputMode = true;
        RefreshUI();
    }

    void OnNewGameOrCancel()
    {
        if (isNewGameInputMode)
        {
            OnCancelNewGame();
            SetButtonText(newGameButton, cancelButtonText);
        }
        else
        {
            OnNewGame();
            SetButtonText(newGameButton, newGameButtonText);
        }
    }

    void OnCancelNewGame()
    {
        isNewGameInputMode = false;
        RefreshUI();
    }

    static void CopySettingsFrom(PlayerSaveData from, PlayerSaveData to)
    {
        to.bgmVolume = from.bgmVolume;
        to.sfxVolume = from.sfxVolume;
        to.screenWidth = from.screenWidth;
        to.screenHeight = from.screenHeight;
        to.fullscreen = from.fullscreen;
    }

    static void SetButtonText(Button btn, string text)
    {
        var t = btn.GetComponentInChildren<TMP_Text>();
        if (t != null) t.text = text;
    }

    void OnCreateSave()
    {
        if (IsTestMode()) return;

        string name = nameInputField != null ? nameInputField.text?.Trim() : "";
        if (string.IsNullOrEmpty(name))
            name = PlayerSaveSystem.DefaultSaveName;

        PlayerSaveData oldSettings = null;
        if (isNewGameInputMode && PlayerSaveSystem.HasSave())
            oldSettings = PlayerSaveSystem.Load();

        if (isNewGameInputMode)
            PlayerSaveSystem.Delete();
        SaveSystem.DeleteAllLevelSaves();

        var data = PlayerSaveSystem.CreateNew();
        data.username = name;
        if (oldSettings != null)
            CopySettingsFrom(oldSettings, data);
        PlayerSaveSystem.Save(data);
        PlayerSaveContext.CurrentData = data;
        isNewGameInputMode = false;
        ApplyLoadedSave(data);
        Hide();
    }

    void OnContinue()
    {
        if (IsTestMode()) return;
        if (!PlayerSaveSystem.HasSave()) return;

        var data = PlayerSaveSystem.Load();
        if (data == null) return;

        PlayerSaveContext.CurrentData = data;
        ApplyLoadedSave(data);
        Hide();
    }

    void ApplyLoadedSave(PlayerSaveData data)
    {
        PlayerSaveContext.ApplyPlayerChessToGame();
        PlayerSaveContext.ApplyLevelClearStateToGame();
        PlayerSaveContext.ApplySettingsToGame();
        UIManage.GetView<StartUI>()?.RefreshSaveButtonText();
        UIManage.GetView<StartUI>()?.RefreshShopButtonVisibility();
    }

    static bool IsTestMode()
    {
        return GameManage.instance != null && GameManage.instance.mode == GameMode.Test;
    }
}
