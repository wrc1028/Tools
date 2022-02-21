#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

public class CheckModelProperty : OdinEditorWindow
{
    private static CheckModelProperty window;
    [MenuItem("Odin/检查模型属性")]
    private static void ShowWindow()
    {
        window = (CheckModelProperty)EditorWindow.GetWindow(typeof(CheckModelProperty), false, "检查模型冗余属性");
        window.maxSize = new Vector2(800, 540);
        window.Show();
    }
    #region Odin属性
    public enum CheckType
    {
        指定路径, 自定义设置, 全部资源,
    }
    [HorizontalGroup("检查设置", LabelWidth = 60)]
    [LabelText("检查范围")]
    public CheckType checkType = CheckType.指定路径;
    // ---
    [ShowIf("checkType", CheckType.指定路径)]
    [HorizontalGroup("指定路径", LabelWidth = 60)]
    [FolderPath(ParentFolder = "Assets", AbsolutePath = false)]
    [LabelText("路径位置")]
    public string assignPath;
    [ShowIf("checkType", CheckType.自定义设置)]
    [HorizontalGroup("自定义", LabelWidth = 60)]
    [LabelText("设置文件")]
    public ModelCheckSetting checkSetting;
    // ----
    [HideIfGroup("checkType", CheckType.自定义设置)]
    [BoxGroup("checkType/模型属性限制")]
    [HorizontalGroup("checkType/模型属性限制/属性1", LabelWidth = 60)]
    [ToggleLeft]
    [LabelText("坐标")]
    public bool isHavePosition = true;
    [HorizontalGroup("checkType/模型属性限制/属性1", LabelWidth = 60)]
    [ToggleLeft]
    [LabelText("法线")]
    public bool isHaveNormal = true;
    [HorizontalGroup("checkType/模型属性限制/属性1", LabelWidth = 60)]
    [ToggleLeft]
    [LabelText("切线")]
    public bool isHaveTangent = true;
    [HorizontalGroup("checkType/模型属性限制/属性1", LabelWidth = 60)]
    [ToggleLeft]
    [LabelText("颜色")]
    public bool isHaveColor;
    [HorizontalGroup("checkType/模型属性限制/属性2", LabelWidth = 60)]
    [ToggleLeft]
    [LabelText("UV1")]
    public bool isHaveUV1 = true;
    [HorizontalGroup("checkType/模型属性限制/属性2", LabelWidth = 60)]
    [ToggleLeft]
    [LabelText("UV2")]
    public bool isHaveUV2;
    [HorizontalGroup("checkType/模型属性限制/属性2", LabelWidth = 60)]
    [ToggleLeft]
    [LabelText("UV3")]
    public bool isHaveUV3;
    [HorizontalGroup("checkType/模型属性限制/属性2", LabelWidth = 60)]
    [ToggleLeft]
    [LabelText("UV4")]
    public bool isHaveUV4;
    public struct CheckResult
    {
        [HideInEditorMode]
        public string modelName;
        [HideInEditorMode]
        public List<int> redundanceCode;
        [HideInEditorMode]
        public string modelPath;

        [HorizontalGroup("物体", LabelWidth = 60)]
        [LabelText("模型信息")]
        public GameObject modelObject;

