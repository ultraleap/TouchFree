using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[DefaultExecutionOrder(-10)]
public class AdvancedSettings : MonoBehaviour
{
    [Tooltip("Used to destroy all Reach elements if Reach is 'Disabled'. The GameObject containing this Component should be last in the Array.")]
    public GameObject[] reachGameobjects;

    private void Awake()
    {
        if(Directory.Exists(Application.streamingAssetsPath))
        {
            if(File.Exists(Path.Combine(Application.streamingAssetsPath, "AdvancedSettings.txt")))
            {
                ReadSettings(File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "AdvancedSettings.txt")));
            }
        }
    }

    void ReadSettings(string _fileText)
    {
        string[] lines = File.ReadAllLines(Path.Combine(Application.streamingAssetsPath, "AdvancedSettings.txt"));

        if (_fileText.Contains("Disable Reach"))
        {
            foreach (var go in reachGameobjects)
            {
                Destroy(go);
            }
        }

        if (_fileText.Contains("Cursor Window Size"))
        {
            string lineFromFile = FindLineThatContains("Cursor Window Size", lines);

            if (lineFromFile != null)
            {
                int cursorWindowSize;
                if (int.TryParse(lineFromFile.Replace("Cursor Window Size", "").Replace(" ", ""), out cursorWindowSize))
                {
                    GlobalSettings.CursorWindowSize = cursorWindowSize;
                }
            }
        }

        if(_fileText.Contains("Quality"))
        {
            string lineFromFile = FindLineThatContains("Quality", lines);

            if (lineFromFile != null)
            {
                int qualityLevel;
                if (int.TryParse(lineFromFile.Replace("Quality", "").Replace(" ", ""), out qualityLevel))
                {
                    QualitySettings.SetQualityLevel(qualityLevel, true);
                }
            }
        }
    }
    
    static string FindLineThatContains(string _contains, string[] _fileLines = null)
    {
        if (_fileLines == null)
        {
            _fileLines = File.ReadAllLines(Path.Combine(Application.streamingAssetsPath, "AdvancedSettings.txt"));
        }

        foreach (var line in _fileLines)
        {
            if (line.Contains(_contains))
            {
                return line;
            }
        }

        return null;
    }
}