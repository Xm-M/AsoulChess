using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector.Editor;
/// <summary>
/// ��������˼��һ��Ҫ��ʲô��
/// </summary>
[CustomEditor(typeof(Chess))]
public class ChessEditor : OdinEditor
{
    protected override void OnEnable()
    {
        // ����Ƿ��Ѿ��������Ҫ����� 
        base.OnEnable();
        Chess myScript = (Chess)target;
        if (myScript.GetComponent<Collider2D>() == null)
        {
            myScript.gameObject.AddComponent<BoxCollider2D>();
            myScript.GetComponent<Collider2D>().isTrigger = true;
            Debug.Log("�Զ������BoxCollider2D���");
        }
        if (myScript.GetComponent<Animator>() == null)
        {
            myScript.gameObject.AddComponent<Animator>();
            myScript.animator = myScript.GetComponent<Animator>();
        }
        if (myScript.transform.childCount == 0)
        {
            GameObject childObject = new GameObject("sprite");
            childObject.transform.SetParent(myScript.transform);
            childObject.transform.localPosition = Vector3.zero;
            childObject.AddComponent<SpriteRenderer>();
            myScript.sprite = childObject.GetComponent<SpriteRenderer>();
            myScript.sprite.sortingLayerName = "Chess";
            myScript.sprite.sortingOrder = 1;
            Debug.Log("�Զ�������������");
        }
    }
}//
