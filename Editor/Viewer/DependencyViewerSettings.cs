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
        ScriptableObject    = 0x01,
        Component           = 0x02,
        MonoScript          = 0x04
    }

    public enum SceneSearchMode
    {
        NoSearch,
        SearchOnlyInCurrentScene,
        SearchEverywhere
    }


    [Header("References")]
    
    [SerializeField]
    [Tooltip("If checked, the references will be searched")]
    private bool _findReferences = true;
    public bool FindReferences
    {
        get { return _findReferences; }
        set { _findReferences = value; }
    }

    [SerializeField]
    [Tooltip("Filters when browsing project files for asset referencing")]
    private string _excludeAssetFilters = ".dll,.a,.so,.asmdef,.aar,.bundle,.jar";
    public string ExcludeAssetFilters
    {
        get { return _excludeAssetFilters; }
        set { _excludeAssetFilters = value; }
    }

    [SerializeField]
    [Tooltip("If set, only these directories will be browsed for references. Can really improve search speed.")]
    private string[] _referencesAssetsDirectories;
    public string[] ReferencesAssetDirectories
    {
        get { return _referencesAssetsDirectories; }
        set { _referencesAssetsDirectories = value; }
    }

    [SerializeField]
    private SceneSearchMode _sceneSearchType = SceneSearchMode.SearchEverywhere;
    public SceneSearchMode SceneSearchType
    {
        get { return _sceneSearchType; }
        set { _sceneSearchType = value; }
    }


    [Header("Dependencies")]

    [SerializeField]
    [Tooltip("If checked, the dependencies will be searched")]
    private bool _findDependencies = true;
    public bool FindDependencies
    {
        get { return _findDependencies; }
        set { _findDependencies = value; }
    }

    [SerializeField]
    [Tooltip("Defines the depth of the search among the dependencies")]
    private int _dependenciesDepth = 1;
    public int DependenciesDepth
    {
        get { return _dependenciesDepth; }
        set { _dependenciesDepth = value; }
    }


    [Header("Common")]
    
    [SerializeField]
    [EnumFlags]
    [Tooltip("Defines the object types to analyze")]
    private ObjectType _objectTypesFilter = (ObjectType) 0xFFFF;
    public ObjectType ObjectTypesFilter
    {
        get { return _objectTypesFilter; }
        set { _objectTypesFilter = value; }
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

    public bool CanObjectTypeBeIncluded(UnityEngine.Object obj)
    {
        if ((ObjectTypesFilter & ObjectType.ScriptableObject) == 0 &&
            obj is ScriptableObject)
        {
            return false;
        }

        if ((ObjectTypesFilter & ObjectType.Component) == 0 &&
            obj is Component)
        {
            return false;
        }
       
        if ((ObjectTypesFilter & ObjectType.MonoScript) == 0 &&
            obj is MonoScript)
        {
            return false;
        }

        return true;
    }

    void OnValidate()
    {
        if (onSettingsChanged != null)
        {
            onSettingsChanged.Invoke();
        }
    }
}
