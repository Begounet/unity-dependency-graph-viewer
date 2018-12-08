using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

internal class DependencyViewerSettingsOverlay
{
    private static readonly Rect OverlayRect = new Rect(10, 10, 200, 100);

    internal event Action onSettingsChanged;

    private DependencyViewerSettings _settings;
    private SerializedObject _settingsSO;


    public DependencyViewerSettingsOverlay(DependencyViewerSettings settings)
    {
        _settings = settings;
        _settingsSO = new SerializedObject(_settings);
    }

    public void Draw()
    {
        GUILayout.BeginArea(OverlayRect, EditorStyles.helpBox);
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
        GUILayout.EndArea();
    }
}
