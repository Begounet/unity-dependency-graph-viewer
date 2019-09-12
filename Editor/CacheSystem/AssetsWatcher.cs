using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AssetsWatcher : UnityEditor.AssetPostprocessor
{
    public event Action<string> OnAssetChanged;
    public event Action<string> OnAssetDeleted;

    private static HashSet<string> _uniqueImportedAssets = new HashSet<string>();
    private static HashSet<string> _uniqueDeletedAssets = new HashSet<string>();

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        _uniqueImportedAssets.UnionWith(importedAssets);
        _uniqueDeletedAssets.UnionWith(deletedAssets);
    }

    public void Start()
    {
        EditorApplication.update += Update;
    }

    public void Stop()
    {
        EditorApplication.update -= Update;
    }

    ~AssetsWatcher()
    {
        Stop();
    }

    public void ForceUpdate()
    {
        Update();
    }

    private void Update()
    {
        NotifyAssetsEvents(_uniqueImportedAssets, OnAssetChanged);
        NotifyAssetsEvents(_uniqueDeletedAssets, OnAssetDeleted);
    }

    void NotifyAssetsEvents(HashSet<string> assetPaths, Action<string> evt)
    {
        if (assetPaths.Count > 0)
        {
            var it = assetPaths.GetEnumerator();
            while (it.MoveNext())
            {
                evt?.Invoke(it.Current);
            }
            assetPaths.Clear();
        }
    }
}
