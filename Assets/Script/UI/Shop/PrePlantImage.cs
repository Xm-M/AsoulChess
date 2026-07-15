using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using TMPro;

public class PrePlantImage : MonoBehaviour
{
    public static PrePlantImage instance;
    public Image image;
    public Animator child;
    Dictionary<HandItemType, BaseHandPanel> handDIc;
    public BaseHandPanel currentHand;

    [Tooltip("铲子圆圈检测半径（世界单位）")]
    [SerializeField] float shovelDetectRadius = 1f;
    [Tooltip("铲子当前目标名称；可空，运行时会自动创建子节点")]
    [SerializeField] TMP_Text shovelTargetName;

    public float ShovelDetectRadius => shovelDetectRadius <= 0f ? 1f : shovelDetectRadius;

    private void Awake()
    {
        instance = this;
        handDIc = new Dictionary<HandItemType, BaseHandPanel>();
        handDIc.Add(HandItemType.Plants, new PlantsPanel());
        handDIc.Add(HandItemType.ColumnPlants, new PlantsPanel_Column());
        handDIc.Add(HandItemType.Shovel,new ShovelPanel());
        handDIc.Add(HandItemType.Hammer, new HammerPanel());
        gameObject.SetActive(false);
        EventController.Instance.AddListener(EventName.WhenLeaveLevel.ToString(), OnLeaveLevel);
    }
    private void OnDestroy()
    {
        EventController.Instance?.RemoveListener(EventName.WhenLeaveLevel.ToString(), OnLeaveLevel);
    }
    void OnLeaveLevel()
    {
        if (currentHand != null)
            ForceHide();
    }
    private void OnDisable()
    {
        StopAllCoroutines();
        ClearShovelTargetName();
        OverPlayAnim();
    }

    public void SetShovelTargetName(string plantName)
    {
        EnsureShovelTargetName();
        if (shovelTargetName == null) return;
        bool show = !string.IsNullOrEmpty(plantName);
        if (shovelTargetName.gameObject.activeSelf != show)
            shovelTargetName.gameObject.SetActive(show);
        shovelTargetName.text = show ? plantName : string.Empty;
    }

    public void ClearShovelTargetName()
    {
        if (shovelTargetName == null) return;
        shovelTargetName.text = string.Empty;
        if (shovelTargetName.gameObject.activeSelf)
            shovelTargetName.gameObject.SetActive(false);
    }

    void EnsureShovelTargetName()
    {
        if (shovelTargetName != null) return;

        Transform existing = transform.Find("ShovelTargetName");
        if (existing != null)
            shovelTargetName = existing.GetComponent<TMP_Text>();
        if (shovelTargetName != null) return;

        var go = new GameObject("ShovelTargetName", typeof(RectTransform));
        go.transform.SetParent(transform, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -8f);
        rt.sizeDelta = new Vector2(280f, 48f);

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = 28f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        tmp.color = Color.white;
        if (TMP_Settings.defaultFontAsset != null)
            tmp.font = TMP_Settings.defaultFontAsset;
        shovelTargetName = tmp;
        go.SetActive(false);
    }
    public void TryToPlant(UnityAction CancelPlant,UnityAction<Chess> Plant,PrePlantImage_Data data,HandItemType type)
    {
        if (!LevelManage.instance.IfGameStart) return;
        gameObject.SetActive(true);
        transform.position = Input.mousePosition;
        image.sprite = data.preSprite;
        if(currentHand != null)
        {
            Debug.Log("还有在用的");
            currentHand.CancleUse();
            StopAllCoroutines();
        }
        currentHand = handDIc[type];
        StartCoroutine(currentHand.Plants(() => WhenCancelPlant(CancelPlant),(chess)=>WhenPlant(chess,Plant),data));
    }
    public void TryToPlant(UnityAction CancelPlant, UnityAction<Chess> Plant, PrePlantImage_Data data, BaseHandPanel handPanel)
    {
        if (!LevelManage.instance.IfGameStart) return;
        if (currentHand != null)
        {
            //Debug.Log("还有在用的");
            return;
        }
        gameObject.SetActive(true);
        transform.position = Input.mousePosition;
        image.sprite = data.preSprite;
        currentHand = handPanel;
        StartCoroutine(currentHand.Plants(() => WhenCancelPlant(CancelPlant), (chess) => WhenPlant(chess, Plant), data));
    }
    
    public void PlayChildAnim(string name)
    {
        image.color = new Color(0, 0, 0, 0);
        child.gameObject.SetActive(true);
        child.Play(name);
    }
    public void OverPlayAnim()
    {
        image.color = new Color(1, 1, 1, 0.5f);
        child.gameObject.SetActive(false);
    }

    /// <summary>强制隐藏并清理（如离开锤僵尸关卡时），避免半透明方块残留</summary>
    public void ForceHide()
    {
        StopAllCoroutines();
        currentHand = null;
        ClearShovelTargetName();
        OverPlayAnim();
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (currentHand != null)
        {
            transform.position = Input.mousePosition;
        }
    }

    public void WhenCancelPlant(UnityAction CancelPlant)
    {
        ClearShovelTargetName();
        CancelPlant?.Invoke();
        gameObject.SetActive(false);
        currentHand = null;
    }
    public void WhenPlant(Chess chess,UnityAction<Chess> Plant)
    {
        ClearShovelTargetName();
        Plant?.Invoke(chess);
        gameObject.SetActive(false);
        currentHand = null;
    }

}
/// <summary>
/// 反正要什么数据就后面往这里面塞就是了
/// </summary>
[Serializable]
public class PrePlantImage_Data
{
    public Sprite preSprite;
    public PropertyCreator creator;
    public string tag;
    public DamageMessege DM;
}
public enum HandItemType
{
    Plants,//种植
    ColumnPlants,//列种植（排山倒海）
    Shovel,//手套
    Hammer,//锤子
}