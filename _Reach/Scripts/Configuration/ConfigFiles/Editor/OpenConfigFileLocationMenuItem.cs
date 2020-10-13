using System.Diagnostics;
using UnityEditor;

public static class OpenConfigFileLocationMenuItem
{
	[MenuItem("Ultraleap/Reach/Open Config File Location")]
	static void _OpenConfigFileLocation()
	{
		Process.Start(PhysicalConfigurable.ConfigFileDirectory);
	}
}
