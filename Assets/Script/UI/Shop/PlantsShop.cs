using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;
using Sirenix.OdinInspector;
using TMPro;
/// <summary>
/// 这个是上方的那个栏，懂我意思吧
/// 包括旁边的栏，反正这个就是整个选牌体系都放在这了
/// 问题是有些图是没有这个部署栏的怎么办
/// </summary>
public class PlantsShop : View
{
    public GameObject shopSelectIconPre;//植物选择卡牌
    public Transform selectIconParent;//这个是选牌的时候的那个栏
    public GameObject shopIconPre;//植物卡牌
    public Transform shopIconParent;//这个是游戏开始的时候上面的那个牌
    public List<ShopSelectIcon> currentSelectIcons;//
    public List<ShopIcon> currentShopIcons;//这个就是你进入游戏后有的牌了
    public List<ShopSelectIcon> allSelectIcons;//这个是你拥有的棋子 就是下面那个版的
    public GameObject Shovel;//铲子 
    public int maxCount=10;
    int _baselineMaxCount = 10;
    public Animator anim;
    /// <summary>Show 前设置则使用此列表作为仓库卡池，Show 后自动清空</summary>
    public static List<PropertyCreator> OverrideCreators;
    public AudioPlayer shopAudio;
    [FoldoutGroup("初始位置")]
    public Vector2 startPos1,startPos2;
    [FoldoutGroup("初始位置")]
    public RectTransform p1,p2;

    [Header("植物详情")]
    [SerializeField] private TMP_Text detailName;
    [SerializeField] private Image detailPlantImage;
    [SerializeField] private TMP_Text detailTags;
    [SerializeField] private TMP_Text detailAttributes;
    [SerializeField] private ScrollRect descriptionScroll;
    [SerializeField] private TMP_Text descriptionText;

