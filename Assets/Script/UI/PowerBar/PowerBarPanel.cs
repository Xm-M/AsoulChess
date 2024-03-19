using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerBarPanel : View
{
    public static List<PowerBar> powerBars ;
     
    public override void Init()
    {
        powerBars = new List<PowerBar>();
        var prefabs = Resources.LoadAll<Transform>("PowerBar");//º”‘ÿÀ˘”–PowerBar;
        foreach (Transform view in prefabs)
        {
            Transform prefab = Instantiate(view, transform);
            powerBars.Add(prefab.GetComponent<PowerBar>());
        }
        EventController.Instance.AddListener(EventName.GameOver.ToString(), Hide);
        EventController.Instance.AddListener(EventName.GameStart.ToString(), Show);
    }
    public override void Show()
    {
        base.Show();
        for(int i = 0; i < powerBars.Count; i++) {
            powerBars[i].InitBar();
        }
    }
    public override void Hide()
    {
        base .Hide();
        for(int i = 0; i < powerBars.Count; i++)
            powerBars[i].gameObject.SetActive(false);
    }
    public static T GetView<T>() where T:PowerBar{
        for(var i = 0; i < powerBars.Count; i++)
        {
            if(powerBars[i] is T value)
            {
                if (!powerBars[i].gameObject.activeSelf)
                    powerBars[i].gameObject.SetActive(true);
                return value;
            }
        }
        return null;
    }
}
