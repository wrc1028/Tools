using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
public class CombineTex : OdinEditorWindow
{
    #region 属性
    public enum TexType
    {
        PNG, JPG, TGA,
    }
    public enum FileNamingRule
    {
        自定义, 首部输入对象, 尾部输入对象,
    }
    public enum MixedModel
    {
        合并保留通道, 正常, 正片叠底,
    }
    public static EditorWindow ToolWindow;  // 创建窗口对象
    private static Shader combineShader;    // 混合Shader
    private static string tempPath = "";    // 临时路径
    private List<Texture2D> inputTexs = new List<Texture2D>();      // 输入图片列表
    private List<Vector4> inputTexChannels = new List<Vector4>();   // 对应输入图片保留的通道(1为保留，0为舍弃)
    private List<int> inputMixedModels = new List<int>();            // 记录图片当前想要的混合模式，传入Shader中进行处理
    #endregion
    //---------------------------------------------------------------------------------------------------------
    #region 窗口
    [MenuItem("Odin/贴图拆分与合并3.0")]
    public static void ShowWindow()
    {
        ToolWindow = (CombineTex)EditorWindow.GetWindow(typeof(CombineTex), false, "贴图拆分与合并");
        ToolWindow.minSize = new Vector2(360, 240);
        ToolWindow.Show();
        combineShader = Shader.Find("Unlit/CombineTex");
    }

    // 输入----------------------------------------------------------------------------
    [LabelText("文件路径输入")]
    public InputFile[] inputFiles = new InputFile[1];
    public struct InputFile
    {
        [BoxGroup("选择路径", false)]
        [LabelText("路径")]
        [HorizontalGroup("选择路径/Path", LabelWidth = 30)]
        [Sirenix.OdinInspector.FilePath(ParentFolder = "Assets", AbsolutePath = true)]
        public string texPath;

        [HorizontalGroup("选择路径/Button", marginLeft : 32)]
        [LabelText("复制")]
        [Button]
        public void CopyPath()
        {
            tempPath = texPath;
        }

        [HorizontalGroup("选择路径/Button")]
        [LabelText("粘贴")]
        [Button]
        public void PastePath()
        {
            texPath = tempPath;
        }

        [HorizontalGroup("选择路径/Button", marginRight : 18)]
        [LabelText("清除")]
        [Button]
        public void ClearPath()
        {
            texPath = "";
        }
        
        [HorizontalGroup("模式", LabelWidth = 150)]
        [BoxGroup("模式/混合模式")]
        [LabelText("")]
        public MixedModel mixedModel;

        [BoxGroup("模式/通道选择")]
        [HorizontalGroup("模式/通道选择/Channel", LabelWidth = 40)]
        [LabelText("R通道")]
        [ToggleLeft]
        public bool rChannel;
        [HorizontalGroup("模式/通道选择/Channel", LabelWidth = 40)]
        [LabelText("G通道")]
        [ToggleLeft]
        public bool gChannel;
        [HorizontalGroup("模式/通道选择/Channel", LabelWidth = 40)]
        [LabelText("B通道")]
        [ToggleLeft]
        public bool bChannel;
        [HorizontalGroup("模式/通道选择/Channel", LabelWidth = 40)]
        [LabelText("A通道")]
        [ToggleLeft]
        public bool aChannel;
    }

    // 输出----------------------------------------------------------------------------
    [BoxGroup("输出")]
    [HorizontalGroup("输出/SavePath", LabelWidth = 50)]
    [LabelText("输出路径")]
    [FolderPath(ParentFolder = "Assets", AbsolutePath = true)]
    public string savePath;

    [HorizontalGroup("输出/Type", LabelWidth = 50)]
    [LabelText("命名参考")]
    public FileNamingRule fileNamingRule = FileNamingRule.自定义;

    [HorizontalGroup("输出/Type", LabelWidth = 50)]
    [LabelText("输出格式")]
    public TexType texType = TexType.PNG;

    [ShowIf("fileNamingRule", FileNamingRule.自定义)]
    [HorizontalGroup("输出/name", LabelWidth = 50)]
    [LabelText("文件名称")]
    [FolderPath(ParentFolder = "Assets", AbsolutePath = true)]
    public string inputFileName;
    
    [HorizontalGroup("输出/Type", LabelWidth = 50)]
    [LabelText("是否进行伽马矫正")]
    [ToggleLeft]
    public bool isGamma = false;

    [Button(buttonSize : 25), GUIColor(0, 1.0f, 0.4f)]
    [LabelText("处理")]
    public void Excute()
    {
        inputTexs = GetTexture2Ds(inputFiles);
        inputTexChannels = TexRetentionChannel(inputFiles);
        inputMixedModels = GetMixedModel(inputFiles);
        //TODO: 命名参考:建议改成第几个参考路径
        //TODO: 判断合法性
        CombineChannel(inputTexs, inputTexChannels, inputMixedModels, texType, savePath, inputFileName, isGamma ? 1.0f / 2.2f : 1.0f);
    }
    #endregion

