using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Ultraleap.ScreenControl.Core
{
    public abstract class ConfigUI : MonoBehaviour
    {
        Coroutine saveConfigCoroutine;

        protected virtual void OnEnable()
        {
            LoadConfigValuesIntoFields();
            AddValueChangedListeners();
        }

        protected virtual void OnDisable()
        {
            RemoveValueChangedListeners();
            if (saveConfigCoroutine != null)
            {
                CommitValuesToFile();
            }
        }

        protected abstract void AddValueChangedListeners();
        protected abstract void RemoveValueChangedListeners();
        protected abstract void LoadConfigValuesIntoFields();
        protected abstract void ValidateValues();
        protected abstract void SaveValuesToConfig();
        protected abstract void CommitValuesToFile();

        protected float TryParseNewStringToFloat(ref float _original, string _newText, bool _convertToStorageUnits = false, bool _convertToDisplayUnits = false)
        {
            // Match any character that is not period (.), hypen (-), or numbers 0 to 9, and strip them out.
            _newText = Regex.Replace(_newText, "[^.0-9-]", "");

            float val;

            if (!float.TryParse(_newText, NumberStyles.Number, CultureInfo.CurrentCulture, out val))
                val = _original; // string was not compatible!

            if (_convertToDisplayUnits)
            {
                val = ScreenControlUtility.ToDisplayUnits(val);
            }
            else if (_convertToStorageUnits)
            {
                val = ScreenControlUtility.FromDisplayUnits(val);
            }

            return val;
        }

        protected void OnValueChanged(string _)
        {
            OnValueChanged();
        }

        protected void OnValueChanged(float _)
        {
            OnValueChanged();
        }

        protected void OnValueChanged(int _)
        {
            OnValueChanged();
        }

        protected void OnValueChanged(bool _)
        {
            OnValueChanged();
        }

        protected void OnValueChanged()
        {
            ValidateValues();
            SaveValuesToConfig();
        }
    }
}