#if UNITY_EDITOR
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

public class AlphaToDither : OdinEditorWindow
{
    public enum SaveType
    {
        覆盖同名文件, 增加_dither后缀,
    }

    private string alphaToDitherCSPath;
    private ComputeShader alphaToDitherCS;
    private Material downAlphaChannelSampleMat;
    private string convolutionPattem;
    private string texturePattem;
    private float[] ditherConvolution;
    private bool isInit = false;
    private List<Texture> selectedTextures;
    private List<string> selectedTexturesExtension;
    private List<string> selectedTexturesPath;

    public static EditorWindow ToolWindow;
    [MenuItem("Odin/透明度转Dither")]
    public static void ShowWindow()
    {
        ToolWindow = (AlphaToDither)EditorWindow.GetWindow(typeof(AlphaToDither), false, "透明度转Dither");
        ToolWindow.minSize = new Vector2(360, 240);
        ToolWindow.Show();
    }
    #region 属性
    [BoxGroup("组件1", false)]
    [BoxGroup("组件1/简介", false)]
    [HideLabel]
    [DisplayAsString(false)]
    public string infoSelect = "默认卷积核大小为4x4; 默认保存路径为当前选择贴图的路径, 保存方式为覆盖";
    [HorizontalGroup("组件1/选项")]
    [ToggleLeft]
    [LabelText("使用自定义卷积核")]
    public bool isUsedCustomConvolution;
    [HorizontalGroup("组件1/选项")]
    [ToggleLeft]
    [LabelText("修改保存选项")]
    public bool isUsedCustomOutput;

    [BoxGroup("自定义卷积", false)]
    [ShowIf("isUsedCustomConvolution")]
    [InfoBox("请输入自定义的卷积核，卷积核数量为正整数的平方，用竖线隔开")]
    [LabelText("自定义卷积核")]
    public string customConvolution;

    [BoxGroup("自定义输出保存选项", false)]
    [ShowIfGroup("自定义输出保存选项/isUsedCustomOutput")]
    [LabelText("保存方式")]
    public SaveType saveType = SaveType.覆盖同名文件;
    
    [ShowIfGroup("自定义输出保存选项/isUsedCustomOutput")]
    [FolderPath(ParentFolder = "Assets", AbsolutePath = true)]
    [LabelText("保存路径")]
    public string savePath;

    [Button(buttonSize : 25), GUIColor(0, 1.1f, 0.4f)]
    [LabelText("转换")]
    public void Excute()
    {
        InitConstProp();
        InitVariableProp();
        LoadSelectedTextures();
        AssetDatabase.Refresh();
    }
    [BoxGroup("层级范围", false)]
    [HorizontalGroup("层级范围/说明")]
    [HideLabel]
    [DisplayAsString(false)]
    public string infoLayer = "设置Alpha层级范围";
    [HorizontalGroup("层级范围/等级")]
    [HideLabel]
    [DisplayAsString(false)]
    public string layer01 = "1.0";
    [HorizontalGroup("层级范围/等级")]
    [HideLabel]
    public float layer02 = 0.8f;
    [HorizontalGroup("层级范围/等级")]
    [HideLabel]
    public float layer03 = 0.6f;
    [HorizontalGroup("层级范围/等级")]
    [HideLabel]
    public float layer04 = 0.4f;
    [HorizontalGroup("层级范围/等级")]
    [HideLabel]
    public float layer05 = 0.2f;
    [HorizontalGroup("层级范围/等级")]
    [HideLabel]
    [DisplayAsString(false)]
    public string layer06 = "0.0";
    #endregion

