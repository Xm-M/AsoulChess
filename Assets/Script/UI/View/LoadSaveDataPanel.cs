using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 读档选择面板。Show 时读取所有存档并按名字排列，点击可读档。
/// Test 模式下不执行相关逻辑。
/// </summary>
public class LoadSaveDataPanel : View
{
    [Header("存档列表")]
    public Transform listParent;
    public GameObject saveItemPrefab;

    [Header("无存档提示")]
    public GameObject noSavePrompt;
    public Button createNewButton;

    [Header("新建存档默认名")]
    public string defaultNewUsername = "新玩家";

    List<GameObject> itemInstances = new List<GameObject>();

    public override void Init()
    {
        if (createNewButton != null)
            createNewButton.onClick.AddListener(OnCreateNewSave);
    }

    public override void Show()
    {
        base.Show();
        RefreshList();
    }

    void RefreshList()
    {
        if (IsTestMode())
        {
            if (noSavePrompt != null) noSavePrompt.SetActive(true);
            if (listParent != null) ClearList();
            return;
        }

        var usernames = PlayerSaveSystem.GetAllSaveUsernames();
        usernames.Sort();

        ClearList();

        if (usernames.Count == 0)
        {
            if (noSavePrompt != null) noSavePrompt.SetActive(true);
            if (listParent != null) listParent.gameObject.SetActive(false);
        }
        else
        {
            if (noSavePrompt != null) noSavePrompt.SetActive(false);
            if (listParent != null)
            {
                listParent.gameObject.SetActive(true);
                foreach (var name in usernames)
                {
                    var item = CreateSaveItem(name);
                    if (item != null) itemInstances.Add(item);
                }
            }
        }
    }

    GameObject CreateSaveItem(string username)
    {
        if (saveItemPrefab == null || listParent == null) return null;
        var go = Instantiate(saveItemPrefab, listParent);
        var btn = go.GetComponent<Button>();
        var text = go.GetComponentInChildren<TMP_Text>();
        if (text != null) text.text = username;
        if (btn != null)
        {
            var u = username;
            btn.onClick.AddListener(() => OnLoadSave(u));
        }
        return go;
    }

    void ClearList()
    {
        foreach (var go in itemInstances)
        {
            if (go != null) Destroy(go);
        }
        itemInstances.Clear();
    }

    void OnLoadSave(string username)
    {
        if (IsTestMode()) return;
        var data = PlayerSaveSystem.Load(username);
        if (data == null) return;

        PlayerSaveContext.CurrentUsername = username;
        PlayerSaveContext.CurrentData = data;

        ApplyLoadedSave(data);
        Hide();
    }

    void OnCreateNewSave()
    {
        if (IsTestMode()) return;
        string name = defaultNewUsername;
        int n = 1;
        while (PlayerSaveSystem.HasSave(name))
        {
            name = defaultNewUsername + n;
            n++;
        }
        var data = PlayerSaveSystem.CreateNew(name);
        PlayerSaveSystem.Save(data);
        PlayerSaveContext.CurrentUsername = name;
        PlayerSaveContext.CurrentData = data;
        ApplyLoadedSave(data);
        Hide();
    }

    void ApplyLoadedSave(PlayerSaveData data)
    {
        ApplyPlayerChess(data);
        ApplyLevelClearState(data);
    }

    void ApplyPlayerChess(PlayerSaveData data)
    {
        if (GameManage.instance == null || data?.ownedCreatorIds == null) return;

        GameManage.instance.PlayerChess.Clear();
        GameManage.instance.playerOwnedCreators = new List<PropertyCreator>();
        foreach (var creatorId in data.ownedCreatorIds)
        {
            var creator = GetCreatorByChessName(creatorId);
            if (creator == null) continue;
            GameManage.instance.playerOwnedCreators.Add(creator);
            var prefab = creator.PlantEntrepotCardPre ?? creator.PlantCardPre;
            if (prefab != null)
                GameManage.instance.PlayerChess.Add(prefab);
        }
    }

    void ApplyLevelClearState(PlayerSaveData data)
    {
        if (data?.completedLevelIds == null) return;

        var allLevels = Resources.LoadAll<LevelData>("LevelData");
        foreach (var level in allLevels)
        {
            if (level != null && data.completedLevelIds.Contains(level.levelName))
                level.ifClearStadge = true;
        }
    }

    static PropertyCreator GetCreatorByChessName(string chessName)
    {
        if (string.IsNullOrEmpty(chessName) || GameManage.instance?.allChess == null) return null;
        foreach (var c in GameManage.instance.allChess)
        {
            if (c != null && c.chessName == chessName) return c;
        }
        return null;
    }

    static bool IsTestMode()
    {
        return GameManage.instance != null && GameManage.instance.mode == GameMode.Test;
    }
}
