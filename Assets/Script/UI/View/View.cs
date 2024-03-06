using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class View : MonoBehaviour
{
    public abstract void Init();//定义一个抽象方法使得每个UI界面继承该脚本都必须实现Init方法
    public virtual void Hide() => gameObject.SetActive(false);//设置当前物体隐藏
    public virtual void Show() => gameObject.SetActive(true);//设置当前物体显示
}
