using System.Collections.Generic;
using System.IO;
using System.Linq;

using Unity.Plastic.Newtonsoft.Json;

using UnityEditor;
using UnityEditor.Compilation;

using UnityEngine;

public class AssemblyDefinitionModifier : Editor {
	public static string log;
	/// <summary>
	/// Main function to call to add from MenuContext
	/// </summary>
	/// <param name="referencesToAdd">The names, or GUID of the AssemblyReferences you want to add.</param>
	public static void AddAssemblyReferencesToSelected(params string[] referencesToAdd) {
		log = "";
		Object assembly = AssemblyDefinitionModifier.GetAssemblyObjectFrom(Selection.objects);

		string path = AssetDatabase.GetAssetPath(assembly);
		Assembly targetAssembly = GetAssemblyWithPath(path);
		string assemblyText = File.ReadAllText(Path.GetFullPath(path));
		Dictionary<string, object> assemblyData = JsonConvert.DeserializeObject<Dictionary<string, object>>(assemblyText);
		List<string> crntReferncesStrs = new List<string>();
		//Debug.Log("Loading : \n" + assemblyText + "\n");
		log += "Loading : \n" + assemblyText + "\n";

		ExtractReferences(assemblyData, ref crntReferncesStrs);

		foreach (var refs in crntReferncesStrs) {
			//Debug.Log("crntRef : " + refs + "\n");
			log += "crntRef : " + refs + "\n";
		}

		// Add references that are not already there
		List<string> referencesToAddFiltered = crntReferncesStrs;
		referencesToAddFiltered.AddRange(referencesToAdd.Except(crntReferncesStrs));

		//Debug.Log("[ADM]: if1=" + (referencesToAddFiltered.Count > 0));
		if (referencesToAddFiltered.Count > 0) {
			bool isValidOperation = true;
			List<Assembly> newAssemblyReferences;
			isValidOperation = GetAssemblies(referencesToAddFiltered, out newAssemblyReferences);

			//Debug.Log("[ADM]: if2=" + !isValidOperation);
			if (!isValidOperation) {
				return;
			}

			SetAssemblyReferences(targetAssembly, newAssemblyReferences.ToArray());

			RefreshEditorAsset(targetAssembly);

			//Debug.Log("[ADM]: References added to " + targetAssembly.name + ": " + string.Join(", ", referencesToAddFiltered) + "\n");
			log += "References added to " + targetAssembly.name + ": " + string.Join(", ", referencesToAddFiltered) + "\n";
		}
		else {
			log += "WRN All references already exist in " + targetAssembly.name + "\n";
		}

		//Debug.Log(log);
	}

	/// <summary>
	/// Main function to call to add from Scripts
	/// </summary>
	/// <param name="assembly">Object of the assembly you want to add.</param>
	/// <param name="referencesToAdd">The names, or GUID of the AssemblyReferences you want to add.</param>
	public static void AddAssemblyReferencesToPassed(Object assembly, params string[] referencesToAdd) {
		string path = AssetDatabase.GetAssetPath(assembly);
		Assembly targetAssembly = GetAssemblyWithPath(path);
		string assemblyText = File.ReadAllText(Path.GetFullPath(path));
		Dictionary<string, object> assemblyData = JsonConvert.DeserializeObject<Dictionary<string, object>>(assemblyText);
		List<string> crntReferncesStrs = new List<string>();
		log += "Loading : \n" + assemblyText + "\n";

		ExtractReferences(assemblyData, ref crntReferncesStrs);

		foreach (var refs in crntReferncesStrs) {
			log += "crntRef : " + refs + "\n";
		}

		// Add references that are not already there
		List<string> referencesToAddFiltered = crntReferncesStrs;
		referencesToAddFiltered.AddRange(referencesToAdd.Except(crntReferncesStrs));

		if (referencesToAddFiltered.Count > 0) {
			bool isValidOperation = true;
			List<Assembly> newAssemblyReferences;
			isValidOperation = GetAssemblies(referencesToAddFiltered, out newAssemblyReferences);

			if (!isValidOperation) {
				return;
			}

			SetAssemblyReferences(targetAssembly, newAssemblyReferences.ToArray());

			RefreshEditorAsset(targetAssembly);

			log += "References added to " + targetAssembly.name + ": " + string.Join(", ", referencesToAddFiltered) + "\n";
		}
		else {
			log += "WRN All references already exist in " + targetAssembly.name + "\n";
		}

		//Debug.Log(log);
	}

	private static void ExtractReferences(Dictionary<string, object> assemblyData, ref List<string> crntReferncesStrs) {
		string[] crntReferences = new string[0];

		if (assemblyData.ContainsKey("references")) {
			log += "Extracting : References \n";

			crntReferences = assemblyData["references"].ToString().Split(new char[] { '"' });

			foreach (var str in crntReferences) {
				if (crntReferences.Length == 1) {
					continue;
				}
				if (crntReferences.Length >= 3) {
					if (str.Contains('[') || str.Contains(']') || str.Contains(',')) {
						continue;
					}

					crntReferncesStrs.Add(str);
				}
			}
		}
		else {
			log += "Extracting : No References \n";
		}
	}

