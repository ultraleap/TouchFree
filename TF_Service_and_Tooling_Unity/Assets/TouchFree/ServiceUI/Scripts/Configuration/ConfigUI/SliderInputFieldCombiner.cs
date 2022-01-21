using System.Globalization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Ultraleap.TouchFree.ServiceShared;

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
            InputField.onEndEdit.AddListener(OnInputFieldValueChanged);
            Slider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        private void OnDestroy()
        {
            InputField.onEndEdit.RemoveListener(OnInputFieldValueChanged);
            Slider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }

        void OnInputFieldValueChanged(string val)
        {
            float newVal = ServiceUtility.TryParseNewStringToFloat(Slider.value, val);
            newVal = Mathf.Clamp(newVal, Slider.minValue, Slider.maxValue);

            InputField.SetTextWithoutNotify(newVal.ToString(InputFieldValueFormat));
            Slider.SetValueWithoutNotify(newVal);
            onValueChanged?.Invoke(newVal);
        }

        void OnSliderValueChanged(float val)
        {
            InputField.SetTextWithoutNotify(val.ToString(InputFieldValueFormat));
            onValueChanged?.Invoke(val);
        }

        public void SetValueWithoutNotify(float val)
        {
            Slider.SetValueWithoutNotify(val);
            InputField.SetTextWithoutNotify(val.ToString(InputFieldValueFormat));
        }

        public class OnChangeEvent : UnityEvent<float> { }
    }
}