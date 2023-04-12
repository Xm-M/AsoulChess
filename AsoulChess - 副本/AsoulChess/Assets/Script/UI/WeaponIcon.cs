using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class WeaponIcon : MonoBehaviour,IPointerDownHandler,IDragHandler,IPointerUpHandler,IPointerEnterHandler
{
    public static WeaponIcon dragIcon;
    Item item;
    public Vector2 startPos;
    public RectTransform rectTransform;
    public Image itemImage;

    public Chess SelectChess;
    void Awake()
    {
        startPos=transform.position;
        itemImage.color=new Color(0,0,0,0);
        item=null;
    }
    public bool AddItem(Item item){
        Debug.Log(this.item);
        if(this.item!=null)return false;
        else{
            Debug.Log("添加了新装备");
            itemImage.color=new Color(255,255,255,255);
            this.item=item;
            itemImage.sprite=item.ItemSprite;
            return true;
        }
    }
    public void RemoveItem(Chess chess){
        chess.itemController.AddItem(item);
        item=null;
        itemImage.color=new Color(0,0,0,0);
    }





    public void OnPointerDown(PointerEventData eventData){
        if(item==null)return;
        dragIcon=this;
    }
    public void OnDrag(PointerEventData data){
        if(item==null)return;
        Vector2 pos=Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position=pos;
    }
    public void OnPointerUp(PointerEventData data){
        if(SelectChess!=null){
            RemoveItem(SelectChess);
        }
        rectTransform.anchoredPosition=Vector3.zero;
        dragIcon=null;
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player")){
            SelectChess=other.GetComponent<Chess>();
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if(other.gameObject==SelectChess.gameObject){
            SelectChess=null;
        }
    }
    public void OnPointerEnter(PointerEventData data){
    
    }
}