	private static void SetAssemblyReferences(Assembly assembly, Assembly[] references) {
		string asmdefPath = CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName(assembly.name);
		//Debug.Log("[ADM]: Path= " + asmdefPath + "\n");
		log += "[ADM]: Path: " + asmdefPath + "\n";
		if (!string.IsNullOrEmpty(asmdefPath)) {
			string asmdefText = File.ReadAllText(asmdefPath);

			Dictionary<string, object> asmdefData = JsonConvert.DeserializeObject<Dictionary<string, object>>(asmdefText);

			//Debug.Log("[ADM]:" + !asmdefData.ContainsKey("references") + "\n");
			log += "[ADM]:" + !asmdefData.ContainsKey("references") + "\n";
			if (!asmdefData.ContainsKey("references")) {
				asmdefData["references"] = references.Select(r => r.name).ToList();
			}
			else {
				// If "references" key exists, append the provided references to it
				var existingReferences = (asmdefData["references"] as object[])?.Select(r => r.ToString()).ToList() ?? new List<string>();

				foreach (var refs in existingReferences) {
					//Debug.Log("[ADM]: REFS : " + refs + "\n");
					log += "[ADM]: REFS : " + refs + "\n";
				}

				existingReferences.AddRange(references.Select(r => r.name));
				asmdefData["references"] = existingReferences;
			}

			string updatedJson = JsonConvert.SerializeObject(asmdefData, Formatting.Indented);
			//Debug.Log("[ADM]: " + updatedJson + "\n");
			log += "[ADM]: " + updatedJson + "\n";

			File.WriteAllText(asmdefPath, updatedJson);

			//Debug.Log("[ADM]: References section updated.\n");
			log += "[ADM]: References section updated.\n";
			return;
		}

		//Debug.Log("[ADM][WRN]: Failed to find .asmdef file for assembly: " + assembly.name + "\n");
		log += "[ADM][WRN]: Failed to find .asmdef file for assembly: " + assembly.name + "\n";
	}

	private static bool GetAssemblies(List<string> assembliesToGet, out List<Assembly> results) {
		results = new List<Assembly>();
		bool isValidOperation = true;

		foreach (string reference in assembliesToGet) {
			//Debug.Log("[ADM]: ADM.GetAssemblies(): Getting " + reference);

			Assembly referenceAssembly;
			if (reference.Contains("GUID")) {
				//Debug.Log("[ADM]: ADM.GetAssemblies(): Getting assembly by GUID.");
				referenceAssembly = GetAssemblyWithGUID(reference);
			}
			else {
				//Debug.Log("[ADM]: ADM.GetAssemblies(): Getting assembly by Name.");
				referenceAssembly = GetAssemblyWithName(reference);
			}

			if (referenceAssembly != null) {
				results.Add(referenceAssembly);
			}
			else {
				//Debug.Log("[ADM]:" + reference + " assembly not found.\n");
				log += reference + " assembly not found.\n";
				isValidOperation = false;
			}
		}

		return isValidOperation;
	}

	private static Object GetAssemblyObjectFrom(params Object[] selectedObjects) {
		if (selectedObjects == null || selectedObjects.Length == 0) {
			log += "No assembly definition file selected.";
			return null;
		}

		foreach (Object selectedObject in selectedObjects) {
			string path = AssetDatabase.GetAssetPath(selectedObject);

			if (!AssetDatabase.IsValidFolder(path)) {
				Assembly assemblyDefinition = GetAssemblyWithPath(path);

				if (assemblyDefinition != null)
					return selectedObject;
			}
		}

		return null;
	}
	private static Assembly GetAssemblyWithPath(string path) {
		Assembly[] playerAssemblies = CompilationPipeline.GetAssemblies(AssembliesType.Player);

		log += "ParsingAssemblies";
		foreach (var assembly in playerAssemblies) {
			if (path.Contains(assembly.name)) {
				return assembly;
			}
		}

		return null;
	}
	private static Assembly GetAssemblyWithName(string name) {
		Assembly[] playerAssemblies = CompilationPipeline.GetAssemblies(AssembliesType.Player);

		log += "ParsingAssemblies";
		foreach (var assembly in playerAssemblies) {
			if (assembly.name == name) {
				return assembly;
			}
		}

		return null;
	}
	private static Assembly GetAssemblyWithGUID(string name) {
		Assembly[] playerAssemblies = CompilationPipeline.GetAssemblies(AssembliesType.Player);
		name = name.Replace("GUID:", "");
		string assetPath = AssetDatabase.GUIDToAssetPath(name);

		return GetAssemblyWithPath(assetPath);
	}

	private static void RefreshEditorAsset(Assembly assembly) {
		// Find the corresponding asset in the project
		string assetPath = CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName(assembly.name);
		UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);

		if (asset != null) {
			EditorUtility.SetDirty(asset);
		}
		else {
			log += "WRN Failed to find asset for assembly: " + assembly.name;
		}

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		CompilationPipeline.RequestScriptCompilation();
	}

	public static bool isSelectedAssembly() {
		Object[] selectedObjects = Selection.objects;
		if (selectedObjects != null && selectedObjects.Length == 1) {
			string path = AssetDatabase.GetAssetPath(selectedObjects[0]);
			return path.EndsWith(".asmdef");
		}
		return false;
	}
}
