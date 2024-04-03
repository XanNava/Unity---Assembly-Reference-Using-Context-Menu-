using UnityEditor;

public class UsingsSerinex : Editor {
	[MenuItem("Assets/Create/Scripting/Usings/Sirenix/Attributes", false, 24)]
	private static void AddUnityLoggingUsings() {
		SysZero.UnityExtends.AssemblyDefinition.Modifier.AddAssemblyReferencesToSelected("Sirenix.Utilities.dll", "Sirenix.OdinInspector.Attributes.dll");
	}

	[MenuItem("Assets/Create/Scripting/Usings/Sirenix/Attributes", true, 24)]
	private static bool ValidateAssemblyDefinition() {
		return SysZero.UnityExtends.AssemblyDefinition.Modifier.isSelectedAssembly();
	}
}