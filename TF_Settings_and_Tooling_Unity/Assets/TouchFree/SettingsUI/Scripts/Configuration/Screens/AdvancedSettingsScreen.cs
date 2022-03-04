using UnityEngine;
using UnityEngine.UI;
using SFB;

using Ultraleap.TouchFree.ServiceShared;
using System.IO;

namespace Ultraleap.TouchFree.ServiceUI
{
    public class AdvancedSettingsScreen : ConfigScreen
    {
        [Header("File Location")]
        public InputField fileLocation;

        [Header("Analytics")]
        public Toggle EnableAnalyticsToggle;

        protected override void OnEnable()
        {
            base.OnEnable();
            fileLocation.text = ConfigFileUtils.ConfigFileDirectory;

            // This combination allows users to highlight the text (to copy if desired) without
            // being able to edit
            fileLocation.interactable = true;
            fileLocation.readOnly = true;

            if (DiagnosticAPIManager.diagnosticAPI == null)
            {
                DiagnosticAPIManager.diagnosticAPI = new DiagnosticAPI(this);
            }

            DiagnosticAPI.OnGetAnalyticsEnabledResponse += HandleAnalyticsEnabledResponse;

            EnableAnalyticsToggle.onValueChanged.AddListener(OnAnalyticsToggled);

            DiagnosticAPIManager.diagnosticAPI.GetAnalyticsMode();
        }

        protected virtual void OnDisable()
        {
            EnableAnalyticsToggle.onValueChanged.RemoveListener(OnAnalyticsToggled);
        }

        public void SetFileLocation()
        {
            string[] paths = StandaloneFileBrowser.OpenFolderPanel("", Path.GetDirectoryName(fileLocation.text), false);

            if (paths.Length > 0)
            {
                if (ConfigFileUtils.ChangeConfigFileDirectory(paths[0]))
                {
                    fileLocation.text = paths[0];
                    ConfigManager.LoadConfigsFromFiles();
                }
            }
        }

        void OnAnalyticsToggled(bool _)
        {
            DiagnosticAPIManager.diagnosticAPI.SetAnalyticsMode(EnableAnalyticsToggle.isOn);
        }

        void HandleAnalyticsEnabledResponse(bool analyticsEnabled)
        {
            EnableAnalyticsToggle.isOn = analyticsEnabled;
        }

        public void PrivacyPolicyPressed()
        {
            Application.OpenURL("https://www.ultraleap.com/privacy-policy");
        }
    }
}