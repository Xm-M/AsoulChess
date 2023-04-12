using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManage : MonoBehaviour
{
    public List<RoomType> rooms;
    public static RoomManage instance;
    void Awake()
    {
        if(instance!=this)instance=this;
        Invoke("startRoom",1);
    }
    public void startRoom()=>ChangeRoom(currentRoom);
    public RoomType currentRoom;
    public void ChangeRoom(RoomType room){
        
        currentRoom.WhenLeaveRoom();
        //EventController.Instance.TriggerEvent(EventName)
        currentRoom=room;
        room.WhenEnterRoom();
        EventController.Instance.TriggerEvent(EventName.EnterNewRoom.ToString());
    }
    public void RandomRoom(){
        ChangeRoom(rooms[Random.Range(0,rooms.Count)]);
    }
    
}
