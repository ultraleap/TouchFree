using System.Globalization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

namespace Ultraleap.TouchFree.ServiceUI
{
    public class SliderInputFieldCombiner : MonoBehaviour
    {
        public Slider Slider;
        public InputField InputField;
        public string InputFieldValueFormat = "#0.00#";
        public OnChangeEvent onValueChanged = new OnChangeEvent();

        float valueChangedDelay = 0;

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

            if (valueChangedDelay <= 0)
            {
                valueChangedDelay = 0.1f;
                StartCoroutine(SetValueChangedAfterDelay());
            }
            else
            {
                valueChangedDelay = 0.1f;
            }
        }

        public void SetValueWithoutNotify(float val)
        {
            Slider.SetValueWithoutNotify(val);
            InputField.SetTextWithoutNotify(val.ToString(InputFieldValueFormat));
        }

        /// <summary>
        /// Used to delay slider-based file saving to ensure we don't write too many
        /// file changes in a short period of time
        /// </summary>
        IEnumerator SetValueChangedAfterDelay()
        {
            while(valueChangedDelay > 0)
            {
                valueChangedDelay -= Time.deltaTime;
                yield return null;
            }

            OnInputFieldValueChanged(InputField.text);
        }

        public class OnChangeEvent : UnityEvent<float> { }
    }
}