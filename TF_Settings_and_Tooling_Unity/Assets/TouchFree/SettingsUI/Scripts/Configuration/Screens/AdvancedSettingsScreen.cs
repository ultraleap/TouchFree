using System.IO;
using System;
using System.Diagnostics;

using UnityEngine;
using UnityEngine.UI;
using SFB;

using Ultraleap.TouchFree.ServiceShared;
using Ultraleap.TouchFree.Tooling.Tracking;
using Ultraleap.TouchFree.Tooling.Connection;

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

            EnableAnalyticsToggle.onValueChanged.AddListener(OnAnalyticsToggled);
            AllowImagesToggle.onValueChanged.AddListener(OnAllowImagesToggled);

            TrackingManager.RequestTrackingState(HandleTrackingResponse);

            versionPath = Path.Combine(Application.dataPath, "../Version.txt");
            PopulateVersion();

            DiagnosticAPI.OnTrackingServerInfoResponse += HandleVersionCheck;
            DiagnosticAPI.OnTrackingDeviceInfoResponse += HandleDeviceCheck;

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
            var programDataLocation = Environment.GetEnvironmentVariable("PROGRAMDATA");
            OpenFolderInFileExplorer(programDataLocation + "\\Ultraleap\\TouchFree\\Logs\\");
        }

        public void OpenTrackingLogFileLocation()
        {
            var programDataLocation = Environment.GetEnvironmentVariable("PROGRAMDATA");
            OpenFolderInFileExplorer(programDataLocation + "\\Ultraleap\\HandTracker\\Logs\\");
        }

        private void OpenFolderInFileExplorer(string folderLocation)
        {
            Process p = new Process();
            p.StartInfo = new ProcessStartInfo("explorer.exe") { Arguments = folderLocation };
            p.Start();
        }

        void OnAnalyticsToggled(bool _)
        {
            var newState = new TrackingState(null, null, null, EnableAnalyticsToggle.isOn);

            TrackingManager.RequestTrackingChange(newState);
        }

        void OnAllowImagesToggled(bool _)
        {
            var newState = new TrackingState(null, null, AllowImagesToggle.isOn, null);

            TrackingManager.RequestTrackingChange(newState);
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

        private void HandleTrackingResponse(TrackingStateResponse _response)
        {
            var allowImages = _response.allowImages.Value.content;
            var analyticsEnabled = _response.analyticsEnabled.Value.content;

            if (analyticsEnabled.HasValue)
            {
                EnableAnalyticsToggle.isOn = analyticsEnabled.Value;
            }

            if (allowImages.HasValue)
            {
                AllowImagesToggle.isOn = allowImages.Value;
            }
        }
    }
}