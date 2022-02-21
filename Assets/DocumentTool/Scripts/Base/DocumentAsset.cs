#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;

namespace TopJoy.Document
{
    [CreateAssetMenu(fileName = "文档", menuName = "文档/文档")]
    [Serializable]
    public class DocumentAsset : ScriptableObject
    {
        [HideInInspector]
        public bool isCollect;
        [HideInInspector]
        // TODO: 暂时先这样设置，未来从DocumentSetting中获得
        protected string toolRootPath = "Assets/DocumentTool";
        protected string documentRootPath = "Assets/DocumentTool/Documents";
        protected string recycleBinPath = "Assets/DocumentTool/Documents/回收站";
        protected string templateListPath = "Assets/DocumentTool/Scripts/EditorItem/TreeAssets/TemplateList.asset";
        // layer 01
        [HorizontalGroup("文档类型", Order = 0)]
        [LabelText("文档类型")]
        [LabelWidth(58)]
        public DocumentType documentType;
        [HorizontalGroup("文档类型", Order = 0)]
        [DisableIf("@documentType == DocumentType.节点")]
        [LabelText("所属部门")]
        [LabelWidth(58)]
        public Department department;
        [HorizontalGroup("文档类型", width : 122, Order = 0)]
        [Button("重置"), GUIColor(1.1f, 0.2f, 0.2f)]
        public void Reset() 
        {
            if (IsCanReset()) ResetAll();
        }
        // layer 02
        [HorizontalGroup("工具", Order = 1)]
        [ShowIf("documentType", DocumentType.工具使用)]
        [LabelText("工具类型")]
        [LabelWidth(58)]
        public ToolType toolType;
        [HorizontalGroup("工具", Order = 1)]
        [ShowIf("documentType", DocumentType.工具使用)]
        [LabelText("工具名称")]
        [LabelWidth(58)]
        public string toolName;
        [HorizontalGroup("工具", Width = 122, Order = 1)]
        [ShowIf("documentType", DocumentType.工具使用), GUIColor(0.7f, 0.7f, 1.5f)]
        [Button("尝试打开")]
        public void TryOpen()
        {
            if (!string.IsNullOrEmpty(toolName))
                EditorApplication.ExecuteMenuItem(toolName);
            else
                UnityEditor.EditorUtility.DisplayDialog("错误", "请填入工具名称!", "确定");
        }
        // layer03
        [HorizontalGroup("命名", Order = 2)]
        [LabelText("文档命名")]
        [LabelWidth(58)]
        public string documentName;
        [HorizontalGroup("命名", Order = 2)]
        [LabelText("网页地址")]
        [LabelWidth(58)]
        public string documentUrl;
        [HorizontalGroup("命名", Width = 122, Order = 2), GUIColor(0.7f, 0.7f, 1.5f)]
        [Button("尝试访问")]
        public void TryAccess()
        {
            if (!string.IsNullOrEmpty(documentUrl))
                Application.OpenURL(documentUrl);
            else
                UnityEditor.EditorUtility.DisplayDialog("无网站", "未设置访问网站!", "确定");
        }
        // layer04
        [HorizontalGroup("路径", Order = 3)]
        [LabelText("保存路径")]
        [LabelWidth(58)]
        [FolderPath(ParentFolder = "Assets/DocumentTool/Documents", AbsolutePath = false)]
        public string documentSavePath;
        [HorizontalGroup("段落", Order = 4)]
        [LabelText("段落编辑")]
        public List<Paragraph> paragraphs = new List<Paragraph>();
        // 是否能重置
        protected bool IsCanReset()
        {
            bool isReset = false;
            bool isEdited = !string.IsNullOrEmpty(documentName) || !string.IsNullOrEmpty(documentSavePath) || paragraphs.Count > 0;
            if (isEdited) isReset = UnityEditor.EditorUtility.DisplayDialog("重置", "是否重置当前文档?", "确认", "取消");
            if (isReset && isEdited) return true;
            return false;
        }
        // 是否能覆盖
        protected bool IsCanOverride()
        {
            bool isOverride = false;
            bool isEdited = !string.IsNullOrEmpty(documentName) || !string.IsNullOrEmpty(documentSavePath) || paragraphs.Count > 0;
            if (isEdited) isOverride = UnityEditor.EditorUtility.DisplayDialog("覆盖", "是否覆盖当前文档?", "确认", "取消");
            if (isOverride || !isEdited) return true;
            return false;
        }
        // 重置全部
        protected void ResetAll()
        {
            isCollect = false;
            documentType = DocumentType.基础文档;
            department = Department.全体;
            toolType = ToolType.基础功能;
            toolName = "";
            documentName = "";
            documentUrl = "";
            documentSavePath = "";
            paragraphs = new List<Paragraph>();
        }
        // 传输段落的数据
        protected void TransferContent(ref List<Paragraph> srcParagraphs, List<Paragraph> desParagraphs)
        {
            foreach (var paragraph in desParagraphs)
            {
                Paragraph tempParagraph = new Paragraph();
                tempParagraph.title = paragraph.title;
                tempParagraph.sentences = new List<Sentence>();
                foreach (var sentence in paragraph.sentences)
                {
                    Sentence tempSentence = new Sentence();
                    tempSentence.titleFormat = sentence.titleFormat;
                    tempSentence.contentFormat = sentence.contentFormat;
                    tempSentence.content = sentence.content;
                    tempSentence.copyContent = sentence.copyContent;
                    tempSentence.texPath = sentence.texPath;
                    tempSentence.fileUrl = sentence.fileUrl;
                    tempParagraph.sentences.Add(tempSentence);
                }
                srcParagraphs.Add(tempParagraph);
            }
        }
        // 创建资源
        protected void CreateAsset(string savePath, ref DocumentAsset asset)
        { 
            if (documentType != DocumentType.工具使用) toolType = ToolType.基础功能;
            asset.isCollect = false;
            asset.documentType = documentType;
            asset.department = department;
            asset.toolType = toolType;
            asset.toolName = toolName;
            asset.documentName = documentName;
            asset.documentUrl = documentUrl;
            asset.documentSavePath = documentSavePath;
            asset.paragraphs = paragraphs;
            AssetDatabase.CreateAsset(asset, savePath);
            AssetDatabase.SaveAssets();
        }
    }
}
#endif