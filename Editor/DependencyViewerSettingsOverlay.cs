using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

internal class DependencyViewerSettingsOverlay
{
    private static readonly Rect OverlayRect = new Rect(10, 10, 300, EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 3);

    internal event Action onSettingsChanged;

    private DependencyViewerSettings _settings;
    private SerializedObject _settingsSO;

    private bool _isExpanded;

    public DependencyViewerSettingsOverlay(DependencyViewerSettings settings)
    {
        _settings = settings;
        _settingsSO = new SerializedObject(_settings);
        _isExpanded = true;
    }

    public void Draw()
    {
        Rect currentOverlayRect = OverlayRect;
        if (_isExpanded)
        {
            currentOverlayRect.height += CalculateOverlayHeight();
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

    public float CalculateOverlayHeight()
    {
        float height = 0;
        SerializedProperty sp = _settingsSO.GetIterator();
        sp.NextVisible(true); // Skip script property

        while (sp.NextVisible(true))
        {
            height += EditorGUI.GetPropertyHeight(sp, true);
            height += EditorGUIUtility.standardVerticalSpacing;
        }

        return height;
    }
}
