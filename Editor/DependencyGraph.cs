using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class DependencyViewerGraph
{
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
        RearrangeNodesInputsLayout(_refTargetNode, _refTargetNode.LeftInputs, DependencyViewerNode.NodeInputSide.Left);
        RearrangeNodesInputsLayout(_refTargetNode, _refTargetNode.RightInputs, DependencyViewerNode.NodeInputSide.Right);

        TreeLayout.ForeachNode_PostOrderTraversal(_refTargetNode, DependencyViewerNode.NodeInputSide.Right, (data) =>
        {
            //data.currentNode.Position = new Vector2(data.depth * data.currentNode.GetWidth() * 1.5f, data.childIdx * data.currentNode.GetHeight());
            data.currentNode.Position = new Vector2(data.depth, data.childIdx);
        });

        TreeLayout.ForeachNode_PostOrderTraversal(_refTargetNode, DependencyViewerNode.NodeInputSide.Right, (data) =>
        {
            List<DependencyViewerNode> childNodes = data.currentNode.GetInputNodesFromSide(DependencyViewerNode.NodeInputSide.Right);
            if (childNodes.Count == 1)
            {
                data.currentNode.SetPositionY(childNodes[0].Position.y);
            }
            else if (childNodes.Count > 1)
            {
                float min = childNodes[0].Position.y;
                float max = childNodes[childNodes.Count - 1].Position.y;
                data.currentNode.SetPositionY(Mathf.Lerp(min, max, 0.5f));
            }
        });

        TreeLayout.ForeachNode_PostOrderTraversal(_refTargetNode, DependencyViewerNode.NodeInputSide.Right, (data) =>
        {
            data.currentNode.Position = data.currentNode.Position * data.currentNode.GetSize();
        });
    }

    private void RearrangeNodesInputsLayout(DependencyViewerNode refNode, List<DependencyViewerNode> inputNodes, DependencyViewerNode.NodeInputSide inputSide)
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

        int layoutFlowDirection = (inputSide == DependencyViewerNode.NodeInputSide.Left ? -1 : 1);

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
