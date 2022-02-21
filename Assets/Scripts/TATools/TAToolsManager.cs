#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

public class TAToolsManager : OdinEditorWindow
{
    public static EditorWindow ToolWindow;
    [MenuItem("Odin/TA工具")]
    public static void ShowWindow()
    {
        ToolWindow = (TAToolsManager)EditorWindow.GetWindow(typeof(TAToolsManager), false, "TA工具");
        ToolWindow.minSize = new Vector2(480, 320);
        ToolWindow.Show();
    }

    [HorizontalGroup("main", Width = 160, LabelWidth = 40)]
    [BoxGroup("main/left", false)] 
    [LabelText("右边")]
    public string left;
    [HorizontalGroup("main", Width = 320, LabelWidth = 40)]
    [BoxGroup("main/right", false)]
    [LabelText("左边")]
    public string right;
}
#endif