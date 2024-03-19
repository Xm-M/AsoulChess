using PixelsoftGames.PixelUI;
using UnityEngine;

public abstract class PowerBar :MonoBehaviour
{
    public UIStatBar bar;
    public abstract void InitBar();
    public abstract void UpdateBar(float addValue);

    public abstract float GetBarValue();
}
