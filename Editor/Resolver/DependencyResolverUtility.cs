using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class DependencyResolverUtility
{
    public static bool IsObjectAnAsset(UnityEngine.Object obj)
    {
        return AssetDatabase.Contains(obj);
    }

    public static bool IsPrefab(UnityEngine.Object obj)
    {
        return IsObjectAnAsset(obj) && (obj is GameObject);
    }
}
