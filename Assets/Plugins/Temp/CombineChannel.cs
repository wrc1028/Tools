using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

public class CombineChannel : OdinEditorWindow
{
    #region 属性
    public enum TexType
    {
        PNG, JPG, TGA,
    }
    public enum CombineSplitExchange
    {
        合并, 拆分, 交换通道,
    }
    public enum FileORFolder
    {
        文件, 文件夹,
    }
    public enum CombineFileNamingRule
    {
        自定义, R通道, G通道, B通道, A通道,
    }
    public enum SplitFileNamingRule
    {
        自定义, 文件名, 文件名去第一个后缀, 
    }
    
    public enum FiltrateFile
    {
        ALL, PNG, JPG, TGA,
    }
    public enum SplitChannelNaming
    {
        自定义, _Metallic, _Smoothness, _AO,
    }
    public enum ExchangeChannelInfo
    {
        R通道数据, G通道数据, B通道数据, A通道数据,
    }
    public static EditorWindow ToolWindow;  // 创建窗口对象
    private static Shader combineShader;    // 混合Shader
    // 单文件合并属性
    private List<Texture2D> inputCombineTexs = new List<Texture2D>();      // 输入图片列表
    
    private List<Vector4> inputCombineTexChannels = new List<Vector4>();   // 对应输入图片保留的通道(1为保留，0为舍弃)
    // 多文件合并属性
    private Dictionary<int, List<Texture2D>> inputCombineTexsDict = new Dictionary<int, List<Texture2D>>(); // 输入的多图片列表
    
    private Dictionary<int, List<string>> inputCombineFileNamesDict = new Dictionary<int, List<string>>();  // 保存筛选过后的图片名称

    private List<Vector4> inputCombineTexsChannels = new List<Vector4>();   // 对应输入图片保留的通道(1为保留，0为舍弃)

    private Vector4 checkUnnecessaryInput = new Vector4(0, 0, 0, 0);// 记录通道的重复数，检查当前输入选择的通道是否和记录的重复

    // 单文件拆分属性
    private List<Texture2D> inputSplitTexs = new List<Texture2D>();
    // 多文件拆分属性
    private Dictionary<int, List<Texture2D>> inputSplitTexsDict = new Dictionary<int, List<Texture2D>>();
    private Dictionary<int, List<string>> inputSplitFileNamesDict = new Dictionary<int, List<string>>();

    // 单文件交换通道
    private List<Texture2D> inputExchangeTexs = new List<Texture2D>();
    // 多文件交换通道
    private Dictionary<int, List<Texture2D>> inputExchangeTexsDict = new Dictionary<int, List<Texture2D>>();
    private Dictionary<int, List<string>> inputExchangeFileNamesDict = new Dictionary<int, List<string>>();

    #endregion
    //---------------------------------------------------------------------------------------------------------
    #region 窗口
    [MenuItem("Odin/贴图拆分与合并2.5")]
    public static void ShowWindow()
    {
        ToolWindow = (CombineChannel)EditorWindow.GetWindow(typeof(CombineChannel), false, "贴图拆分与合并");
        ToolWindow.minSize = new Vector2(360, 240);
        ToolWindow.Show();
        combineShader = Shader.Find("Unlit/CombineChannel");
    }

    // 输入----------------------------------------------------------------------------
    [EnumToggleButtons, HideLabel, GUIColor(0.25f, 1.1f, 1.1f)]
    public CombineSplitExchange combineSplitExchange;

    #region 合并
    [BoxGroup("合并", false)]
    [ShowIfGroup("合并/combineSplitExchange", Value = CombineSplitExchange.合并)]
    [EnumToggleButtons, HideLabel, GUIColor(0.9f, 1.2f, 0.6f)]
    public FileORFolder combineFileOrFolder;
    // 文件输入
    [ShowIfGroup("合并/combineSplitExchange", Value = CombineSplitExchange.合并)]
    [ShowIf("combineFileOrFolder", FileORFolder.文件)]
    [LabelText("文件路径输入")]
    public InputCombineFile[] inputCombineFiles = new InputCombineFile[1];
    public struct InputCombineFile
    {
        [BoxGroup("选择路径", false)]
        [LabelText("路径")]
        [HorizontalGroup("选择路径/Path", LabelWidth = 40)]
        [Sirenix.OdinInspector.FilePath(ParentFolder = "Assets", AbsolutePath = true, Extensions = "PNG, JPG, TGA")]
        public string texPath;

        [BoxGroup("选择保留通道")]
        [HorizontalGroup("选择保留通道/Channel", LabelWidth = 40)]
        [LabelText("R通道")]
        [ToggleLeft]
        public bool rChannel;
        [HorizontalGroup("选择保留通道/Channel", LabelWidth = 40)]
        [LabelText("G通道")]
        [ToggleLeft]
        public bool gChannel;
        [HorizontalGroup("选择保留通道/Channel", LabelWidth = 40)]
        [LabelText("B通道")]
        [ToggleLeft]
        public bool bChannel;
        [HorizontalGroup("选择保留通道/Channel", LabelWidth = 40)]
        [LabelText("A通道")]
        [ToggleLeft]
        public bool aChannel;
    }
    // 文件夹输入
    [ShowIfGroup("合并/combineSplitExchange", Value = CombineSplitExchange.合并)]
    [ShowIf("combineFileOrFolder", FileORFolder.文件夹)]
    [LabelText("文件夹路径输入")]
    public InputCombineFolder[] inputCombineFolders = new InputCombineFolder[1];
    public struct InputCombineFolder
    {
        [BoxGroup("选择路径", false)]
        [LabelText("路径")]
        [HorizontalGroup("选择路径/Path", LabelWidth = 40)]
        [FolderPath(ParentFolder = "Assets", AbsolutePath = true)]
        public string folderPath;
        
        [HorizontalGroup("模式", LabelWidth = 102)]
        [BoxGroup("模式/格式筛选")]
        [LabelText("")]
        public FiltrateFile fltrateFile;
        [BoxGroup("模式/关键字筛选")]
        [LabelText("")]
        public string fltrateWorld;