    public bool SelectOver { get; private set; }
    ShopIcon currentPlant;
    string Planttag = "Player";
    public override void Init()
    {
        //初始化要做什么呢？
        currentSelectIcons = new List<ShopSelectIcon>();
        allSelectIcons = new List<ShopSelectIcon>();
        currentShopIcons=new List<ShopIcon>();
        _baselineMaxCount = maxCount;
        EventController.Instance.AddListener(EventName.WhenLeaveLevel.ToString(),
            Hide);
        SelectOver = false;
    }
    public override void Show()
    {
        // 生存模式读档由 Prepare 插件调用 ShowLockedHand，不走自动开战
        bool survivalLoad = SaveLoadContext.IsLoadFromSave
            && LevelManage.instance?.currentLevel?.levelMode == LevelMode.SurvivalMode;
        if (!survivalLoad
            && SaveLoadContext.IsLoadFromSave
            && SaveLoadContext.CurrentSaveData?.plantsShopData != null)
        {
            ApplyLoadoutSlotLimitForCurrentLevel();
            ShowForLoad(SaveLoadContext.CurrentSaveData.plantsShopData);
            return;
        }
        ApplyLoadoutSlotLimitForCurrentLevel();
        base.Show();
        currentSelectIcons.Clear();
        currentShopIcons.Clear();
        allSelectIcons.Clear();
        var creatorsToShow = OverrideCreators != null && OverrideCreators.Count > 0
            ? OverrideCreators
            : (GameManage.instance.playerOwnedCreators != null && GameManage.instance.playerOwnedCreators.Count > 0
                ? GameManage.instance.playerOwnedCreators
                : GameManage.instance.allChess);
        OverrideCreators = null;
        if (creatorsToShow == null) return;
        for (int i = 0; i < creatorsToShow.Count; i++)
        {
            var creator = creatorsToShow[i];
            if (creator == null) continue;
            ShopSelectIcon selectIcon = null;
            if (creator.PlantEntrepotCardPre == null)
                selectIcon = Instantiate(shopSelectIconPre, selectIconParent).GetComponent<ShopSelectIcon>();
            else selectIcon = Instantiate(creator.PlantEntrepotCardPre, selectIconParent).GetComponent<ShopSelectIcon>();
            selectIcon.InitSelectIcon(creator);
            allSelectIcons.Add(selectIcon);
        }

        if (allSelectIcons.Count > 0 && allSelectIcons[0].select != null)
            ShowPlantDetail(allSelectIcons[0].select);
        else
            ClearPlantDetail();
        
    }
    public override void Hide()
    {
        
        Debug.Log("PlantsShop清理完成");
        //if (currentSelectIcons .Count==0) return;
        for (int i = selectIconParent.childCount - 1; i >= 0; i--)
        {
            Destroy(selectIconParent.GetChild(i).gameObject);
        }
        allSelectIcons.Clear();
        //Debug.Log(selectIconParent.childCount);
        for (int i = shopIconParent.childCount - 1; i >= 0; i--)
        {
            Destroy(shopIconParent.GetChild(i).gameObject);
        }
        //Debug.Log(shopIconParent.childCount);
        currentSelectIcons.Clear();
        currentShopIcons.Clear();
        p1.anchoredPosition = startPos1;
        p2.anchoredPosition = startPos2;
        SelectOver = false;
        ClearPlantDetail();
        base.Hide();
    }
    public void SelectPlant(ShopIcon icon)
    {
        currentPlant = icon;
    }
    public bool IfCanBuyCard(PropertyCreator c){
        //Debug.Log("尝试购买");
        if(c.IfCanBuyCard()){
            PrePlantImage_Data data = new PrePlantImage_Data();
            data.creator = c;
            data.preSprite = c.chessSprite;
            data.tag = "Player";
            PrePlantImage.instance.TryToPlant( CancelBuyCard, BuyPlant,data,HandItemType.Plants);
            //Debug.Log("可以购买");
            return true;
        }
        return false;
    }
    public void BuyPlant(Chess chess){
        SunLightPanel.instance.ChangeSunLight(-currentPlant.good.baseProperty.price);
        currentPlant.ColdDown();
        currentPlant = null;
        CancelBuyCard();
    }
    public void CancelBuyCard(){
        currentPlant=null;
    }
    public bool AddSelection(ShopSelectIcon selectIcon){
        if(!currentSelectIcons.Contains(selectIcon)&&currentSelectIcons.Count<maxCount){
            currentSelectIcons.Add(selectIcon);
            GameObject shopIcon = null;
            if (selectIcon.select.PlantCardPre==null)
                shopIcon= Instantiate(shopIconPre,shopIconParent);
            else shopIcon= Instantiate(selectIcon.select.PlantCardPre, shopIconParent);
            shopIcon.GetComponent<ShopIcon>().InitShopIcon(selectIcon);
            return true;
        }return false;
    }
    public void RemoveSelection(ShopSelectIcon selectIcon){
        currentSelectIcons.Remove(selectIcon);
    }
    /// <summary>
    /// 这是铲掉植物
    /// </summary>
    public void DigPlant(Image image)
    {
        PrePlantImage_Data data = new PrePlantImage_Data();
        data.preSprite = image.sprite;
        PrePlantImage.instance.TryToPlant(() => shopAudio.PlaySub(0,"cancel"), (Chess) => shopAudio.PlaySub(0, "dig"), data,HandItemType.Shovel);
    }
    /// <summary>
    /// 这是使用锤子
    /// </summary>
    public void UseHammer(Image image)
    {
        PrePlantImage_Data data = new PrePlantImage_Data();
        data.preSprite = image.sprite;
        data.DM = new DamageMessege();
        data.DM.damage = 900;
        PrePlantImage.instance.TryToPlant(() => shopAudio.PlaySub(0, "cancel"), (Chess) => shopAudio.PlaySub(0, "dig"), data, HandItemType.Hammer);
    }

    /// <summary>
    /// 冒险模式读档：跳过选卡，直接恢复顶栏并自动开战。
    /// </summary>
    public void ShowForLoad(PlantsShopSaveData data)
    {
        ShowLockedHand(data, autoStart: true, restoreSunLight: true);
    }

