using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

public class CreateVolumePure : OdinEditorWindow
{
    public static CreateVolumePure ToolWindow;
    private static GameObject instance;
    private static Material sliceMat;
    private static Material blurMat;
    private static int objectNum = 0;
    private static List<string> objectNames = new List<string>();

    private Camera renderCam;
    private RenderTexture renderTexture;
    private Texture3D volumeTex;
    
    [MenuItem("Tools/生成纯色3D贴图")]
    public static void ShowWindow()
    {
        ToolWindow = (CreateVolumePure)EditorWindow.GetWindow(typeof(CreateVolumePure), false, "生成纯色3D贴图");
        ToolWindow.minSize = new Vector2(360, 240);
        ToolWindow.Show();
        // 初始化一个线框
        string wireframePath = "Assets/Prefabs/Locating Point.prefab";
        GameObject boxPrefab = (GameObject)AssetDatabase.LoadAssetAtPath(wireframePath, typeof(GameObject));
        instance = (GameObject)Instantiate(boxPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        // 获取切分材质和模糊材质
        sliceMat = new Material(Shader.Find("Unlit/SliceShader"));
        blurMat = new Material(Shader.Find("Unlit/BlurVolume"));
    }

    // 扫描框位置
    #region 窗口
    [BoxGroup("扫描框信息")]
    [BoxGroup("扫描框信息/位置", false)]
    [LabelText("位置")]
    public Vector3 wireframePos = new Vector3(0, 0, 0);
    [BoxGroup("扫描框信息/大小", false)]
    [LabelText("大小")]
    public Vector3 wireframeSize = new Vector3(1, 1, 1);
    //---------------------
    [BoxGroup("3D贴图信息")]
    [BoxGroup("3D贴图信息/贴图大小", false)]
    [LabelText("贴图大小")]
    public Vector3Int volumeSize = new Vector3Int(64, 64, 64);
    [BoxGroup("3D贴图信息/模糊", false)]
    [LabelText("模糊")]
    [ToggleLeft]
    public bool isBlur = false;
    [BoxGroup("3D贴图信息/模糊", false)]
    [LabelText("模糊迭代")]
    [ShowIf("isBlur")]
    [Range(1, 5)]
    public int blurIteration;
    [BoxGroup("3D贴图信息/模糊", false)]
    [LabelText("模糊系数")]
    [ShowIf("isBlur")]
    [Range(0, 1)]
    public float blurRadius;
    //---------------------
    [BoxGroup("选择物体", false)]
    [LabelText("选择场景中的物体")]
    public GameObject[] targetObjects;
    //---------------------
    [BoxGroup("预览", false)]
    [LabelText("预览效果")]
    [Range(0, 1)]
    public float sliceValue = 0;
    //---------------------
    [BoxGroup("输出", false)]
    [BoxGroup("输出/文件名称",false)]
    [LabelText("文件名称")]
    public string volumeName = "volume";
    [BoxGroup("输出/路径",false)]
    [LabelText("保存路径")]
    [FolderPath(ParentFolder = "Assets", AbsolutePath = false)]
    public string savePath;

    [BoxGroup("输出")]
    [Button("处理", ButtonSizes.Medium), GUIColor(0.5f, 1.2f, 0.5f)]
    public void Excude()
    {
        CreateNewCamera();
        volumeTex = new Texture3D(volumeSize.x, volumeSize.y, volumeSize.z, TextureFormat.RGBA32, false);
        Render3DTex();
        AssetDatabase.Refresh();
    }
    #endregion

    #region 实时更新状态：数值变化、物体销毁
    // 当编辑器发生改变时
    private void OnValidate()
    {
        // 在编辑器中更新线框的信息
        if (instance != null)
        {
            instance.transform.position = wireframePos;
            instance.transform.localScale = wireframeSize;
        }
        else
            Debug.LogError("线框不存在，请重新打开编辑器");
        
        //给选择的物体添加sliceMat
        GiveObjectSliceMat();
        // 预览切片情况
        sliceMat.SetFloat("_SliceValue", wireframePos.y + wireframeSize.y * (0.5f - sliceValue));
        blurMat.SetFloat("_BlurRadius", blurRadius);
    }

    // 判断生成3DTex的物体是否挂载了MeshRender
    // 同时给挂载了MeshRender的物体统一挂载sliceMat
    private void GiveObjectSliceMat()
    {
        // 预制多次调用该函数TODO: 还是判断里面有文件变化，不然做替换会发生bug
        if (!ObjectsChanged(objectNum, objectNames)) return;
        objectNames.Clear();
        foreach (var targetObject in targetObjects)
        {
            if (targetObject.GetComponent<MeshRenderer>() != null)
                targetObject.GetComponent<MeshRenderer>().sharedMaterial = sliceMat;
            else
            {
                Debug.Log(string.Format("已为{0}物体的子物体赋予材质", targetObject.name));
                MeshRenderer[] meshRenderers = targetObject.GetComponentsInChildren<MeshRenderer>();
                foreach (var meshRenderer in meshRenderers)
                {
                    if (meshRenderer.sharedMaterial != sliceMat)
                        meshRenderer.sharedMaterial = sliceMat;
                }
            }
            objectNames.Add(targetObject.name);
        }
        objectNum = targetObjects.Length;
    }
    // 判断所选文件是否发生变化
    private bool ObjectsChanged(int _length, List<string> _names)
    {
        if (_length != targetObjects.Length) return true;
        for (int i = 0; i < _names.Count; i++)
        {
            if (_names[i] != targetObjects[i].name) return true;
        }
        return false;
    }
    // 当定位框的Inspector发生变化时
    private void OnInspectorUpdate()
    {
        if (instance != null)
        {
            wireframePos = instance.transform.position;
            wireframeSize = instance.transform.localScale;
        }
    }
    // 关闭窗口时，销毁定位锚点
    private void OnDestroy()
    {
        if (instance != null)
            DestroyImmediate(instance);
    }
    #endregion

    #region 生成3D Tex逻辑代码
    // 创建一个Camera
    private void CreateNewCamera()
    {
        if (renderCam == null)
        {
            // 创建一个相机，设置基础参数
            GameObject cam = new GameObject("Render Camera");
            cam.transform.localEulerAngles = new Vector3(90, 0, 90);
            renderCam = cam.AddComponent<Camera>();
            renderCam.clearFlags = CameraClearFlags.SolidColor;
            renderCam.backgroundColor = Color.black;
            renderCam.orthographic = true;
            renderCam.nearClipPlane = 0.0001f;
        }
        renderCam.transform.position = wireframePos + wireframeSize.y * 0.51f * Vector3.up;
        renderCam.orthographicSize = Mathf.Max(wireframeSize.x, wireframeSize.z) * 0.5f;
        renderCam.farClipPlane = wireframeSize.y * 1.2f;
        // 创建一个rt，并将相机的输出对象设置为rt
        renderTexture = CreateRT(volumeSize.x, volumeSize.z);
        renderCam.targetTexture = renderTexture;
    }
    // 创建一张RenderTexture
    private RenderTexture CreateRT(int _width, int _height)
    {
        RenderTexture rt = new RenderTexture(_width, _width, 16, RenderTextureFormat.ARGB32);
        rt.wrapMode = TextureWrapMode.Repeat;
        rt.filterMode = FilterMode.Point;
        return rt;
    }
    // 开始渲染3D Tex
    private void Render3DTex()
    {
        // 扫描起点、间隔以及扫描点位置
        float startY = wireframePos.y - wireframeSize.y * 0.5f;
        float intervalY = wireframeSize.y / volumeSize.y;
        float scanY = 0.0f;
        // 保存3D纹理Color
        Color[] tex3DColors = new Color[volumeSize.x * volumeSize.y * volumeSize.z];
        // 逐层级渲染
        for (int layer = 0; layer < volumeSize.y; layer++)
        {
            // 渲染层级对应的位置
            scanY = startY + ((float)layer + 0.5f) * intervalY;
            sliceMat.SetFloat("_SliceValue", scanY);
            // 进度条
            // float progress = (float)layer / texture3DSize.y; 
            // bool isCancel = EditorUtility.DisplayCancelableProgressBar("正在执行..", string.Format("生成3D Texture中...{0:f2}%", progress * 100), progress);
            // 启动渲染，并且输出为Texture2D
            renderCam.Render();
            Texture2D tex2D = RendertTex2Tex2D(renderTexture);
            // SaveToPng(tex2D, layer);
            for (int x = 0; x < volumeSize.x; x++)
            {
                for (int z = 0; z < volumeSize.z; z++)
                {
                    int index = x + layer * volumeSize.x + z * volumeSize.x * volumeSize.z;
                    tex3DColors[index] = tex2D.GetPixel(z, x);
                }
            }
        }
        // 把tex3DColors应用到3D Tex
        volumeTex.SetPixels(tex3DColors);
        volumeTex.Apply();
        if (isBlur)
        {
            volumeTex = BlurVolume(volumeTex);
        }
        string saveInfo = string.Format("Assets/{0}/{1}_{2}.asset", savePath, volumeName, isBlur ? "_blur" : "");
        try
        {
            AssetDatabase.DeleteAsset(saveInfo);
            AssetDatabase.CreateAsset(volumeTex, saveInfo);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }
    // 将RenderTexture转变成Texture2D
    private Texture2D RendertTex2Tex2D(RenderTexture _rt)
    {
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = _rt;
        Texture2D tempTex2D = new Texture2D(_rt.width, _rt.height, TextureFormat.ARGB32, false);
        tempTex2D.ReadPixels(new Rect(0, 0, _rt.width, _rt.height), 0, 0);
        tempTex2D.Apply();
        RenderTexture.active = prev;
        return tempTex2D;
    }
    // 模糊3D Tex
    private Texture3D BlurVolume(Texture3D _volume)
    {
        // 保存3D纹理Color
        Color[] tex3DColors = new Color[volumeSize.x * volumeSize.y * volumeSize.z];
        for (int iteration = 1; iteration <= blurIteration; iteration++)
        {
            // 逐层级渲染
            for (int layer = 0; layer < volumeSize.y; layer++)
            {
                // 逐层(模糊)渲染
                blurMat.SetTexture("_VolumeTex", _volume);
                blurMat.SetFloat("_SliceValue", (float)layer / volumeSize.y);
                RenderTexture blurRT = new RenderTexture(volumeSize.x, volumeSize.z, 16, RenderTextureFormat.ARGB32);
                Graphics.Blit(null, blurRT, blurMat);
                Texture2D tex2D = RendertTex2Tex2D(blurRT);
                // SaveToPng(tex2D, layer);
                // 保存Color信息
                for (int x = 0; x < volumeSize.x; x++)
                {
                    for (int z = 0; z < volumeSize.z; z++)
                    {
                        int index = x + layer * volumeSize.x + z * volumeSize.x * volumeSize.z;
                        tex3DColors[index] = tex2D.GetPixel(x, z);
                    }
                }
            }
            _volume.SetPixels(tex3DColors);
            _volume.Apply();
        }
        return _volume;
    }
    #endregion
    
    // 选项将Tex2D输出为png
    private void SaveToPng(Texture2D _tex2D, int _layer)
    {
        byte[] texBytes = _tex2D.EncodeToPNG();
        if (string.IsNullOrEmpty(volumeName)) return;
        FileStream texFile = File.Open(string.Format("C:/Users/DELL/Desktop/new/{0}_{1}.png", volumeName, _layer), FileMode.Create);
        BinaryWriter writeTex = new BinaryWriter(texFile);
        writeTex.Write(texBytes);
        texFile.Close();
    }
}