        [BoxGroup("模式/选择保留通道")]
        [HorizontalGroup("模式/选择保留通道/Channel", LabelWidth = 30)]
        [LabelText("R通道")]
        [ToggleLeft]
        public bool rChannel;
        [HorizontalGroup("模式/选择保留通道/Channel", LabelWidth = 30)]
        [LabelText("G通道")]
        [ToggleLeft]
        public bool gChannel;
        [HorizontalGroup("模式/选择保留通道/Channel", LabelWidth = 30)]
        [LabelText("B通道")]
        [ToggleLeft]
        public bool bChannel;
        [HorizontalGroup("模式/选择保留通道/Channel", LabelWidth = 30)]
        [LabelText("A通道")]
        [ToggleLeft]
        public bool aChannel;
    }
    
    // 合并命名
    [BoxGroup("合并命名")]
    [ShowIfGroup("合并命名/combineSplitExchange", Value = CombineSplitExchange.合并)]
    [HorizontalGroup("合并命名/combineSplitExchange/ref", LabelWidth = 50, MarginRight = 18)]
    [LabelText("命名参考")]
    public CombineFileNamingRule combineFileNamingRule = CombineFileNamingRule.自定义;

    [ShowIfGroup("合并命名/combineSplitExchange", Value = CombineSplitExchange.合并)]
    [HorizontalGroup("合并命名/combineSplitExchange/name", LabelWidth = 50, MarginRight = 18)]
    [ShowIf("combineFileNamingRule", CombineFileNamingRule.自定义)]
    [LabelText("文件名称")]
    public string inputCombineFileName;

    #endregion
    
    #region 拆分
    [BoxGroup("拆分", false)]
    [ShowIfGroup("拆分/combineSplitExchange", Value = CombineSplitExchange.拆分)]
    [EnumToggleButtons, HideLabel, GUIColor(0.9f, 1.2f, 0.6f)]
    public FileORFolder splitFileOrFolder;
    // 文件输入
    [ShowIfGroup("拆分/combineSplitExchange", Value = CombineSplitExchange.拆分)]
    [ShowIf("splitFileOrFolder", FileORFolder.文件)]
    [HorizontalGroup("拆分/combineSplitExchange/路径", LabelWidth = 50, MarginRight = 3)]
    [LabelText("路径选择")]
    [Sirenix.OdinInspector.FilePath(ParentFolder = "Assets", AbsolutePath = true, Extensions = "PNG, JPG, TGA")]
    public string[] inputSplitTexPaths = new string[1];
    
    // 文件夹输入
    [ShowIfGroup("拆分/combineSplitExchange", Value = CombineSplitExchange.拆分)]
    [ShowIf("splitFileOrFolder", FileORFolder.文件夹)]
    [HorizontalGroup("拆分/combineSplitExchange/路径", LabelWidth = 50, MarginRight = 3)]
    [LabelText("路径选择")]
    public InputSplitFolder[] inputSplitFolder = new InputSplitFolder[1];
    public struct InputSplitFolder
    {
        [BoxGroup("选择路径", false)]
        [LabelText("路径")]
        [HorizontalGroup("选择路径/Path", LabelWidth = 40)]
        [FolderPath(ParentFolder = "Assets", AbsolutePath = true)]
        public string folderPath;
        
        [BoxGroup("筛选", false)]
        [HorizontalGroup("筛选/模式", LabelWidth = 40, MarginRight = 10)]
        [LabelText("格式")]
        public FiltrateFile fltrateFile;
        [HorizontalGroup("筛选/模式", LabelWidth = 40, MarginRight = 18)]
        [LabelText("关键字")]
        public string fltrateWorld;
    }
    
    #region  拆分通道选择及命名
    [BoxGroup("选择拆分通道")]
    [ShowIfGroup("选择拆分通道/combineSplitExchange", Value = CombineSplitExchange.拆分)]
    [HorizontalGroup("选择拆分通道/combineSplitExchange/Channel", LabelWidth = 40)]
    [LabelText("R通道")]
    [ToggleLeft]
    public bool splitRChannel;
    [ShowIfGroup("选择拆分通道/combineSplitExchange", Value = CombineSplitExchange.拆分)]
    [HorizontalGroup("选择拆分通道/combineSplitExchange/Channel", LabelWidth = 40)]
    [LabelText("G通道")]
    [ToggleLeft]
    public bool splitGChannel;
    [ShowIfGroup("选择拆分通道/combineSplitExchange", Value = CombineSplitExchange.拆分)]
    [HorizontalGroup("选择拆分通道/combineSplitExchange/Channel", LabelWidth = 40)]
    [LabelText("B通道")]
    [ToggleLeft]
    public bool splitBChannel;
    [ShowIfGroup("选择拆分通道/combineSplitExchange", Value = CombineSplitExchange.拆分)]
    [HorizontalGroup("选择拆分通道/combineSplitExchange/Channel", LabelWidth = 40)]
    [LabelText("A通道")]
    [ToggleLeft]
    public bool splitAChannel;
    
    [BoxGroup("拆分命名")]
    [ShowIfGroup("拆分命名/combineSplitExchange", Value = CombineSplitExchange.拆分)]
    [HorizontalGroup("拆分命名/combineSplitExchange/ref", LabelWidth = 50, MarginRight = 18)]
    [LabelText("前缀命名")]
    public SplitFileNamingRule splitFileNamingRule = SplitFileNamingRule.自定义;

    [ShowIfGroup("拆分命名/combineSplitExchange", Value = CombineSplitExchange.合并)]
    [HorizontalGroup("拆分命名/combineSplitExchange/prefixName", LabelWidth = 50, MarginRight = 18)]
    [ShowIf("splitFileNamingRule", SplitFileNamingRule.自定义)]
    [LabelText("输入前缀")]
    public string inputSplitPrefixFileName;
    // R通道
    [ShowIfGroup("拆分命名/combineSplitExchange/splitRChannel")]
    [HorizontalGroup("拆分命名/combineSplitExchange/splitRChannel/suffixName", LabelWidth = 50, MarginRight = 18)]
    [LabelText("R通道")]
    public SplitChannelNaming splitRChannelNaming = SplitChannelNaming.自定义;

