using UnityEngine;

namespace Ultraleap.TouchFree.ServiceUI
{
    public class ConfigScreen : MonoBehaviour
    {
        public GameObject[] permissionsBlockers;

        protected virtual void OnEnable()
        {
            if (!PermissionController.hasFilePermission)
            {
                foreach (var blocker in permissionsBlockers)
                {
                    blocker.SetActive(true);
                }
            }
        }
    }
}