using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;
using Ultraleap.TouchFree.ServiceShared;

namespace Ultraleap.TouchFree.ServiceUI
{
    public abstract class ConfigUI : ConfigScreen
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            LoadConfigValuesIntoFields();
            AddValueChangedListeners();
        }

        protected virtual void OnDisable()
        {
            RemoveValueChangedListeners();
            CommitValuesToFile();
        }

        protected abstract void AddValueChangedListeners();
        protected abstract void RemoveValueChangedListeners();
        protected abstract void LoadConfigValuesIntoFields();
        protected abstract void ValidateValues();
        protected abstract void SaveValuesToConfig();
        protected abstract void CommitValuesToFile();

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