    [ShowIfGroup("拆分命名/combineSplitExchange/splitRChannel")]
    [HorizontalGroup("拆分命名/combineSplitExchange/splitRChannel/customSuffixName", LabelWidth = 50, MarginRight = 18)]
    [ShowIf("splitRChannelNaming", SplitChannelNaming.自定义)]
    [LabelText("输入后缀")]
    public string inputSplitRFileName;
    // G通道
    [ShowIfGroup("拆分命名/combineSplitExchange/splitGChannel")]
    [HorizontalGroup("拆分命名/combineSplitExchange/splitGChannel/suffixName", LabelWidth = 50, MarginRight = 18)]
    [LabelText("G通道")]
    public SplitChannelNaming splitGChannelNaming = SplitChannelNaming.自定义;

    [ShowIfGroup("拆分命名/combineSplitExchange/splitGChannel")]
    [HorizontalGroup("拆分命名/combineSplitExchange/splitGChannel/customSuffixName", LabelWidth = 50, MarginRight = 18)]
    [ShowIf("splitGChannelNaming", SplitChannelNaming.自定义)]
    [LabelText("输入后缀")]
    public string inputSplitGFileName;
    // B通道
    [ShowIfGroup("拆分命名/combineSplitExchange/splitBChannel")]
    [HorizontalGroup("拆分命名/combineSplitExchange/splitBChannel/suffixName", LabelWidth = 50, MarginRight = 18)]
    [LabelText("B通道")]
    public SplitChannelNaming splitBChannelNaming = SplitChannelNaming.自定义;

    [ShowIfGroup("拆分命名/combineSplitExchange/splitBChannel")]
    [HorizontalGroup("拆分命名/combineSplitExchange/splitBChannel/customSuffixName", LabelWidth = 50, MarginRight = 18)]
    [ShowIf("splitBChannelNaming", SplitChannelNaming.自定义)]
    [LabelText("输入后缀")]
    public string inputSplitBFileName;
    // B通道
    [ShowIfGroup("拆分命名/combineSplitExchange/splitAChannel")]
    [HorizontalGroup("拆分命名/combineSplitExchange/splitAChannel/suffixName", LabelWidth = 50, MarginRight = 18)]
    [LabelText("A通道")]
    public SplitChannelNaming splitAChannelNaming = SplitChannelNaming.自定义;

    [ShowIfGroup("拆分命名/combineSplitExchange/splitAChannel")]
    [HorizontalGroup("拆分命名/combineSplitExchange/splitAChannel/customSuffixName", LabelWidth = 50, MarginRight = 18)]
    [ShowIf("splitAChannelNaming", SplitChannelNaming.自定义)]
    [LabelText("输入后缀")]
    public string inputSplitAFileName;
    #endregion
    
    #endregion
    
    #region 通道交换
    [BoxGroup("交换通道", false)]
    [ShowIfGroup("交换通道/combineSplitExchange", Value = CombineSplitExchange.交换通道)]
    [EnumToggleButtons, HideLabel, GUIColor(0.9f, 1.2f, 0.6f)]
    public FileORFolder exchangeFileOrFolder;
    // 文件输入
    [ShowIfGroup("交换通道/combineSplitExchange", Value = CombineSplitExchange.交换通道)]
    [ShowIf("exchangeFileOrFolder", FileORFolder.文件)]
    [HorizontalGroup("交换通道/combineSplitExchange/路径", LabelWidth = 50, MarginRight = 3)]
    [LabelText("路径选择")]
    [Sirenix.OdinInspector.FilePath(ParentFolder = "Assets", AbsolutePath = true, Extensions = "PNG, JPG, TGA")]
    public string[] inputExchangeTexPaths = new string[1];

    // 文件夹输入
    [ShowIfGroup("交换通道/combineSplitExchange", Value = CombineSplitExchange.拆分)]
    [ShowIf("exchangeFileOrFolder", FileORFolder.文件夹)]
    [HorizontalGroup("交换通道/combineSplitExchange/路径", LabelWidth = 50, MarginRight = 3)]
    [LabelText("路径选择")]
    public InputExchangeFolder[] inputExchangeFolder = new InputExchangeFolder[1];
    public struct InputExchangeFolder
    {
        [BoxGroup("选择路径", false)]
        [LabelText("路径")]
        [HorizontalGroup("选择路径/Path", LabelWidth = 40)]
        [FolderPath(ParentFolder = "Assets", AbsolutePath = true)]
        public string folderPath;
        
        [BoxGroup("筛选", false)]
        [HorizontalGroup("筛选/模式", LabelWidth = 40, MarginRight = 10)]
        [LabelText("格式")]
        public FiltrateFile fltrateFile;
        [HorizontalGroup("筛选/模式", LabelWidth = 40, MarginRight = 18)]
        [LabelText("关键字")]
        public string fltrateWorld;
    }

    #region 通道交换及命名
    [BoxGroup("设置通道")]
    [ShowIfGroup("设置通道/combineSplitExchange", Value = CombineSplitExchange.交换通道)]
    [HorizontalGroup("设置通道/combineSplitExchange/Channel", LabelWidth = 50)]
    [LabelText("R通道=>"), GUIColor(1.8f, 1.0f, 1.0f)]
    public ExchangeChannelInfo exRChannel = ExchangeChannelInfo.R通道数据;
    [ShowIfGroup("设置通道/combineSplitExchange", Value = CombineSplitExchange.交换通道)]
    [HorizontalGroup("设置通道/combineSplitExchange/Channel", LabelWidth = 50)]
    [LabelText("G通道=>"), GUIColor(1.0f, 1.8f, 1.0f)]
    public ExchangeChannelInfo exGChannel = ExchangeChannelInfo.G通道数据;
    [ShowIfGroup("设置通道/combineSplitExchange", Value = CombineSplitExchange.交换通道)]
    [HorizontalGroup("设置通道/combineSplitExchange/Channel", LabelWidth = 50)]
    [LabelText("B通道=>"), GUIColor(1.0f, 1.0f, 1.8f)]
    public ExchangeChannelInfo exBChannel = ExchangeChannelInfo.B通道数据;
    [ShowIfGroup("设置通道/combineSplitExchange", Value = CombineSplitExchange.交换通道)]
    [HorizontalGroup("设置通道/combineSplitExchange/Channel", LabelWidth = 50)]
    [LabelText("A通道=>"), GUIColor(1.8f, 1.8f, 1.8f)]
    public ExchangeChannelInfo exAChannel = ExchangeChannelInfo.A通道数据;
    #endregion
    
