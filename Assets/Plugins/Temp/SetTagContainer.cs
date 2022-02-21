#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

public class SetTagContainer : OdinEditorWindow
{
    private static SetTagContainer window;
    [MenuItem("Tools/SetTag容器")]
    private static void ShowWindow()
    {
        window = (SetTagContainer)EditorWindow.GetWindow(typeof(SetTagContainer), false, "SetTag");
        window.Show();
    }

    public struct PSInfo
    {
        [HorizontalGroup("layer01")]
        [LabelText("物体名称")]
        [LabelWidth(58)]
        public GameObject psGameObject;
        [HorizontalGroup("layer01", Width = 30)]
        [LabelText("是否处理")]
        [LabelWidth(58)]
        public bool isExcute;
    }
    [LabelText("处理列表")]
    public List<PSInfo> psInfors = new List<PSInfo>();

    [Button("查找")]
    public void FindObject()
    {
        var selectedGUIDs = Selection.assetGUIDs;
        psInfors = new List<PSInfo>();
        foreach (var selectedGUID in selectedGUIDs)
        {
            string folderPath = AssetDatabase.GUIDToAssetPath(selectedGUID);
            string[] filePaths = Directory.GetFiles(folderPath, "*.prefab");
            foreach (var filePath in filePaths)
            {
                if (Path.GetFileName(filePath).Contains("lv2"))
                {
                    GameObject lv2Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(filePath);
                    int psCount = lv2Prefab.GetComponentsInChildren<ParticleSystem>().Length;
                    if (psCount > 1)
                        psInfors.Add(new PSInfo(){ psGameObject = lv2Prefab, isExcute = false});
                }
            }
        }
    }
}
#endif