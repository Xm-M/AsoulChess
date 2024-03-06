using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIRoot : MonoBehaviour
{
    public static Dictionary<string, View> m_views = new Dictionary<string, View>(); //定义一个字典存储所有UI界面
    private void Awake()
    {
        var prefabs = Resources.LoadAll<Transform>("UIPrefab");//加载UIPrefab文件夹下的所有UI预制体
        foreach (Transform view in prefabs)
        {
            if (!m_views.ContainsKey(view.name))
            {
                Transform prefab = Instantiate(view, transform);
                prefab.name = prefab.name.Replace("(Clone)", "");//去除实例化物体的(Clone)后缀，和预制体名字统一
                m_views.Add(prefab.name, prefab.GetComponent<View>());//存入字典
            }
        }
    }
}
