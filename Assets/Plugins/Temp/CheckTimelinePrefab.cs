#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

public class CheckTimelinePrefab : OdinEditorWindow
{
    public struct CheckResult
    {
        [LabelText("预制体")]
        [LabelWidth(60)]
        public GameObject prefab;
        [LabelText("网格信息")]
        [LabelWidth(60)]
        [DisplayAsString]
        public List<string> filterInfos;
    }
    private static CheckTimelinePrefab window;
    [MenuItem("Odin/查找丢失模型引用的预制体")]
    private static void ShowWindow()
    {
        window = (CheckTimelinePrefab)EditorWindow.GetWindow(typeof(CheckTimelinePrefab), false, "查找丢失模型引用的预制体");
        window.minSize = new Vector2(360, 200);
        window.Show();
    }

    [LabelText("位置")]
    [LabelWidth(60)]
    [FolderPath(AbsolutePath = false)]
    public string searchPath = "Assets";
    
    [LabelText("结果")]
    public List<CheckResult> results = new List<CheckResult>();

    [Button("处理")]
    public void Excute()
    {
        results = new List<CheckResult>();
        GetAllPrefab(searchPath);
    }

    #region 方法
    private void GetAllPrefab(string filePath)
    {
        // 先遍历这个当前文件夹的所有文件，找到符合要求的文件
        foreach (var path in Directory.GetFiles(filePath))
        {
            if (Path.GetExtension(path) == ".prefab")
            {
                // 处理
                CheckPrefab(path);
            }
        }
        // 在遍历当前文件夹内的所有文件夹，并且递归调用当前
        if (Directory.GetDirectories(filePath).Length > 0)
        {
            foreach (var path in Directory.GetDirectories(filePath))
            {
                GetAllPrefab(path);
            }
        }
    }
    // 处理预制体文件
    private void CheckPrefab(string path)
    {
        GameObject tempPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        var meshFilters = tempPrefab.GetComponentsInChildren<MeshFilter>();
        var skinnedMeshRenderers = tempPrefab.GetComponentsInChildren<SkinnedMeshRenderer>();
        CheckResult result = new CheckResult();
        result.prefab = tempPrefab;
        result.filterInfos = new List<string>();
        bool isPass = true;
        foreach (var meshFilter in meshFilters)
        {
            if (meshFilter.sharedMesh != null) continue;
            isPass = false;
            result.filterInfos.Add(meshFilter.name);
        }
        foreach (var skinnedMeshRenderer in skinnedMeshRenderers)
        {
            if (skinnedMeshRenderer.sharedMesh != null) continue;
            isPass = false;
            result.filterInfos.Add(skinnedMeshRenderer.name);
        }
        if (!isPass) results.Add(result);
    }
    #endregion
}
#endif