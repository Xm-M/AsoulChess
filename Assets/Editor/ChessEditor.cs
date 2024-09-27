using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector.Editor;
/// <summary>
/// 那我们来思考一下要做什么吧
/// </summary>
[CustomEditor(typeof(Chess))]
public class ChessEditor : OdinEditor
{
    protected override void OnEnable()
    {
        // 检查是否已经添加了需要的组件 
        base.OnEnable();
        Chess myScript = (Chess)target;
        if (myScript.GetComponent<Collider2D>() == null)
        {
            myScript.gameObject.AddComponent<BoxCollider2D>();
            myScript.GetComponent<Collider2D>().isTrigger = true;
            Debug.Log("自动添加了BoxCollider2D组件");
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
            Debug.Log("自动创建了子物体");
        }
    }
}//
