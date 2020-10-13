using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Diagnostics;

public class FileScreenUI : MonoBehaviour
{
    public InputField nameInput;
    public Dropdown fileDropdown;

    public GameObject invalidNameObject;
    public GameObject exportSuccessObject;

    public GameObject notificationBG;
    public GameObject notificationImport;
    public GameObject notificationImportFailed;
    public GameObject notificationImportCorrupt;
    public GameObject notificationImportMissing;

    private void OnEnable()
    {
        LoadFilesIntoDropdown();

        invalidNameObject.SetActive(false);
        exportSuccessObject.SetActive(false);
        notificationBG.SetActive(false);
        notificationImport.SetActive(false);
        notificationImportFailed.SetActive(false);
        notificationImportCorrupt.SetActive(false);
        notificationImportMissing.SetActive(false);
    }

    void LoadFilesIntoDropdown()
    {
        fileDropdown.ClearOptions();

        if(!Directory.Exists(PhysicalConfigurable.CustomDefaultConfigFileDirectory))
        {
            fileDropdown.interactable = false;
            return;
        }

        // find all folder names for custom default
        string[] fileNames = Directory.GetDirectories(PhysicalConfigurable.CustomDefaultConfigFileDirectory);

        if (fileNames != null && fileNames.Length > 0)
        {
            fileDropdown.interactable = true;

            foreach (var option in fileNames)
            {
                // remove the full file path from the folder path
                fileDropdown.options.Add(new Dropdown.OptionData(option.Replace(PhysicalConfigurable.CustomDefaultConfigFileDirectory, "")));
            }

            int dropdownIndex = 0;
            fileDropdown.SetValueWithoutNotify(dropdownIndex);
            fileDropdown.RefreshShownValue();
        }
        else
        {
            fileDropdown.interactable = false;
        }
    }

    public void OpenFileLocation()
    {
        // open config files folder
        ProcessStartInfo info = new ProcessStartInfo(PhysicalConfigurable.ConfigFileDirectory);
        info.Verb = "open";
        Process.Start(info);
        //ConfigurationSetupController.Instance.closeConfig = true;
    }

    public void OpenCustomFileLocation()
    {
        if (!Directory.Exists(PhysicalConfigurable.CustomDefaultConfigFileDirectory))
            return;

        ProcessStartInfo info = new ProcessStartInfo(PhysicalConfigurable.CustomDefaultConfigFileDirectory);
        info.Verb = "open";
        Process.Start(info);
        //ConfigurationSetupController.Instance.closeConfig = true;
    }

    public void ValidateName()
    {
        var invalidChars = Path.GetInvalidFileNameChars();

        foreach(var c in invalidChars)
        {
            nameInput.text = nameInput.text.Replace(c.ToString(), "");
        }
    }

    public void SaveCustomPreset()
    {
        if (nameInput.text == "")
        {
            StartCoroutine(ShowInvalidName());
            return;
        }

        ConfigFileUtils.SaveCustomDefaults(nameInput.text);
        LoadFilesIntoDropdown();

        // show the user that they exported the files
        StartCoroutine(ShowExportSuccess());
    }

    public void LoadCustomPreset()
    {
        if (!Directory.Exists(PhysicalConfigurable.CustomDefaultConfigFileDirectory))
        {
            StartCoroutine(ShowNotification(notificationImportFailed));
            return;
        }

        bool corrupt;
        bool missing;

        ConfigFileUtils.CheckForInvalidCustomDefault(fileDropdown.options[fileDropdown.value].text, out corrupt, out missing);

        if(corrupt)
        {
            StartCoroutine(ShowNotification(notificationImportCorrupt));
            return;
        }

        if (missing)
        {
            StartCoroutine(ShowNotification(notificationImportMissing));
        }
        else
        {
            StartCoroutine(ShowNotification(notificationImport));
        }

        ConfigFileUtils.LoadCustomDefaultsOnAllConfigFiles(fileDropdown.options[fileDropdown.value].text);
    }

    IEnumerator ShowInvalidName(float _time = 1f)
    {
        invalidNameObject.SetActive(true);
        yield return new WaitForSeconds(_time);
        invalidNameObject.SetActive(false);
    }

    IEnumerator ShowExportSuccess(float _time = 1f)
    {
        exportSuccessObject.SetActive(true);
        yield return new WaitForSeconds(_time);
        exportSuccessObject.SetActive(false);
    }

    IEnumerator ShowNotification(GameObject _notificationToShow, float _time = 1f)
    {
        notificationBG.SetActive(true);
        _notificationToShow.SetActive(true);

        yield return new WaitForSeconds(_time);

        notificationBG.SetActive(false);
        _notificationToShow.SetActive(false);
    }
}