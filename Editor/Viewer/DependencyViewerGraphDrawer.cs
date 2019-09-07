using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

internal class DependencyViewerGraphDrawer
{
    enum NodeInputSide { Left, Right }

    internal static readonly Vector2 DistanceBetweenNodes = new Vector2(30, 30);
    internal static readonly Vector2 NodeInsidePadding = new Vector2(10, 10);
    internal const float NodeWidth = 200;
    internal const float NodeHeight = 50;
    internal const float LinkWidth = 2;
    internal static readonly Color LinkColor = Color.black;

    public event Action<UnityEngine.Object> requestViewDependency;

    private DependencyViewerGraph _graph;
    private GUIStyle _titleLabelStyle;

    private Vector2 _screenOffset;
    public Vector2 ScreenOffset
    {
        get { return _screenOffset; }
        set { _screenOffset = value; }
    }

    public DependencyViewerNode RefTargetNode
    { get { return _graph.RefTargetNode; } }

    private Rect _lastWindowRect;

    public DependencyViewerGraphDrawer(DependencyViewerGraph graph)
    {
        _graph = graph;
        _titleLabelStyle = new GUIStyle()
        {
            alignment = TextAnchor.MiddleCenter
        };
    }

    public void CenterViewerOnGraph(Rect windowRect)
    {
        if (_graph == null)
        {
            return;
        }

        Vector2 size = _graph.RefTargetNode.GetSize();
        _screenOffset = new Vector2(-(windowRect.width - size.x) / 2, -(windowRect.height - size.y) / 2);
    }

    public void ShiftView(Vector2 delta)
    {
        _screenOffset += delta;
    }

    public void Draw(Rect windowRect)
    {
        _lastWindowRect = windowRect;

        if (RefTargetNode != null)
        {
            DrawHierarchyNodesFromRefTargetNode();
            DrawLinksFromRefTargetNode();
        }
    }

    private void DrawHierarchyNodesFromRefTargetNode()
    {
        DrawNode(RefTargetNode);
        DrawInputsNodesRecursively(RefTargetNode.LeftInputs, NodeInputSide.Left);
        DrawInputsNodesRecursively(RefTargetNode.RightInputs, NodeInputSide.Right);
    }

    private void DrawInputsNodesRecursively(List<DependencyViewerNode> nodes, NodeInputSide inputSide)
    {
        for (int i = 0; i < nodes.Count; ++i)
        {
            DrawNode(nodes[i]);
            DrawInputsNodesRecursively(inputSide == NodeInputSide.Left ? nodes[i].LeftInputs : nodes[i].RightInputs, inputSide);
        }
    }

    private Vector2 GetRelativePosition(Vector2 worldPosition)
    {
        return new Vector2(worldPosition.x - _screenOffset.x, worldPosition.y - _screenOffset.y);
    }

    private void DrawNode(DependencyViewerNode node)
    {
        Rect boxRect = new Rect(GetRelativePosition(node.Position), node.GetSize());

        Rect localWindowRect = GetLocalWindowRect();
        if (!localWindowRect.Overlaps(boxRect))
        {
            //Debug.Log("Node " + node.Name + " not drawn");
            return;
        }

        GUI.Box(boxRect, GUIContent.none, GUI.skin.FindStyle("flow node 0"));

        DrawNodeTitleBar(node, boxRect);

        Rect boxInsideRect =
            new Rect(
                boxRect.x + NodeInsidePadding.x,
                boxRect.y + NodeInsidePadding.y + EditorGUIUtility.singleLineHeight,
                boxRect.width - NodeInsidePadding.x * 2,
                boxRect.height - NodeInsidePadding.y * 2);

        GUILayout.BeginArea(boxInsideRect);
        {
            bool allowSceneObjects = false;
            EditorGUILayout.ObjectField(node.TargetObject, node.TargetObject.GetType(), allowSceneObjects);

            if (node.PrefabContainer != null)
            {
                DrawPrefabContainer(node);
            }
        }
        GUILayout.EndArea();
    }

    private void DrawPrefabContainer(DependencyViewerNode node)
    {
        bool allowSceneObjects = false;
        EditorGUILayout.BeginHorizontal();
        {
            GUIContent label = new GUIContent("Prefab", $"Prefab reference, on GameObject named '{node.GameObjectNameAsPrefabChild}'");
            EditorGUILayout.LabelField(label, GUILayout.Width(40));
            EditorGUILayout.ObjectField(node.PrefabContainer, typeof(GameObject), allowSceneObjects);
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawNodeTitleBar(DependencyViewerNode node, Rect boxRect)
    {
        Rect boxTitleRect =
            new Rect(
                boxRect.x, boxRect.y + 4,
                boxRect.width, EditorGUIUtility.singleLineHeight);
        
        GUI.Label(boxTitleRect, node.Name, _titleLabelStyle);

        if (node != RefTargetNode)
        {
            Vector2 buttonSize = Vector2.one * 15;
            Vector2 padding = new Vector2(10, 5);

            Rect viewDependencyRect =
                new Rect(boxRect.x + boxRect.width - (padding.x + buttonSize.x), boxRect.y + padding.y, buttonSize.x, buttonSize.y);
            
            if (GUI.Button(viewDependencyRect, new GUIContent("", "View dependency for this object"), GUI.skin.FindStyle("Icon.ExtrapolationContinue")))
            {
                requestViewDependency(node.TargetObject);
            }
        }
    }

    private void DrawLinksFromRefTargetNode()
    {
        DrawNodeLinks(RefTargetNode, RefTargetNode.LeftInputs, NodeInputSide.Left);
        DrawNodeLinks(RefTargetNode, RefTargetNode.RightInputs, NodeInputSide.Right);
    }

    private void DrawNodeLinks(DependencyViewerNode node, List<DependencyViewerNode> inputs, NodeInputSide inputSide)
    {
        for (int i = 0; i < inputs.Count; ++i)
        {
            DependencyViewerNode inputNode = inputs[i];

            DependencyViewerNode leftNode = (inputSide == NodeInputSide.Left ? node : inputNode);
            DependencyViewerNode rightNode = (inputSide == NodeInputSide.Right ? node : inputNode);


            Vector2 start = GetRelativePosition(leftNode.GetLeftInputAnchorPosition());
            Vector2 end = GetRelativePosition(rightNode.GetRightInputAnchorPosition());

            Drawing.DrawLine(start, end, LinkColor, LinkWidth);

            DrawNodeLinks(
                inputNode, 
                inputSide == NodeInputSide.Left ? inputNode.LeftInputs : inputNode.RightInputs, 
                inputSide);
        }
    }

    private Rect GetLocalWindowRect()
    {
        Rect localWindowRect = _lastWindowRect;
        localWindowRect.x = 0;
        localWindowRect.y = 0;
        return localWindowRect;
    }
}
