using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class DependencyViewer : EditorWindow
{
    public UnityEngine.Object refTarget;
    private DependencyViewerSettings _settings;
    internal DependencyViewerSettings Settings
    {
        get { return _settings; }
        set { _settings = value; }
    }

    private DependencyViewerGraph _graph;
    private DependencyViewerGraphDrawer _graphDrawer;
    private DependencyViewerSettingsOverlay _settingsOverlay;
    private DependencyViewerStatusBar _statusBar;
    private DependencyResolver _resolver;

    private bool _readyToDrag;
    private bool _isDragging;

    private IEnumerator<DependencyViewerOperation> _resolverWorkHandle;

    [MenuItem("GameObject/View Dependencies", priority = 10)]
    private static void ViewReferenceInCurrentSceneFromMenuCommand(MenuCommand menuCommand)
    {
        ViewReferenceInCurrentScene();
    }

    [MenuItem("Assets/View Dependencies")]
    public static void ViewReferenceInCurrentScene()
    {
        DependencyViewer referenceViewer = EditorWindow.GetWindow<DependencyViewer>("Dependency Viewer");
        referenceViewer.Settings.ShouldSearchInCurrentScene = true;
        referenceViewer.ViewDependencies(Selection.activeObject);
    }

    public void ViewDependencies(UnityEngine.Object targetObject)
    {
        refTarget = targetObject;
        BuildGraph();
    }

    IEnumerator GetEnumerator()
    {
        yield return null;
    }

    private void OnEnable()
    {
        _graph = new DependencyViewerGraph();
        _graphDrawer = new DependencyViewerGraphDrawer(_graph);
        _settings = DependencyViewerSettings.Create();
        _settingsOverlay = new DependencyViewerSettingsOverlay(_settings);
        _resolver = new DependencyResolver(_graph, _settings);
        _statusBar = new DependencyViewerStatusBar();

        _graphDrawer.requestViewDependency += ViewDependencies;
        _settingsOverlay.onSettingsChanged += OnSettingsChanged;

        if (refTarget != null)
        {
            BuildGraph();
        }
    }

    private void OnSettingsChanged()
    {
        BuildGraph();
    }

    public void BuildGraph()
    {
        _graph.CreateReferenceTargetNode(refTarget);
        _resolverWorkHandle = _resolver.BuildGraph();
        _resolverWorkHandle.MoveNext();
        _graph.RearrangeNodesLayout();
        CenterViewerOnGraph();
    }

    private void Update()
    {
        if (_resolverWorkHandle != null)
        {
            bool isResolverWorkCompleted = !_resolverWorkHandle.MoveNext();

            // Update status bar according to current operation state
            DependencyViewerOperation currentOperation = _resolverWorkHandle.Current;
            if (currentOperation != null)
            {
                _statusBar.SetText(currentOperation.GetStatus());
            }

            _graph.RearrangeNodesLayout();
            if (isResolverWorkCompleted)
            {
                _resolverWorkHandle = null;
                _statusBar.SetText("Completed!");
            }

            Repaint();
        }
   }

    private void CenterViewerOnGraph()
    {
        _graphDrawer.CenterViewerOnGraph(position);
    }

    private void OnGUI()
    {
        if (refTarget == null)
        {
            return;
        }

        UpdateInputs();

        _graphDrawer.Draw();
        _settingsOverlay.Draw();
        _statusBar.Draw(position);
    }

    private void UpdateInputs()
    {
        Event e = Event.current;

        if (e.type == EventType.MouseDown && e.button == 0 && position.Contains(e.mousePosition))
        {
            _readyToDrag = true;
        }

        if ((_readyToDrag || _isDragging) && e.type == EventType.MouseDrag && e.button == 0)
        {
            _readyToDrag = false;
            _isDragging = true;

            Vector2 offset = e.delta;
            _graphDrawer.ShiftView(-offset);

            Repaint();
        }

        if (_isDragging && e.type == EventType.MouseUp && e.button == 0)
        {
            _isDragging = false;
        }
    }
}
