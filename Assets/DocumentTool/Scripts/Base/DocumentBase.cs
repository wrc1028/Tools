#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;

namespace TopJoy.Document
{
    #region 枚举
    public enum DocumentType { 基础文档, 资源规范, 生产流程, 工具使用, 其他, 节点, }
    public enum Department { 全体, 角色, 动作, 场景, 特效, 原画, TA, UI, 视听, 其他, }
    public enum ToolType { 基础功能, 资源校验, 文件处理, 其他, }
    
    public enum ContentFormat { 普通, 复制, 图片, 启动, }
    public enum TitleFormat { 一级, 二级, 三级, 顶格, }
    #endregion

    #region 结构体
    // 单条内容的排版(格式)
    [Serializable]
    public struct Sentence
    {
        [HorizontalGroup("layer01", Width = 46)]
        [HideLabel]
        public ContentFormat contentFormat;

        [HorizontalGroup("layer01", Width = 46)]
        [HideLabel]
        public TitleFormat titleFormat;

        [HorizontalGroup("layer01")]
        [HideLabel]
        public string content;

        [ShowIf("contentFormat", ContentFormat.复制)]
        [HorizontalGroup("layer02", LabelWidth = 96)]
        [LabelText("复制内容")]
        public string copyContent;

        [ShowIf("contentFormat", ContentFormat.图片)]
        [Sirenix.OdinInspector.FilePath(AbsolutePath = false)]
        [HorizontalGroup("layer03", LabelWidth = 96)]
        [LabelText("图片地址")]
        public string texPath;

        [ShowIf("contentFormat", ContentFormat.启动)]
        [HorizontalGroup("layer04", LabelWidth = 96)]
        [LabelText("文件(或网页)地址")]
        public string fileUrl;
    }
    
    // 段落的排版(格式)
    [Serializable]
    public class Paragraph
    {
        [LabelText("标题"), GUIColor(0.5f, 1.1f, 0.5f)]
        [LabelWidth(34)]
        public string title;
        
        [LabelText("内容")]
        public List<Sentence> sentences;
    }
    // 一些通用(常用)的熟悉和方法
    public static class CommonPropAndMethod
    {
        public static string toolSettingPath = "Assets/DocumentTool/Scripts/EditorItem/TreeAssets/DocumentSetting.asset";
        public static DocumentSetting Setting 
        {
            get
            {
                return AssetDatabase.LoadAssetAtPath<DocumentSetting>(toolSettingPath);
            }
        }
        // 创建备份资源
        public static void CreateAssetBackup(DocumentAsset asset, string savePath)
        {
            if (File.Exists(savePath)) File.Delete(savePath);
            FileStream assetBackup = File.Open(savePath, FileMode.CreateNew, FileAccess.ReadWrite);
            StreamWriter sw = new StreamWriter(assetBackup, Encoding.UTF8);
            sw.WriteLine("<DocumentType>" + (int)asset.documentType + "</DocumentType>");
            sw.WriteLine("<Department>" + (int)asset.department + "</Department>");
            sw.WriteLine("<ToolType>" + (int)asset.toolType + "</ToolType>");
            sw.WriteLine("<ToolName>" + asset.toolName + "</ToolName>");
            sw.WriteLine("<DocumentName>" + asset.documentName + "</DocumentName>");
            sw.WriteLine("<DocumentUrl>" + asset.documentUrl + "</DocumentUrl>");
            sw.WriteLine("<DocumentSavePath>" + asset.documentSavePath + "</DocumentSavePath>");
            foreach (var paragraph in asset.paragraphs)
            {
                sw.WriteLine("\n<Title>" + paragraph.title + "</Title>");
                foreach (var sentence in paragraph.sentences)
                {
                    sw.WriteLine("<ContentFormat>" + (int)sentence.contentFormat + "</ContentFormat>" + 
                        "<TitleFormat>" + (int)sentence.titleFormat + "</TitleFormat>" + 
                        "<Content>" + sentence.content + "</Content>" + 
                        "<Copy>" + sentence.copyContent + "</Copy>" + 
                        "<TexPath>" + sentence.texPath + "</TexPath>" + 
                        "<FilePath>" + sentence.fileUrl + "</FilePath>");
                }
            }
            sw.Close();
            assetBackup.Close();
        }
        // 由资源路径转换成备份路径
        public static void TransformAssetPathToBackupPath(string assetPath, out string backupPath)
        {
            string assetName = Path.GetFileNameWithoutExtension(assetPath);
            backupPath = Regex.Replace(assetPath, string.Format("{0}.asset", assetName), string.Format(".{0}.txt", assetName));
        }
        // 由备份路径转换成资源路径
        public static void TransformBackupPathToAssetPath(string backupPath, out string assetPath)
        {
            string backupName = Path.GetFileNameWithoutExtension(backupPath);
            assetPath = Regex.Replace(backupPath, string.Format("{0}.asset", backupName), string.Format(".{0}.txt", backupName));;
        }
    }
    #endregion
}
#endif