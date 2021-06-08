using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class UpdateFPSLabel : MonoBehaviour
{
    public Text textLabel;

    void Start()
    {
        textLabel = GetComponent<Text>();
    }

    public void SetLabelTextAsInt(float value)
    {
        textLabel.text = value.ToString("F0");
    }
}
