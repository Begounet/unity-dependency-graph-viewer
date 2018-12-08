using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

internal class DependencyViewerSettingsOverlay
{
    private static readonly Rect OverlayRect = new Rect(10, 10, 300, 80);

    internal event Action onSettingsChanged;

    private DependencyViewerSettings _settings;
    private SerializedObject _settingsSO;

    private bool _isExpanded;

    public DependencyViewerSettingsOverlay(DependencyViewerSettings settings)
    {
        _settings = settings;
        _settingsSO = new SerializedObject(_settings);
        _isExpanded = false;
    }

    public void Draw()
    {
        Rect currentOverlayRect = OverlayRect;
        if (!_isExpanded)
        {
            currentOverlayRect.height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 2;
        }

        GUILayout.BeginArea(currentOverlayRect, EditorStyles.helpBox);
        {
            _isExpanded = EditorGUILayout.Foldout(_isExpanded, "Settings");

            if (_isExpanded)
            {
                _settingsSO.Update();

                SerializedProperty sp = _settingsSO.GetIterator();

                sp.NextVisible(true); // Skip script property

                EditorGUI.BeginChangeCheck();
                {
                    while (sp.NextVisible(true))
                    {
                        EditorGUILayout.PropertyField(sp, true);
                    }
                    _settingsSO.ApplyModifiedProperties();
                }
                if (EditorGUI.EndChangeCheck() && onSettingsChanged != null)
                {
                    onSettingsChanged();
                }
            }
        }
        GUILayout.EndArea();
    }
}
