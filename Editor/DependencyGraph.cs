using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        OrganizeNodesInTree(DependencyViewerNode.NodeInputSide.Right);
        OrganizeNodesInTree(DependencyViewerNode.NodeInputSide.Left);
        CenterTreeToRoot(DependencyViewerNode.NodeInputSide.Right);
    }

    private void OrganizeNodesInTree(DependencyViewerNode.NodeInputSide treeSide)
    {
        InitializeNodes(treeSide);
        CalculateInitialY(treeSide);
        CalculateFinalPositions(_refTargetNode, treeSide);
    }

    private void InitializeNodes(DependencyViewerNode.NodeInputSide treeSide)
    {
        TreeLayout.ForeachNode_PostOrderTraversal(_refTargetNode, treeSide, (data) =>
        {
            int direction = (treeSide == DependencyViewerNode.NodeInputSide.Right ? 1 : -1);
            data.currentNode.Position = new Vector2(data.depth * (data.currentNode.GetWidth() + DependencyViewerGraphDrawer.DistanceBetweenNodes.x) * direction, -1);
            data.currentNode.Mod = 0;
        });
    }

    private void CalculateInitialY(DependencyViewerNode.NodeInputSide treeSide)
    {
        TreeLayout.ForeachNode_PostOrderTraversal(_refTargetNode, treeSide, (data) =>
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
                    node.SetPositionY(previousSibling.Position.y + previousSibling.GetHeight() + DependencyViewerGraphDrawer.DistanceBetweenNodes.y);
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
                    node.SetPositionY(previousSibling.Position.y + previousSibling.GetHeight() + DependencyViewerGraphDrawer.DistanceBetweenNodes.y);
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
                    node.SetPositionY(node.GetPreviousSibling(data.TreeSide).Position.y + node.GetHeight() + DependencyViewerGraphDrawer.DistanceBetweenNodes.y);
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
        float minDistance = node.GetHeight() + DependencyViewerGraphDrawer.DistanceBetweenNodes.y;
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

    private void CenterTreeToRoot(DependencyViewerNode.NodeInputSide side)
    {
        var children = _refTargetNode.GetChildren(side);

        if (children.Count > 1)
        {
            float size = _refTargetNode.GetLastChild(side).Position.y - _refTargetNode.GetFirstChild(side).Position.y;
            float actualY = _refTargetNode.GetFirstChild(side).Position.y;
            float desiredY = _refTargetNode.Position.y - size / 2.0f;
            float shiftY = desiredY - actualY;

            for (int i = 0; i < children.Count; ++i)
            {
                children[i].ForeachChildrenRecursively(side, (node) => node.SetPositionY(node.Position.y + shiftY));
            }
        }
    }

}
