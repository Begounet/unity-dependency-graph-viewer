using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using System;
using System.Linq;
using System.IO;

internal class DependencyResolver
{
    private DependencyViewerGraph _graph;
    private DependencyViewerSettings _settings;
    private DependencyResolver_References _referencesResolver;
    private DependencyResolver_Dependencies _dependenciesResolver;

    public DependencyResolver(DependencyViewerGraph graph, DependencyViewerSettings settings)
    {
        _graph = graph;
        _settings = settings;
        _referencesResolver = new DependencyResolver_References(graph, settings);
        _dependenciesResolver = new DependencyResolver_Dependencies(settings);
    }

    public IEnumerator<DependencyViewerOperation> BuildGraph()
    {
        if (_settings.FindDependencies)
        {
            _dependenciesResolver.FindDependencies(_graph.RefTargetNode, _settings.DependenciesDepth);
        }

        foreach (var currentOperation in _referencesResolver.FindReferences())
        {
            yield return currentOperation;
        }
    }
}
