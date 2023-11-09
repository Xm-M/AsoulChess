using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// roomtype就是具体的每个房间对吧
/// 其实就是每个场景了
/// 一个roomType对应一个场景
/// </summary>
public class RoomType : ScriptableObject
{
    public string sceneName;
    public string roomName;

    public virtual void WhenEnterRoom(){       
        
    }
    public virtual void WhenLeaveRoom(){
         
    }
    
    public virtual void WhenStayRoom(){

    }
}
