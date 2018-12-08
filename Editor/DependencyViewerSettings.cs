using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class DependencyViewerSettings : ScriptableObject
{
    [SerializeField]
    private int _referencesDepth = 1;
    public int ReferencesDepth
    {
        get { return _referencesDepth; }
        set { _referencesDepth = value; }
    }

    [SerializeField]
    private bool _shouldSearchInCurrentScene = true;
    public bool ShouldSearchInCurrentScene
    {
        get { return _shouldSearchInCurrentScene; }
        set { _shouldSearchInCurrentScene = value; }
    }

    [SerializeField]
    private bool _displayReferences = true;
    public bool DisplayReferences
    {
        get { return _displayReferences; }
        set { _displayReferences = value; }
    }

    [SerializeField]
    private bool _displayDependencies = true;
    public bool DisplayDependencies
    {
        get { return _displayDependencies; }
        set { _displayDependencies = value; }
    }


    public static DependencyViewerSettings Create()
    {
        DependencyViewerSettings settings = ScriptableObject.CreateInstance<DependencyViewerSettings>();
        settings.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
        return settings;
    }
}
