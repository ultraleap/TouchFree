using System.Globalization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Ultraleap.TouchFree.ServiceUI
{
    public class SliderInputFieldCombiner : MonoBehaviour
    {
        public Slider Slider;
        public InputField InputField;
        public string InputFieldValueFormat = "#0.00#";
        public OnChangeEvent onValueChanged = new OnChangeEvent();

        public float Value
        {
            get { return Slider.value; }
            set
            {
                Slider.value = value;
                OnSliderValueChanged(value);
            }
        }

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
                Mathf.Clamp(result, Slider.minValue, Slider.maxValue);
                Slider.SetValueWithoutNotify(result);
                onValueChanged?.Invoke(result);
            }
        }

        void OnSliderValueChanged(float val)
        {
            InputField.SetTextWithoutNotify(val.ToString(InputFieldValueFormat));
            onValueChanged?.Invoke(val);
        }

        public void SetValueWithoutNotify(float val)
        {
            Slider.SetValueWithoutNotify(val);
            OnSliderValueChanged(val);
        }

        public class OnChangeEvent : UnityEvent<float> { }
    }
}