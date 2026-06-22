using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 肉鸽开局选乐队。成员展示：在 <see cref="memberPos"/> 下实例化 <see cref="PropertyCreator.chessPre"/>，
/// 由场景 Camera 渲染到 RawImage（布局与投射在预制体中配置）。
/// </summary>
public class RoguelikeBandSelectPanel : View
{
    [Header("数据（二选一或都配：优先 Catalog）")]
    [SerializeField] RoguelikeBandCatalog bandCatalog;
    [SerializeField] RunMapConfig runMapConfig;

    [Header("UI 绑定（拖引用）")]
    [SerializeField] Image backgroundImage;
    [SerializeField] TMP_Text bandNameText;
    [SerializeField] TMP_Text bandDescriptionText;
    [SerializeField] Button prevBandButton;
    [SerializeField] Button nextBandButton;
    [SerializeField] Button startChallengeButton;
    [SerializeField] Button backButton;

    [Header("成员站位（按顺序拖 Transform，生成 chessPre 于其下）")]
    [SerializeField] List<Transform> memberPos = new List<Transform>();

    [Header("切换动画")]
    [SerializeField] Animator panelAnimator;
    [Tooltip("Animator 状态名，对应 change.anim")]
    [SerializeField] string switchAnimStateName = "change";

    [Header("BGM")]
    [SerializeField] AudioPlayer bandBgmPlayer;

    [Header("可选")]
    [SerializeField] bool hideEmptyMemberSlots = true;

    RunMapConfig _activeRunConfig;
    List<BandMes> _bands = new List<BandMes>();
    int _currentIndex;
    int _pendingBandDelta;
    bool _bandSwitchInProgress;
    string _lastPlayedBgmKey;
    readonly List<GameObject> _spawnedMembers = new List<GameObject>();

    public override void Init()
    {
        if (panelAnimator == null)
            panelAnimator = GetComponent<Animator>();
        if (prevBandButton != null)
            prevBandButton.onClick.AddListener(ShowPreviousBand);
        if (nextBandButton != null)
            nextBandButton.onClick.AddListener(ShowNextBand);
        if (startChallengeButton != null)
            startChallengeButton.onClick.AddListener(OnStartChallenge);
        if (backButton != null)
            backButton.onClick.AddListener(OnBack);
    }

    /// <summary>从主菜单进入：新开局选乐队。</summary>
    public static void OpenForNewRun(RunMapConfig config)
    {
        if (config == null)
        {
            Debug.LogError("[RoguelikeBandSelectPanel] RunMapConfig 为空");
            return;
        }

        var panel = UIManage.GetView<RoguelikeBandSelectPanel>();
        if (panel == null)
        {
            Debug.LogError("[RoguelikeBandSelectPanel] 未找到面板，请在 Resources/UIPrefab 下创建预制体并挂本脚本");
            return;
        }

        panel.Prepare(config, 0);
        panel.Show();
    }

    void Prepare(RunMapConfig config, int startIndex)
    {
        _activeRunConfig = config;
        if (bandCatalog != null && bandCatalog.Count > 0)
            _bands = new List<BandMes>(bandCatalog.bands);
        else if (config != null)
            _bands = new List<BandMes>(config.GetStartingBands());
        else
            _bands = new List<BandMes>();

        _currentIndex = _bands.Count > 0 ? Mathf.Clamp(startIndex, 0, _bands.Count - 1) : 0;
        _pendingBandDelta = 0;
        _bandSwitchInProgress = false;
    }

    public override void Show()
    {
        base.Show();
        _pendingBandDelta = 0;
        _bandSwitchInProgress = false;
        RefreshView();
    }

    public override void Hide()
    {
        _bandSwitchInProgress = false;
        _pendingBandDelta = 0;
        _lastPlayedBgmKey = null;
        ClearMemberPreviews();
        base.Hide();
    }

    void ShowPreviousBand()
    {
        RequestBandSwitch(-1);
    }

    void ShowNextBand()
    {
        RequestBandSwitch(1);
    }

    void RequestBandSwitch(int delta)
    {
        if (_bands.Count <= 1 || _bandSwitchInProgress)
            return;

        _pendingBandDelta = delta;
        _bandSwitchInProgress = true;
        SetBandNavInteractable(false);

        if (panelAnimator != null && !string.IsNullOrEmpty(switchAnimStateName))
        {
            panelAnimator.Play(switchAnimStateName, 0, 0f);
            return;
        }

        OnBandSwitchAnimationApply();
        OnBandSwitchAnimationFinished();
    }

    /// <summary>change.anim 在合适帧添加 Animation Event 调用，用于真正切换乐队展示。</summary>
    public void OnBandSwitchAnimationApply()
    {
        if (_bands.Count == 0 || _pendingBandDelta == 0)
            return;

        _currentIndex = (_currentIndex + _pendingBandDelta + _bands.Count) % _bands.Count;
        _pendingBandDelta = 0;
        RefreshView();
    }

    /// <summary>change.anim 末帧 Animation Event 调用，恢复按钮。</summary>
    public void OnBandSwitchAnimationFinished()
    {
        _bandSwitchInProgress = false;
        _pendingBandDelta = 0;
        SetBandNavInteractable(_bands.Count > 1);
    }

    void SetBandNavInteractable(bool navEnabled)
    {
        if (prevBandButton != null)
            prevBandButton.interactable = navEnabled;
        if (nextBandButton != null)
            nextBandButton.interactable = navEnabled;
    }