    #region 方法
    // 初始化常量
    private void InitConstProp()
    {
        if (isInit) return;
        alphaToDitherCSPath = "Assets/Scripts/Dither/AlphaToDither.compute";
        alphaToDitherCS = (ComputeShader)AssetDatabase.LoadAssetAtPath<ComputeShader>(alphaToDitherCSPath);
        convolutionPattem = @"\b[^0-9]+\b";
        texturePattem = @".(png|PNG|tga|TGA|jpg|JPG)";
        isInit = true;
    }
    // 初始化变量
    private void InitVariableProp()
    {
        // 使用自定义卷积核
        if (!isUsedCustomConvolution)
            ditherConvolution = new float[16] {
                15, 07, 13, 05, 
                03, 11, 01, 09, 
                12, 04, 14, 06, 
                00, 08, 02, 10};
        else
        {
            string[] matchs = Regex.Split(customConvolution, convolutionPattem);
            if (Mathf.Sqrt(matchs.Length) % 1 > 0) 
            {
                Debug.LogWarning(string.Format("输入的卷积核数量不满足需求(数量为{0}位)，已使用默认卷积核参与计算", matchs.Length));
                return;
            }
            ditherConvolution = new float[matchs.Length];
            for (int i = 0; i < matchs.Length; i++)
            {
                ditherConvolution[i] = float.Parse(matchs[i]);
            }
        }
    }
    // 加载选中的图片
    private void LoadSelectedTextures()
    {
        string[] selectedGUIDs = Selection.assetGUIDs;
        foreach (var guid in selectedGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string directory = Path.GetDirectoryName(path);
            string name = Path.GetFileNameWithoutExtension(path);
            string extension = Path.GetExtension(path);
            if (Regex.IsMatch(extension, texturePattem))
            {
                if (isUsedCustomOutput) directory = savePath;
                Texture rgbTex = AssetDatabase.LoadAssetAtPath<Texture>(path);
                SaveTexture2DTofile(Alpha2Dither(rgbTex), directory + "/" + name, extension);
            }
        }
    }
    // 处理选中的图片
    private Texture2D Alpha2Dither(Texture texture)
    {
        alphaToDitherCS.SetTexture(0, "_InputTex", texture);
        alphaToDitherCS.SetInt("_SqrtLength", (int)Mathf.Sqrt(ditherConvolution.Length));

        alphaToDitherCS.SetFloats("_LayerRange", layer02, layer03, layer04, layer05);

        ComputeBuffer ditherBuffer = new ComputeBuffer(ditherConvolution.Length, sizeof(float));
        ditherBuffer.SetData(ditherConvolution);
        alphaToDitherCS.SetBuffer(0, "_DitherConvolution", ditherBuffer);

        Color[] destColors = new Color[texture.width * texture.height];
        ComputeBuffer colorsBuffer = new ComputeBuffer(destColors.Length, sizeof(float) * 4);
        colorsBuffer.SetData(destColors);
        alphaToDitherCS.SetBuffer(0, "Result", colorsBuffer);

        alphaToDitherCS.Dispatch(0, Mathf.CeilToInt((float)texture.width / 8), Mathf.CeilToInt((float)texture.height / 8), 1);

        colorsBuffer.GetData(destColors);
        Texture2D destTex = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);
        destTex.filterMode = FilterMode.Point;
        destTex.SetPixels(destColors);
        destTex.Apply();
        colorsBuffer.Dispose();
        colorsBuffer.Release();
        ditherBuffer.Release();

        return destTex;
    }

    private void SaveTexture2DTofile(Texture2D tex, string path, string extension)
    {
        if (saveType == SaveType.增加_dither后缀) path = path + "_dither";
        byte[] texBytes;
        switch(extension)
        {
            case ".PNG": case ".png":
                texBytes = tex.EncodeToPNG();
                break;
            case ".TGA": case ".tga":
                texBytes = tex.EncodeToTGA();
                break;
            case ".JPG": case ".jpg":
                texBytes = tex.EncodeToJPG();
                break;
            default:
                texBytes = tex.EncodeToEXR();
                break;
        }
        FileStream texFile = File.Open(path + extension, FileMode.OpenOrCreate);
        BinaryWriter texWrite = new BinaryWriter(texFile);
        texWrite.Write(texBytes);
        texFile.Close();
        Texture2D.DestroyImmediate(tex);
        tex = null;
    }
    #endregion
}
#endif