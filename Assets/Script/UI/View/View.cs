using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class View : MonoBehaviour
{
    public abstract void Init();//����һ�����󷽷�ʹ��ÿ��UI����̳иýű�������ʵ��Init����
    public virtual void Hide() => gameObject.SetActive(false);//���õ�ǰ��������
    public virtual void Show() => gameObject.SetActive(true);//���õ�ǰ������ʾ
}
