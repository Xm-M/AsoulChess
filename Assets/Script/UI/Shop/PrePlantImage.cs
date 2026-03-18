using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using UnityEngine.XR;

public class PrePlantImage : MonoBehaviour
{
    public static PrePlantImage instance;
    public Image image;
    public Animator child;
    Dictionary<HandItemType, BaseHandPanel> handDIc;
    public BaseHandPanel currentHand;

    private void Awake()
    {
        instance = this;
        handDIc = new Dictionary<HandItemType, BaseHandPanel>();
        handDIc.Add(HandItemType.Plants, new PlantsPanel());
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
        OverPlayAnim();
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
        CancelPlant?.Invoke();
        gameObject.SetActive(false);
        currentHand = null;
    }
    public void WhenPlant(Chess chess,UnityAction<Chess> Plant)
    {
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
    Shovel,//手套
    Hammer,//锤子
}