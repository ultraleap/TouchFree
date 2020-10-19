using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-1)]
public class SliderAndInputFieldMirrorValues : MonoBehaviour
{
    public Slider Slider;
    public InputField InputField;
    public string InputFieldValueFormat = "#0.00#";

    private void Awake()
    {
        InputField.onValueChanged.AddListener(OnInputFieldValueChanged);
        Slider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    private void OnDestroy()
    {
        InputField.onValueChanged.RemoveListener(OnInputFieldValueChanged);
        Slider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }

    void OnInputFieldValueChanged(string val)
    {
        if (float.TryParse(val, NumberStyles.Number, CultureInfo.CurrentCulture, out float result))
        {
            Slider.SetValueWithoutNotify(result);
        }
    }

    void OnSliderValueChanged(float val)
    {
        InputField.SetTextWithoutNotify(val.ToString(InputFieldValueFormat));
    }
}
