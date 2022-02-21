#if UNITY_EDITOR
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using System.Linq;

namespace TopJoy.Document
{
    [CreateAssetMenu(fileName = "CreateDocument", menuName = "文档/编辑器资源/新建文档")]
    public class CreateDocument : DocumentAsset
    {
        [HorizontalGroup("指定模板", LabelWidth = 60, Order = -1)]
        [DisableIf("@documentType == DocumentType.节点")]
        [LabelText("指定模板")]
        public DocumentAsset targetTemplate;
        
        [HorizontalGroup("指定模板", Width = 122, Order = -1)]
        [DisableIf("@documentType == DocumentType.节点")]
        [Button("应用模板")]
        public void AffirmTemplate()
        {
            // TODO: 优化
            // 应用模板，如果指定模板不为空，则应用模板容器内的模板
            if (targetTemplate == null)
            {
                DocumentList documentTemplateList = AssetDatabase.LoadAssetAtPath<DocumentList>(templateListPath);
                switch (documentType)
                {
                    case DocumentType.基础文档:
                        ApplyTemplate(documentTemplateList.baseDocuments);
                        break;
                    case DocumentType.资源规范:
                        ApplyTemplate(documentTemplateList.regularDocuments);
                        break;
                    case DocumentType.生产流程:
                        ApplyTemplate(documentTemplateList.processDocuments);
                        break;
                    case DocumentType.工具使用:
                        ApplyTemplate(documentTemplateList.toolDocuments);
                        break;
                    default:
                        ApplyTemplate(documentTemplateList.otherDocuments);
                        break;
                }
            }
            else TransferAttribute(targetTemplate);
        }
        private void ApplyTemplate(List<DocumentAsset> templateList)
        {
            bool isHaveTemplate = false;
            foreach (var template in templateList)
            {
                if (department == template.department && toolType == template.toolType)
                {
                    TransferAttribute(template);
                    isHaveTemplate = true;
                    break;
                }
            }
            if (!isHaveTemplate)
            {
                UnityEditor.EditorUtility.DisplayDialog("提示", "没有当前选项的模板!", "确定");
                // TODO: 重置的必要
                ResetAll();
            }
        }
        // 传输当前数据
        private void TransferAttribute(DocumentAsset template)
        {
            if (!IsCanOverride()) return;
            toolType = template.toolType;
            toolName = template.toolName;
            documentName = template.documentName;
            documentUrl = template.documentUrl;
            documentSavePath = "";
            paragraphs = new List<Paragraph>();
            TransferContent(ref paragraphs, template.paragraphs);
        }
        // 创建文档
        [HorizontalGroup("创建按钮", Order = 5)]
        [Button("创建文档"), GUIColor(0, 1.2f, 0.2f)]
        public void CreateNewDocument()
        {
            bool isEdited = !string.IsNullOrEmpty(documentName) || !string.IsNullOrEmpty(documentSavePath) || paragraphs.Count > 0;
            if (!isEdited) 
            {
                UnityEditor.EditorUtility.DisplayDialog("创建失败", "当前文档尚未编辑(命名、地址或内容为空)!", "确认");
                return;
            }
            string savePath = documentRootPath + "/" + documentSavePath + "/" + documentName + ".asset";
            string backupSavePath = documentRootPath + "/" + documentSavePath + "/." + documentName + ".txt";
            if (File.Exists(savePath)) 
            {
                UnityEditor.EditorUtility.DisplayDialog("文件存在", "当前文件已存在，请重新命名或更改保存路径!", "确认", "取消");
                return;
            }
            DocumentAsset asset = new DocumentAsset();
            CreateAsset(savePath, ref asset);
            CommonPropAndMethod.CreateAssetBackup(asset, backupSavePath);
            ResetAll();
        }
    }
}
#endif