    #region  逻辑
    //TODO: 图片信息
    private List<Texture2D> GetTexture2Ds(InputFile[] _inputFiles)
    {
        List<Texture2D> tempTexs = new List<Texture2D>();
        for (int i = 0; i < _inputFiles.Length; i++)
        {
            tempTexs.Add(GetTexture2DFromPath(_inputFiles[i].texPath));
        }
        return tempTexs;
    }
    //TODO: 图片通道保留信息
    private List<Vector4> TexRetentionChannel(InputFile[] _inputFiles)
    {
        List<Vector4> tempVectors = new List<Vector4>();
        for (int i = 0; i < _inputFiles.Length; i++)
        {
            tempVectors.Add(new Vector4(
                _inputFiles[i].rChannel ? 1 : 0, 
                _inputFiles[i].gChannel ? 1 : 0, 
                _inputFiles[i].bChannel ? 1 : 0, 
                _inputFiles[i].aChannel ? 1 : 0
            ));
        }
        tempVectors[0] = new Vector4(tempVectors[0].x, tempVectors[0].y, tempVectors[0].z, 1);
        return tempVectors;
    }
    private List<int> GetMixedModel(InputFile[] _inputFiles)
    {
        List<int> tempModel = new List<int>();
        for (int i = 0; i < _inputFiles.Length; i++)
        {
            tempModel.Add((int)_inputFiles[i].mixedModel);
        }
        return tempModel;
    }
    //TODO: 获得Texture2D的，如果给的路径为空则返回一个空的Texture2D
    public Texture2D GetTexture2DFromPath(string _texPath)
    {
        if (string.IsNullOrEmpty(_texPath))
        {
            return new Texture2D(0, 0);
        }
        FileStream texFile = new FileStream(_texPath, FileMode.Open, FileAccess.Read);
        byte[] texBytes = new byte[texFile.Length];
        texFile.Read(texBytes, 0, texBytes.Length);
        texFile.Close();
        Texture2D tempTex = new Texture2D(1000, 1000);
        tempTex.LoadImage(texBytes);
        return tempTex;
    }
    //TODO: 使用Graphics合并通道
    public void CombineChannel(List<Texture2D> _texs, List<Vector4> _vecs, List<int> _mixedModels, TexType _texType, string _savePath, string _fileName, float _gammaValue)
    {
        RenderTexture outputRT = RenderTexture.GetTemporary(_texs[0].width, _texs[0].height, 0, RenderTextureFormat.ARGB32);
        RenderTexture inputRT = RenderTexture.GetTemporary(_texs[0].width, _texs[0].height, 0, RenderTextureFormat.ARGB32);
        Material mat = new Material(combineShader);
        for (int i = 0; i < _texs.Count; i++)
        {
            mat.SetTexture("_AddTex", _texs[i]);
            mat.SetVector("_ChannelMask", _vecs[i]);
            mat.SetFloat("_GammaValue", _gammaValue);
            Graphics.Blit(inputRT, outputRT, mat, _mixedModels[i]);
            inputRT = outputRT;
            outputRT = RenderTexture.GetTemporary(_texs[0].width, _texs[0].height, 0, RenderTextureFormat.ARGB32);
        }
        SaveToImage(inputRT, _texType, _savePath, _fileName);
        RenderTexture.ReleaseTemporary(outputRT);
        RenderTexture.ReleaseTemporary(inputRT);
    }
    //TODO: 保存图片
    public void SaveToImage(RenderTexture _rt, TexType _texType, string _savePath, string _fileName)
    {
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = _rt;
        Texture2D tex2D = new Texture2D(_rt.width, _rt.height, TextureFormat.ARGB32, false);
        tex2D.ReadPixels(new Rect(0, 0, _rt.width, _rt.height), 0, 0);
        byte[] texBytes;
        string fileExtensions = "";
        switch (_texType)
        {
            case TexType.PNG:
                texBytes = tex2D.EncodeToPNG();
                fileExtensions = "PNG";
                break;
            case TexType.JPG:
                texBytes = tex2D.EncodeToJPG();
                fileExtensions = "JPG";
                break;
            case TexType.TGA:
                texBytes = tex2D.EncodeToTGA();
                fileExtensions = "TGA";
                break;
            default:
                texBytes = tex2D.EncodeToPNG();
                fileExtensions = "PNG";
                break;
        }
        FileStream texFile = File.Open(_savePath + "/" + _fileName + "." + fileExtensions, FileMode.Create);
        BinaryWriter texWrite = new BinaryWriter(texFile);
        texWrite.Write(texBytes);
        texFile.Close();
        Texture2D.DestroyImmediate(tex2D);
        tex2D = null;
        RenderTexture.active = prev;
    }
    #endregion
}
