using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleLabelSetter : MonoBehaviour
{
    public string onText = "Disable";
    public string offText = "Enable";

    public Toggle targetToggle;
    public Text targetText;

    private void OnEnable()
    {
        StartCoroutine(UpdateAfterTime());
    }

    public void SetToggleText()
    {
        if(targetToggle.isOn)
        {
            targetText.text = onText;
        }
        else
        {
            targetText.text = offText;
        }
    }

    IEnumerator UpdateAfterTime()
    {
        yield return new WaitForSeconds(0.1f);
        SetToggleText();
    }
}