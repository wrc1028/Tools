using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

// [CustomEditor(typeof(LightControllerComponent_v2))]
public class LightControllerGUI_v2 : OdinEditor
{
    public float offset;
    public override void OnInspectorGUI()
    {
        LightControllerComponent_v2 controller = (LightControllerComponent_v2)target;
        controller.sunriseOffset = EditorGUILayout.Slider("日出偏移", controller.sunriseOffset, -180, 180);
        controller.clock = EditorGUILayout.Slider("当前时间", controller.clock, 0, 24);
        
        controller.lightGradient = EditorGUILayout.GradientField("颜色渐变", controller.lightGradient);
        controller.lightIntensity = EditorGUILayout.Slider("灯光强度", controller.lightIntensity, 0, 3);
        
        EditorUtility.SetDirty(target);
    }
}
