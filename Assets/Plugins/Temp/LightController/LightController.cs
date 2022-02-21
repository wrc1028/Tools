#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LightController : EditorWindow
{
    private static LightController window;

    [MenuItem("Tools/控制平行光")]
    private static void ShowWindow()
    {
        window = (LightController)EditorWindow.GetWindow(typeof(LightController), true, "控制平行光");
        window.minSize = new Vector2(240, 480);
        window.maxSize = new Vector2(240, 480);
        window.Show();
    }

    #region 属性
    // 资源加载
    private bool isInit = false;
    private Light currentLight;
    private Transform lightTransform;
    private Texture2D[] icons = new Texture2D[4];
    private string iconsPath = "Assets/Resources/Texture2D/";
    private Material material;
    private Material gradientMat;

    // 圆环数据
    private float radius;
    private Vector2 centerPos;
    private Vector2 controllerSize;
    private Rect controllerRect;
    private Vector2 controllerDir;
    
    // 方向数据
    private enum EastRef
    {
        正X方向, 负X方向, 正Z方向, 负Z方向
    }
    private Vector3[] refDirections;
    private Vector3 refDirection;
    private EastRef eastRef = EastRef.正X方向;
    private EastRef currentRef = EastRef.正X方向;
    private float clockwiseDrift = 0;

    // 鼠标事件
    private int controlID;
    private bool isDrag;

    // GUI属性
    private float currentTime;

    // 计算灯光方向属性
    private Vector3 horizontalFactor;
    private float RdotC;
    private float verticalFactor;
    private Vector3 lightPos;
    private Color currentColor;
    private Texture2D gradientTex;
    private Dictionary<int, Color> colorIndexDict;
    private List<int> colorIndex;
    private List<Color> lightColor;

    // 昼夜属性
    private float sunriseTime;
    private float sundownTime;

    #endregion

    #region 方法
    private void OnGUI()
    {
        InitProp();
        NormalGUI();
        MouseEvent();
        ApplyLightSetting();
    }

    #region 初始化
    // 初始化值:执行一次
    private void InitProp()
    {
        if (isInit) return;
        // 资源加载
        currentLight = FindMainDirectionalLight();
        lightTransform = currentLight.transform;
        icons[0] = AssetDatabase.LoadAssetAtPath(iconsPath + "circle.png", typeof(Texture2D)) as Texture2D;
        icons[1] = AssetDatabase.LoadAssetAtPath(iconsPath + "TouchPoint.png", typeof(Texture2D)) as Texture2D;
        icons[2] = AssetDatabase.LoadAssetAtPath(iconsPath + "daynight.png", typeof(Texture2D)) as Texture2D;
        icons[3] = AssetDatabase.LoadAssetAtPath(iconsPath + "time.png", typeof(Texture2D)) as Texture2D;
        material = new Material(Shader.Find("Unlit/Transparent"));
        gradientMat = new Material(Shader.Find("Unlit/LightGradientColor"));

        // 圆环数据
        radius = 99.0f;
        centerPos = new Vector2(120, 170);
        controllerSize = new Vector2(26, 26);
        controllerRect = new Rect(Vector2.zero, controllerSize);
        controllerDir = CalculateSunriseAngle(lightTransform.forward);
        controllerRect.position = centerPos + controllerDir * radius - controllerSize / 2;

        // 太阳升起方向数据
        refDirections = new Vector3[4] { Vector3.right, Vector3.left, Vector3.forward, Vector3.back };
        clockwiseDrift = CalculateClockwiseDrift(lightTransform.forward, out currentRef, out refDirection);
        eastRef = currentRef;

        // 阳光渐变图
        currentColor = Color.white;
        gradientTex = new Texture2D(512, 1, TextureFormat.ARGB32, false, true);
        gradientTex.wrapMode = TextureWrapMode.Clamp;
        colorIndexDict = new Dictionary<int, Color>();
        colorIndex = new List<int>();
        lightColor = new List<Color>();
        colorIndex.Add(0);
        lightColor.Add(Color.black);
        colorIndex.Add(512);
        lightColor.Add(Color.black);
        RankColor(255, Color.white);
        sunriseTime = 6;
        sundownTime = 18;

        NormalGUI();
        isInit = true;
    }
    // 返回一个场景中的平行光
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
    // 计算太阳升起的角度
    private Vector2 CalculateSunriseAngle(Vector3 _lightForward)
    {
        Vector2 tempVector = Vector2.left;
        Vector3 riseDir = new Vector3(_lightForward.x, 0, _lightForward.z);
        riseDir = riseDir.normalized;
        tempVector.x =  -Vector3.Dot(_lightForward, riseDir);
        tempVector.y = Mathf.Sqrt(1 - tempVector.x * tempVector.x) * (_lightForward.y > 0 ? 1 : -1);
        return tempVector.normalized;
    }
    // 计算太阳升起的方向及偏移值
    private float CalculateClockwiseDrift(Vector3 _lightForward, out EastRef _currentRef, out Vector3 _refDirection)
    {
        _currentRef = EastRef.正X方向;
        _refDirection = Vector3.left;
        Vector3 riseDir = new Vector3(_lightForward.x, 0, _lightForward.z);
        riseDir = riseDir.normalized;
        for (int i = 0; i < refDirections.Length; i++)
        {
            if (!(Mathf.Sqrt(0.5f) <= Vector3.Dot(-riseDir, refDirections[i]))) continue;
            switch (i)
            {
                case 0:
                    _currentRef = EastRef.正X方向;
                    _refDirection = Vector3.right;
                    break;
                case 1:
                    _currentRef = EastRef.负X方向;
                    _refDirection = Vector3.left;
                    break;
                case 2:
                    _currentRef = EastRef.正Z方向;
                    _refDirection = Vector3.forward;
                    break;
                default:
                    _currentRef = EastRef.负Z方向;
                    _refDirection = Vector3.back;
                    break;
            }
        }
        float angleHorizontal = Mathf.Acos(Vector3.Dot(-riseDir, _refDirection)) * 180 / Mathf.PI;
        bool isInvertH = Vector3.Cross(-riseDir, _refDirection).y > 0 ? true : false;
        return isInvertH ? -angleHorizontal : angleHorizontal;
    }
    #endregion

    private void NormalGUI()
    {
        // 显示太阳升起的方向，显示当前时间
        CalculateClock();
        GUI.Label(new Rect(10, 10, 60, 20), "当前时间 : ");
        GUI.Label(new Rect(70, 10, 150, 20), string.Format("{0}:{1}", Mathf.FloorToInt(currentTime), (int)(currentTime % 1 * 60)));
        // 显示圆环
        GUI.DrawTexture(new Rect(5, 55, 230, 230), icons[2]);
        GUI.DrawTexture(new Rect(5, 55, 230, 230), icons[0]);
        controllerRect.position = centerPos + controllerDir * radius - controllerSize / 2;
        GUI.DrawTexture(controllerRect, icons[1]);
        // 显示其他设置
        GUI.Label(new Rect(10, 300, 90, 20), "太阳升起方向");
        eastRef = (EastRef)EditorGUI.EnumPopup(new Rect(95, 300, 135, 20), eastRef);
        GUI.Label(new Rect(10, 320, 90, 20), "顺时针偏移值");
        clockwiseDrift = GUI.HorizontalSlider(new Rect(95, 320, 135, 20), clockwiseDrift, -45.0f, 45.0f);
        
        // 显示颜色
        GUI.Label(new Rect(10, 340, 90, 20), "设置阳光颜色");
        currentColor = EditorGUI.ColorField(new Rect(95, 340, 80, 20), currentColor);
        if(GUI.Button(new Rect(175, 340, 55, 20), "设置"))
        {
            int index = (int)Mathf.Floor(512 * currentTime / 24);
            RankColor(index, currentColor);
        }
        EditorGUI.MinMaxSlider(new Rect(12, 380, 220, 20), "", ref sunriseTime, ref sundownTime, 0, 24);
        GUI.Label(new Rect(10, 360, 90, 20), "设置白天范围");
        GUI.Label(new Rect(95, 360, 65, 20), string.Format("清晨 {0}:{1}", Mathf.FloorToInt(sunriseTime), (int)(sunriseTime % 1 * 60)));
        GUI.Label(new Rect(165, 360, 65, 20), string.Format("傍晚 {0}:{1}", Mathf.FloorToInt(sundownTime), (int)(sundownTime % 1 * 60)));
        GUI.DrawTexture(new Rect(12, 397, 220, 14), icons[3]);
        GUI.DrawTexture(new Rect(12, 414, 220, 16), gradientTex);
    }
    // 冒泡算法
    private void RankColor(int _index, Color _color)
    {
        for (int i = 0; i < colorIndex.Count; i++)
        {
            if (colorIndex[i] < _index && colorIndex[i + 1] > _index)
            {
                colorIndex.Insert(i + 1, _index);
                lightColor.Insert(i + 1, _color);
                break;
            }
            else if (colorIndex[i] ==  _index)
            {
                lightColor[i] = _color;
                break;
            }
        }
        LerpLightColor();
    }

    // 线性插值颜色
    private void LerpLightColor()
    {
        float lerpValue;
        Color lerpColor = Color.black;
        for (int i = 0; i < colorIndex.Count - 1; i++)
        {
            for (int j = 0; j < colorIndex[i + 1] - colorIndex[i]; j++)
            {
                lerpValue = ((float)j + 1) / ((float)colorIndex[i + 1] - (float)colorIndex[i]);
                lerpColor.r = Mathf.Lerp(lightColor[i].r, lightColor[i + 1].r, lerpValue);
                lerpColor.g = Mathf.Lerp(lightColor[i].g, lightColor[i + 1].g, lerpValue);
                lerpColor.b = Mathf.Lerp(lightColor[i].b, lightColor[i + 1].b, lerpValue);
                gradientTex.SetPixel(j + colorIndex[i], 1, lerpColor);
            }
        }
        gradientTex.Apply();
    }

    private void CalculateClock()
    {
        currentTime = controllerDir.x < 0 ? Mathf.Acos(Vector2.Dot(controllerDir, Vector2.up)) : 
            2 * Mathf.PI - Mathf.Acos(Vector2.Dot(controllerDir, Vector2.up));
        currentTime = currentTime * 12 / Mathf.PI;
    }

    // 鼠标事件
    private void MouseEvent()
    {
        Event e = Event.current;
        controlID = GUIUtility.GetControlID(FocusType.Passive);
        switch (e.GetTypeForControl(controlID))
        {
            case EventType.MouseDown:
                if (controllerRect.Contains(e.mousePosition)) isDrag = true;
                break;
            case EventType.MouseUp:
                isDrag = false;
                break;
            case EventType.MouseDrag:
                if (isDrag)
                    controllerDir = (e.mousePosition - centerPos).normalized;
                controllerRect.position = centerPos + controllerDir * radius - controllerSize / 2;
                EditorGUI.DrawPreviewTexture(controllerRect, icons[1], material);
                e.Use();
                break;
            default:
                break;
        }
    }
    // 计算灯光
    private void ApplyLightSetting()
    {
        // 水平方向
        if (currentRef != eastRef) currentRef = eastRef;
        else
        {
            switch (currentRef)
            {
                case EastRef.正X方向:
                    horizontalFactor.x = Mathf.Cos((clockwiseDrift / 180.0f) * Mathf.PI);
                    horizontalFactor.y = 0;
                    horizontalFactor.z = -Mathf.Sin((clockwiseDrift / 180.0f) * Mathf.PI);
                    break;
                case EastRef.负X方向:
                    horizontalFactor.x = -Mathf.Cos((clockwiseDrift / 180.0f) * Mathf.PI);
                    horizontalFactor.y = 0;
                    horizontalFactor.z = Mathf.Sin((clockwiseDrift / 180.0f) * Mathf.PI);
                    break;
                case EastRef.正Z方向:
                    horizontalFactor.z = Mathf.Cos((clockwiseDrift / 180.0f) * Mathf.PI);
                    horizontalFactor.y = 0;
                    horizontalFactor.x = Mathf.Sin((clockwiseDrift / 180.0f) * Mathf.PI);
                    break;
                default:
                    horizontalFactor.z = -Mathf.Cos((clockwiseDrift / 180.0f) * Mathf.PI);
                    horizontalFactor.y = 0;
                    horizontalFactor.x = -Mathf.Sin((clockwiseDrift / 180.0f) * Mathf.PI);
                    break;
            }
        }
        // 垂直方向
        RdotC = Vector3.Dot(Vector2.left, controllerDir);
        verticalFactor = controllerDir.y < 0 ? Mathf.Acos(RdotC) : 2 * Mathf.PI - Mathf.Acos(RdotC); //0到24 映射到 0到2PI之间，线性的映射
        verticalFactor = TimeMapping(verticalFactor, sunriseTime, sundownTime);
        lightPos.x = horizontalFactor.x * Mathf.Cos(verticalFactor);
        lightPos.y = Mathf.Sin(verticalFactor);
        lightPos.z = horizontalFactor.z * Mathf.Cos(verticalFactor);
        lightTransform.forward = -lightPos.normalized;
        // 灯光颜色
        currentLight.color = gradientTex.GetPixel((int)(512 * currentTime / 24), 1);
    }

    private float TimeMapping(float _time, float _rise, float _down)
    {
        _time = _time / Mathf.PI * 12 + 6;
        _time = _time > 24 ? _time - 24 : _time;
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

        mappingValue = (mappingValue - 6) * Mathf.PI / 12;
        mappingValue = mappingValue < 0 ? mappingValue + Mathf.PI * 2 : mappingValue;
        return mappingValue;
    }
    #endregion
}
#endif