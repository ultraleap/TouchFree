using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

namespace Ultraleap.ScreenControl.Core
{
    public class HomeScreen : MonoBehaviour
    {
        [Space]
        public GameObject userInterfaceSettingsScreen;

        public Text versionText;
        string versionPath;

        private void Awake()
        {
            versionPath = Path.Combine(Application.dataPath, "../Version.txt");
            PopulateVersion();
        }

        void PopulateVersion()
        {
            string version = "N/A";

            if (File.Exists(versionPath))
            {
                var fileLines = File.ReadAllLines(versionPath);
                foreach (var line in fileLines)
                {
                    if (line.Contains("ScreenControl Service Version"))
                    {
                        version = line.Replace("ScreenControl Service Version: ", "");
                        break;
                    }
                }
            }
            versionText.text = "Version " + version;
        }

        public void ChangeToSetupCamera()
        {
            Debug.Log("Need to launch Service UI here");
        }

        public void ChangeToUserInterfaceSettings()
        {
            ScreenManager.Instance.ChangeScreen(userInterfaceSettingsScreen);
        }
    }
}