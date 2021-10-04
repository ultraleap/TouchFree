using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ultraleap.TouchFree.ServiceShared;

public class PermissionController : MonoBehaviour
{
    public static bool hasFilePermission = true;

    // Force a save which causes the check for file access and if we do not
    // have it, sets hasFilePermission to false
    private void Awake()
    {
        ConfigManager.PhysicalConfig.SaveConfig();
    }
}