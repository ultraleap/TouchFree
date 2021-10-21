using System.Collections;
using UnityEngine;

using Ultraleap.TouchFree.Tooling.Configuration;

public class MultiCursorEnabler : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return new WaitForSeconds(2);
        ConfigurationManager.EnableMultiCursor();
    }
}
