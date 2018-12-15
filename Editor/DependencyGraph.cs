using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

internal class DependencyViewerGraph
{
    // Distance between nodes
    private const float SiblingDistance = 20;

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
        DependencyViewerNode.NodeInputSide referenceTreeSide = DependencyViewerNode.NodeInputSide.Left;
        DependencyViewerNode.NodeInputSide dependencyTreeSide = DependencyViewerNode.NodeInputSide.Right;

        RearrangeNodesInputsLayout(_refTargetNode, _refTargetNode.LeftInputs, referenceTreeSide);
        RearrangeNodesInputsLayout(_refTargetNode, _refTargetNode.RightInputs, dependencyTreeSide);

        InitializeNodes();
        CalculateInitialY();
        CalculateFinalPositions(_refTargetNode, dependencyTreeSide);
    }

    private void InitializeNodes()
    {
        DependencyViewerNode.NodeInputSide dependencyTreeSide = DependencyViewerNode.NodeInputSide.Right;
        TreeLayout.ForeachNode_PostOrderTraversal(_refTargetNode, dependencyTreeSide, (data) =>
        {
            data.currentNode.Position = new Vector2(data.depth * (data.currentNode.GetWidth() + SiblingDistance), -1);
            data.currentNode.Mod = 0;
        });
    }

    private void CalculateInitialY()
    {
        TreeLayout.ForeachNode_PostOrderTraversal(_refTargetNode, DependencyViewerNode.NodeInputSide.Right, (data) =>
        {
            var node = data.currentNode;

            if (node.IsLeaf(data.TreeSide))
            {
                if (node.IsFirstSibling(data.TreeSide))
                {
                    node.SetPositionY(0);
                }
                else
                {
                    var previousSibling = node.GetPreviousSibling(data.TreeSide);
                    node.SetPositionY(previousSibling.Position.y + previousSibling.GetHeight() + SiblingDistance);
                }
            }
            else if (node.GetNumChildren(data.TreeSide) == 1)
            {
                if (node.IsFirstSibling(data.TreeSide))
                {
                    node.SetPositionY(node.GetChildren(data.TreeSide)[0].Position.y);
                }
                else
                {
                    var previousSibling = node.GetPreviousSibling(data.TreeSide);
                    node.SetPositionY(previousSibling.Position.y + previousSibling.GetHeight() + SiblingDistance);
                    node.Mod = node.Position.y - node.GetChildren(data.TreeSide)[0].Position.y;
                }
            }
            else
            {
                var prevChild = node.GetFirstChild(data.TreeSide);
                var nextChild = node.GetLastChild(data.TreeSide);
                float mid = (nextChild.Position.y - prevChild.Position.y) / 2;

                if (node.IsFirstSibling(data.TreeSide))
                {
                    node.SetPositionY(mid);
                }
                else
                {
                    node.SetPositionY(node.GetPreviousSibling(data.TreeSide).Position.y + node.GetHeight() + SiblingDistance);
                    node.Mod = node.Position.y - mid;
                }
            }

            if (node.GetNumChildren(data.TreeSide) > 0 && !node.IsFirstSibling(data.TreeSide))
            {
                CheckForConflicts(node, data.depth, data.TreeSide);
            }
        });
    }

    private void CheckForConflicts(DependencyViewerNode node, int depth, DependencyViewerNode.NodeInputSide treeSide)
    {
        float minDistance = node.GetHeight() + SiblingDistance;
        float shiftValue = 0.0f;
        
        var nodeContour = new Dictionary<int, float>();
        TreeLayout.GetStartContour(node, depth, treeSide, 0, ref nodeContour);

        var sibling = node.GetFirstSibling(treeSide);
        while (sibling != null && sibling != node)
        {
            var siblingContour = new Dictionary<int, float>();
            TreeLayout.GetEndContour(sibling, depth, treeSide, 0, ref siblingContour);

            int maxContourDepth = Mathf.Min(siblingContour.Keys.Max(), nodeContour.Keys.Max());
            for (int level = depth + 1; level <= maxContourDepth; ++level)
            {
                float distance = nodeContour[level] - siblingContour[level];
                if (distance + shiftValue < minDistance)
                {
                    shiftValue = minDistance - distance;
                }
            }

            if (shiftValue > 0)
            {
                node.SetPositionY(node.Position.y + shiftValue);
                node.Mod += shiftValue;

                CenterNodesBetween(node, sibling, treeSide, depth);

                shiftValue = 0;
            }

            sibling = sibling.GetNextSibling(treeSide);
        }
    }

    private void CenterNodesBetween(DependencyViewerNode node, DependencyViewerNode sibling, DependencyViewerNode.NodeInputSide treeSide, int depth)
    {
        int firstNodeIdx = sibling.GetSiblingIndex(treeSide);
        int lastSiblingNodeIdx = node.GetSiblingIndex(treeSide);

        int numNodesBetween = (lastSiblingNodeIdx - firstNodeIdx) - 1;

        if (numNodesBetween > 0)
        {
            float distanceBetweenNodes = (node.Position.y - sibling.Position.y) / (numNodesBetween + 1);

            int count = 1;
            for (int i = firstNodeIdx + 1; i < lastSiblingNodeIdx; ++i)
            {
                var middleNode = node.GetParent(treeSide).GetChildren(treeSide)[i];
                float desiredY = sibling.Position.y + (distanceBetweenNodes * count);
                float offset = desiredY - middleNode.Position.y;
                middleNode.SetPositionY(middleNode.Position.y + offset);
                middleNode.Mod += offset;

                ++count;
            }

            CheckForConflicts(node, depth, treeSide);
        }
    }

    private void CalculateFinalPositions(DependencyViewerNode node, DependencyViewerNode.NodeInputSide treeSide, float modSum = 0)
    {
        node.SetPositionY(node.Position.y + modSum);
        modSum += node.Mod;

        foreach (var child in node.GetChildren(treeSide))
        {
            CalculateFinalPositions(child, treeSide, modSum);
        }
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
