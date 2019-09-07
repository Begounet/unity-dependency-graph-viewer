using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

internal class DependencyResolver_References
{
    private const int NumAssetPropertiesReferencesResolvedPerFrame = 100;

    private DependencyViewerGraph _graph;
    private DependencyViewerSettings _settings;

    public DependencyResolver_References(DependencyViewerGraph graph, DependencyViewerSettings settings)
    {
        _graph = graph;
        _settings = settings;
    }

    public IEnumerable<DependencyViewerOperation> FindReferences()
    {
        if (_settings.SceneSearchType != DependencyViewerSettings.SceneSearchMode.NoSearch)
        {
            // Search references in scenes
            List<Scene> currentOpenedScenes = DependencyViewerUtility.GetCurrentOpenedScenes();
            if (_settings.FindReferences)
            {
                foreach (var currentOperation in FindReferencesAmongGameObjects(_graph.RefTargetNode, currentOpenedScenes))
                {
                    yield return currentOperation;
                }
            }
        }

        bool searchOnlyInCurrentScene = (_settings.SceneSearchType == DependencyViewerSettings.SceneSearchMode.SearchOnlyInCurrentScene);
        if (_settings.FindReferences && !searchOnlyInCurrentScene)
        {
            // Search references in assets
            foreach (var currentOperation in FindReferencesAmongAssets(_graph.RefTargetNode))
            {
                yield return currentOperation;
            }
        }
    }

    private IEnumerable<DependencyViewerOperation> FindReferencesAmongGameObjects(DependencyViewerNode node, List<Scene> scenes)
    {
        AssetDependencyResolverOperation operationStatus = new AssetDependencyResolverOperation();
        operationStatus.node = node;

        List<GameObject> allGameObjects = GetAllGameObjectsFromScenes(scenes);
        operationStatus.numTotalAssets = allGameObjects.Count;

        int numPropertiesCheck = 0;
        for (int i = 0; i < allGameObjects.Count; ++i)
        {
            GameObject currentGo = allGameObjects[i];
            operationStatus.AssetBeingProcessed = currentGo;

            Component[] components = currentGo.GetComponents<Component>();
            for (int componentIndex = 0; componentIndex < components.Length; ++componentIndex)
            {
                Component component = components[componentIndex];
                if (component == null)
                {
                    continue;
                }

                SerializedObject componentSO = new SerializedObject(component);
                SerializedProperty componentSP = componentSO.GetIterator();

                while (componentSP.NextVisible(true))
                {
                    // Reference found!
                    if (componentSP.propertyType == SerializedPropertyType.ObjectReference &&
                        componentSP.objectReferenceValue == node.TargetObject &&
                        IsObjectAllowedBySettings(component))
                    {
                        DependencyViewerNode referenceNode = new DependencyViewerNode(component);
                        DependencyViewerGraph.CreateNodeLink(referenceNode, node);
                    }

                    ++numPropertiesCheck;
                    if (numPropertiesCheck > NumAssetPropertiesReferencesResolvedPerFrame)
                    {
                        numPropertiesCheck = 0;
                        yield return operationStatus;
                    }
                }
            }

            ++operationStatus.numProcessedAssets;
        }
    }

    private IEnumerable<DependencyViewerOperation> FindReferencesAmongAssets(DependencyViewerNode node)
    {
        string[] excludeFilters = _settings.ExcludeAssetFilters.Split(',');

        var allLocalAssetPaths = from assetPath in AssetDatabase.GetAllAssetPaths()
                                 where assetPath.StartsWith("Assets/") && !IsAssetPathExcluded(assetPath, ref excludeFilters)
                                 select assetPath;

        AssetDependencyResolverOperation operationStatus = new AssetDependencyResolverOperation
        {
            node = node,
            numTotalAssets = allLocalAssetPaths.Count()
        };

        foreach (string assetPath in allLocalAssetPaths)
        {
            ++operationStatus.numProcessedAssets;

            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (obj != null)
            {
                bool isPrefab = (obj is GameObject);
                if (isPrefab)
                {
                    GameObject prefab = obj as GameObject;
                    foreach (var op in FindReferencesAmongPrefabChildren(node, prefab, operationStatus, prefab))
                    {
                        yield return op;
                    }
                }
                else
                {
                    foreach (var op in FindReferencesOnUnityObject(node, obj, operationStatus))
                    {
                        yield return op;
                    }
                }
            }
        }
    }