    [BoxGroup("交换通道命名")]
    [ShowIfGroup("交换通道命名/combineSplitExchange", Value = CombineSplitExchange.交换通道)]
    [HorizontalGroup("交换通道命名/combineSplitExchange/PrefixNaming", LabelWidth = 50)]
    [LabelText("前缀命名")]
    public SplitFileNamingRule exPrefixName = SplitFileNamingRule.自定义;

    [ShowIfGroup("交换通道命名/combineSplitExchange", Value = CombineSplitExchange.交换通道)]
    [ShowIf("exPrefixName", SplitFileNamingRule.自定义)]
    [HorizontalGroup("交换通道命名/combineSplitExchange/PrefixNamingStr", LabelWidth = 50)]
    [LabelText("输入前缀")]
    public string inputExPrefixFileName;

    [ShowIfGroup("交换通道命名/combineSplitExchange", Value = CombineSplitExchange.交换通道)]
    [HorizontalGroup("交换通道命名/combineSplitExchange/SuffixNamingStr", LabelWidth = 50)]
    [LabelText("输入后缀")]
    public string inputExSuffixFileName;

    #endregion
    
    // 输出----------------------------------------------------------------------------
    [BoxGroup("输出")]
    [HorizontalGroup("输出/Type", LabelWidth = 50)]
    [LabelText("输出格式")]
    public TexType texType = TexType.PNG;
    
    [HorizontalGroup("输出/Type", LabelWidth = 50)]
    [LabelText("是否进行伽马矫正")]
    [ToggleLeft]
    public bool isGamma = false;
    
    [HorizontalGroup("输出/SavePath", LabelWidth = 50)]
    [LabelText("输出路径")]
    [FolderPath(ParentFolder = "Assets", AbsolutePath = true)]
    public string savePath;

    [Button(buttonSize : 25), GUIColor(0, 1.1f, 0.4f)]
    [LabelText("处理")]
    public void Excute()
    {
        if (combineSplitExchange == CombineSplitExchange.合并)
        {    
            if (combineFileOrFolder == FileORFolder.文件)
            {
                GetTexsInfo(inputCombineFiles, out inputCombineTexs, out inputCombineTexChannels);
                string fileName = FileRefer(inputCombineFiles);
                if (!OutputIsLegal(inputCombineFiles, checkUnnecessaryInput, savePath, fileName)) return;
                checkUnnecessaryInput = new Vector4(0, 0, 0, 0);
                CombineChannels(inputCombineTexs, inputCombineTexChannels, texType, savePath, fileName, isGamma ? 1.0f / 2.2f : 1.0f);
            }
            else
            {
                int minTexsNum = 0;
                inputCombineTexsChannels = GetTexsInfo(inputCombineFolders); // 获得保留通道信息
                if (!OutputIsLegal(inputCombineFolders, checkUnnecessaryInput, savePath)) return;
                checkUnnecessaryInput = new Vector4(0, 0, 0, 0);
                GetTexsDict(inputCombineFolders, out inputCombineTexsDict, out inputCombineFileNamesDict);
                checkUnnecessaryInput = new Vector4(0, 0, 0, 0);
                minTexsNum = GetLessTexsNum(inputCombineTexsDict);
                List<string> fileNames = FileRefer(inputCombineFileNamesDict, inputCombineTexsChannels, minTexsNum);
                for (int i = 0; i < minTexsNum; i++)
                {
                    List<Texture2D> tempTex = new List<Texture2D>();
                    for (int j = 0; j < inputCombineTexsDict.Count; j++)
                    {
                        tempTex.Add(inputCombineTexsDict[j][i]);
                    }
                    CombineChannels(tempTex, inputCombineTexsChannels, texType, savePath, fileNames[i], isGamma ? 1.0f / 2.2f : 1.0f);
                }
            }
        }
        else if (combineSplitExchange == CombineSplitExchange.拆分)
        {
            if (splitFileOrFolder == FileORFolder.文件)
            {
                if (inputSplitTexPaths.Length == 0)
                {
                    Debug.LogError("请至少添加一个输入文件夹!");
                    return;
                }
                GetTexsInfo(inputSplitTexPaths, out inputSplitTexs);
                List<string> filesPrefixName = FileRefer(inputSplitTexPaths, splitFileNamingRule, inputSplitPrefixFileName);
                if (!OutputIsLegal(inputSplitTexPaths, savePath, filesPrefixName[0])) return;
                for (int i = 0; i < inputSplitTexs.Count; i++)
                {
                    SplitChannel(inputSplitTexs[i], texType, savePath, filesPrefixName[i], isGamma ? 1.0f / 2.2f : 1.0f);
                }
            }
            else
            {
                if (!OutputIsLegal(inputSplitFolder, savePath)) return;
                GetTexsDict(inputSplitFolder, out inputSplitTexsDict, out inputSplitFileNamesDict);
                for (int i = 0; i < inputSplitTexsDict.Count; i++)
                {
                    for (int j = 0; j < inputSplitTexsDict[i].Count; j++)
                    {
                        SplitChannel(inputSplitTexsDict[i][j], texType, savePath, inputSplitFileNamesDict[i][j], isGamma ? 1.0f / 2.2f : 1.0f);
                    }
                }
            }
        }
        else
        {
            if (exchangeFileOrFolder == FileORFolder.文件)
            {
                if (inputExchangeTexPaths.Length == 0)
                {
                    Debug.LogError("请至少添加一个输入文件夹!");
                    return;
                }
                GetTexsInfo(inputExchangeTexPaths, out inputExchangeTexs);
                List<string> filesPrefixName = FileRefer(inputExchangeTexPaths, exPrefixName, inputExPrefixFileName);
                if (!OutputIsLegal(inputExchangeTexPaths, savePath, filesPrefixName[0])) return;
                for (int i = 0; i < inputExchangeTexs.Count; i++)
                {
                    ExchangeChannel(inputExchangeTexs[i], texType, savePath, filesPrefixName[i] + inputExSuffixFileName, isGamma ? 1.0f / 2.2f : 1.0f);
                }
            }
            else
            {
                if (!OutputIsLegal(inputExchangeFolder, savePath)) return;
                GetTexsDict(inputExchangeFolder, out inputExchangeTexsDict, out inputExchangeFileNamesDict);
                for (int i = 0; i < inputExchangeTexsDict.Count; i++)
                {
                    for (int j = 0; j < inputExchangeTexsDict[i].Count; j++)
                    {
                        ExchangeChannel(inputExchangeTexsDict[i][j], texType, savePath, inputExchangeFileNamesDict[i][j] + inputExSuffixFileName, isGamma ? 1.0f / 2.2f : 1.0f);
                    }
                }
            }
        }
    }
    #endregion

