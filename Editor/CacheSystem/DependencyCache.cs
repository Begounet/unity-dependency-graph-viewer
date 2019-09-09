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
        private Dictionary<string, DependencyData> _data;

        public DependencyCache(DependencyViewerSettings settings)
        {
            _settings = settings;
        }

        public void Load()
        {

        }

        public void Clear()
        {

        }

        public void Build()
        {
            var it = BuildAsync().GetEnumerator();
            while (it.MoveNext());
        }

        public IEnumerable<CacheBuildOperation> BuildAsync()
        {
            _data = new Dictionary<string, DependencyData>();

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
                DependencyData newData = CreateOrGetDependencyDataFromGUID(objectGUID);

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
                if (IsPropertyADependency(sp))
                {
                    // Found dependency!
                    if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sp.objectReferenceValue, out string guid, out long localId))
                    {
                        bool isUnityLibraryResources = guid.StartsWith("0000");
                        if (!isUnityLibraryResources)
                        {
                            DependencyData dependency = CreateOrGetDependencyDataFromGUID(guid);
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

        private DependencyData CreateOrGetDependencyDataFromGUID(string guid)
        {
            if (_data.TryGetValue(guid, out DependencyData data))
            {
                return data;
            }

            DependencyData newData = new DependencyData()
            {
                objectGuid = guid
            };
            _data.Add(guid, newData);
            return newData;
        }

        private bool IsPropertyADependency(SerializedProperty sp)
        {
            return sp.propertyType == SerializedPropertyType.ObjectReference &&
                    sp.objectReferenceValue != null &&
                    _settings.CanObjectTypeBeIncluded(sp.objectReferenceValue);
        }

        public void DumpCache()
        {
            foreach (DependencyData data in _data.Values)
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
            return _data.TryGetValue(guid, out dependencyData);
        }
    }
}