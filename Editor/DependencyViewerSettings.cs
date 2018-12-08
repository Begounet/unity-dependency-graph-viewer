using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class DependencyViewerSettings : ScriptableObject
{
    //[SerializeField] // Disabled for now because not managed yet
    private int _referencesDepth = 1;
    public int ReferencesDepth
    {
        get { return _referencesDepth; }
        set { _referencesDepth = value; }
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
    [Tooltip("Filters when browsing project files for asset referencing")]
    private string _excludeAssetFilters = ".dll,.a,.so,.asmdef,.aar,.bundle,.jar";
    public string ExcludeAssetFilters
    {
        get { return _excludeAssetFilters; }
        set { _excludeAssetFilters = value; }
    }

    public static DependencyViewerSettings Create()
    {
        DependencyViewerSettings settings = ScriptableObject.CreateInstance<DependencyViewerSettings>();
        settings.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
        return settings;
    }
}
