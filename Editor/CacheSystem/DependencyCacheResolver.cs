using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace UDGV.CacheSystem
{
    internal class DependencyCacheResolver
    {
        private const int NumPropertiesToProceedByFrame = 100;


        private DependencyViewerSettings _settings;
        private DependencyCacheDataHandler _dataHandler;


        public DependencyCacheResolver(DependencyCacheDataHandler dataHandler, DependencyViewerSettings settings)
        {
            _dataHandler = dataHandler;
            _settings = settings;
        }

        public IEnumerable<CacheBuildOperation> FindDependencies(DependencyData data, UnityEngine.Object targetObject, CacheBuildOperation operation)
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
                        if (!DependencyResolverUtility.IsGuidFromUnityResources(guid))
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

        public IEnumerable<CacheBuildOperation> FindDependencies(DependencyData data, IEnumerable<GameObject> gameObjects, CacheBuildOperation operation)
        {
            foreach (GameObject gameObject in gameObjects)
            {
                foreach (var op in FindDependencies(data, gameObject, operation)) yield return op;
            }
        }

        public IEnumerable<CacheBuildOperation> FindDependencies(DependencyData data, GameObject gameObject, CacheBuildOperation operation)
        {
            // Search among *all* the component on the GameObject and the children ones
            Component[] childrenComponents = gameObject.GetComponentsInChildren<Component>(true);

            foreach (var op in FindDependencies(data, childrenComponents, operation)) yield return op;
        }

        public IEnumerable<CacheBuildOperation> FindDependencies(DependencyData data, IEnumerable<Component> components, CacheBuildOperation operation)
        {
            foreach (Component component in components)
            {
                foreach (var op in FindDependencies(data, component, operation)) yield return op;
            }
        }

        public IEnumerable<CacheBuildOperation> FindDependencies(DependencyData data, SceneAsset scene, CacheBuildOperation operation)
        {
            string scenePath = AssetDatabase.GetAssetPath(scene);
            string sceneContent = File.ReadAllText(scenePath);

            // Find all references to guids, because that's all that interest us. Don't need to load the scene
            Regex guidRegex = new Regex(@"guid: (?<guid>[a-f\d]*)[,|}]");
            MatchCollection matches = guidRegex.Matches(sceneContent);
            for (int i = 0; i < matches.Count; ++i)
            {
                // The group that interest us...
                Group group = matches[i].Groups[1];
                string guid = group.Value;
                if (!DependencyResolverUtility.IsGuidFromUnityResources(guid))
                {
                    DependencyData dependency = _dataHandler.CreateOrGetDependencyDataFromGuid(guid);
                    DependencyData.Connect(data, dependency);
                }
            }

            yield return operation;
        }
    }
}
