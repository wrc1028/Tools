#if UNITY_EDITOR
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;

namespace TopJoy.Document
{
    [CreateAssetMenu(fileName = "DocumentSetting", menuName = "文档/编辑器资源/文档设置")]
    public class DocumentSetting : ScriptableObject
    {
        // ------------------------------------------------
        [TitleGroup("字体设置")]
        [HorizontalGroup("字体设置/title", LabelWidth = 60)]
        [LabelText("标题大小")]
        [Range(8, 28)]
        public int titleSize = 18;
        [HorizontalGroup("字体设置/title", LabelWidth = 60)]
        [LabelText("标题颜色")]
        public Color titleColor = Color.green;
        [HorizontalGroup("字体设置/content", LabelWidth = 60)]
        [LabelText("正文大小")]
        [Range(2, 22)]
        public int contentSize = 12;
        [HorizontalGroup("字体设置/content", LabelWidth = 60)]
        [LabelText("正文颜色")]
        public Color contentColor = Color.white;
        // ------------------------------------------------
        [TitleGroup("路径设置")]
        [HorizontalGroup("路径设置/01", LabelWidth = 80)]
        [FolderPath(AbsolutePath = false)]
        [LabelText("工具根目录")]
        public string toolRootPath;
        [HorizontalGroup("路径设置/02", LabelWidth = 80)]
        [FolderPath(AbsolutePath = false)]
        [LabelText("文档根目录")]
        public string documentRootPath;
        [HorizontalGroup("路径设置/03", LabelWidth = 80)]
        [FolderPath(AbsolutePath = false)]
        [LabelText("回收站路径")]
        public string recycleBinPath;
        [HorizontalGroup("路径设置/04", LabelWidth = 80)]
        [FolderPath(AbsolutePath = false)]
        [LabelText("收藏夹路径")]
        public string collectPath;
        [HorizontalGroup("路径设置/05", LabelWidth = 80)]
        [Sirenix.OdinInspector.FilePath(AbsolutePath = false)]
        [LabelText("模板列表路径")]
        public string templatListPath;
    }
}
#endif