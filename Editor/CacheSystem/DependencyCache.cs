using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace UDGV.CacheSystem
{
    public class DependencyCache
    {
        private DependencyViewerSettings _settings;
        private DependencyCacheDataHandler _dataHandler;
        private DependencyCacheResolver _dependencyResolver;


        public DependencyCache(DependencyViewerSettings settings)
        {
            _dataHandler = new DependencyCacheDataHandler();
            _settings = settings;
            _dependencyResolver = new DependencyCacheResolver(_dataHandler, _settings);
        }

        public void Save()
        {
            string data = JsonConvert.SerializeObject(_dataHandler);
            EditorPrefs.SetString(_settings.Developer.EditorPrefCacheSaveKey, data);
            Utility.Logger.Log($"Dependency cache saved : {data}");
        }

        public void Load()
        {
            string cacheSaveKey = _settings.Developer.EditorPrefCacheSaveKey;
            if (EditorPrefs.HasKey(cacheSaveKey))
            {
                string jsonData = EditorPrefs.GetString(cacheSaveKey);
                _dataHandler = JsonConvert.DeserializeObject<DependencyCacheDataHandler>(jsonData);
            }
            Utility.Logger.Log("Dependency cache loaded");
        }

        public void Clear()
        {
            _dataHandler.Clear();
        }

        public bool Clear(UnityEngine.Object obj)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            return Clear(path);
        }

        public bool Clear(string assetPath)
        {
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            return _dataHandler.Clear(guid);
        }

        public void Build()
        {
            var it = BuildAsync().GetEnumerator();
            while (it.MoveNext()) ;
        }

        public void RebuildDependencies(UnityEngine.Object obj)
        {
            string assetPath = AssetDatabase.GetAssetPath(obj);
            RebuildDependencies(assetPath);
        }

        public void RebuildDependencies(string assetPath)
        {
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (_dataHandler.TryGetValue(guid, out DependencyData data))
            {
                data.DisconnectAllDependencies(_dataHandler);
            }

            var it = RebuildDependenciesAsync(assetPath).GetEnumerator();
            while (it.MoveNext()) ;
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
                foreach (var op in RebuildDependenciesAsync(path, operationStatus)) yield return op;
            }

            Utility.Logger.Log($"Cache system build completed");
        }

        public IEnumerable<CacheBuildOperation> RebuildDependenciesAsync(string assetPath)
        {
            CacheBuildOperation operationStatus = new CacheBuildOperation { numTotalAssets = 1 };
            foreach (var op in RebuildDependenciesAsync(assetPath, operationStatus)) yield return op;

            Utility.Logger.Log($"Dependencies rebuild for '{assetPath}'");
        }

        private IEnumerable<CacheBuildOperation> RebuildDependenciesAsync(string assetPath, CacheBuildOperation operationStatus = null)
        {
            string objectGUID = AssetDatabase.AssetPathToGUID(assetPath);
            DependencyData newData = _dataHandler.CreateOrGetDependencyDataFromGuid(objectGUID);

            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (operationStatus != null)
            {
                operationStatus.AssetBeingProcessed = obj;
            }


            // If obj is a prefab...
            if (obj is GameObject)
            {
                GameObject prefab = obj as GameObject;
                foreach (var op in _dependencyResolver.FindDependencies(newData, prefab, operationStatus))
                {
                    yield return op;
                }
            }
            // ... else if obj is a scene...
            else if (obj is SceneAsset)
            {
                SceneAsset scene = obj as SceneAsset;
                foreach (var op in _dependencyResolver.FindDependencies(newData, scene, operationStatus))
                {
                    yield return op;
                }
            }
            // ... else make a default search
            else
            {
                foreach (var op in _dependencyResolver.FindDependencies(newData, obj, operationStatus))
                {
                    yield return op;
                }
            }
        }

        public void DeleteAssetFromCache(string assetPath)
        {
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (TryGetDependencyDataForAsset(guid, out DependencyData dependencyData))
            {
                dependencyData.DisconnectFromAllReferences(_dataHandler);
                dependencyData.DisconnectAllDependencies(_dataHandler);
                _dataHandler.Clear(guid);
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

        public bool IsAssetInCache(UnityEngine.Object obj)
        {
            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out string guid, out long localId))
            {
                return IsAssetInCache(guid);
            }
            throw new Exception($"Cannot get guid for object '{obj}'");
        }

        public bool IsAssetInCache(string guid)
        {
            return _dataHandler.Contains(guid);
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
    }
}