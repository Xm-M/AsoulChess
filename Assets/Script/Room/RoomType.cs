using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// roomtype���Ǿ����ÿ������԰�
/// ��ʵ����ÿ��������
/// һ��roomType��Ӧһ������
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
