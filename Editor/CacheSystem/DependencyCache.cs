using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UDGV
{
    public class DependencyCache
    {
        private const int NumPropertiesToProceedByFrame = 100;

        private DependencyViewerSettings _settings;
        private DependencyCacheDataHandler _dataHandler;

        public DependencyCache(DependencyViewerSettings settings)
        {
            _dataHandler = new DependencyCacheDataHandler();
            _settings = settings;
        }

        public void Load()
        {

        }

        public void Clear()
        {
            _dataHandler.Clear();
        }

        public void Build()
        {
            var it = BuildAsync().GetEnumerator();
            while (it.MoveNext());
        }

        public IEnumerable<CacheBuildOperation> BuildAsync()
        {
            _dataHandler.Clear();

            string[] allAssetsPath = AssetDatabase.GetAllAssetPaths();
            string[] excludeFilters = _settings.ExcludeAssetFilters.Split(',');

            var allLocalAssetPaths = from assetPath in AssetDatabase.GetAllAssetPaths()
                                     where assetPath.StartsWith("Assets/") && !DependencyResolverUtility.IsAssetPathExcluded(assetPath, ref excludeFilters, _settings)
                                     select assetPath;

            CacheBuildOperation operationStatus = new CacheBuildOperation
            {
                numTotalAssets = allLocalAssetPaths.Count()
            };

            foreach (string path in allLocalAssetPaths)
            {
                string objectGUID = AssetDatabase.AssetPathToGUID(path);
                DependencyData newData = _dataHandler.CreateOrGetDependencyDataFromGuid(objectGUID);

                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                operationStatus.AssetBeingProcessed = obj;

                // If obj is a prefab...
                if (obj is GameObject)
                {

                }
                // ... else if obj is a scene...
                else if (obj is SceneAsset)
                {

                }
                // ... else make a default search
                else
                {
                    foreach (var op in FindDependencies(newData, obj, operationStatus))
                    {
                        yield return op;
                    }
                }
            }
        }

        private IEnumerable<CacheBuildOperation> FindDependencies(DependencyData data, UnityEngine.Object targetObject, CacheBuildOperation operation)
        {
            SerializedObject so = new SerializedObject(targetObject);
            SerializedProperty sp = so.GetIterator();
            while (sp.NextVisible(true))
            {
                if (DependencyResolverUtility.IsPropertyADependency(_settings, sp))
                {
                    // Found dependency!
                    if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sp.objectReferenceValue, out string guid, out long localId))
                    {
                        bool isUnityLibraryResources = guid.StartsWith("0000");
                        if (!isUnityLibraryResources)
                        {
                            DependencyData dependency = _dataHandler.CreateOrGetDependencyDataFromGuid(guid);
                            dependency.localId = localId;
                            DependencyData.Connect(data, dependency);
                        }
                    }
                }

                // Check if we should make a pause
                ++operation.numProcessedProperties;
                if (operation.numProcessedProperties > NumPropertiesToProceedByFrame)
                {
                    operation.numProcessedProperties = 0;
                    yield return operation;
                }
            }
        }

        public void DumpCache()
        {
            foreach (DependencyData data in _dataHandler.GetDependenciesData())
            {
                string path = AssetDatabase.GUIDToAssetPath(data.objectGuid);
                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                Debug.Log($"== {obj.name} ({path})");

                Debug.Log($"= Dependencies");
                DumpGUIDs(data.Dependencies);

                Debug.Log($"= References");
                DumpGUIDs(data.References);
            }
        }

        private void DumpGUIDs(HashSet<string> guids)
        {
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                Debug.Log($"{obj?.name ?? "null"} ({path})");
            }
        }

        public bool TryGetDependencyDataForAsset(string guid, out DependencyData dependencyData)
        {
            return _dataHandler.TryGetValue(guid, out dependencyData);
        }

        public bool HasDirectDependencyOn(string mainObjectGuid, string otherObjectGuid)
        {
            return HasDependencyOn(mainObjectGuid, otherObjectGuid, 1);
        }

        public bool HasDependencyOn(string mainObjectGuid, string otherObjectGuid, int depth = -1)
        {
            if (TryGetDependencyDataForAsset(mainObjectGuid, out DependencyData dependencyData))
            {
                return dependencyData.HasDependencyOn(_dataHandler, otherObjectGuid, depth);
            }
            return false;
        }
    }
}