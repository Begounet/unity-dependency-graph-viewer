using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

internal class DependencyViewerSettingsOverlay
{
    private static readonly Rect OverlayRect = new Rect(10, 10, 120, EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 3);
    private DependencyViewerSettings _settings;
    
    public DependencyViewerSettingsOverlay(DependencyViewerSettings settings)
    {
        _settings = settings;
    }

    public void Draw()
    {
        if (GUI.Button(OverlayRect, "Open Settings..."))
        {
            Selection.activeObject = _settings;
        }
    }
}
