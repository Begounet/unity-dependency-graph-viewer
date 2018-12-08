using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using System;

internal class DependencyResolver
{
    private const int NumAssetPropertiesReferencesResolvedPerFrame = 20;

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

    public IEnumerator BuildGraph()
    {
        if (_settings.ShouldSearchInCurrentScene)
        {
            bool visitChildren = true;
            List<Scene> currentOpenedScenes = DependencyViewerUtility.GetCurrentOpenedScenes();
            if (_settings.FindReferences)
            {
                DependencyViewerUtility.ForeachGameObjectInScenes(currentOpenedScenes,
                    visitChildren, (go) => FindReferenceInGameObject(_graph.RefTargetNode, go));
            }
        }

        if (_settings.FindDependencies)
        {
            FindDependencies(_graph.RefTargetNode);
        }

        if (_settings.FindReferences)
        {
            foreach (var it in FindReferencesAmongAssets(_graph.RefTargetNode))
            {
                yield return it;
            }
        }
    }

    private IEnumerable FindReferencesAmongAssets(DependencyViewerNode node)
    {
        string[] excludeFilters = _settings.ExcludeAssetFilters.Split(',');
        int numPropertyChecked = 0;

        string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
        for (int assetPathIdx = 0; assetPathIdx < allAssetPaths.Length; ++assetPathIdx)
        {
            if (IsAssetPathExcluded(allAssetPaths[assetPathIdx], ref excludeFilters))
            {
                continue;
            }
            
            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(allAssetPaths[assetPathIdx]);
            if (obj != null)
            {
                SerializedObject objSO = new SerializedObject(obj);
                SerializedProperty sp = objSO.GetIterator();
                while (sp.NextVisible(true))
                {
                    if (sp.propertyType == SerializedPropertyType.ObjectReference && sp.objectReferenceValue == node.TargetObject)
                    {
                        // Reference found!
                        DependencyViewerNode reference = new DependencyViewerNode(obj);
                        DependencyViewerGraph.CreateNodeLink(reference, node);
                    }
                    ++numPropertyChecked;

                    if (numPropertyChecked > NumAssetPropertiesReferencesResolvedPerFrame)
                    {
                        numPropertyChecked = 0;
                        yield return new object();
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
                if (sp.propertyType == SerializedPropertyType.ObjectReference && sp.objectReferenceValue == node.TargetObject)
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
            if (sp.propertyType == SerializedPropertyType.ObjectReference && sp.objectReferenceValue != null)
            {
                DependencyViewerNode dependencyNode = new DependencyViewerNode(sp.objectReferenceValue);
                DependencyViewerGraph.CreateNodeLink(node, dependencyNode);
            }
        }
    }

}
