using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

internal class DependencyViewerSettings : ScriptableObject
{
    private const string DependencyViewerSettingsSaveName = "DependencyViewerSettings";

    public event Action onSettingsChanged;

    [Flags]
    public enum ObjectType
    {
        ScriptableObject = 0x01,
        Everything = 0xFF
    }


    //[SerializeField] // Disabled for now because not managed yet
    private bool _shouldSearchInCurrentScene = true;
    public bool ShouldSearchInCurrentScene
    {
        get { return _shouldSearchInCurrentScene; }
        set { _shouldSearchInCurrentScene = value; }
    }

    [SerializeField]
    private bool _findReferences = true;
    public bool FindReferences
    {
        get { return _findReferences; }
        set { _findReferences = value; }
    }

    [SerializeField]
    private bool _findDependencies = true;
    public bool FindDependencies
    {
        get { return _findDependencies; }
        set { _findDependencies = value; }
    }
    
    [SerializeField]
    [EnumFlags]
    private ObjectType _objectTypesFilter = ObjectType.Everything;
    public ObjectType ObjectTypesFilter
    {
        get { return _objectTypesFilter; }
        set { _objectTypesFilter = value; }
    }

    [SerializeField]
    private string _assetsSearchRootDirectory;
    public string AssetsSearchRootDirectory
    {
        get { return _assetsSearchRootDirectory; }
        set { _assetsSearchRootDirectory = value; }
    }

    [Header("Dependencies")]

    [SerializeField]
    private int _dependenciesDepth = 1;
    public int DependenciesDepth
    {
        get { return _dependenciesDepth; }
        set { _dependenciesDepth = value; }
    }


    [SerializeField]
    [Tooltip("Filters when browsing project files for asset referencing")]
    private string _excludeAssetFilters = ".dll,.a,.so,.asmdef,.aar,.bundle,.jar";
    public string ExcludeAssetFilters
    {
        get { return _excludeAssetFilters; }
        set { _excludeAssetFilters = value; }
    }

    [Header("Common")]

    [SerializeField]
    [Tooltip("If enabled, the scripts will also be displayed")]
    private bool _displayScripts = false;
    public bool DisplayScripts
    {
        get { return _displayScripts; }
        set { _displayScripts = value; }
    }


    public static DependencyViewerSettings Create()
    {
        DependencyViewerSettings settings = ScriptableObject.CreateInstance<DependencyViewerSettings>();
        settings.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
        return settings;
    }

    public void Save()
    {
        var data = EditorJsonUtility.ToJson(this, false);
        EditorPrefs.SetString(DependencyViewerSettingsSaveName, data);
    }

    public void Load()
    {
        if (EditorPrefs.HasKey(DependencyViewerSettingsSaveName))
        {
            var data = EditorPrefs.GetString(DependencyViewerSettingsSaveName);
            EditorJsonUtility.FromJsonOverwrite(data, this);
        }        
    }

    void OnValidate()
    {
        if (onSettingsChanged != null)
        {
            onSettingsChanged.Invoke();
        }
    }
}
