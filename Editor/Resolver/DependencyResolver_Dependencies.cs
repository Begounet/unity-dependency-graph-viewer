using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

internal class DependencyResolver_Dependencies
{
    private DependencyViewerSettings _settings;

    public DependencyResolver_Dependencies(DependencyViewerSettings settings)
    {
        _settings = settings;
    }

    public void FindDependencies(DependencyViewerNode node, int depth = 1)
    {
        if (node.TargetObject is GameObject)
        {
            GameObject targetGameObject = node.TargetObject as GameObject;
            Component[] components = targetGameObject.GetComponents<Component>();
            for (int i = 0; i < components.Length; ++i)
            {
                FindDependencies(node, components[i], depth);
            }

            if (DependencyResolverUtility.IsPrefab(node.TargetObject))
            {
                UDGV.GameObjectUtility.ForeachChildrenGameObject(targetGameObject, (childGo) =>
                {
                    bool isPrefabChild = true;
                    components = childGo.GetComponents<Component>();
                    for (int i = 0; i < components.Length; ++i)
                    {
                        FindDependencies(node, components[i], depth, isPrefabChild);
                    }
                });
            }
        }
        else
        {
            FindDependencies(node, node.TargetObject, depth);
        }
    }

    private void FindDependencies(DependencyViewerNode node, UnityEngine.Object obj, int depth = 1, bool isPrefabChild = false)
    {
        SerializedObject targetObjectSO = new SerializedObject(obj);
        SerializedProperty sp = targetObjectSO.GetIterator();
        while (sp.NextVisible(true))
        {
            if (sp.propertyType == SerializedPropertyType.ObjectReference &&
                sp.objectReferenceValue != null &&
                IsObjectAllowedBySettings(sp.objectReferenceValue))
            {
                // Dependency found!
                DependencyViewerNode dependencyNode = new DependencyViewerNode(sp.objectReferenceValue);
                DependencyViewerGraph.CreateNodeLink(node, dependencyNode);
                if (isPrefabChild)
                {
                    Component comp = obj as Component;
                    dependencyNode.GameObjectNameAsPrefabChild = comp.gameObject.name;
                }

                if (depth > 1)
                {
                    FindDependencies(dependencyNode, sp.objectReferenceValue, depth - 1);
                }
            }
        }
    }

    private bool IsObjectAllowedBySettings(UnityEngine.Object obj)
    {
        return (_settings.CanObjectTypeBeIncluded(obj));
    }
}