    void RefreshView()
    {
        bool hasBands = _bands.Count > 0;
        if (!_bandSwitchInProgress)
            SetBandNavInteractable(_bands.Count > 1);

        if (!hasBands)
        {
            if (bandNameText != null)
                bandNameText.text = "未配置乐队";
            if (bandDescriptionText != null)
                bandDescriptionText.text = "请在 RunMapConfig.startingBands 或 RoguelikeBandCatalog 中配置 BandMes";
            if (backgroundImage != null)
            {
                backgroundImage.sprite = null;
                backgroundImage.color = new Color(1f, 1f, 1f, 0.15f);
            }
            ClearMemberPreviews();
            if (startChallengeButton != null)
                startChallengeButton.interactable = false;
            StopBandBgm();
            return;
        }

        var band = _bands[_currentIndex];
        if (bandNameText != null)
            bandNameText.text = band.bandName ?? string.Empty;
        if (bandDescriptionText != null)
            bandDescriptionText.text = band.bandDescription ?? string.Empty;

        if (backgroundImage != null)
        {
            backgroundImage.sprite = band.backgroundImage;
            backgroundImage.color = band.backgroundImage != null ? Color.white : new Color(1f, 1f, 1f, 0.15f);
            backgroundImage.preserveAspect = true;
        }

        RefreshMemberPreviews(band);
        PlayBandBgm(band);

        if (startChallengeButton != null)
            startChallengeButton.interactable = band.IsValidForRun(out _);
    }

    void PlayBandBgm(BandMes band)
    {
        if (bandBgmPlayer == null)
            return;

        string key = band?.bgm;
        if (string.IsNullOrEmpty(key))
        {
            if (!string.IsNullOrEmpty(_lastPlayedBgmKey))
            {
                bandBgmPlayer.Stop();
                _lastPlayedBgmKey = null;
            }
            return;
        }

        if (key == _lastPlayedBgmKey)
            return;

        if (bandBgmPlayer.audioSource == null)
            bandBgmPlayer.audioSource = bandBgmPlayer.GetComponent<AudioSource>();
        if (bandBgmPlayer.audioSource == null)
        {
            Debug.LogWarning("[RoguelikeBandSelectPanel] bandBgmPlayer 缺少 AudioSource", bandBgmPlayer);
            return;
        }

        _lastPlayedBgmKey = key;
        bandBgmPlayer.SetLoop(true);
        bandBgmPlayer.PlayAudio(key);
    }

    void StopBandBgm()
    {
        if (bandBgmPlayer == null)
            return;

        bandBgmPlayer.Stop();
        _lastPlayedBgmKey = null;
    }

    void RefreshMemberPreviews(BandMes band)
    {
        ClearMemberPreviews();
        if (memberPos == null || memberPos.Count == 0)
            return;

        var members = band.startingMembers;
        for (int i = 0; i < memberPos.Count; i++)
        {
            Transform pos = memberPos[i];
            if (pos == null)
                continue;

            bool hasMember = members != null && i < members.Count && members[i] != null;
            if (hasMember && members[i].chessPre != null)
            {
                pos.gameObject.SetActive(true);
                SpawnMemberPreview(members[i], pos);
            }
            else if (hideEmptyMemberSlots)
            {
                pos.gameObject.SetActive(false);
            }
            else
            {
                pos.gameObject.SetActive(true);
            }
        }
    }

    void SpawnMemberPreview(PropertyCreator creator, Transform parent)
    {
        if (creator == null || creator.chessPre == null || parent == null)
            return;

        var go = Instantiate(creator.chessPre.gameObject, parent, false);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.SetActive(true);

        var chess = go.GetComponent<Chess>();
        if (chess != null)
        {
            chess.InitChess();
            PlayMemberIdle(chess);
        }

        _spawnedMembers.Add(go);
    }

    void PlayMemberIdle(Chess chess)
    {
        if (chess == null)
            return;

        if (isActiveAndEnabled)
        {
            StartCoroutine(PlayMemberIdleWhenReady(chess));
            return;
        }

        if (chess.gameObject.activeInHierarchy)
            chess.animatorController?.PlayIdle();
    }

    /// <summary>切换动画会晚一帧才激活成员展示父节点，需等 activeInHierarchy 后再 PlayIdle。</summary>
    IEnumerator PlayMemberIdleWhenReady(Chess chess)
    {
        if (chess == null)
            yield break;

        for (int i = 0; i < 30 && chess != null && !chess.gameObject.activeInHierarchy; i++)
            yield return null;

        if (chess != null && chess.gameObject.activeInHierarchy)
            chess.animatorController?.PlayIdle();
    }

    void ClearMemberPreviews()
    {
        for (int i = _spawnedMembers.Count - 1; i >= 0; i--)
        {
            if (_spawnedMembers[i] != null)
                Destroy(_spawnedMembers[i]);
        }
        _spawnedMembers.Clear();
    }

    void OnStartChallenge()
    {
        if (_bandSwitchInProgress)
            return;

        if (_activeRunConfig == null)
        {
            Debug.LogWarning("[RoguelikeBandSelectPanel] RunMapConfig 未设置");
            return;
        }

        if (_bands.Count == 0)
        {
            Debug.LogWarning("[RoguelikeBandSelectPanel] 无可用乐队");
            return;
        }

        var band = _bands[_currentIndex];
        if (!band.IsValidForRun(out var err))
        {
            Debug.LogWarning($"[RoguelikeBandSelectPanel] {err}");
            return;
        }

        Hide();
        RoguelikeMapPanel.OpenRun(_activeRunConfig, band);
    }

    void OnBack()
    {
        if (_bandSwitchInProgress)
            return;

        StopBandBgm();
        Hide();
        UIManage.GetView<StartUI>()?.Show();
    }
}
