using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

public class AlternateVolumeFile : OdinEditorWindow
{
    private static EditorWindow toolWindow;
    private List<GameObject> prefabGOs;
    [MenuItem("Tools/后处理替换回高配文件")]
    public static void ShowWindow()
    {
        toolWindow = (AlternateVolumeFile)EditorWindow.GetWindow(typeof(AlternateVolumeFile), false, "更换", false);
        toolWindow.minSize = new Vector2(360, 240);
        toolWindow.Show();
    }
    
    [FolderPath(ParentFolder = "Assets")]
    [HorizontalGroup("", LabelWidth = 65)]
    [LabelText("文件夹路径")]
    public string srcPath = "Prefabs";
    
    [Button(ButtonSizes.Medium)]
    public void Excute()
    {
        prefabGOs = new List<GameObject>();
        GetAllPrefabs("Assets/" + srcPath);
        ShowPrefabName();
    }
    private void GetAllPrefabs(string _folderPath)
    {
        foreach (var path in Directory.GetFiles(_folderPath))
        {
            if (Path.GetExtension(path) == ".prefab")
            {
                // 处理
                SaveAllPrefab(path);
            }
        }
        if (Directory.GetDirectories(_folderPath).Length > 0)
        {
            foreach (var path in Directory.GetDirectories(_folderPath))
            {
                GetAllPrefabs(path);
            }
        }
    }
    private void SaveAllPrefab(string _filePath)
    {
        // 填充要处理的预制体
        var go = (GameObject)AssetDatabase.LoadAssetAtPath(_filePath, typeof(GameObject)) as GameObject;
        prefabGOs.Add(go);
    }
    private void ShowPrefabName()
    {
        foreach (var item in prefabGOs)
        {
            Debug.Log(item.name);
        }
    }
    private void Alternate()
    {
        for (int i = 0; i < prefabGOs.Count; i++)
        {
            // 如果预制体的名字内包含lv1
            if (Regex.IsMatch(prefabGOs[i].name, "lv1"))
            {
                // ShaderConfig[] sc = prefabGOs[i].GetComponentsInChildren<ShaderConfig>();
            }
        }
    }
}
