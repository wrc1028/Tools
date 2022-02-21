using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LightControllerComponent))]
public class LightControllerGUI : Editor
{
    private enum AdjustMode
    {
        手动, 自动
    }
    private AdjustMode adjustMode;
    private Vector3 lightForwardDir;
    private float horizontalAngle;
    private float clock;
    private float verticalAngle;
    private Gradient lightColorGradient;
    private float lightIntensity;
    private float sunriseTime;
    private float sundownTime;

    private void OnEnable()
    {
        adjustMode = AdjustMode.手动;
        LightControllerComponent controller = (LightControllerComponent)target;
        lightColorGradient = new Gradient();
        sunriseTime = 6;
        sundownTime = 18;
        lightIntensity = 1;

        Vector3 riseDir = new Vector3(controller.transform.forward.x, 0, controller.transform.forward.z);
        riseDir = riseDir.normalized;
        float LdotR = Vector3.Dot(Vector3.left, riseDir);
        horizontalAngle = controller.transform.forward.z < 0 ? 
            Mathf.Acos(LdotR) * 180 / Mathf.PI : - Mathf.Acos(LdotR) * 180 / Mathf.PI;
        float FdotR = Vector3.Dot(controller.transform.forward, riseDir);
        verticalAngle = controller.transform.forward.y > 0 ? 
            (360 - Mathf.Acos(FdotR) * 180 / Mathf.PI) : Mathf.Acos(FdotR) * 180 / Mathf.PI;
        verticalAngle = verticalAngle > 270 ? verticalAngle - 360 : verticalAngle;
        clock = (verticalAngle + 90) / 15;
    }

    public override void OnInspectorGUI()
    {
        LightControllerComponent controller = (LightControllerComponent)target;
        
        adjustMode = (AdjustMode)EditorGUILayout.EnumPopup("调整模式", adjustMode);

        if (adjustMode == AdjustMode.手动)
        {
            horizontalAngle = EditorGUILayout.Slider("日出偏移", horizontalAngle, -180, 180);
            clock = EditorGUILayout.Slider("当前时间", clock, 0, 24);
            
            EditorGUILayout.MinMaxSlider("白天范围", ref sunriseTime, ref sundownTime, 0, 24);
            lightColorGradient = EditorGUILayout.GradientField("颜色渐变", lightColorGradient);
            lightIntensity = EditorGUILayout.Slider("灯光强度", lightIntensity, 0, 3);
            
            verticalAngle = TimeMapping(clock, sunriseTime, sundownTime) * 15 - 90;

            lightForwardDir.x = Mathf.Cos(horizontalAngle * Mathf.PI / 180) * Mathf.Cos(verticalAngle * Mathf.PI / 180);
            lightForwardDir.y = Mathf.Sin(verticalAngle * Mathf.PI / 180);
            lightForwardDir.z = Mathf.Sin(horizontalAngle * Mathf.PI / 180) * Mathf.Cos(verticalAngle * Mathf.PI / 180);

            controller.lightColor = lightColorGradient.Evaluate((verticalAngle + 90) / 360);
            controller.lightIntensity = lightIntensity;
            controller.lightForwardDir = -lightForwardDir.normalized;
            EditorUtility.SetDirty(target);
        }
    }

    private float TimeMapping(float _time, float _rise, float _down)
    {
        float mappingValue;
        if (_time < _rise)
        {
            mappingValue = _time + 24;
            mappingValue = (mappingValue - _down) / (24 + _rise - _down) * 12 - 6;
            mappingValue = mappingValue < 0 ? mappingValue + 24 : mappingValue;
        }
        else if (_time > _down)
        {
            mappingValue = (_time - _down) / (24 + _rise - _down) * 12 + 18;
            mappingValue = mappingValue > 24 ? mappingValue - 24 : mappingValue;
        }
        else
            mappingValue = (_time - _rise) / (_down - _rise) * 12 + 6;
        return mappingValue;
    }
}