    private IEnumerable<DependencyViewerOperation> FindReferencesOnUnityObject(
        DependencyViewerNode node, 
        UnityEngine.Object obj, 
        AssetDependencyResolverOperation op, 
        GameObject prefabRoot = null)
    {
        SerializedObject objSO = new SerializedObject(obj);
        SerializedProperty sp = objSO.GetIterator();
        while (sp.NextVisible(true))
        {
            if (IsPropertyADependency(sp, node))
            {
                // Reference found!
                DependencyViewerNode reference = new DependencyViewerNode(obj);
                DependencyViewerGraph.CreateNodeLink(reference, node);
                if (prefabRoot != null)
                {
                    reference.SetAsPrefabContainerInfo(prefabRoot, (obj as Component).gameObject.name);
                }
            }

            ++op.numProcessedProperties;

            if (op.numProcessedProperties > NumAssetPropertiesReferencesResolvedPerFrame)
            {
                op.AssetBeingProcessed = obj;

                op.numProcessedProperties = 0;
                yield return op;
            }
        }
    }

    private IEnumerable<DependencyViewerOperation> FindReferencesAmongPrefabChildren(
        DependencyViewerNode node, 
        GameObject gameObject, 
        AssetDependencyResolverOperation op,
        GameObject prefabRoot)
    {
        // Find references among the components of the GameObject...
        Component[] components = gameObject.GetComponents<Component>();
        for (int i = 0; i < components.Length; ++i)
        {
            foreach (var operation in FindReferencesOnUnityObject(node, components[i], op, prefabRoot))
            {
                yield return operation;
            }
        }

        // ...then make same thing on children
        Transform trans = gameObject.transform;
        for (int i = 0; i < trans.childCount; ++i)
        {
            GameObject child = trans.GetChild(i).gameObject;
            foreach (var operation in FindReferencesAmongPrefabChildren(node, child, op, prefabRoot))
            {
                yield return operation;
            }
        }
    }

    private bool IsPropertyADependency(SerializedProperty sp, DependencyViewerNode node)
    {
        return sp.propertyType == SerializedPropertyType.ObjectReference &&
                sp.objectReferenceValue == node.TargetObject &&
                IsObjectAllowedBySettings(sp.objectReferenceValue);
    }

    private List<GameObject> GetAllGameObjectsFromScenes(List<Scene> scenes)
    {
        List<GameObject> gameObjects = new List<GameObject>();
        List<GameObject> gameObjectsToCheck = new List<GameObject>();

        List<GameObject> rootGameObjects = new List<GameObject>();
        for (int sceneIdx = 0; sceneIdx < scenes.Count; ++sceneIdx)
        {
            Scene scene = scenes[sceneIdx];
            scene.GetRootGameObjects(rootGameObjects);
            gameObjectsToCheck.AddRange(rootGameObjects);
        }

        for (int gameObjectsToCheckIdx = 0; gameObjectsToCheckIdx < gameObjectsToCheck.Count; ++gameObjectsToCheckIdx)
        {
            GameObject currentGo = gameObjectsToCheck[gameObjectsToCheckIdx];
            for (int childIdx = 0; childIdx < currentGo.transform.childCount; ++childIdx)
            {
                gameObjectsToCheck.Add(currentGo.transform.GetChild(childIdx).gameObject);
            }
            gameObjects.Add(currentGo);
        }

        return gameObjects;
    }

    private bool IsObjectAllowedBySettings(UnityEngine.Object obj)
    {
        return (_settings.CanObjectTypeBeIncluded(obj));
    }

    private bool IsAssetPathExcluded(string assetPath, ref string[] excludeFilters)
    {
        for (int i = 0; i < excludeFilters.Length; ++i)
        {
            if (assetPath.EndsWith(excludeFilters[i]))
            {
                return true;
            }
        }

        if (_settings.ReferencesAssetDirectories != null &&
            _settings.ReferencesAssetDirectories.Length > 0)
        {
            bool isAssetAmongReferencesDirectory = false;
            string assetFullPath = Path.GetFullPath(assetPath);
            for (int i = 0; i < _settings.ReferencesAssetDirectories.Length; ++i)
            {
                if (Directory.Exists(_settings.ReferencesAssetDirectories[i]))
                {
                    string referenceAssetFullPath = Path.GetFullPath(_settings.ReferencesAssetDirectories[i]);
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
