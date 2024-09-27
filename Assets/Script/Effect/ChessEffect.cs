using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 哦 这个是特效
/// 出现的特效那种
/// 
/// </summary>
public abstract class ChessEffect : MonoBehaviour
{
    public abstract void InitChessEffect(Chess user,params Chess[] chesses);
    public abstract void InitChessEffect(Chess user, List<Chess> chesses);
}
