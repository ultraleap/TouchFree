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

        protected override void OnEnable()
        {
            base.OnEnable();
            fileLocation.text = ConfigFileUtils.ConfigFileDirectory;

            // This combination allows users to highlight the text (to copy if desired) without
            // being able to edit
            fileLocation.interactable = true;
            fileLocation.readOnly = true;
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
    }
}