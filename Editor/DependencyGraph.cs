using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class DependencyViewerGraph
{
    enum NodeInputSide { Left, Right }

    private DependencyViewerNode _refTargetNode;
    internal DependencyViewerNode RefTargetNode
    {
        get { return _refTargetNode; }
        set { _refTargetNode = value; }
    }

    public void CreateReferenceTargetNode(UnityEngine.Object refTarget)
    {
        _refTargetNode = new DependencyViewerNode(refTarget);
    }
    
    public void RearrangeNodesLayout()
    {
        RearrangeNodesInputsLayout(_refTargetNode, _refTargetNode.LeftInputs, NodeInputSide.Left);
        RearrangeNodesInputsLayout(_refTargetNode, _refTargetNode.RightInputs, NodeInputSide.Right);
    }

    private void RearrangeNodesInputsLayout(DependencyViewerNode refNode, List<DependencyViewerNode> inputNodes, NodeInputSide inputSide)
    {
        int numInputs = inputNodes.Count;
        float totalNodeHeights = 0;
        for (int inputIdx = 0; inputIdx < numInputs; ++inputIdx)
        {
            totalNodeHeights += inputNodes[inputIdx].GetHeight();
            if (inputIdx + 1 < numInputs)
            {
                totalNodeHeights += DependencyViewerGraphDrawer.DistanceBetweenNodes.y;
            }
        }

        int layoutFlowDirection = (inputSide == NodeInputSide.Left ? -1 : 1);

        float offsetY = (refNode.Position.y + refNode.GetHeight() / 2) - (totalNodeHeights / 2);
        for (int inputIdx = 0; inputIdx < numInputs; ++inputIdx)
        {
            DependencyViewerNode inputNode = inputNodes[inputIdx];
            inputNode.Position = new Vector2(
                (_refTargetNode.Position.x + DependencyViewerGraphDrawer.DistanceBetweenNodes.x) * layoutFlowDirection,
                offsetY);

            offsetY += inputNode.GetHeight() + DependencyViewerGraphDrawer.DistanceBetweenNodes.y;
        }
    }

    internal static void CreateNodeLink(DependencyViewerNode leftNode, DependencyViewerNode rightNode)
    {
        if (!leftNode.RightInputs.Contains(rightNode))
        {
            leftNode.RightInputs.Add(rightNode);
        }

        if (!rightNode.LeftInputs.Contains(leftNode))
        {
            rightNode.LeftInputs.Add(leftNode);
        }
    }
}
