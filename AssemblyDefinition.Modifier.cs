using System.Collections.Generic;
using System.IO;
using System.Linq;

using Newtonsoft.Json;

using UnityEditor;
using UnityEditor.Compilation;

using UnityEngine;
namespace SysZero.UnityExtends {
	public class AssemblyDefinition {
		public class Modifier : Editor {
			public static string log;
			/// <summary>
			/// Main function to call to add from MenuContext
			/// </summary>
			/// <param name="referencesToAdd">The names, or GUID of the AssemblyReferences(including precompiled.dll) you want to add.</param>
			public static void AddAssemblyReferencesToSelected(params string[] referencesToAdd) {
				Debug.Log("HERE2");
				log = "";

				UnityEngine.Object assemblyObject = AssemblyDefinition.Modifier.GetAssemblyObjectFrom(Selection.objects);

				List<string> precompReferencesToAdd = referencesToAdd.Where(x => x.Contains(".dll")).ToList();
				List<string> referencesToAddList = referencesToAdd.Except(precompReferencesToAdd).ToList();



				string path = AssetDatabase.GetAssetPath(assemblyObject);
				Assembly targetAssembly = GetAssemblyWithPath(path);
				string assemblyText = File.ReadAllText(Path.GetFullPath(path));

				AssemblyDefinitionRequest assemblyRequest = JsonConvert.DeserializeObject<AssemblyDefinitionRequest>(assemblyText);
				assemblyRequest.filePath = path;
				assemblyRequest.AssemblyText = assemblyText;
				assemblyRequest.editorObject = assemblyObject;

				log += "[DBG][AD.M] AddAssemblyReferencesToSelected(): Loading Assembly \n" + assemblyText + "\n";
				UnityEngine.Debug.Log(log);
				foreach (var refs in assemblyRequest.references) {
					log += "[DBG][AD.M] AddAssemblyReferencesToSelected(): crntRef = " + refs + "\n";
				}

				// Add all new references to current references.
				List<string> newReferencesToAdd = referencesToAddList.Except(assemblyRequest.references).ToList();
				assemblyRequest.references.AddRange(newReferencesToAdd);

				List<string> newPrecompReferencesToAdd = precompReferencesToAdd.Except(assemblyRequest.precompiledReferences).ToList();
				assemblyRequest.precompiledReferences.AddRange(newPrecompReferencesToAdd);

				foreach (var refs in newReferencesToAdd) {
					log += "[DBG][AD.M] AddAssemblyReferencesToSelected(): Adding Ref = " + refs + "\n";
				}

				foreach (var refs in newPrecompReferencesToAdd) {
					log += "[DBG][AD.M] AddAssemblyReferencesToSelected(): Adding PrecompRef = " + refs + "\n";
				}

				if (newReferencesToAdd.Count > 0 || newPrecompReferencesToAdd.Count > 0) {
					SetAssemblyReferences(assemblyRequest);

					RefreshEditorAsset(targetAssembly);

					log += "[DBG][AD.M] AddAssemblyReferencesToSelected(): References added to " + targetAssembly.name + ": " + string.Join(", ", newReferencesToAdd) + "\n";
				}
				else {
					log += "[WRN][AD.M] AddAssemblyReferencesToSelected(): All references already exist in " + targetAssembly.name + "\n";
				}

				UnityEngine.Debug.Log(log);
			}

			/// <summary>
			/// Main function to call to add from Scripts
			/// </summary>
			/// <param name="assemblyObject">Object of the assembly you want to add.</param>
			/// <param name="referencesToAdd">The names, or GUID of the AssemblyReferences(including precompiled.dll) you want to add.</param>
			public static void AddAssemblyReferencesToPassed(UnityEngine.Object assemblyObject, params string[] referencesToAdd) {
				List<string> precompReferencesToAdd = referencesToAdd.Where(x => x.Contains(".dll")).ToList();
				List<string> referencesToAddList = referencesToAdd.Except(precompReferencesToAdd).ToList();
				string path = AssetDatabase.GetAssetPath(assemblyObject);
				Assembly targetAssembly = GetAssemblyWithPath(path);
				string assemblyText = File.ReadAllText(Path.GetFullPath(path));

				AssemblyDefinitionRequest assemblyRequest = JsonConvert.DeserializeObject<AssemblyDefinitionRequest>(assemblyText);
				assemblyRequest.AssemblyText = assemblyText;
				assemblyRequest.editorObject = assemblyObject;

				log += "[DBG][AD.M] AddAssemblyReferencesToPassed(): Loading Assembly \n" + assemblyText + "\n";

				foreach (var refs in assemblyRequest.references) {
					log += "[DBG][AD.M] AddAssemblyReferencesToPassed(): crntRef = " + refs + "\n";
				}

				// Add all new references to current references.
				List<string> newReferencesToAdd = referencesToAddList.Except(assemblyRequest.references).ToList();
				assemblyRequest.references.AddRange(newReferencesToAdd);

				List<string> newPrecompReferencesToAdd = precompReferencesToAdd.Except(assemblyRequest.precompiledReferences).ToList();
				assemblyRequest.precompiledReferences.AddRange(newPrecompReferencesToAdd);

				if (newReferencesToAdd.Count > 0 || newPrecompReferencesToAdd.Count > 0) {
					SetAssemblyReferences(assemblyRequest);

					RefreshEditorAsset(targetAssembly);

					log += "[DBG][AD.M] AddAssemblyReferencesToPassed(): References added to " + targetAssembly.name + ": " + string.Join(", ", newReferencesToAdd) + "\n";
				}
				else {
					log += "[WRN][AD.M] AddAssemblyReferencesToPassed(): All references already exist in " + targetAssembly.name + "\n";
				}

				UnityEngine.Debug.Log(log);
			}

