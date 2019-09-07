using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

internal class DependencyViewerNode
{
    public enum NodeInputSide { Left, Right }

    public string Name
    {
        get
        {
            if (_targetObject == null)
            {
                return "(null)";
            }

            string suffix = (_targetObject is UnityEditor.MonoScript) ? " (Script)" : string.Empty;

            return $"{_targetObject.name}{suffix}";
        }
    }

    private UnityEngine.Object _targetObject;
    public UnityEngine.Object TargetObject
    {
        get { return _targetObject; }
        private set { _targetObject = value; }
    }

    public string GameObjectNameAsPrefabChild { get; private set; }
    public GameObject PrefabContainer { get; private set; }

    private Vector2 _position;
    public Vector2 Position
    {
        get { return _position; }
        set { _position = value; }
    }

    private List<DependencyViewerNode> _leftInputs;
    public List<DependencyViewerNode> LeftInputs
    {
        get { return _leftInputs; }
        set { _leftInputs = value; }
    }

    private List<DependencyViewerNode> _rightInputs;
    public List<DependencyViewerNode> RightInputs
    {
        get { return _rightInputs; }
        set { _rightInputs = value; }
    }

    private float _mod;
    public float Mod
    {
        get { return _mod; }
        set { _mod = value; }
    }


    public DependencyViewerNode(UnityEngine.Object targetObject)
    {
        _targetObject = targetObject;
        _leftInputs = new List<DependencyViewerNode>();
        _rightInputs = new List<DependencyViewerNode>();
    }

    public void SetAsPrefabContainerInfo(GameObject prefabContainer, string gameObjectName)
    {
        PrefabContainer = prefabContainer;
        GameObjectNameAsPrefabChild = gameObjectName;
    }

    public float GetHeight()
    {
        int numAdditionalLines = 0;
        if (PrefabContainer != null)
        {
            ++numAdditionalLines;
        }
        return DependencyViewerGraphDrawer.NodeHeight + numAdditionalLines * EditorGUIUtility.singleLineHeight;
    }

    public float GetWidth()
    {
        return DependencyViewerGraphDrawer.NodeWidth;
    }

    public Vector2 GetSize()
    {
        return new Vector2(GetWidth(), GetHeight());
    }

    public Vector2 GetLeftInputAnchorPosition()
    {
        return new Vector2(_position.x, _position.y + GetHeight() / 2);
    }

    public Vector2 GetRightInputAnchorPosition()
    {
        return new Vector2(_position.x + GetWidth(), _position.y + GetHeight() / 2);
    }

    public void SetPositionX(float x)
    {
        _position.x = x;
    }

    public void SetPositionY(float y)
    {
        _position.y = y;
    }

    public List<DependencyViewerNode> GetInputNodesFromSide(NodeInputSide side)
    {
        return side == NodeInputSide.Left ? _leftInputs : _rightInputs;
    }

    public DependencyViewerNode GetParent(NodeInputSide treeSide)
    {
        var parentList = (treeSide == NodeInputSide.Left ? _rightInputs : _leftInputs);
        return (parentList.Count > 0 ? parentList[0] : null);
    }

    public List<DependencyViewerNode> GetChildren(NodeInputSide treeSide)
    {
        return GetInputNodesFromSide(treeSide);
    }

    public int GetSiblingIndex(NodeInputSide treeSide)
    {
        var parentNode = GetParent(treeSide);
        if (parentNode == null)
        {
            return 0;
        }

        var siblings = parentNode.GetChildren(treeSide);
        for (int siblingIdx = 0; siblingIdx < siblings.Count; ++siblingIdx)
        {
            if (siblings[siblingIdx] == this)
            {
                return siblingIdx;
            }
        }
        return -1;
    }

    public DependencyViewerNode GetFirstSibling(NodeInputSide treeSide)
    {
        var parent = GetParent(treeSide);
        if (parent == null)
        {
            return this;
        }

        var childNodes = parent.GetChildren(treeSide);
        return (childNodes.Count > 0 ? childNodes[0] : null);
    }

    public DependencyViewerNode GetLastSibling(NodeInputSide treeSide)
    {
        var parent = GetParent(treeSide);
        if (parent == null)
        {
            return this;
        }

        var childNodes = parent.GetChildren(treeSide);
        return (childNodes.Count > 0 ? childNodes[childNodes.Count - 1] : null);
    }

    public DependencyViewerNode GetFirstChild(NodeInputSide treeSide)
    {
        var childNodes = GetChildren(treeSide);
        return (childNodes.Count > 0 ? childNodes[0] : null);
    }

    public DependencyViewerNode GetLastChild(NodeInputSide treeSide)
    {
        var childNodes = GetChildren(treeSide);
        return (childNodes.Count > 0 ? childNodes[childNodes.Count - 1] : null);
    }

    public DependencyViewerNode GetPreviousSibling(NodeInputSide treeSide)
    {
        var parent = GetParent(treeSide);
        if (parent == null)
        {
            return null;
        }

        var childNodes = parent.GetChildren(treeSide);
        int childIdx = GetSiblingIndex(treeSide);
        return (childIdx > 0 ? childNodes[childIdx - 1] : null);
    }

    public DependencyViewerNode GetNextSibling(NodeInputSide treeSide)
    {
        var parent = GetParent(treeSide);
        if (parent == null)
        {
            return null;
        }

        var childNodes = parent.GetChildren(treeSide);
        int childIdx = GetSiblingIndex(treeSide);
        return (childIdx + 1 < childNodes.Count ? childNodes[childIdx + 1] : null);
    }
    
    public bool IsLeaf(NodeInputSide treeSide)
    {
        return (GetNumChildren(treeSide) == 0);
    }

    public bool IsFirstSibling(NodeInputSide treeSide)
    {
        return GetSiblingIndex(treeSide) == 0;
    }

    public int GetNumChildren(NodeInputSide treeSide)
    {
        return GetChildren(treeSide).Count;
    }

    public void ForeachChildrenRecursively(NodeInputSide treeSide, Action<DependencyViewerNode> onEachNodeCallback)
    {
        ForeachChildrenRecursively(treeSide, this, onEachNodeCallback);
    }

    private void ForeachChildrenRecursively(NodeInputSide treeSide, DependencyViewerNode node, Action<DependencyViewerNode> onEachNodeCallback)
    {
        var children = node.GetChildren(treeSide);
        for (int i = 0; i < children.Count; ++i)
        {
            ForeachChildrenRecursively(treeSide, children[i], onEachNodeCallback);
        }

        onEachNodeCallback(node);
    }
}
