using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UDGV
{
    internal static class DependencyResolverUtility
    {
        public static bool IsObjectAnAsset(UnityEngine.Object obj)
        {
            return AssetDatabase.Contains(obj);
        }

        public static bool IsPrefab(UnityEngine.Object obj)
        {
            return IsObjectAnAsset(obj) && (obj is GameObject);
        }

        public static bool IsAssetPathExcluded(string assetPath, ref string[] excludeFilters, DependencyViewerSettings settings)
        {
            if (AssetDatabase.IsValidFolder(assetPath))
            {
                return true;
            }

            for (int i = 0; i < excludeFilters.Length; ++i)
            {
                if (assetPath.EndsWith(excludeFilters[i]))
                {
                    return true;
                }
            }

            if (settings.ReferencesAssetDirectories != null &&
                settings.ReferencesAssetDirectories.Length > 0)
            {
                bool isAssetAmongReferencesDirectory = false;
                string assetFullPath = Path.GetFullPath(assetPath);
                for (int i = 0; i < settings.ReferencesAssetDirectories.Length; ++i)
                {
                    if (Directory.Exists(settings.ReferencesAssetDirectories[i]))
                    {
                        string referenceAssetFullPath = Path.GetFullPath(settings.ReferencesAssetDirectories[i]);
                        if (assetFullPath.StartsWith(referenceAssetFullPath))
                        {
                            isAssetAmongReferencesDirectory = true;
                            break;
                        }
                    }
                }

                return !isAssetAmongReferencesDirectory;
            }

            return false;
        }
    }
}