			private static void ExtractReferences(Dictionary<string, object> assemblyData, ref List<string> crntReferncesStrs) {
				string[] crntReferences = new string[0];

				if (assemblyData.ContainsKey("references")) {
					log += "[DBG][AD.M] ExtractReferences(): References \n";

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
					log += "[DBG][AD.M] ExtractReferences(): No References \n";
				}
			}

			private static void SetAssemblyReferences(AssemblyDefinitionRequest assembly) {
				log += "[DBG][AD.M] SetAssemblyReferences(): Path: " + assembly.filePath + "\n";
				if (!string.IsNullOrEmpty(assembly.filePath)) {
					string asmdefText = File.ReadAllText(assembly.filePath);

					Dictionary<string, object> asmdefData = JsonConvert.DeserializeObject<Dictionary<string, object>>(asmdefText);

					log += "[DBG][AD.M] SetAssemblyReferences():" + !asmdefData.ContainsKey("references") + "\n";

					asmdefData["references"] = assembly.references;

					string updatedJson = JsonConvert.SerializeObject(assembly, Formatting.Indented);
					log += "[DBG][AD.M] SetAssemblyReferences(): " + updatedJson + "\n";

					File.WriteAllText(assembly.filePath, updatedJson);

					log += "[DBG][AD.M] SetAssemblyReferences(): References section updated.\n";
					return;
				}

				log += "[WRN][AD.M] SetAssemblyReferences(): Failed to find .asmdef file for assembly: " + assembly.name + "\n";
			}

			private static bool GetAssemblies(List<string> assembliesToGet, out List<Assembly> results) {
				results = new List<Assembly>();
				bool isValidOperation = true;

				foreach (string reference in assembliesToGet) {

					Assembly referenceAssembly;
					if (reference.Contains("GUID")) {
						referenceAssembly = GetAssemblyWithGUID(reference);
					}
					else {
						referenceAssembly = GetAssemblyWithName(reference);
					}

					if (referenceAssembly != null) {
						results.Add(referenceAssembly);
					}
					else {
						log += "[WRN][AD.M] GetAssemblies():" + reference + " assembly not found.\n";
						isValidOperation = false;
					}
				}

				return isValidOperation;
			}

			private static UnityEngine.Object GetAssemblyObjectFrom(params UnityEngine.Object[] selectedObjects) {
				if (selectedObjects == null || selectedObjects.Length == 0) {
					log += "[WRN][AD.M] GetAssemblyObjectFrom(): No assembly definition file selected.\n";
					return null;
				}

				foreach (UnityEngine.Object selectedObject in selectedObjects) {
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

				log += "[DBG][AD.M] GetAssemblyWithPath(): ParsingAssemblies\n";
				foreach (var assembly in playerAssemblies) {
					if (path.Contains(assembly.name)) {
						return assembly;
					}
				}

				return null;
			}
			private static Assembly GetAssemblyWithName(string name) {
				Assembly[] playerAssemblies = CompilationPipeline.GetAssemblies(AssembliesType.Player);

				log += "[DBG][AD.M] GetAssemblyWithName(): ParsingAssemblies\n";
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
					log += "[WRN][AD.M] RefreshEditorAsset(): Failed to find asset for assembly \n" + assembly.name;
				}

				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
				CompilationPipeline.RequestScriptCompilation();
			}

			public static bool isSelectedAssembly() {
				UnityEngine.Object[] selectedObjects = Selection.objects;
				if (selectedObjects != null && selectedObjects.Length == 1) {
					string path = AssetDatabase.GetAssetPath(selectedObjects[0]);
					return path.EndsWith(".asmdef");
				}
				return false;
			}
		}

	}
}
