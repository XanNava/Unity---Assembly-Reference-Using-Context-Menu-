using UnityEditor;
using UnityEditor.Compilation;

public class UsingLogging : Editor {
	[MenuItem("Assets/Create/Scripting/Usings/Unity.Logging", false, 24)]
	private static void AddUnityLoggingUsing(MenuCommand command) {
		AssemblyDefinitionUsings.AddAssemblyReferences("Unity.Logging", "Unity.Burst", "Unity.Collections");
	}

	[MenuItem("Assets/Create/Scripting/Usings/Unity.Logging", true, 24)]
	private static bool ValidateAddUnityLoggingUsing() {
		string assetPath = CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName("Unity.Logging");
		UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);

		if (asset != null) {
			return false;
		}
		return AssemblyDefinitionUsings.isSelectedAssembly();
	}
}