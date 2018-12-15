using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using System;
using System.Linq;

internal class DependencyResolver
{
    private const int NumAssetPropertiesReferencesResolvedPerFrame = 100;

    private bool _isResolvingCompleted;
    public bool IsResolvingCompleted
    {
        get { return _isResolvingCompleted; }
        set { _isResolvingCompleted = value; }
    }

    private DependencyViewerGraph _graph;
    private DependencyViewerSettings _settings;

    public DependencyResolver(DependencyViewerGraph graph, DependencyViewerSettings settings)
    {
        _graph = graph;
        _settings = settings;
    }

    public IEnumerator<DependencyViewerOperation> BuildGraph()
    {
        if (_settings.FindDependencies)
        {
            FindDependencies(_graph.RefTargetNode, _settings.DependenciesDepth);
        }

        if (_settings.ShouldSearchInCurrentScene)
        {
            List<Scene> currentOpenedScenes = DependencyViewerUtility.GetCurrentOpenedScenes();
            if (_settings.FindReferences)
            {
                foreach (var currentOperation in FindReferencesAmongGameObjects(_graph.RefTargetNode, currentOpenedScenes))
                {
                    yield return currentOperation;
                }
            }
        }

        if (_settings.FindReferences)
        {
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
                        !IsObjectExludedBySettings(componentSP.objectReferenceValue))
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
    
    private IEnumerable<DependencyViewerOperation> FindReferencesAmongAssets(DependencyViewerNode node)
    {
        AssetDependencyResolverOperation operationStatus = new AssetDependencyResolverOperation();
        operationStatus.node = node;

        string[] excludeFilters = _settings.ExcludeAssetFilters.Split(',');
        int numPropertyChecked = 0;

        var allLocalAssetPaths = from assetPath in AssetDatabase.GetAllAssetPaths()
                                 where assetPath.StartsWith("Assets/") && !IsAssetPathExcluded(assetPath, ref excludeFilters)
                                 select assetPath;
        
        operationStatus.numTotalAssets = allLocalAssetPaths.Count();
        foreach (string assetPath in allLocalAssetPaths)
        {
            ++operationStatus.numProcessedAssets;

            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (obj != null)
            {
                SerializedObject objSO = new SerializedObject(obj);
                SerializedProperty sp = objSO.GetIterator();
                while (sp.NextVisible(true))
                {
                    if (sp.propertyType == SerializedPropertyType.ObjectReference && 
                        sp.objectReferenceValue == node.TargetObject &&
                        !IsObjectExludedBySettings(sp.objectReferenceValue))
                    {
                        // Reference found!
                        DependencyViewerNode reference = new DependencyViewerNode(obj);
                        DependencyViewerGraph.CreateNodeLink(reference, node);
                    }

                    ++numPropertyChecked;

                    if (numPropertyChecked > NumAssetPropertiesReferencesResolvedPerFrame)
                    {
                        operationStatus.AssetBeingProcessed = obj;

                        numPropertyChecked = 0;
                        yield return operationStatus;
                    }
                }
            }
        }
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
        return false;
    }

    private void FindReferenceInGameObject(DependencyViewerNode node, GameObject rootGameObject, int depth = 1)
    {
        Component[] components = rootGameObject.GetComponents<MonoBehaviour>();
        for (int componentsIdx = 0; componentsIdx < components.Length; ++componentsIdx)
        {
            Component component = components[componentsIdx];
            SerializedObject so = new SerializedObject(component);
            SerializedProperty sp = so.GetIterator();
            while (sp.NextVisible(true))
            {
                if (sp.propertyType == SerializedPropertyType.ObjectReference && 
                    sp.objectReferenceValue == node.TargetObject &&
                    !IsObjectExludedBySettings(sp.objectReferenceValue))
                {
                    // Reference found!
                    DependencyViewerNode reference = new DependencyViewerNode(component);
                    DependencyViewerGraph.CreateNodeLink(reference, node);
                }
            }
        }
    }

    private void FindDependencies(DependencyViewerNode node, int depth = 1)
    {
        if (node.TargetObject is GameObject)
        {
            GameObject targetGameObject = node.TargetObject as GameObject;
            Component[] components = targetGameObject.GetComponents<Component>();
            for (int i = 0; i < components.Length; ++i)
            {
                FindDependencies(node, components[i], depth);
            }
        }
        else
        {
            FindDependencies(node, node.TargetObject, depth);
        }
    }

    private void FindDependencies(DependencyViewerNode node, UnityEngine.Object obj, int depth = 1)
    {
        SerializedObject targetObjectSO = new SerializedObject(obj);
        SerializedProperty sp = targetObjectSO.GetIterator();
        while (sp.NextVisible(true))
        {
            if (sp.propertyType == SerializedPropertyType.ObjectReference && 
                sp.objectReferenceValue != null &&
                !IsObjectExludedBySettings(sp.objectReferenceValue))
            {
                DependencyViewerNode dependencyNode = new DependencyViewerNode(sp.objectReferenceValue);
                DependencyViewerGraph.CreateNodeLink(node, dependencyNode);

                if (depth > 1)
                {
                    FindDependencies(dependencyNode, sp.objectReferenceValue, depth - 1);
                }
            }
        }
    }

    private bool IsObjectExludedBySettings(UnityEngine.Object obj)
    {
        return (obj is MonoScript && !_settings.DisplayScripts);
    }
}
