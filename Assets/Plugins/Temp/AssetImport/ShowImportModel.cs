using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

public class ShowImportModel : OdinEditorWindow
{
    private static ShowImportModel window;
    private string toolPath = "Assets/Plugins/Temp/AssetImport";
    private ModelCheckSetting modelCheckSetting;
    [MenuItem("Tools/检查模型属性")]
    private static void ShowWindow()
    {
        window = (ShowImportModel)EditorWindow.GetWindow(typeof(ShowImportModel), false, "检查模型冗余属性");
        window.minSize = new Vector2(800, 540);
        window.Show();
    }
    #region Odin属性
    public struct ModelInfo
    {
        [HideInEditorMode]
        public string modelName;
        [HorizontalGroup("物体", LabelWidth = 60)]
        [LabelText("模型信息")]
        public GameObject modelPath;
        [HorizontalGroup("网格", LabelWidth = 60)]
        [LabelText("网格信息")]
        [DisplayAsString]
        [ListDrawerSettings(NumberOfItemsPerPage = 5)]
        public List<string> filtersInfo;
    }

    [LabelText("查询结果")]
    [ListDrawerSettings(NumberOfItemsPerPage = 5)]
    [Searchable]
    public List<ModelInfo> models = new List<ModelInfo>();
    
    [Button(buttonSize : 24), GUIColor(0, 1.1f, 0.4f)]
    [LabelText("检查资源")]
    public void CheckAllModels()
    {
        InitAsset();
        for (int i = 0; i < modelCheckSetting.settings.Length; i++)
        {
            GetAllModelsPath(modelCheckSetting.settings[i].checkPath, ref modelCheckSetting.settings[i]);
        }
    }
    #endregion
    #region 方法
    // 初始化资源
    private void InitAsset()
    {
        modelCheckSetting = (ModelCheckSetting)AssetDatabase.LoadAssetAtPath<ModelCheckSetting>(toolPath + "/ModelCheck.asset");
    }
    // 遍历全部资源
    private void GetAllModelsPath(string filePath, ref ModelCheckSetting.Setting setting)
    {
        // 先遍历这个当前文件夹的所有文件，找到符合要求的文件
        foreach (var path in Directory.GetFiles(filePath))
        {
            if (Path.GetExtension(path) == ".fbx" || Path.GetExtension(path) == ".FBX")
            {
                GetModelInfo(path, setting);
            }
        }
        // 在遍历当前文件夹内的所有文件夹，并且递归调用当前
        if (Directory.GetDirectories(filePath).Length > 0)
        {
            foreach (var path in Directory.GetDirectories(filePath))
            {
                GetAllModelsPath(path, ref setting);
            }
        }
    }
    // 根据资源位置获取模型数据
    private void GetModelInfo(string path, ModelCheckSetting.Setting setting)
    {
        GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        MeshFilter[] filters = model.GetComponentsInChildren<MeshFilter>();
        ModelInfo modelInfo = new ModelInfo();
        modelInfo.modelName = model.name;
        modelInfo.modelPath = model;
        modelInfo.filtersInfo = new List<string>();
        bool isHave = true;
        string info = "";
        foreach (var filter in filters)
        {
            isHave = CheckMeshFilter(filter, setting, out info);
            if (isHave) modelInfo.filtersInfo.Add("网格：" + filter.name + "\t" + info);
        }
        if (isHave) models.Add(modelInfo);
    }
    // 检查MeshFilter是否符合要求
    private bool CheckMeshFilter(MeshFilter filter, ModelCheckSetting.Setting setting, out string info)
    {
        info = "额外有：";
        bool isHave = false;
        if (!setting.isHavePosition && filter.sharedMesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.Position))
        {
            info += "顶点位置; ";
            isHave = true;
        }
        if (!setting.isHaveNormal && filter.sharedMesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.Normal))
        {
            info += "法线; ";
            isHave = true;
        } 
        if (!setting.isHaveTangent && filter.sharedMesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.Tangent)) 
        {
            info += "切线; ";
            isHave = true;
        }
        if (!setting.isHaveColor && filter.sharedMesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.Color)) 
        {
            Debug.Log("执行");
            info += "顶点颜色; ";
            isHave = true;
        }
        if (!setting.isHaveUV1 && filter.sharedMesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.TexCoord0)) 
        {
            info += "UV1; ";
            isHave = true;
        }
        if (!setting.isHaveUV2 && filter.sharedMesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.TexCoord1)) 
        {
            info += "UV2; ";
            isHave = true;
        }
        if (!setting.isHaveUV3 && filter.sharedMesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.TexCoord2)) 
        {
            info += "UV3; ";
            isHave = true;
        }
        if (!setting.isHaveUV4 && filter.sharedMesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.TexCoord3)) 
        {
            info += "UV4; ";
            isHave = true;
        }
        return isHave;
    }
    #endregion
}
