using System.Net.Mime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BaseText : BaseUIComponent
{
    public Text baseText;
    public void ShowMes(string mes){
        baseText.text=mes;
    }
}
