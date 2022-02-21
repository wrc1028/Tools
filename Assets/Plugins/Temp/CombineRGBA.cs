using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

public class CombineRGBA : OdinEditorWindow
{
    #region 属性
    public enum TexType
    {
        PNG, JPG, TGA,
    }
    public enum FileORFolder
    {
        文件, 文件夹,
    }
    public enum FileNamingRule
    {
        自定义, R通道, G通道, B通道, A通道,
    }
    public enum ChannelPermutation
    {
        OneTexInput, TowTexInput, FourTexInput,
    }
    public static EditorWindow ToolWindow;
    private static string tempPath = "";    // 用于保存复制的通道
    private static Shader combineShader;    // 合并用的Shader
    private float gammaValue = 1.0f;        // 伽马值
    private List<string> fileChannelPaths = new List<string>() {"", "", "", ""};    // 合并单张图片四个通道对应的图片地址
    private Texture2D[] fileChannelTexs = new Texture2D[4];                         // 合并单张图片四个通道对应的图片
    private List<string> rFileChannelPaths = new List<string>();                    // 合并多张图片R通道对应的图片地址
    private List<string> gFileChannelPaths = new List<string>();                    // 合并多张图片G通道对应的图片地址
    private List<string> bFileChannelPaths = new List<string>();                    // 合并多张图片B通道对应的图片地址
    private List<string> aFileChannelPaths = new List<string>();                    // 合并多张图片A通道对应的图片地址
    
    
    #endregion
    
    #region 窗口
    [MenuItem("Odin/贴图拆分与合并2.0")]
    public static void ShowWindow()
    {
        ToolWindow = (CombineRGBA)EditorWindow.GetWindow(typeof(CombineRGBA), false, "贴图拆分与合并");
        ToolWindow.minSize = new Vector2(360, 240);
        ToolWindow.Show();
        combineShader = Shader.Find("Unlit/CombineRGBA");
    }
    // 输入--------------------------------------------------------------------------------------
    [EnumToggleButtons, HideLabel]
    public FileORFolder FileOrFolder;
    // 文件输入
    [ShowIf("FileOrFolder", FileORFolder.文件)]
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
        public void ClearcPath()
        {
            texPath = "";
        }
        
