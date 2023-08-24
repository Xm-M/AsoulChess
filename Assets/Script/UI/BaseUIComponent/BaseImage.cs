using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BaseImage : BaseUIComponent
{
    public Image baseImage;
    public void ShowImage(Sprite sprite){
        baseImage.sprite=sprite;
    }    
}