        [HorizontalGroup("网格", LabelWidth = 60)]
        [LabelText("网格信息")]
        [DisplayAsString]
        [ListDrawerSettings(NumberOfItemsPerPage = 5)]
        public List<string> filtersInfo;
    }
    [LabelText("查询结果")]
    [ListDrawerSettings(NumberOfItemsPerPage = 5)]
    [Searchable]
    public List<CheckResult> results = new List<CheckResult>();
    [Button("检查", buttonSize : 24), GUIColor(0, 1.1f, 0.5f, 1)]
    public void Excute()
    {
        if (checkType == CheckType.全部资源)
        {
            checkPath = "Assets";
            propToggles = UpdataPropToggles();
            results = new List<CheckResult>();
            pattem = @"\b\w+\b";
            GetAllModelsPath(checkPath, propToggles, pattem);
        }
        else if (checkType == CheckType.指定路径)
        {
            if (string.IsNullOrEmpty(assignPath))
            {
                UnityEditor.EditorUtility.DisplayDialog("设置为空", "指定路径为空!", "确认", "取消");
                return;
            }
            checkPath =  "Assets/" + assignPath;
            propToggles = UpdataPropToggles();
            results = new List<CheckResult>();
            pattem = @"\b\w+\b";
            GetAllModelsPath(checkPath, propToggles, pattem);
        }
        else
        {
            if (checkSetting == null) 
            {
                checkSetting = (ModelCheckSetting)AssetDatabase.LoadAssetAtPath<ModelCheckSetting>(toolPath + "/DefaultModelCheckSetting.asset");
                if (checkSetting == null)
                {
                    UnityEditor.EditorUtility.DisplayDialog("设置为空", "检查资源为空!", "确认", "取消");
                    return;
                }
            }
            results = new List<CheckResult>();
            foreach (var setting in checkSetting.settings)
            {
                if (!setting.checkThisItem) continue;
                checkPath = "Assets/" + setting.checkPath;
                propToggles = UpdataPropToggles(setting);
                if (!string.IsNullOrEmpty(setting.checkName)) pattem = @"\b\w*" + setting.checkName + @"\w*\b";
                else pattem = @"\b\w+\b";
                GetAllModelsPath(checkPath, propToggles, pattem);
            }
        }
    }
    // 导出数据
    [Button("导出结果", buttonSize : 24), GUIColor(0.5f, 1.1f, 0.5f, 1)]
    public void ExportResult()
    {
        Debug.Log("导出结果");
        if (results.Count == 0) 
        {
            UnityEditor.EditorUtility.DisplayDialog("无数据", "处理结果为空，或者未进行处理!", "确认", "取消");
            return;
        }
        FileStream txtFile = File.Open(toolPath + "/去除模型冗余数据" + DateTime.Now.ToFileTime() + ".txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
        StreamWriter sw = new StreamWriter(txtFile, Encoding.UTF8);
        sw.WriteLine("生成时间:" + DateTime.Now);
        sw.WriteLine("路径|去除属性编码");
        for (int i = 0; i < results.Count; i++)
        {
            int binaryOrOperationResult = 0;
            foreach (var binaryCode in results[i].redundanceCode)
            {
                binaryOrOperationResult = binaryOrOperationResult | binaryCode;
            }
            sw.WriteLine(results[i].modelPath + "|" + binaryOrOperationResult);
        }
        sw.WriteLine("public enum ModelProp { position = 1, normal = 2, tangent = 4, color = 8, uv1 = 16, uv2 = 32, uv3 = 64, uv4 = 128, }");
        sw.Close();
        txtFile.Close();
    }
    #endregion

    #region 常规属性
    private string checkPath;
    private string toolPath = "Assets/Plugins/Temp/AssetImport";
    private string pattem;
    private bool[] propToggles;
    #endregion

    #region 方法
    // 更新属性开关
    private bool[] UpdataPropToggles()
    {
        bool[] temp = new bool[8];
        temp[0] = isHavePosition;
        temp[1] = isHaveNormal;
        temp[2] = isHaveTangent;
        temp[3] = isHaveColor;
        temp[4] = isHaveUV1;
        temp[5] = isHaveUV2;
        temp[6] = isHaveUV3;
        temp[7] = isHaveUV4;
        return temp;
    }
    private bool[] UpdataPropToggles(ModelCheckSetting.Setting setting)
    {
        bool[] temp = new bool[8];
        temp[0] = setting.isHavePosition;
        temp[1] = setting.isHaveNormal;
        temp[2] = setting.isHaveTangent;
        temp[3] = setting.isHaveColor;
        temp[4] = setting.isHaveUV1;
        temp[5] = setting.isHaveUV2;
        temp[6] = setting.isHaveUV3;
        temp[7] = setting.isHaveUV4;
        return temp;
    }
    // 根据路径遍历
    private void GetAllModelsPath(string checkPath, bool[] toggles, string filter)
    {
        // 先遍历这个当前文件夹的所有文件，找到符合要求的文件
        foreach (var path in Directory.GetFiles(checkPath))
        {
            if (Regex.IsMatch(Path.GetFileNameWithoutExtension(path), filter) && Regex.IsMatch(Path.GetExtension(path), @"(.fbx|.FBX)"))
            {
                GetModelInfo(path, toggles);
            }
        }
        // 在遍历当前文件夹内的所有文件夹，并且递归调用当前
        if (Directory.GetDirectories(checkPath).Length > 0)
        {
            foreach (var path in Directory.GetDirectories(checkPath))
            {
                GetAllModelsPath(path, toggles, filter);
            }
        }
    }
    // 检查模型属性
    private void GetModelInfo(string path, bool[] toggles)
    {
        if (string.IsNullOrEmpty(path)) return;
        GameObject model = (GameObject)AssetDatabase.LoadAssetAtPath<GameObject>(path);
        MeshFilter[] meshFilters = model.GetComponentsInChildren<MeshFilter>();
        SkinnedMeshRenderer[] skinnedMeshRenderers = model.GetComponentsInChildren<SkinnedMeshRenderer>();
        CheckResult result = new CheckResult();
        result.modelName = model.name;
        result.modelObject = model;
        result.modelPath = path;
        result.filtersInfo = new List<string>();
        result.redundanceCode = new List<int>();
        bool isHaveRedundanceProp = false;
        string redundanceProp = "";
        int redundanceCode = 0;
        foreach (var meshFilter in meshFilters)
        {
            isHaveRedundanceProp = CheckMeshFilterProp(meshFilter.sharedMesh, toggles, out redundanceProp, out redundanceCode);
            if (isHaveRedundanceProp)
            {
                result.filtersInfo.Add(meshFilter.name + "\t\t" + redundanceProp + "\t\t" + Convert.ToString(redundanceCode, 2));
                result.redundanceCode.Add(redundanceCode);
            }
        }
        foreach (var skinnedMeshRenderer in skinnedMeshRenderers)
        {
            isHaveRedundanceProp = CheckMeshFilterProp(skinnedMeshRenderer.sharedMesh, toggles, out redundanceProp, out redundanceCode);
            if (isHaveRedundanceProp) 
            {
                result.filtersInfo.Add(skinnedMeshRenderer.name + "\t\t" + redundanceProp + "\t\t" + Convert.ToString(redundanceCode, 2));
                result.redundanceCode.Add(redundanceCode);
            }
        }
        if (isHaveRedundanceProp) results.Add(result);
    }
    // 检查网格是否含有多余属性
    private bool CheckMeshFilterProp(Mesh mesh, bool[] toggles, out string redundanceProp, out int deleteCode)
    {
        redundanceProp = "额外包含：";
        bool isHave = false;
        deleteCode = 0;
        if (!toggles[0] && mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.Position))
        {
            redundanceProp += "坐标; ";
            isHave = true;
            deleteCode += 1;
        }
        if (!toggles[1] && mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.Normal))
        {
            redundanceProp += "法线; ";
            isHave = true;
            deleteCode += 2;
        } 
        if (!toggles[2] && mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.Tangent)) 
        {
            redundanceProp += "切线; ";
            isHave = true;
            deleteCode += 4;
        }
        if (!toggles[3] && mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.Color)) 
        {
            redundanceProp += "颜色; ";
            isHave = true;
            deleteCode += 8;
        }
        if (!toggles[4] && mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.TexCoord0)) 
        {
            redundanceProp += "UV1; ";
            isHave = true;
            deleteCode += 16;
        }
        if (!toggles[5] && mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.TexCoord1)) 
        {
            redundanceProp += "UV2; ";
            isHave = true;
            deleteCode += 32;
        }
        if (!toggles[6] && mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.TexCoord2)) 
        {
            redundanceProp += "UV3; ";
            isHave = true;
            deleteCode += 64;
        }
        if (!toggles[7] && mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.TexCoord3)) 
        {
            redundanceProp += "UV4; ";
            isHave = true;
            deleteCode += 128;
        }
        // deleteCode += 256;
        return isHave;
    }
    #endregion
}
#endif