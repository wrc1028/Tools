#if UNITY_EDITOR
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using Sirenix;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

public class AutoSetTag : OdinEditorWindow
{
    // filterStr 筛选的关键字
    public struct Filter
    {
        public string filterStr;
        public List<GameObject> prefabGOs;
    }
    private static AutoSetTag window;
    private List<string> fileNames;
    private List<Filter> filterResults;
    private string filterPattern;
    private string tagStr = "LowEffect";

    [MenuItem("Tools/自动设置Tag")]
    public static void ShowWindow()
    {
        window = (AutoSetTag)OdinEditorWindow.GetWindow(typeof(AutoSetTag), false, "自动设置Tag");
        window.Show();
    }

    [LabelText("处理路径")]
    [LabelWidth(58)]
    [FolderPath(AbsolutePath = false)]
    public string checkPath;

    [Button("处理")]
    public void Excute()
    {
        if (string.IsNullOrEmpty(checkPath)) return;
        filterPattern = @"e_\w+_([a-zA-Z]+)_\w+";
        fileNames = new List<string>();
        GetAllEffectPrefab(checkPath, ref fileNames);
    }

    private void GetAllEffectPrefab(string rootPath, ref List<string> result)
    {
        if (Directory.GetFiles(rootPath, "*.prefab").Length > 0)
        {
            string[] prefabs = Directory.GetFiles(rootPath, "*.prefab");
            foreach (var prefab in prefabs)
            {
                GameObject tempPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefab);
                result.Add(prefab);
                FilterPrefab(tempPrefab);
            }
        }
        if (Directory.GetDirectories(rootPath).Length > 0)
        {
            foreach (var folderPath in Directory.GetDirectories(rootPath))
            {
                GetAllEffectPrefab(folderPath, ref result);
            }
        }
    }

    private void FilterPrefab(GameObject prefab)
    {
        // 获得带有粒子系统的物体
        var particleObject = prefab.GetComponentsInChildren<ParticleSystem>();
        if (particleObject.Length < 4) return;
        GameObject instantiatedPrefab = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        int childCount = instantiatedPrefab.transform.childCount;
        // 用脚本内的引用代替
        for (int i = 0; i < childCount; i++)
        {
            if (!instantiatedPrefab.transform.GetChild(i).name.Contains("_v2")) continue;
            filterResults = new List<Filter>();
            // 将子物体分类
            foreach (var lv2ParticleObject in instantiatedPrefab.transform.GetChild(i).GetComponentsInChildren<ParticleSystem>())
            {
                string filterStr = Regex.Match(lv2ParticleObject.transform.name, filterPattern).Groups[1].Value;
                int countFilterNum = 0;
                foreach (var filterResult in filterResults)
                {
                    if (filterResult.filterStr == filterStr)
                    {
                        filterResult.prefabGOs.Add(lv2ParticleObject.gameObject);
                        break;
                    }
                    countFilterNum ++;
                }
                if (countFilterNum == filterResults.Count)
                {
                    Filter tempResult = new Filter();
                    tempResult.filterStr = filterStr;
                    tempResult.prefabGOs = new List<GameObject>();
                    tempResult.prefabGOs.Add(lv2ParticleObject.gameObject);
                    filterResults.Add(tempResult);
                }
            }
            foreach (var filterResult in filterResults)
            {
                // 对物体进行处理
                if (filterResult.prefabGOs.Count == 1 && Random.value > 0.6)
                    filterResult.prefabGOs[0].tag = tagStr;
                else
                {
                    int index = 0;
                    foreach (var prefabGO in filterResult.prefabGOs)
                    {
                        if (index % 2 == 1) prefabGO.tag = tagStr;
                    }
                }
            }
            PrefabUtility.ApplyPrefabInstance(instantiatedPrefab, InteractionMode.AutomatedAction);
            DestroyImmediate(instantiatedPrefab);
            AssetDatabase.SaveAssets();
        }
    }

}
#endif