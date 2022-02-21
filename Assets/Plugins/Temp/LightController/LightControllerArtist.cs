#if  UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
// 美术版本:两个环
public class LightControllerArtist : EditorWindow
{
    private static LightControllerArtist window;

    [MenuItem("Tools/控制平行光美术版")]
    private static void ShowWindow()
    {
        window = (LightControllerArtist)EditorWindow.GetWindow(typeof(LightControllerArtist), true, "控制平行光美术版");
        window.minSize = new Vector2(240, 480);
        window.maxSize = new Vector2(240, 480);
        window.Show();
    }
    #region 属性
    // 资源加载
    private Texture[] icons = new Texture[3];
    private string iconsPath = "Assets/Resources/Texture2D/";
    private Material material;
    private bool isInit = false;

    // 圆环数据
    private Vector2 centerPos;
    private Vector2 touchPointSize;
    private Rect clockRect;
    private Vector2 clockDir;
    private float clockRadius;
    private Rect sunriseRect;
    private Vector2 sunriseDir;
    private float sunriseRadius;
    private int controlID;
    private bool isDragClock;
    private bool isDragSunrise;

    // 平行光数据
    private Transform lightTransform;
    private Vector3 lightPos;
    private Vector3 horizontalPos;
    private Vector3 aroundDir;
    private float verticalAngle;

    // 灯光数据
    public enum LightSelectMode
    {
        Automatic, Custom,
    }
    private LightSelectMode lightSelectMode;
    private Light currentLight;
    private Light customLight;
    private bool isUseCustomLight;
    private Season spring;

    #endregion

