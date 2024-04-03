using SysZero.UnityExtends;

using UnityEditor;

using UnityEngine;

public class UsingUnityLogging : Editor {
	[MenuItem("Assets/Create/Scripting/Usings/UnityPackages/Logging", false, 24)]
	private static void AddUnityLoggingUsings() {
		Debug.Log("HERE1");
		AssemblyDefinition.Modifier.AddAssemblyReferencesToSelected("Unity.Logging", "Unity.Burst", "Unity.Collections");
	}

	[MenuItem("Assets/Create/Scripting/Usings/UnityPackages/Logging", true, 24)]
	private static bool ValidateAssemblyDefinition() {
		return AssemblyDefinition.Modifier.isSelectedAssembly();
	}
}
