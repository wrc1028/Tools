using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

public class FileClassify : OdinEditorWindow
{
    public enum AddType
    {
        前缀, 后缀,
    }
    public static FileClassify ToolWindow;

    [MenuItem("Odin/文件归类")]
    public static void ShowWindow()
    {
        ToolWindow = (FileClassify)EditorWindow.GetWindow(typeof(FileClassify), false, "文件归类");
        ToolWindow.minSize = new Vector2(360, 240);
        ToolWindow.Show();
    }
    
    #region 界面
    [BoxGroup("选项", false)]
    [HorizontalGroup("选项/row01")]
    [ToggleLeft]
    [LabelText("开发选项")]
    public bool isDev = false;

    [HorizontalGroup("选项/row01")]
    [ToggleLeft]
    [LabelText("修改名称")]
    public bool isModifyName = false;
    // 开发选项----------
    [ShowIf("isDev")]
    [BoxGroup("Dev", false)]
    [InfoBox("默认支持以下四种资源的归类：材质、模型、预制体、贴图，如果有新的归类需求，点击右边的加号进行添加")]
    [LabelText("文件类型")]
    public FileType[] addFileType = new FileType[0];

    public struct FileType
    {
        [HorizontalGroup("文件", LabelWidth = 65)]
        [LabelText("文件夹名称")]
        public string folderName;
        [HorizontalGroup("文件")]
        [LabelText("正则表达式")]
        public string regexPattem;
    }

    // 修改移动文件名称---
    [ShowIf("isModifyName")]
    [InfoBox("仅支持在原来名称的基础上添加前缀和后缀，如果需要改名，请使用TA工具中的文件处理")]

    [ShowIf("isModifyName")]
    [BoxGroup("Add", false)]
    [HorizontalGroup("Add/Type", LabelWidth = 50)]
    [LabelText("添加方式")]
    public AddType addType = AddType.前缀;

    [ShowIf("isModifyName")]
    [HorizontalGroup("Add/Content", LabelWidth = 50)]
    [LabelText("添加内容")]
    public string addString = "";

    // 开始归类
    [Button("归类", buttonSize : 25), GUIColor(0.7f, 1.25f, 0.7f)]
    public void Excude()
    {
        // 选中的文件夹
        var selectFolders = Selection.assetGUIDs;
        foreach (var folder in selectFolders)
        {
            GetAllFilesFromPath(AssetDatabase.GUIDToAssetPath(folder));
        }
        AssetDatabase.Refresh();
    }
    #endregion
    
    #region 逻辑

    // 获得文件并进行归类
    private void GetAllFilesFromPath(string _folderPath)
    {
        string[] filesPath = Directory.GetFiles(_folderPath);
        foreach (var path in filesPath)
        {
            // Debug.Log(Regex.IsMatch(Path.GetFileName(path), "\\w+.asset$"));
            switch (Path.GetExtension(path))
            {
                // case ".asset":
                //     MoveToFolder(path, _folderPath, "Asset");
                //     break;
                case ".mat":
                    MoveToFolder(path, _folderPath, "Material");
                    break;
                case ".OBJ": case ".obj": case ".FBX": case ".fbx":
                    MoveToFolder(path, _folderPath, "Model");
                    break;
                case ".prefab":
                    MoveToFolder(path, _folderPath, "Prefab");
                    break;
                case ".PNG": case ".png": case ".JPG": case ".jpg": case ".TGA": case ".tga":
                    MoveToFolder(path, _folderPath, "Texture");
                    break;
                default:
                    // 开启自定义
                    if (isDev)
                        foreach (var fileType in addFileType)
                        {
                            if (Regex.IsMatch(Path.GetFileName(path), fileType.regexPattem))
                                MoveToFolder(path, _folderPath, fileType.folderName);
                        }
                    break;
            }
            
            
        }
    }

    // 归类
    private void MoveToFolder(string _filePath, string _folderPath, string _fileType)
    {
        string targetFolder = string.Format("{0}/{1}", _folderPath, _fileType);
        string fileName = isModifyName ? AddPrefixOrSuffix(Path.GetFileNameWithoutExtension(_filePath))
            +  Path.GetExtension(_filePath) : Path.GetFileName(_filePath);
        string newPath = string.Format("{0}/{1}", targetFolder, fileName);
        if (!Directory.Exists(targetFolder))
        {
            Directory.CreateDirectory(targetFolder);
            AssetDatabase.Refresh();
        }
        AssetDatabase.MoveAsset(_filePath, newPath);
    }
    // 添加前缀或者后缀
    private string AddPrefixOrSuffix(string _fileName)
    {
        return addType == AddType.前缀 ? (addString + _fileName) : (_fileName + addString);
    }
    #endregion
}
