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

        protected void RestartSaveConfigTimer()
        {
            if (saveConfigCoroutine != null)
            {
                StopCoroutine(saveConfigCoroutine);
            }
            saveConfigCoroutine = StartCoroutine(SaveConfigCoroutine());

            IEnumerator SaveConfigCoroutine()
            {
                yield return new WaitForSeconds(3f);
                CommitValuesToFile();
                saveConfigCoroutine = null;
            }
        }

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

        protected string TryParseHexColour(ref string _original, string _newColourHex)
        {
            // Strip out non-hexidecimal characters, including #.
            _newColourHex = Regex.Replace(_newColourHex, @"[^0-9abcdefABCDEF]+", "");

            if (!_newColourHex.StartsWith("#"))
                _newColourHex = "#" + _newColourHex; // Add hex denotion # to start of the string.

            if (!ColorUtility.TryParseHtmlString(_newColourHex, out _))
                return _original; // string was not compatible!

            return _newColourHex;
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