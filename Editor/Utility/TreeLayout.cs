using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class TreeLayout
{
    public class PostOrderTraversalData
    {
        public PostOrderTraversalData(DependencyViewerNode.NodeInputSide treeSide)
        {
            _treeSide = treeSide;
        }

        public DependencyViewerNode parentNode;
        public DependencyViewerNode currentNode;
        public int childIdx;
        public int depth;
        private DependencyViewerNode.NodeInputSide _treeSide;

        public DependencyViewerNode.NodeInputSide TreeSide
        { get { return _treeSide; } }
    }

    public static void ForeachNode_PostOrderTraversal(
        DependencyViewerNode rootNode, 
        DependencyViewerNode.NodeInputSide side,
        Action<PostOrderTraversalData> callback)
    {
        ForeachNode_PostOrderTraversal(null, rootNode, side, callback, 0, 0);
    }

    private static void ForeachNode_PostOrderTraversal(
        DependencyViewerNode parentNode,
        DependencyViewerNode rootNode, 
        DependencyViewerNode.NodeInputSide side, 
        Action<PostOrderTraversalData> callback, int childIdx, int depth)
    {
        List<DependencyViewerNode> children = rootNode.GetInputNodesFromSide(side);
        for (int i = 0; i < children.Count; ++i)
        {
            ForeachNode_PostOrderTraversal(rootNode, children[i], side, callback, i, depth + 1);
        }

        PostOrderTraversalData data = new PostOrderTraversalData(side)
        {
            childIdx = childIdx,
            currentNode = rootNode,
            parentNode = parentNode,
            depth = depth
        };
        callback(data);
    }

    public static void GetStartContour(DependencyViewerNode node, int depth, DependencyViewerNode.NodeInputSide childrenSide, float modSum, ref Dictionary<int /* depth */, float /* minY */> values)
    {
        GetContour(node, depth, childrenSide, Mathf.Min, modSum, ref values);
    }

    public static void GetEndContour(DependencyViewerNode node, int depth, DependencyViewerNode.NodeInputSide childrenSide, float modSum, ref Dictionary<int /* depth */, float /* minY */> values)
    {
        GetContour(node, depth, childrenSide, Mathf.Max, modSum, ref values);
    }

    private static void GetContour(
        DependencyViewerNode node, 
        int depth, 
        DependencyViewerNode.NodeInputSide childrenSide, 
        Func<float, float, float> getContourCallback,
        float modSum, 
        ref Dictionary<int /* depth */, float /* minY */> values)
    {
        if (!values.ContainsKey(depth))
        {
            values.Add(depth, node.Position.y + modSum);
        }
        else
        {
            values[depth] = getContourCallback(values[depth], node.Position.y + modSum);
        }

        modSum += node.Mod;

        var children = node.GetInputNodesFromSide(childrenSide);
        foreach (var child in children)
        {
            GetContour(child, depth + 1, childrenSide, getContourCallback, modSum, ref values);
        }
    }
}
