using UnityEditor;

public class UsingLogging : Editor {
	[MenuItem("Assets/Create/Scripting/Usings/Unity.Logging", false, 24)]
	private static void AddUnityLoggingUsings() {
		AssemblyDefinitionModifier.AddAssemblyReferencesToSelected("Unity.Logging", "Unity.Burst", "Unity.Collections");
	}

	[MenuItem("Assets/Create/Scripting/Usings/Unity.Logging", true, 24)]
	private static bool ValidateAssemblyDefinition() {
		return AssemblyDefinitionModifier.isSelectedAssembly();
	}
}
