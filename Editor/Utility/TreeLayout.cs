using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class TreeLayout
{
    public class PostOrderTraversalData
    {
        public DependencyViewerNode parentNode;
        public DependencyViewerNode currentNode;
        public int childIdx;
        public int depth;
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

        PostOrderTraversalData data = new PostOrderTraversalData()
        {
            childIdx = childIdx,
            currentNode = rootNode,
            parentNode = parentNode,
            depth = depth
        };
        callback(data);
    }
}
