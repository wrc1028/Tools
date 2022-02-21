using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AlphaToDitherComponent))]
public class UpdateDitherGUI : Editor
{
    public override void OnInspectorGUI()
    {
        AlphaToDitherComponent controller = (AlphaToDitherComponent)target;
        base.OnInspectorGUI();
        if (GUILayout.Button("更新"))
        {
            controller.UpdateDither();
        }
        if (GUILayout.Button("保存"))
        {
            controller.SaveTexture();
        }
    }
}
