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
    private static void ViewReferencesFromMenuCommand(MenuCommand menuCommand)
    {
        ViewReferences();
    }

    [MenuItem("Assets/View Dependencies")]
    public static void ViewReferences()
    {
        DependencyViewer referenceViewer = EditorWindow.GetWindow<DependencyViewer>("Dependency Viewer");
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
        _settings.Load();
        _settingsOverlay = new DependencyViewerSettingsOverlay(_settings);
        _resolver = new DependencyResolver(_graph, _settings);
        _statusBar = new DependencyViewerStatusBar();

        _settings.onSettingsChanged += OnSettingsChanged;
        _graphDrawer.requestViewDependency += ViewDependencies;
        
        if (refTarget != null)
        {
            BuildGraph();
        }

        // If the active object is already a DependencyViewerSettings,
        // it probably means that some old settings are actually inspected.
        // Update the active object to now use the new settings object.
        if (Selection.activeObject is DependencyViewerSettings)
        {
            Selection.activeObject = _settings;
        }
    }

    private void OnDisable()
    {
        _settings.Save();
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

        _graphDrawer.Draw(position);
        _settingsOverlay.Draw();
        _statusBar.Draw(position);

        UpdateInputs();
    }

    private void UpdateInputs()
    {
        Event e = Event.current;

        Rect localWindowRect = position;
        localWindowRect.x = localWindowRect.y = 0;

        if (e.type == EventType.MouseDown && e.button == 0 && localWindowRect.Contains(e.mousePosition))
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
