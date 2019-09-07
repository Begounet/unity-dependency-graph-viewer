using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

internal class DependencyViewerStatusBar
{
    private static readonly float StatusBarHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 2;
    private static readonly Vector2 StatusBarMargin = new Vector2(5, 5);
    private static readonly Vector2 StatusBarPadding = new Vector2(2, 2);

    private string _statusText;

    public void SetText(string newText)
    {
        _statusText = newText;
    }
    
    public void ClearText()
    {
        SetText(string.Empty);
    }

    public void Draw(Rect windowPosition)
    {
        Rect statusBarRect = 
            new Rect(StatusBarMargin.x, windowPosition.height - (StatusBarHeight + StatusBarMargin.y), 
            windowPosition.width - StatusBarMargin.x * 2, StatusBarHeight);

        GUI.Box(statusBarRect, GUIContent.none);

        Rect statusBarContentRect = 
            new Rect(statusBarRect.x + StatusBarPadding.x, statusBarRect.y + StatusBarPadding.y,
            statusBarRect.width - StatusBarPadding.x * 2, statusBarRect.height - StatusBarPadding.y * 2);

        GUI.Label(statusBarContentRect, _statusText);
    }
}
