using UnityEngine;
using UnityEngine.UI;
using SFB;

using Ultraleap.TouchFree.ServiceShared;
using System.IO;
using UnityEditor;
using System;

namespace Ultraleap.TouchFree.ServiceUI
{
    public class AdvancedSettingsScreen : ConfigScreen
    {
        [Header("File Location")]
        public InputField fileLocation;

        [Header("Tracking Settings")]
        public Toggle EnableAnalyticsToggle;
        public Toggle AllowImagesToggle;

        [Header("About")]
        public Text versionText;
        public Text trackingVersionText;
        public Text cameraDeviceIdText;
        public Text cameraDeviceFirmwareText;

        string versionPath;

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
            AllowImagesToggle.onValueChanged.AddListener(OnAllowImagesToggled);

            DiagnosticAPIManager.diagnosticAPI.GetAnalyticsMode();
            DiagnosticAPIManager.diagnosticAPI.GetAllowImages();

            versionPath = Path.Combine(Application.dataPath, "../Version.txt");
            PopulateVersion();

            DiagnosticAPI.OnTrackingServerInfoResponse += HandleVersionCheck;
            DiagnosticAPI.OnTrackingDeviceInfoResponse += HandleDeviceCheck;
            DiagnosticAPI.OnAllowImagesResponse += HandleAllowImagesCheck;

            DiagnosticAPIManager.diagnosticAPI.GetDeviceInfo();

            if (!string.IsNullOrWhiteSpace(DiagnosticAPIManager.diagnosticAPI.trackingServiceVersion))
            {
                HandleVersionCheck();
            }
            else
            {
                DiagnosticAPIManager.diagnosticAPI.GetServerInfo();
            }
        }

        protected virtual void OnDisable()
        {
            EnableAnalyticsToggle.onValueChanged.RemoveListener(OnAnalyticsToggled);
            AllowImagesToggle.onValueChanged.RemoveListener(OnAllowImagesToggled);
            DiagnosticAPI.OnTrackingServerInfoResponse -= HandleVersionCheck;
            DiagnosticAPI.OnTrackingDeviceInfoResponse -= HandleDeviceCheck;
            DiagnosticAPI.OnAllowImagesResponse -= HandleAllowImagesCheck;
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

        public void OpenTouchFreeLogFileLocation()
        {
            OpenFolderInFileExplorer(fileLocation.text);
        }

        public void OpenTrackingLogFileLocation()
        {
            var programDataLocation = Environment.GetEnvironmentVariable("PROGRAMDATA");
            OpenFolderInFileExplorer(programDataLocation + "\\Ultraleap\\HandTracker\\Logs\\");
        }

        private void OpenFolderInFileExplorer(string folderLocation)
        {
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo = new System.Diagnostics.ProcessStartInfo("explorer.exe");
            p.StartInfo.Arguments = folderLocation;
            p.Start();
        }

        void OnAnalyticsToggled(bool _)
        {
            DiagnosticAPIManager.diagnosticAPI.SetAnalyticsMode(EnableAnalyticsToggle.isOn);
        }

        void OnAllowImagesToggled(bool _)
        {
            DiagnosticAPIManager.diagnosticAPI.SetAllowImages(AllowImagesToggle.isOn);
        }

        void HandleAnalyticsEnabledResponse(bool analyticsEnabled)
        {
            EnableAnalyticsToggle.isOn = analyticsEnabled;
        }

        public void PrivacyPolicyPressed()
        {
            Application.OpenURL("https://www.ultraleap.com/privacy-policy");
        }

        void PopulateVersion()
        {
            string version = "N/A";

            if (File.Exists(versionPath))
            {
                var fileLines = File.ReadAllLines(versionPath);
                foreach (var line in fileLines)
                {
                    if (line.Contains("TouchFree Service Version"))
                    {
                        version = line.Replace("TouchFree Service Version: ", "");
                        break;
                    }
                }
            }
            if (version != "N/A")
            {
                version = "v" + version;
            }
            versionText.text = version;
        }

        private void HandleVersionCheck()
        {
            trackingVersionText.text = DiagnosticAPIManager.diagnosticAPI.trackingServiceVersion?.Split('-')?[0];
        }

        private void HandleDeviceCheck()
        {
            cameraDeviceIdText.text = DiagnosticAPIManager.diagnosticAPI.connectedDeviceSerial;
            string firmwareVersion = DiagnosticAPIManager.diagnosticAPI.connectedDeviceFirmware;
            if (!string.IsNullOrWhiteSpace(firmwareVersion) && !firmwareVersion.StartsWith("v"))
            {
                firmwareVersion = "v" + firmwareVersion;
            }
            cameraDeviceFirmwareText.text = firmwareVersion;
        }

        private void HandleAllowImagesCheck()
        {
            AllowImagesToggle.isOn = DiagnosticAPIManager.diagnosticAPI.allowImages ?? false;
        }
    }
}