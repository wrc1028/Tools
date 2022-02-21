#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "Check", menuName = "Setting/ModelCheck")]
public class ModelCheckSetting : ScriptableObject
{
    [LabelText("检查设置")]
    public Setting[] settings;
    [System.Serializable]
    public class Setting
    {
        [HorizontalGroup("开关", LabelWidth = 80)]
        [LabelText("检查当前项")]
        public bool checkThisItem = false;
        [HorizontalGroup("检查路径", LabelWidth = 80)]
        [FolderPath(ParentFolder = "Assets", AbsolutePath = false)]
        [LabelText("检查路径")]
        public string checkPath;
        [HorizontalGroup("关键字", LabelWidth = 80)]
        [LabelText("关键字")]
        public string checkName = "";
        [BoxGroup("属性", false)]
        [HorizontalGroup("属性/1", LabelWidth = 40)]
        [LabelText("坐标")]
        public bool isHavePosition = true;
        [HorizontalGroup("属性/1", LabelWidth = 40)]
        [LabelText("法线")]
        public bool isHaveNormal = true;
        [HorizontalGroup("属性/1", LabelWidth = 40)]
        [LabelText("切线")]
        public bool isHaveTangent = true;
        [HorizontalGroup("属性/1", LabelWidth = 40)]
        [LabelText("颜色")]
        public bool isHaveColor;
        [HorizontalGroup("属性/2", LabelWidth = 40)]
        [LabelText("UV1")]
        public bool isHaveUV1 = true;
        [HorizontalGroup("属性/2", LabelWidth = 40)]
        [LabelText("UV2")]
        public bool isHaveUV2;
        [HorizontalGroup("属性/2", LabelWidth = 40)]
        [LabelText("UV3")]
        public bool isHaveUV3;
        [HorizontalGroup("属性/2", LabelWidth = 40)]
        [LabelText("UV4")]
        public bool isHaveUV4;
    }
}
#endif