    /// <summary>
    /// 恢复已锁定手牌。生存轮间 Prepare 用 autoStart=false，等玩家点开战。
    /// </summary>
    public void ShowLockedHand(PlantsShopSaveData data, bool autoStart, bool restoreSunLight)
    {
        if (data == null) return;
        gameObject.SetActive(true);
        ClearSelectPanel();
        ClearTopBar();
        currentSelectIcons.Clear();
        allSelectIcons.Clear();
        currentShopIcons.Clear();
        SelectOver = false;
        ClearPlantDetail();

        if (restoreSunLight)
            SunLightPanel.instance.SetSunLight(data.sunLight);

        if (data.selectedCreatorIds != null && data.selectedCreatorIds.Count > 0)
        {
            foreach (var creatorId in data.selectedCreatorIds)
            {
                var creator = GetCreatorByChessName(creatorId);
                if (creator == null) continue;
                GameObject shopIconObj;
                if (creator.PlantCardPre == null)
                    shopIconObj = Instantiate(shopIconPre, shopIconParent);
                else
                    shopIconObj = Instantiate(creator.PlantCardPre, shopIconParent);
                var shopIcon = shopIconObj.GetComponent<ShopIcon>();
                shopIcon.InitShopIcon(creator);
                AddShopIcon(shopIcon);
            }
        }

        if (autoStart)
        {
            var mapPvz = MapManage.instance as MapManage_PVZ;
            if (mapPvz != null) mapPvz.WhenGameStart();
            for (int i = 0; i < shopIconParent.childCount; i++)
                shopIconParent.GetChild(i).GetComponent<ShopIcon>().SetClearColor();
            anim.Play("end");
            SelectOver = true;
        }
        else
        {
            p1.anchoredPosition = startPos1;
            p2.anchoredPosition = startPos2;
            anim.Play("end");
        }
    }

    void ClearSelectPanel()
    {
        for (int i = selectIconParent.childCount - 1; i >= 0; i--)
            Destroy(selectIconParent.GetChild(i).gameObject);
    }

    void ClearTopBar()
    {
        for (int i = shopIconParent.childCount - 1; i >= 0; i--)
            Destroy(shopIconParent.GetChild(i).gameObject);
    }

    private PropertyCreator GetCreatorByChessName(string chessName)
    {
        if (GameManage.instance?.allChess == null) return null;
        foreach (var c in GameManage.instance.allChess)
        {
            if (c != null && c.chessName == chessName)
                return c;
        }
        return null;
    }

    /// <summary>
    /// 这个是button调用的
    /// </summary>
    public void GameStart(){
        (MapManage_PVZ.instance as MapManage_PVZ ).WhenGameStart();
        for(int i = 0; i < shopIconParent.childCount; i++)
        {
            shopIconParent.GetChild(i).GetComponent<ShopIcon>().SetClearColor();
        }
        anim.Play("gameStart");
        SelectOver = true;
    }
    public void Pause()
    {
        UIManage.GetView<ParsePanel>().ShowMenuPanel();
    }
    public void AddShopIcon(ShopIcon shopIcon)
    {
        currentShopIcons.Add(shopIcon);
    }
    public void RemoveShopIcon(ShopIcon shopicon)
    {
        currentShopIcons.Remove(shopicon);
    }

    /// <summary>
    /// 仓库选卡时在详情面板展示植物信息。
    /// </summary>
    public void ShowPlantDetail(PropertyCreator creator)
    {
        if (creator == null)
        {
            ClearPlantDetail();
            return;
        }

        if (detailName != null)
            detailName.text = creator.chessName;
        if (detailPlantImage != null)
        {
            detailPlantImage.sprite = creator.chessSprite;
            detailPlantImage.enabled = creator.chessSprite != null;
        }
        if (detailTags != null)
            detailTags.text = PlantCreatorDetailHelper.BuildTagsText(creator);
        if (detailAttributes != null)
            detailAttributes.text = PlantCreatorDetailHelper.BuildAttributeText(creator.baseProperty);
        if (descriptionText != null)
            descriptionText.text = PlantCreatorDetailHelper.BuildDescriptionText(creator);
        if (descriptionScroll != null)
            descriptionScroll.normalizedPosition = Vector2.up;
    }

    void ClearPlantDetail()
    {
        if (detailName != null) detailName.text = "";
        if (detailPlantImage != null)
        {
            detailPlantImage.sprite = null;
            detailPlantImage.enabled = false;
        }
        if (detailTags != null) detailTags.text = "";
        if (detailAttributes != null) detailAttributes.text = "";
        if (descriptionText != null) descriptionText.text = "";
    }

    void ApplyLoadoutSlotLimitForCurrentLevel()
    {
        if (RoguelikeRunService.HasActiveRun
            && LevelManage.instance?.currentLevel?.roguelikeKind != RoguelikeLevelKind.None)
        {
            var state = RoguelikeRunService.State;
            if (state != null)
            {
                RoguelikeRunService.NormalizeRunStateFieldsForActiveRun(state);
                maxCount = state.GetLoadoutSlotCount(RoguelikeRunService.ResolveEconomyConfig());
                return;
            }
        }

        maxCount = _baselineMaxCount;
    }
}