        [BoxGroup("选择保留的通道")]
        [ToggleLeft]
        [LabelText("R通道")]
        [HorizontalGroup("选择保留的通道/Channel", LabelWidth = 50)]
        public bool rChannel;
        [ToggleLeft]
        [LabelText("G通道")]
        [HorizontalGroup("选择保留的通道/Channel", LabelWidth = 50)]
        public bool gChannel;
        [ToggleLeft]
        [LabelText("B通道")]
        [HorizontalGroup("选择保留的通道/Channel", LabelWidth = 50)]
        public bool bChannel;
        [ToggleLeft]
        [LabelText("A通道")]
        [HorizontalGroup("选择保留的通道/Channel", LabelWidth = 50)]
        public bool aChannel;
    }
    // 文件夹输入
    [ShowIf("FileOrFolder", FileORFolder.文件夹)]
    [LabelText("文件夹路径输入")]
    public InputFolder[] inputFolders = new InputFolder[1];
    public struct InputFolder
    {
        [BoxGroup("选择路径", false)]
        [LabelText("路径")]
        [HorizontalGroup("选择路径/Path", LabelWidth = 30)]
        [FolderPath(ParentFolder = "Assets", AbsolutePath = true)]
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
        public void ClearcPath()
        {
            texPath = "";
        }
        [BoxGroup("选择保留的通道")]
        [ToggleLeft]
        [LabelText("R通道")]
        [HorizontalGroup("选择保留的通道/Channel", LabelWidth = 50)]
        public bool rChannel;
        [ToggleLeft]
        [LabelText("G通道")]
        [HorizontalGroup("选择保留的通道/Channel", LabelWidth = 50)]
        public bool gChannel;
        [ToggleLeft]
        [LabelText("B通道")]
        [HorizontalGroup("选择保留的通道/Channel", LabelWidth = 50)]
        public bool bChannel;
        [ToggleLeft]
        [LabelText("A通道")]
        [HorizontalGroup("选择保留的通道/Channel", LabelWidth = 50)]
        public bool aChannel;
    }
    // 输出--------------------------------------------------------------------------------------
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
        if (FileOrFolder == FileORFolder.文件)
        {
            ChannelSelect(inputFiles);
            string fileName = FileNaming(fileNamingRule, fileChannelPaths);
            if (!FileIsLegal(inputFiles, fileChannelPaths, savePath, fileName)) return;
            for (int i = 0; i < fileChannelPaths.Count; i++)
            {
                fileChannelTexs[i] = GetTexture(fileChannelPaths[i]);
                fileChannelPaths[i] = "";
            }
            CombineChannel(fileChannelTexs, texType, savePath, fileName, gammaValue = isGamma ? 1.0f / 2.2f : 1.0f);
        }
        else
        {
            ChannelSelect(inputFolders);
            int minimum = FindMinimumInPaths();
            string[] fileNames = FilesNaming(fileNamingRule, minimum);
            if (!FilesIsLegal(inputFolders, savePath, fileNames)) return;
            Debug.Log(fileNames.Length);
            for (int i = 0; i < minimum; i++)
            {
                //存在文件路径为零的情况
                if (rFileChannelPaths.Count != 0) fileChannelTexs[0] = GetTexture(rFileChannelPaths[i]);
                if (gFileChannelPaths.Count != 0) fileChannelTexs[1] = GetTexture(gFileChannelPaths[i]);
                if (bFileChannelPaths.Count != 0) fileChannelTexs[2] = GetTexture(bFileChannelPaths[i]);
                if (aFileChannelPaths.Count != 0) fileChannelTexs[3] = GetTexture(aFileChannelPaths[i]);
                CombineChannel(fileChannelTexs, texType, savePath, fileNames[i], gammaValue = isGamma ? 1.0f / 2.2f : 1.0f);
            }
            rFileChannelPaths.Clear();
            gFileChannelPaths.Clear();
            bFileChannelPaths.Clear();
            aFileChannelPaths.Clear();
        }
    }

    #endregion

    #region 逻辑
    //TODO: 排列判断来获取图片
    
    //TODO: 通道选择，依次从上到下选择已选的通道，如果重复以最先选择的为主，还需要改进
    public void ChannelSelect(InputFile[] _files)
    {
        for (int i = 0; i < _files.Length; i++)
        {
            if (string.IsNullOrEmpty(fileChannelPaths[0]) && _files[i].rChannel)
            {
                fileChannelPaths[0] = _files[i].texPath;
            }
            if (string.IsNullOrEmpty(fileChannelPaths[1]) && _files[i].gChannel)
            {
                fileChannelPaths[1] = _files[i].texPath;
            }
            if (string.IsNullOrEmpty(fileChannelPaths[2]) && _files[i].bChannel)
            {
                fileChannelPaths[2] = _files[i].texPath;
            }
            if (string.IsNullOrEmpty(fileChannelPaths[3]) && _files[i].aChannel)
            {
                fileChannelPaths[3] = _files[i].texPath;
            }
        }
    }
    public void ChannelSelect(InputFolder[] _folders)
    {
        for (int i = 0; i < _folders.Length; i++)
        {
            if (rFileChannelPaths.Count == 0 && _folders[i].rChannel)
            {
                rFileChannelPaths = new List<string>(Directory.GetFiles(_folders[i].texPath));
            }
            if (gFileChannelPaths.Count == 0 && _folders[i].gChannel)
            {
                gFileChannelPaths = new List<string>(Directory.GetFiles(_folders[i].texPath));
            }
            if (bFileChannelPaths.Count == 0 && _folders[i].bChannel)
            {
                bFileChannelPaths = new List<string>(Directory.GetFiles(_folders[i].texPath));
            }
            if (aFileChannelPaths.Count == 0 && _folders[i].aChannel)
            {
                aFileChannelPaths = new List<string>(Directory.GetFiles(_folders[i].texPath));
            }
        }
    }
    //TODO: 文件命名参考
    private string FileNaming(FileNamingRule _fileNamingRule, List<string> _channelPaths)
    {
        switch (_fileNamingRule)
        {
            case FileNamingRule.R通道:
                return Path.GetFileNameWithoutExtension(_channelPaths[0]);
            case FileNamingRule.G通道:
                return Path.GetFileNameWithoutExtension(_channelPaths[1]);
            case FileNamingRule.B通道:
                return Path.GetFileNameWithoutExtension(_channelPaths[2]);
            case FileNamingRule.A通道:
                return Path.GetFileNameWithoutExtension(_channelPaths[3]);
            default:
                return inputFileName;
        }
    }
    private string[] FilesNaming(FileNamingRule _fileNamingRule, int _fileNum)
    {
        string[] tempFilesName = new string[_fileNum];
        for (int i = 0; i < _fileNum; i++)
        {
            switch (_fileNamingRule)
            {
                case FileNamingRule.R通道:
                    if(rFileChannelPaths.Count != 0) tempFilesName[i] = Path.GetFileNameWithoutExtension(rFileChannelPaths[i]);
                    break;
                case FileNamingRule.G通道:
                    if(gFileChannelPaths.Count != 0) tempFilesName[i] = Path.GetFileNameWithoutExtension(gFileChannelPaths[i]);
                    break;
                case FileNamingRule.B通道:
                    if(bFileChannelPaths.Count != 0) tempFilesName[i] = Path.GetFileNameWithoutExtension(bFileChannelPaths[i]);
                    break;
                case FileNamingRule.A通道:
                    if(aFileChannelPaths.Count != 0) tempFilesName[i] = Path.GetFileNameWithoutExtension(aFileChannelPaths[i]);
                    break;
                default:
                    tempFilesName[i] = inputFileName + "_" + i;
                    break;
            }
        }
        return tempFilesName;
    }
    //TODO: 判断文件合法性
    private bool FileIsLegal(InputFile[] _files, List<string> _channelPaths, string _savePath, string _fileName)
    {
        if (_files.Length == 0)
        {
            Debug.LogError("请至少添加一个输入文件!");
            return false;
        }
        else if (string.IsNullOrEmpty(_files[0].texPath))
        {
            Debug.LogError("输入文件路径为空!");
            return false;
        }
        else if (string.IsNullOrEmpty(_channelPaths[0]) && string.IsNullOrEmpty(_channelPaths[1]) &&
                 string.IsNullOrEmpty(_channelPaths[2]) && string.IsNullOrEmpty(_channelPaths[3]))
        {
            Debug.LogError("请至少选择一个输出通道!");
            return false;
        }
        else if (string.IsNullOrEmpty(_savePath))
        {
            Debug.LogError("保存路径不能为空!");
            return false;
        }
        else if (string.IsNullOrEmpty(_fileName))
        {
            Debug.LogError("文件名不能为空!");
            return false;
        }
        else return true;
    }
    private bool FilesIsLegal(InputFolder[] _folders, string _savePath, string[] _filesName)
    {
        int nullNamingFiles = 0;
        for (int i = 0; i < _filesName.Length; i++)
        {
            if (string.IsNullOrEmpty(_filesName[i])) nullNamingFiles++;
        }
        if (_folders.Length == 0)
        {
            Debug.LogError("请至少添加一个输入文件!");
            return false;
        }
        else if (string.IsNullOrEmpty(_folders[0].texPath))
        {
            Debug.LogError("输入文件夹路径为空!");
            return false;
        }
        else if (string.IsNullOrEmpty(_savePath))
        {
            Debug.LogError("保存路径不能为空!");
            return false;
        }
        else if (nullNamingFiles == _filesName.Length)
        {
            Debug.LogError("参考命名为空!");
            return false;
        }
        else return true;
    }
    //TODO: 找到所选文件中最少文件数返回
    private int FindMinimumInPaths()
    {
        int tempCounts = 0;
        // 选出第一个不为零的
        if (rFileChannelPaths.Count != 0) tempCounts = rFileChannelPaths.Count;
        else if (gFileChannelPaths.Count != 0) tempCounts = gFileChannelPaths.Count;
        else if (bFileChannelPaths.Count != 0) tempCounts = bFileChannelPaths.Count;
        else if (aFileChannelPaths.Count != 0) return aFileChannelPaths.Count;
        else Debug.LogError("请至少选择一个输出通道!");
        // 对比选出最小不为零的数
        if (tempCounts > gFileChannelPaths.Count && gFileChannelPaths.Count != 0) tempCounts = gFileChannelPaths.Count;
        if (tempCounts > bFileChannelPaths.Count && bFileChannelPaths.Count != 0) tempCounts = bFileChannelPaths.Count;
        if (tempCounts > aFileChannelPaths.Count && aFileChannelPaths.Count != 0) tempCounts = aFileChannelPaths.Count;
        
        return tempCounts;
    }

    //TODO: 获得Texture2D的，如果给的路径为空则返回一个空的Texture2D
    public Texture2D GetTexture(string _texPath)
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
    public void CombineChannel(Texture2D[] _channelTexs, TexType _texType, string _savePath, string _fileName, float _gammaValue)
    {
        RenderTexture rt = RenderTexture.GetTemporary(_channelTexs[0].width, _channelTexs[0].height, 0, RenderTextureFormat.ARGB32);
        Material mat = new Material(combineShader);
        mat.SetTexture("_RTex", _channelTexs[0]);
        mat.SetTexture("_GTex", _channelTexs[1]);
        mat.SetTexture("_BTex", _channelTexs[2]);
        mat.SetTexture("_ATex", _channelTexs[3]);
        mat.SetFloat("_GammaValue", _gammaValue);
        Graphics.Blit(new Texture2D(0, 0), rt, mat);
        SaveToImage(rt, _texType, _savePath, _fileName);
        RenderTexture.ReleaseTemporary(rt);
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