    #region  逻辑
    //TODO: 获取输入图片信息
    private void GetTexsInfo(InputCombineFile[] _inputFiles, out List<Texture2D> _outTexs, out List<Vector4> _outVecs)
    {
        _outTexs = new List<Texture2D>();
        _outVecs = new List<Vector4>();
        for (int i = 0; i < _inputFiles.Length; i++)
        {
            Vector4 tempVec = new Vector4(
                _inputFiles[i].rChannel ? 1 : 0, 
                _inputFiles[i].gChannel ? 1 : 0, 
                _inputFiles[i].bChannel ? 1 : 0, 
                _inputFiles[i].aChannel ? 1 : 0);
            if (CheckChannelIsSelected(tempVec))
            {
                _outTexs.Add(GetTexture2DFromPath(_inputFiles[i].texPath));
                _outVecs.Add(new Vector4(
                    _inputFiles[i].rChannel ? 1 : 0, 
                    _inputFiles[i].gChannel ? 1 : 0, 
                    _inputFiles[i].bChannel ? 1 : 0, 
                    _inputFiles[i].aChannel ? 1 : 0));
            }
            else
                continue;
        }
    }
    private void GetTexsInfo(string _texFolderPath, FiltrateFile _filtrateFile, string _filtrateWorld, out List<Texture2D> _outTexs, out List<string> _outTexsName)
    {
        List<string> allFilePaths = new List<string>();
        List<string> tempPaths = new List<string>();
        List<string> texPaths = new List<string>();
        // TODO: 筛选图片
        if (_filtrateFile != FiltrateFile.ALL)
        {
            tempPaths = new List<string>(Directory.GetFiles(_texFolderPath, "*." + _filtrateFile.ToString()));
        } 
        else
        {
            allFilePaths =  new List<string>(Directory.GetFiles(_texFolderPath, "*.*"));
            for (int i = 0; i < allFilePaths.Count; i++)
            {
                // 关键字筛选非常需要优化
                if(string.Compare(Path.GetExtension(allFilePaths[i]), ".PNG") == 0 || string.Compare(Path.GetExtension(allFilePaths[i]), ".png") == 0 ||
                   string.Compare(Path.GetExtension(allFilePaths[i]), ".JPG") == 0 || string.Compare(Path.GetExtension(allFilePaths[i]), ".jpg") == 0 ||
                   string.Compare(Path.GetExtension(allFilePaths[i]), ".TGA") == 0 || string.Compare(Path.GetExtension(allFilePaths[i]), ".tga") == 0 )
                {
                    tempPaths.Add(allFilePaths[i]);
                }
            }
        }
        if (!string.IsNullOrEmpty(_filtrateWorld))
        {
            for (int i = 0; i < tempPaths.Count; i++)
            {
                // 关键词筛选
                if(Regex.IsMatch(Path.GetFileNameWithoutExtension(tempPaths[i]), _filtrateWorld))
                {
                    texPaths.Add(tempPaths[i]);
                }
            }
        }
        else
            texPaths = tempPaths;
        //-----------------
        _outTexsName = new List<string>();
        for (int i = 0; i < texPaths.Count; i++)
        {
            _outTexsName.Add(Path.GetFileNameWithoutExtension(texPaths[i]));
        }
        _outTexs = new List<Texture2D>();
        for (int i = 0; i < texPaths.Count; i++)
        {
            _outTexs.Add(GetTexture2DFromPath(texPaths[i]));
        }
    }
    private List<Vector4> GetTexsInfo(InputCombineFolder[] _inputFolders)
    {
        List<Vector4> tempVecs = new List<Vector4>();
        for (int i = 0; i < _inputFolders.Length; i++)
        {
            Vector4 tempVec = new Vector4(
                _inputFolders[i].rChannel ? 1 : 0, 
                _inputFolders[i].gChannel ? 1 : 0, 
                _inputFolders[i].bChannel ? 1 : 0, 
                _inputFolders[i].aChannel ? 1 : 0);
            if (CheckChannelIsSelected(tempVec))
            {
                tempVecs.Add(new Vector4(
                    _inputFolders[i].rChannel ? 1 : 0, 
                    _inputFolders[i].gChannel ? 1 : 0, 
                    _inputFolders[i].bChannel ? 1 : 0, 
                    _inputFolders[i].aChannel ? 1 : 0));
            }
            else
                continue;
        }
        return tempVecs;
    }
    private void GetTexsDict(InputCombineFolder[] _inputFolders, out Dictionary<int, List<Texture2D>> _outTexsDict, out Dictionary<int, List<string>> _outTexsNameDict)
    {
        List<Texture2D> _outTexs = new List<Texture2D>();
        List<string> _outTexsName = new List<string>();
        _outTexsDict = new Dictionary<int, List<Texture2D>>();
        _outTexsNameDict = new Dictionary<int, List<string>>();
        int count = 0;
        for (int i = 0; i < _inputFolders.Length; i++)
        {
            Vector4 tempVec = new Vector4(
            _inputFolders[i].rChannel ? 1 : 0, 
            _inputFolders[i].gChannel ? 1 : 0, 
            _inputFolders[i].bChannel ? 1 : 0, 
            _inputFolders[i].aChannel ? 1 : 0);
            if (CheckChannelIsSelected(tempVec))
            {
                GetTexsInfo(_inputFolders[i].folderPath,  _inputFolders[i].fltrateFile, _inputFolders[i].fltrateWorld, out _outTexs, out _outTexsName);
                _outTexsDict.Add(count, _outTexs);
                _outTexsNameDict.Add(count, _outTexsName);
                count ++;
            }
            else
                continue;
        }
    }
    private void GetTexsDict(InputSplitFolder[] _inputFolders, out Dictionary<int, List<Texture2D>> _outTexsDict, out Dictionary<int, List<string>> _outTexsNameDict)
    {
        List<Texture2D> _outTexs = new List<Texture2D>();
        List<string> _outTexsName = new List<string>();
        _outTexsDict = new Dictionary<int, List<Texture2D>>();
        _outTexsNameDict = new Dictionary<int, List<string>>();
        for (int i = 0; i < _inputFolders.Length; i++)
        {
            GetTexsInfo(_inputFolders[i].folderPath,  _inputFolders[i].fltrateFile, _inputFolders[i].fltrateWorld, out _outTexs, out _outTexsName);
            _outTexsDict.Add(i, _outTexs);
            _outTexsNameDict.Add(i, FileRefer(_outTexsName, splitFileNamingRule, inputSplitPrefixFileName));
        }
    }
    private void GetTexsDict(InputExchangeFolder[] _inputFolders, out Dictionary<int, List<Texture2D>> _outTexsDict, out Dictionary<int, List<string>> _outTexsNameDict)
    {
        List<Texture2D> _outTexs = new List<Texture2D>();
        List<string> _outTexsName = new List<string>();
        _outTexsDict = new Dictionary<int, List<Texture2D>>();
        _outTexsNameDict = new Dictionary<int, List<string>>();
        for (int i = 0; i < _inputFolders.Length; i++)
        {
            GetTexsInfo(_inputFolders[i].folderPath,  _inputFolders[i].fltrateFile, _inputFolders[i].fltrateWorld, out _outTexs, out _outTexsName);
            _outTexsDict.Add(i, _outTexs);
            _outTexsNameDict.Add(i, FileRefer(_outTexsName, exPrefixName, inputExPrefixFileName));
        }
    }
    private void GetTexsInfo(string[] _inputTexsPath, out List<Texture2D> _outTexs)
    {
        _outTexs = new List<Texture2D>();
        for (int i = 0; i < _inputTexsPath.Length; i++)
        {
            _outTexs.Add(GetTexture2DFromPath(_inputTexsPath[i]));
        }
    }
    //TODO: 判断重复通道的文件
    private bool CheckChannelIsSelected(Vector4 _vecs)
    {
        // 0为没被选择，1为被选择
        if (checkUnnecessaryInput.x < _vecs.x || checkUnnecessaryInput.y < _vecs.y ||
            checkUnnecessaryInput.z < _vecs.z || checkUnnecessaryInput.w < _vecs.w)
        {
            checkUnnecessaryInput += _vecs;
            return true;
        }
        else
        {
            return false;
        }
    }
    //TODO: 获得所选文件夹中文件的最少数量
    private int GetLessTexsNum(Dictionary<int, List<Texture2D>> _texsDict)
    {
        int tempCount = 0;
        for (int i = 0; i < _texsDict.Count; i++)
        {
            if (_texsDict[i].Count != 0)
            {
                tempCount = _texsDict[i].Count;
                break;
            }
        }
        for (int i = 0; i < _texsDict.Count; i++)
        {
            if (tempCount > _texsDict[i].Count) tempCount = _texsDict[i].Count;
        }
        return tempCount;
    }
    //TODO: 命名参考
    private string FileRefer(InputCombineFile[] _inputFiles)
    {
        string[] tempName = new string[4];
        for (int i = 0; i < _inputFiles.Length; i++)
        {
            if (_inputFiles[i].rChannel && string.IsNullOrEmpty(tempName[0]))
                tempName[0] = Path.GetFileNameWithoutExtension(_inputFiles[i].texPath);
            if (_inputFiles[i].gChannel && string.IsNullOrEmpty(tempName[1]))
                tempName[1] = Path.GetFileNameWithoutExtension(_inputFiles[i].texPath);
            if (_inputFiles[i].bChannel && string.IsNullOrEmpty(tempName[2]))
                tempName[2] = Path.GetFileNameWithoutExtension(_inputFiles[i].texPath);
            if (_inputFiles[i].aChannel && string.IsNullOrEmpty(tempName[3]))
                tempName[3] = Path.GetFileNameWithoutExtension(_inputFiles[i].texPath);
        }
        switch (combineFileNamingRule)
        {
            case CombineFileNamingRule.R通道:
                return tempName[0];
            case CombineFileNamingRule.G通道:
                return tempName[1];
            case CombineFileNamingRule.B通道:
                return tempName[2];
            case CombineFileNamingRule.A通道:
                return tempName[3];
            default:
                return inputCombineFileName;
        }
    }
    private List<string> FileRefer(Dictionary<int, List<string>> _fileNames, List<Vector4> _vects, int _minTexsNum)
    {
        List<string> tempNames = new List<string>();
        switch (combineFileNamingRule)
        {
            case CombineFileNamingRule.R通道:
                for (int i = 0; i < _vects.Count; i++)
                {
                    if (_vects[i].x == 1) tempNames = _fileNames[i];
                }
                break;
            case CombineFileNamingRule.G通道:
                for (int i = 0; i < _vects.Count; i++)
                {
                    if (_vects[i].y == 1) tempNames = _fileNames[i];
                }
                break;
            case CombineFileNamingRule.B通道:
                for (int i = 0; i < _vects.Count; i++)
                {
                    if (_vects[i].z == 1) tempNames = _fileNames[i];
                }
                break;
            case CombineFileNamingRule.A通道:
                for (int i = 0; i < _vects.Count; i++)
                {
                    if (_vects[i].w == 1) tempNames = _fileNames[i];
                }
                break;
            default:
                for (int i = 0; i < _minTexsNum; i++)
                {
                    tempNames.Add(inputCombineFileName + "_" + i);
                }
                break;
        }
        return tempNames;
    }
    // 通过输入文件位置进行命名
    private List<string> FileRefer(string[] _inputFilesPath, SplitFileNamingRule _namingRule, string _inputFileName)
    {
        List<string> tempName = new List<string>();
        for (int i = 0; i < _inputFilesPath.Length; i++)
        {
            switch (_namingRule)
            {
                case SplitFileNamingRule.文件名:
                    tempName.Add(Path.GetFileNameWithoutExtension(_inputFilesPath[i]));
                    break;
                case SplitFileNamingRule.文件名去第一个后缀:
                    tempName.Add(RemoveSuffix(Path.GetFileNameWithoutExtension(_inputFilesPath[i])));
                    break;
                default:
                    tempName.Add(_inputFileName + "_" + i);
                    break;
            }
        }
        return tempName;
    }
    // 通过文件名进行命名
    private List<string> FileRefer(List<string> _inputFilesName, SplitFileNamingRule _namingRule, string _inputFileName)
    {
        List<string> tempName = new List<string>();
        for (int i = 0; i < _inputFilesName.Count; i++)
        {
            switch (_namingRule)
            {
                case SplitFileNamingRule.文件名:
                    tempName.Add(_inputFilesName[i]);
                    break;
                case SplitFileNamingRule.文件名去第一个后缀:
                    tempName.Add(RemoveSuffix(_inputFilesName[i]));
                    break;
                default:
                    tempName.Add(_inputFileName + "_" + i);
                    break;
            }
        }
        return tempName;
    }
    // 去后缀
    private string RemoveSuffix(string _name)
    {
        int tempCount = _name.LastIndexOf('_');
        // Debug.Log(tempCount);
        if (tempCount != -1)
            return _name.Remove(_name.LastIndexOf('_'));
        else
            return _name;
    }
    //TODO: 判断合法性
    private bool OutputIsLegal(InputCombineFile[] _files, Vector4 _channels, string _savePath, string _name)
    {
        if (_files.Length == 0)
        {
            Debug.LogError("请至少添加一个输入文件!");
            return false;
        }
        else if (string.IsNullOrEmpty(_files[0].texPath))
        {
            Debug.LogError("第一个输入文件路径不能为空!");
            return false;
        }
        else if ((_channels.x + _channels.y + _channels.z + _channels.w) == 0)
        {
            Debug.LogError("请至少选择一个输出通道!");
            return false;
        }
        else if (string.IsNullOrEmpty(_savePath))
        {
            Debug.LogError("保存路径不能为空!");
            return false;
        }
        else if (string.IsNullOrEmpty(_name))
        {
            Debug.LogError("文件名不能为空!");
            return false;
        }
        else return true;
    }
    private bool OutputIsLegal(InputCombineFolder[] _folders, Vector4 _channels, string _savePath)
    {
        if (_folders.Length == 0)
        {
            Debug.LogError("请至少添加一个输入文件夹!");
            return false;
        }
        else if (string.IsNullOrEmpty(_folders[0].folderPath))
        {
            Debug.LogError("第一个输入文件夹路径不能为空!");
            return false;
        }
        else if ((_channels.x + _channels.y + _channels.z + _channels.w) == 0)
        {
            Debug.LogError("请至少选择一个输出通道!");
            return false;
        }
        else if (string.IsNullOrEmpty(_savePath))
        {
            Debug.LogError("保存路径不能为空!");
            return false;
        }
        else return true;
    }
    private bool OutputIsLegal(string[] _inputSplitPaths, string _savePath, string _inputName)
    {
        if (_inputSplitPaths.Length == 0)
        {
            Debug.LogError("请至少添加一个输入文件!");
            return false;
        }
        else if (string.IsNullOrEmpty(_inputSplitPaths[0]))
        {
            Debug.LogError("输入文件路径不能为空!");
            return false;
        }
        else if (!(splitRChannel || splitGChannel || splitBChannel || splitAChannel) && combineSplitExchange == CombineSplitExchange.拆分)
        {
            Debug.LogError("请选择一个拆分通道!");
            return false;
        }
        else if(string.IsNullOrEmpty(_inputName))
        {
            Debug.LogError("前缀名不能为空!");
            return false;
        }
        else if (string.IsNullOrEmpty(_savePath))
        {
            Debug.LogError("保存路径不能为空!");
            return false;
        }
        else
            return true;
    }
    private bool OutputIsLegal(InputSplitFolder[] _inputFolder, string _savePath)
    {
        if (_inputFolder.Length == 0)
        {
            Debug.LogError("请至少添加一个输入文件夹!");
            return false;
        }
        else if (string.IsNullOrEmpty(_inputFolder[0].folderPath))
        {
            Debug.LogError("输入文件夹路径不能为空!");
            return false;
        }
        else if (!(splitRChannel || splitGChannel || splitBChannel || splitAChannel))
        {
            Debug.LogError("请选择一个拆分通道!");
            return false;
        }
        else if (string.IsNullOrEmpty(_savePath))
        {
            Debug.LogError("保存路径不能为空!");
            return false;
        }
        else
            return true;
    }
    private bool OutputIsLegal(InputExchangeFolder[] _inputFolder, string _savePath)
    {
        if (_inputFolder.Length == 0)
        {
            Debug.LogError("请至少添加一个输入文件夹!");
            return false;
        }
        else if (string.IsNullOrEmpty(_inputFolder[0].folderPath))
        {
            Debug.LogError("输入文件夹路径不能为空!");
            return false;
        }
        else if (string.IsNullOrEmpty(_savePath))
        {
            Debug.LogError("保存路径不能为空!");
            return false;
        }
        else
            return true;
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
    public void CombineChannels(List<Texture2D> _texs, List<Vector4> _vecs, TexType _texType, string _savePath, string _fileName, float _gammaValue)
    {
        RenderTexture outputRT = RenderTexture.GetTemporary(_texs[0].width, _texs[0].height, 0, RenderTextureFormat.ARGB32);
        RenderTexture inputRT = RenderTexture.GetTemporary(_texs[0].width, _texs[0].height, 0, RenderTextureFormat.ARGB32); 
        Material mat = new Material(combineShader);
        for (int i = 0; i < _texs.Count; i++)
        {
            mat.SetTexture("_AddTex", _texs[i]);
            mat.SetVector("_ChannelMask", _vecs[i]);
            Graphics.Blit(inputRT, outputRT, mat, 0);
            inputRT = outputRT;
            outputRT = RenderTexture.GetTemporary(_texs[0].width, _texs[0].height, 0, RenderTextureFormat.ARGB32);
        }
        mat.SetFloat("_GammaValue", _gammaValue);
        Graphics.Blit(inputRT, outputRT, mat, 6);
        SaveToImage(outputRT, _texType, _savePath, _fileName);
        RenderTexture.ReleaseTemporary(outputRT);
        RenderTexture.ReleaseTemporary(inputRT);
    }
    public void SplitChannel(Texture2D _texs, TexType _texType, string _savePath, string _fileName, float _gammaValue)
    {
        RenderTexture rRT = RenderTexture.GetTemporary(_texs.width, _texs.height, 0, RenderTextureFormat.R8);
        RenderTexture gRT = RenderTexture.GetTemporary(_texs.width, _texs.height, 0, RenderTextureFormat.R8);
        RenderTexture bRT = RenderTexture.GetTemporary(_texs.width, _texs.height, 0, RenderTextureFormat.R8);
        RenderTexture aRT = RenderTexture.GetTemporary(_texs.width, _texs.height, 0, RenderTextureFormat.R8);
        Material mat = new Material(combineShader);
        if (splitRChannel)
        {
            Graphics.Blit(_texs, rRT, mat, 1);
            if (splitRChannelNaming == SplitChannelNaming.自定义)
                SaveToImage(rRT, _texType, _savePath, _fileName + inputSplitRFileName);
            else
                SaveToImage(rRT, _texType, _savePath, _fileName + splitRChannelNaming.ToString());
        }
        if (splitGChannel)
        {
            Graphics.Blit(_texs, gRT, mat, 2);
            if (splitGChannelNaming == SplitChannelNaming.自定义)
                SaveToImage(gRT, _texType, _savePath, _fileName + inputSplitGFileName);
            else
                SaveToImage(gRT, _texType, _savePath, _fileName + splitGChannelNaming.ToString());
        }
        if (splitBChannel)
        {
            Graphics.Blit(_texs, bRT, mat, 3);
            if (splitBChannelNaming == SplitChannelNaming.自定义)
                SaveToImage(bRT, _texType, _savePath, _fileName + inputSplitBFileName);
            else
                SaveToImage(bRT, _texType, _savePath, _fileName + splitBChannelNaming.ToString());
        }
        if (splitAChannel)
        {
            Graphics.Blit(_texs, aRT, mat, 4);
            if (splitAChannelNaming == SplitChannelNaming.自定义)
                SaveToImage(aRT, _texType, _savePath, _fileName + inputSplitAFileName);
            else
                SaveToImage(aRT, _texType, _savePath, _fileName + splitAChannelNaming.ToString());
        }
        RenderTexture.ReleaseTemporary(rRT);
        RenderTexture.ReleaseTemporary(gRT);
        RenderTexture.ReleaseTemporary(bRT);
        RenderTexture.ReleaseTemporary(aRT);
    }
    public void ExchangeChannel(Texture2D _texs, TexType _texType, string _savePath, string _fileName, float _gammaValue)
    {
        RenderTexture rRT = RenderTexture.GetTemporary(_texs.width, _texs.height, 0, RenderTextureFormat.R8);
        RenderTexture gRT = RenderTexture.GetTemporary(_texs.width, _texs.height, 0, RenderTextureFormat.R8);
        RenderTexture bRT = RenderTexture.GetTemporary(_texs.width, _texs.height, 0, RenderTextureFormat.R8);
        RenderTexture aRT = RenderTexture.GetTemporary(_texs.width, _texs.height, 0, RenderTextureFormat.R8);
        List<RenderTexture> rts = new List<RenderTexture>();
        Material mat = new Material(combineShader);
        Graphics.Blit(_texs, rRT, mat, 1);
        Graphics.Blit(_texs, gRT, mat, 2);
        Graphics.Blit(_texs, bRT, mat, 3);
        Graphics.Blit(_texs, aRT, mat, 4);
        // 做出区分
        rts.Add(GetSplitedChannelRT(exRChannel, rRT, gRT, bRT, aRT));
        rts.Add(GetSplitedChannelRT(exGChannel, rRT, gRT, bRT, aRT));
        rts.Add(GetSplitedChannelRT(exBChannel, rRT, gRT, bRT, aRT));
        rts.Add(GetSplitedChannelRT(exAChannel, rRT, gRT, bRT, aRT));
        ExchangeChannnel(rts, _texType, _savePath, _fileName, _gammaValue);
        RenderTexture.ReleaseTemporary(rRT);
        RenderTexture.ReleaseTemporary(gRT);
        RenderTexture.ReleaseTemporary(bRT);
        RenderTexture.ReleaseTemporary(aRT);
        foreach (var item in rts)
        {
            RenderTexture.ReleaseTemporary(item);
        }
    }
    private RenderTexture GetSplitedChannelRT(ExchangeChannelInfo _channel, RenderTexture _rRT, RenderTexture _gRT, RenderTexture _bRT, RenderTexture _aRT)
    {
        switch (_channel)
        {
            case ExchangeChannelInfo.R通道数据:
                return _rRT;
            case ExchangeChannelInfo.G通道数据:
                return _gRT;
            case ExchangeChannelInfo.B通道数据:
                return _bRT;
            default:
                return _aRT;
        }
    }
    public void ExchangeChannnel(List<RenderTexture> _RTS, TexType _texType, string _savePath, string _fileName, float _gammaValue)
    {
        RenderTexture RT = RenderTexture.GetTemporary(_RTS[0].width, _RTS[0].height, 0, RenderTextureFormat.ARGB32);
        Material mat = new Material(combineShader);
        mat.SetTexture("_RChannelTex", _RTS[0]);
        mat.SetTexture("_GChannelTex", _RTS[1]);
        mat.SetTexture("_BChannelTex", _RTS[2]);
        mat.SetTexture("_AChannelTex", _RTS[3]);
        Graphics.Blit(new Texture2D(0,0), RT, mat, 5);
        SaveToImage(RT, _texType, _savePath, _fileName);
        RenderTexture.ReleaseTemporary(RT);
    }
    //TODO: 保存图片
    public void SaveToImage(RenderTexture _rt, TexType _texType, string _savePath, string _fileName)
    {
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = _rt;
        Texture2D tex2D;
        if(combineSplitExchange == CombineSplitExchange.拆分)
            tex2D = new Texture2D(_rt.width, _rt.height, TextureFormat.R8, false);
        else
            tex2D = new Texture2D(_rt.width, _rt.height, TextureFormat.RGBA32, false);
        tex2D.ReadPixels(new Rect(0, 0, _rt.width, _rt.height), 0, 0);
        tex2D.Apply();
        GraphicsFormat format = _rt.graphicsFormat;
        Color32[] array = tex2D.GetPixels32();
        byte[] texBytes;
        string fileExtensions = "";
        switch (_texType)
        {
            case TexType.PNG:
                texBytes = ImageConversion.EncodeArrayToPNG(array, format, (uint)_rt.width, (uint)_rt.height);
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
        AssetDatabase.Refresh();
    }
    #endregion
}