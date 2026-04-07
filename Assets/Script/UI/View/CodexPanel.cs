using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 棋子图鉴面板。左栏显示已拥有棋子列表（可通过下拉框按类型/标签筛选，输入框搜索），右栏展示详细信息。
/// 详情区域的棋子展示通过 Camera 渲染到 RenderTexture 显示。
/// </summary>
public class CodexPanel : View
{
    // ==================== Inspector 引用 ====================

    [Header("筛选区域")]
    [SerializeField] private TMP_Dropdown typeDropdown;
    [SerializeField] private TMP_Dropdown tagDropdown;
    [SerializeField] private TMP_InputField searchInput;

    [Header("左栏 - 棋子列表")]
    [SerializeField] private ScrollRect iconListScroll;
    [SerializeField] private Transform iconListParent;
    [SerializeField] private GameObject codexIconPrefab;

    [Header("右栏 - 棋子预览（Camera 渲染到 RawImage）")]
    [SerializeField] private Camera previewCamera;
    [SerializeField] private RawImage detailRawImage;
    [SerializeField] private Transform previewAnchor;

    [Header("右栏 - 棋子信息文本")]
    [SerializeField] private TMP_Text detailName;
    [SerializeField] private TMP_Text detailPlantType;
    [SerializeField] private TMP_Text detailTags;
    [SerializeField] private TMP_Text detailAttributes;

    [Header("右栏 - 描述（可滚动）")]
    [SerializeField] private ScrollRect descriptionScroll;
    [SerializeField] private TMP_Text descriptionText;

    [Header("导航")]
    [SerializeField] private Button closeButton;

    // ==================== 内部状态 ====================

    private readonly List<GameObject> iconInstances = new List<GameObject>();
    private readonly List<PropertyCreator> currentList = new List<PropertyCreator>();

    private PropertyCreator selectedCreator;
    private PlantType? activeTypeFilter;
    private string activeTagFilter;
    private string searchText = "";
    private CodexIcon selectedIcon;

    private GameObject previewChessInstance;
    private RenderTexture renderTexture;

    // ==================== PlantType 中文名映射 ====================

    static readonly Dictionary<PlantType, string> PlantTypeNames = new Dictionary<PlantType, string>
    {
        { PlantType.MainPlant, "主力" },
        { PlantType.SupportPlant, "辅助" },
        { PlantType.PotPlant, "地形" },
        { PlantType.Consume, "消耗品" },
        { PlantType.LimitType, "周期限制" },
    };

    // ==================== View 生命周期 ====================

