using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ŷ �������Ч
/// ���ֵ���Ч����
/// 
/// </summary>
public abstract class ChessEffect : MonoBehaviour
{
    public abstract void InitChessEffect(Chess user,params Chess[] chesses);
    public abstract void InitChessEffect(Chess user, List<Chess> chesses);
}
