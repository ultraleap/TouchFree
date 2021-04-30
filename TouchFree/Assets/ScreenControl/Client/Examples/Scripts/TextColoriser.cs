using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextColoriser : MonoBehaviour
{
    public Text targetText;
    public Color targetColor;

    public void SetTextColor()
    {
        targetText.color = targetColor;
    }
}