    public override void Init()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);
        if (typeDropdown != null)
            typeDropdown.onValueChanged.AddListener(OnTypeDropdownChanged);
        if (tagDropdown != null)
            tagDropdown.onValueChanged.AddListener(OnTagDropdownChanged);
        if (searchInput != null)
            searchInput.onValueChanged.AddListener(OnSearchChanged);
    }

    public override void Show()
    {
        base.Show();
        activeTypeFilter = null;
        activeTagFilter = null;
        searchText = "";
        if (searchInput != null) searchInput.text = "";
        selectedCreator = null;
        selectedIcon = null;
        EnsurePreviewSetup();
        RefreshAll();
    }

    public override void Hide()
    {
        ClearIcons();
        DestroyPreviewChess();
        selectedCreator = null;
        selectedIcon = null;
        base.Hide();
    }

    // ==================== 数据源 ====================

    List<PropertyCreator> GetOwnedCreators()
    {
        if (GameManage.instance == null) return null;
        return GameManage.instance.playerOwnedCreators ?? GameManage.instance.allChess;
    }

    // ==================== 预览管理 ====================

    void EnsurePreviewSetup()
    {
        if (detailRawImage == null) return;

        // 根据 RawImage 的 RectTransform 宽高创建匹配的 RenderTexture
        var rt = detailRawImage.rectTransform;
        int w = Mathf.Max(1, Mathf.RoundToInt(rt.rect.width));
        int h = Mathf.Max(1, Mathf.RoundToInt(rt.rect.height));

        if (renderTexture == null || renderTexture.width != w || renderTexture.height != h)
        {
            if (renderTexture != null) Destroy(renderTexture);
            renderTexture = new RenderTexture(w, h, 24);
        }

        if (previewCamera != null)
        {
            previewCamera.targetTexture = renderTexture;
            previewCamera.aspect = (float)w / h;
        }
        detailRawImage.texture = renderTexture;
    }

    void SpawnPreviewChess(PropertyCreator creator)
    {
        DestroyPreviewChess();

        if (creator == null || creator.chessPre == null) return;

        // 实例化棋子并放到锚点下
        var chessGo = Instantiate(creator.chessPre.gameObject);
        chessGo.SetActive(true);

        if (previewAnchor != null)
        {
            chessGo.transform.SetParent(previewAnchor, false);
            chessGo.transform.localPosition = Vector3.zero;
        }
        else if (previewCamera != null)
        {
            chessGo.transform.SetParent(previewCamera.transform);
            chessGo.transform.localPosition = new Vector3(0, 0, 5f);
        }

        // 初始化 Controller 以播放动画
        var chess = chessGo.GetComponent<Chess>();
        if (chess != null)
        {
            chess.InitChess();
            chess.animatorController?.PlayIdle();
        }

        previewChessInstance = chessGo;
    }

    void DestroyPreviewChess()
    {
        if (previewChessInstance != null)
        {
            Destroy(previewChessInstance);
            previewChessInstance = null;
        }
    }

    // ==================== 刷新 ====================

    void RefreshAll()
    {
        var creators = GetOwnedCreators();
        if (creators == null || creators.Count == 0)
        {
            ClearIcons();
            ClearDetailPanel();
            return;
        }
        PopulateDropdowns(creators);
        ApplyFilters();
    }

    void PopulateDropdowns(List<PropertyCreator> creators)
    {
        if (typeDropdown != null)
        {
            typeDropdown.onValueChanged.RemoveAllListeners();
            var options = new List<TMP_Dropdown.OptionData> { new TMP_Dropdown.OptionData("全部") };
            var types = new List<PlantType>();
            var seen = new HashSet<PlantType>();
            foreach (var c in creators)
                if (seen.Add(c.plantType)) types.Add(c.plantType);
            foreach (var type in types)
            {
                string label = PlantTypeNames.TryGetValue(type, out string name) ? name : type.ToString();
                options.Add(new TMP_Dropdown.OptionData(label));
            }
            typeDropdown.options = options;
            typeDropdown.value = 0;
            typeDropdown.onValueChanged.AddListener(OnTypeDropdownChanged);
        }

        if (tagDropdown != null)
        {
            tagDropdown.onValueChanged.RemoveAllListeners();
            var options = new List<TMP_Dropdown.OptionData> { new TMP_Dropdown.OptionData("全部") };
            var tags = new List<string>();
            var seen = new HashSet<string>();
            foreach (var c in creators)
                if (c.plantTags != null)
                    foreach (var tag in c.plantTags)
                        if (!string.IsNullOrEmpty(tag) && seen.Add(tag)) tags.Add(tag);
            foreach (var tag in tags)
                options.Add(new TMP_Dropdown.OptionData(tag));
            tagDropdown.options = options;
            tagDropdown.value = 0;
            tagDropdown.onValueChanged.AddListener(OnTagDropdownChanged);
        }
    }

    // ==================== 筛选回调 ====================

    void OnTypeDropdownChanged(int index)
    {
        var creators = GetOwnedCreators();
        if (creators == null) return;

        if (index <= 0)
            activeTypeFilter = null;
        else
        {
            var types = new List<PlantType>();
            var seen = new HashSet<PlantType>();
            foreach (var c in creators)
                if (seen.Add(c.plantType)) types.Add(c.plantType);
            activeTypeFilter = types[index - 1];
        }
        ApplyFilters();
    }

    void OnTagDropdownChanged(int index)
    {
        var creators = GetOwnedCreators();
        if (creators == null) return;

        if (index <= 0)
            activeTagFilter = null;
        else
        {
            var tags = new List<string>();
            var seen = new HashSet<string>();
            foreach (var c in creators)
                if (c.plantTags != null)
                    foreach (var tag in c.plantTags)
                        if (!string.IsNullOrEmpty(tag) && seen.Add(tag)) tags.Add(tag);
            activeTagFilter = tags[index - 1];
        }
        ApplyFilters();
    }

    void OnSearchChanged(string value)
    {
        searchText = value;
        ApplyFilters();
    }

    // ==================== 筛选 & 列表 ====================

    void ApplyFilters()
    {
        var creators = GetOwnedCreators();
        if (creators == null) return;

        currentList.Clear();
        foreach (var c in creators)
        {
            if (activeTypeFilter != null && c.plantType != activeTypeFilter.Value) continue;
            if (activeTagFilter != null && (c.plantTags == null || !c.plantTags.Contains(activeTagFilter))) continue;
            if (!string.IsNullOrEmpty(searchText) && !string.IsNullOrEmpty(c.chessName)
                && !c.chessName.Contains(searchText)) continue;
            currentList.Add(c);
        }

        BuildIconList(currentList);

        if (selectedCreator == null || !currentList.Contains(selectedCreator))
            SelectCreator(currentList.Count > 0 ? currentList[0] : null);
    }

    void BuildIconList(List<PropertyCreator> list)
    {
        ClearIcons();
        if (iconListParent == null) return;

        if (codexIconPrefab == null || codexIconPrefab.GetComponent<CodexIcon>() == null)
            codexIconPrefab = Resources.Load<GameObject>("Prefab/植物图鉴卡牌");
        if (codexIconPrefab == null) return;

        foreach (var creator in list)
        {
            var go = Instantiate(codexIconPrefab, iconListParent);
            var icon = go.GetComponent<CodexIcon>();
            if (icon != null)
            {
                var capturedCreator = creator;
                icon.Init(creator, () => OnIconClicked(icon, capturedCreator));
            }
            iconInstances.Add(go);
        }
    }

    void OnIconClicked(CodexIcon icon, PropertyCreator creator)
    {
        if (selectedIcon != null && selectedIcon != icon) selectedIcon.SetSelected(false);
        selectedIcon = icon;
        icon.SetSelected(true);
        SelectCreator(creator);
    }

    // ==================== 详情展示 ====================

    void SelectCreator(PropertyCreator creator)
    {
        selectedCreator = creator;
        if (creator == null)
        {
            ClearDetailPanel();
            return;
        }

        // 生成棋子预览
        SpawnPreviewChess(creator);

        if (detailName != null) detailName.text = creator.chessName;
        if (detailPlantType != null)
            detailPlantType.text = PlantTypeNames.TryGetValue(creator.plantType, out string name) ? name : creator.plantType.ToString();
        if (detailTags != null)
            detailTags.text = (creator.plantTags != null && creator.plantTags.Count > 0) ? string.Join("  ", creator.plantTags) : "无";
        if (detailAttributes != null)
            detailAttributes.text = BuildAttributeText(creator.baseProperty);
        if (descriptionText != null)
            descriptionText.text = BuildDescriptionText(creator);

        if (descriptionScroll != null)
            descriptionScroll.normalizedPosition = Vector2.up;
    }

    void ClearDetailPanel()
    {
        DestroyPreviewChess();
        if (detailName != null) detailName.text = "";
        if (detailPlantType != null) detailPlantType.text = "";
        if (detailTags != null) detailTags.text = "";
        if (detailAttributes != null) detailAttributes.text = "";
        if (descriptionText != null) descriptionText.text = "";
    }

    // ==================== 文本构建 ====================

    string BuildAttributeText(Property prop)
    {
        if (prop == null) return "";
        return $"生命值: {(int)prop.HpMax}        攻击力: {prop.attack}\n" +
               $"护甲: {prop.AR}            价格: {prop.price}\n" +
               $"冷却: {prop.CD}s           攻击距离: {prop.attackRange}\n" +
               $"移速: {prop.speed}";
    }

    string BuildDescriptionText(PropertyCreator creator)
    {
        var sb = new StringBuilder();
        if (!string.IsNullOrEmpty(creator.chessDescription))
            sb.AppendLine(creator.chessDescription);
        if (!string.IsNullOrEmpty(creator.chessEffect))
        {
            if (sb.Length > 0) sb.AppendLine();
            sb.AppendLine("【效果】");
            sb.AppendLine(creator.chessEffect);
        }
        if (!string.IsNullOrEmpty(creator.chessShortDescription))
        {
            if (sb.Length > 0) sb.AppendLine();
            sb.AppendLine("【简介】");
            sb.AppendLine(creator.chessShortDescription);
        }
        return sb.ToString();
    }

    // ==================== 清理 ====================

    void ClearIcons()
    {
        foreach (var go in iconInstances) { if (go != null) Destroy(go); }
        iconInstances.Clear();
        selectedIcon = null;
    }
}
