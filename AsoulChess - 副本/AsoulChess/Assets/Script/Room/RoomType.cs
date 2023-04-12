using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomType : ScriptableObject
{

    public virtual void WhenEnterRoom(){       
        
    }
    public virtual void WhenLeaveRoom(){
        ChessFactory.instance.SaveAllChessMessage();
    }
    
}
