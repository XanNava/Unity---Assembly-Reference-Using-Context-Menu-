using System;
using System.Collections.Generic;

using Newtonsoft.Json;

[Serializable]
public class AssemblyDefinitionRequest {
	[JsonIgnore]
	public string AssemblyName;
	[JsonIgnore]
	public string filePath;
	[JsonIgnore]
	public string AssemblyText;
	[JsonIgnore]
	public UnityEngine.Object editorObject;

	public string name;
	public string rootNamespace;
	public List<string> references;
	public List<string> includePlatforms;
	public List<string> excludePlatforms;
	public bool allowUnsafeCode;
	public bool overrideReferences;
	public List<string> precompiledReferences;
	public bool autoReferenced;
	public List<string> defineConstraints;
	public List<versionDefinition> versionDefines;
	public bool noEngineReferences;
}

[Serializable]
public class versionDefinition {
	public string name;
	public int expression;
	public int define;
}