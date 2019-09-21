using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UDGV.CacheSystem
{
    public class DependencyCacheManager
    {
        private static DependencyCacheManager Instance = null;
        public static DependencyViewerSettings Settings => Instance?._settings;

        public static bool IsRunning
        {
            get => Settings.Developer.IsDependencyCacheManagerActive;
            set => Settings.Developer.IsDependencyCacheManagerActive = value;
        }

        private DependencyCache _cache;
        private AssetsWatcher _assetsWatcher;
        private DependencyViewerSettings _settings;

        private List<IEnumerator<CacheBuildOperation>> _currentCacheBuildOperations = null;

        [InitializeOnLoadMethod]
        static void StartUp()
        {
            Instance = new DependencyCacheManager();
        }
        
        private DependencyCacheManager()
        {
            if (_settings == null)
            {
                _settings = DependencyViewerSettings.Create();
                _settings.Load();
            }

            _cache = new DependencyCache(_settings);
            _assetsWatcher = new AssetsWatcher();
            _assetsWatcher.OnAssetChanged += OnAssetChanged;
            _assetsWatcher.OnAssetDeleted += OnAssetDeleted;
            _assetsWatcher.Start();

            _currentCacheBuildOperations = new List<IEnumerator<CacheBuildOperation>>();

            EditorApplication.update += UpdateCacheManager;
        }

        private void OnAssetDeleted(string assetPath)
        {
            if (!IsRunning) return;

            _cache.DeleteAssetFromCache(assetPath);
        }

        private void OnAssetChanged(string assetPath)
        {
            if (!IsRunning) return;

            var op = _cache.RebuildDependenciesAsync(assetPath).GetEnumerator();
            _currentCacheBuildOperations.Add(op);
        }

        private void UpdateCacheManager()
        {
            if (!IsRunning) return;

            UpdateAllCacheBuildOperations();
        }

        void UpdateAllCacheBuildOperations()
        {
            // Update all cache build operations and
            // remove the ones that are completed
            _currentCacheBuildOperations.RemoveAll((op) => !op.MoveNext());
        }
    }
}