    private void OnGUI()
    {
        InitProp();
        ShowLightSetting();
        MouseEvent();
        ShowCircleDdjuster();
        DrawTouchPoint();
        CalculateLightDir();
    }
    // 初始化工具
    private void InitProp()
    {
        if (isInit) return;
        Debug.Log("初始化");
        icons[0] = AssetDatabase.LoadAssetAtPath(iconsPath + "circle_double.png", typeof(Texture2D)) as Texture;
        icons[1] = AssetDatabase.LoadAssetAtPath(iconsPath + "light.png", typeof(Texture2D)) as Texture;
        icons[2] = AssetDatabase.LoadAssetAtPath(iconsPath + "sunrise.png", typeof(Texture2D)) as Texture;
        material = new Material(Shader.Find("Unlit/Transparent"));

        centerPos = new Vector2(120, 130);
        touchPointSize = new Vector2(25.6f, 25.6f);
        clockRect = new Rect(Vector2.zero, touchPointSize);
        clockDir = new Vector2(-1, 0);
        clockRadius = 101.5f;
        sunriseRect = new Rect(Vector2.zero, touchPointSize);
        sunriseRadius = 70.0f;
        sunriseDir = new Vector2(0, -1);
        isDragClock = false;
        isDragSunrise = false;
        
        InitLight(null);

        isInit = true;
    }
    private void InitLight(Light _light)
    {
        if (_light == null)
        {
            currentLight = FindMainDirectionalLight();
            isUseCustomLight = false;
        }
        else
        {
            currentLight = _light;
            isUseCustomLight = true;
        }
        lightTransform = currentLight.transform;
        CalculateToolParameters(lightTransform.forward);
    }
    // 初始化工具数据
    private void CalculateToolParameters(Vector3 _lightPos)
    {
        // 计算朝向按钮位置
        sunriseDir.x = -_lightPos.x;
        sunriseDir.y = _lightPos.z;
        sunriseDir = sunriseDir.normalized;
        // 计算日出按钮位置
        clockDir.x = -Mathf.Sqrt(_lightPos.x * _lightPos.x + _lightPos.z * _lightPos.z);
        clockDir.y = _lightPos.y;
        clockDir = clockDir.normalized;
        // 绘制及计算角度
        DrawTouchPoint();
        CalculateLightDir();
    }
    // 找到场景中最后也是最亮的直射光
    private Light FindMainDirectionalLight()
    {
        Light[] allLights = GameObject.FindObjectsOfType<Light>();
        int priorLight = 0;
        for (int i = 0; i < allLights.Length; i++)
        {
            if (allLights[i].type == LightType.Directional && allLights[i].intensity >= allLights[priorLight].intensity)
                priorLight = i;
        }
        return allLights[priorLight];
    }
    // 鼠标事件
    private void MouseEvent()
    {
        Event e = Event.current;
        controlID = GUIUtility.GetControlID(FocusType.Passive);
        switch (e.GetTypeForControl(controlID))
        {
            case EventType.MouseDown:
                if (clockRect.Contains(e.mousePosition)) isDragClock = true;
                if (sunriseRect.Contains(e.mousePosition)) isDragSunrise = true;
                break;
            case EventType.MouseUp:
                isDragClock = false;
                isDragSunrise = false;
                break;
            case EventType.MouseDrag:
                if (isDragClock)
                    clockDir = (e.mousePosition - centerPos).normalized;
                if (isDragSunrise)
                    sunriseDir = (e.mousePosition - centerPos).normalized;
                DrawTouchPoint();
                e.Use();
                break;
            default:
                break;
        }
    }
    // 组件1:显示圆环
    private void ShowCircleDdjuster()
    {
        GUI.DrawTexture(new Rect(0, 10, 240, 240), icons[0]);
    }
    //功能:画出控制点
    private void DrawTouchPoint()
    {
        clockRect.position = centerPos + clockDir * clockRadius - touchPointSize / 2;
        GUI.DrawTexture(clockRect, icons[1]);
        sunriseRect.position = centerPos + sunriseDir * sunriseRadius - touchPointSize / 2;
        GUI.DrawTexture(sunriseRect, icons[2]);
    }
    // 计算灯光方向
    private void CalculateLightDir()
    {
        // 平面向量
        horizontalPos.x = (sunriseRect.position - centerPos + touchPointSize / 2).normalized.x;
        horizontalPos.z = -(sunriseRect.position - centerPos + touchPointSize / 2).normalized.y;
        horizontalPos = horizontalPos.normalized;
        // 垂直向量
        aroundDir = Vector3.Cross(Vector3.up, horizontalPos);
        CalculateSunriseAngle((clockRect.position - centerPos + touchPointSize / 2).normalized);
        Vector3 newPos = new Vector3(horizontalPos.x * Mathf.Cos(verticalAngle), 
            Mathf.Sin(verticalAngle), horizontalPos.z * Mathf.Cos(verticalAngle));
        lightTransform.forward = (Vector3.zero - newPos).normalized;
    }
    // 计算日出角度
    private void CalculateSunriseAngle(Vector2 _sunriseDir)
    {
        Vector3 mouseDir = new Vector3(_sunriseDir.x, -_sunriseDir.y, 0);
        float RdotM = Vector3.Dot(Vector3.left, mouseDir);
        Vector3 crossValue = Vector3.Cross(Vector3.left, mouseDir);
        float angle = Mathf.Acos(RdotM);
        verticalAngle = crossValue.z > 0 ? 2 * Mathf.PI - angle : angle;
    }
    // 组件2:显示灯光设置
    private void ShowLightSetting()
    {
        GUI.Label(new Rect(15, 260, 80, 20), "灯光选择");
        lightSelectMode = (LightSelectMode)EditorGUI.EnumPopup(new Rect(80, 260, 145, 20), lightSelectMode);
        if (lightSelectMode == LightSelectMode.Automatic)
        {
            if (isUseCustomLight) InitLight(null);
            GUI.Label(new Rect(15, 285, 80, 20), "灯光颜色");
            currentLight.color = EditorGUI.ColorField(new Rect(80, 285, 145, 20), currentLight.color);
            GUI.Label(new Rect(15, 305, 80, 20), "灯光强度");
            currentLight.intensity = EditorGUI.Slider(new Rect(80, 305, 145, 20), currentLight.intensity, 0, 3);
        }
        else
        {
            GUI.Label(new Rect(15, 285, 80, 20), "自定义灯");
            customLight = (Light)EditorGUI.ObjectField(new Rect(80, 285, 145, 20), customLight, typeof(Light), customLight);
            if (customLight != null && !isUseCustomLight) InitLight(customLight);
            GUI.Label(new Rect(15, 305, 80, 20), "灯光颜色");
            currentLight.color = EditorGUI.ColorField(new Rect(80, 305, 145, 20), currentLight.color);
            GUI.Label(new Rect(15, 325, 80, 20), "灯光强度");
            currentLight.intensity = EditorGUI.Slider(new Rect(80, 325, 145, 20), currentLight.intensity, 0, 3);
            // GUI.Label(new Rect(15, 345, 80, 20), "季节");
            // spring = (Season)EditorGUI.ObjectField(new Rect(80, 345, 145, 20), spring, typeof(Season), spring);
            // if (spring != null)
            // {
            //     GUI.Label(new Rect(15, 365, 80, 20), "白天范围");
            //     EditorGUI.MinMaxSlider(new Rect(80, 365, 145, 20), ref spring.sunriseTime, ref spring.sundownTime, 0, 24);
            //     GUI.Label(new Rect(15, 385, 80, 20), "颜色渐变");
            //     spring.lightGradient = EditorGUI.GradientField(new Rect(80, 385, 145, 20), spring.lightGradient);
            // }
        }
    }
}
#endif