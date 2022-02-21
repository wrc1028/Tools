#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;

namespace TopJoy.Document
{
    [CreateAssetMenu(fileName = "列表", menuName = "文档/编辑器资源/列表")]
    [Serializable]
    public class DocumentList : ScriptableObject
    {
        [LabelText("基础文档")]
        public List<DocumentAsset> baseDocuments;
        [LabelText("资源规范")]
        public List<DocumentAsset> regularDocuments;
        [LabelText("生产流程")]
        public List<DocumentAsset> processDocuments;
        [LabelText("工具使用")]
        public List<DocumentAsset> toolDocuments;
        [LabelText("其他")]
        public List<DocumentAsset> otherDocuments;

        [LabelText("待添加资源")]
        [LabelWidth(70)]
        public DocumentAsset addDocument;

        [Button("添加模板", ButtonSizes.Medium), GUIColor(0, 1.2f, 0,2f)]
        public void AddTemplate()
        {
            if (addDocument == null)
            {
                UnityEditor.EditorUtility.DisplayDialog("警告", "待添加的模板为空!", "确定");
                return;
            }
            switch (addDocument.documentType)
            {
                case DocumentType.基础文档:
                    baseDocuments = UpdateTemplateList(baseDocuments, addDocument);
                    break;
                case DocumentType.资源规范:
                    regularDocuments = UpdateTemplateList(regularDocuments, addDocument);
                    break;
                case DocumentType.生产流程:
                    processDocuments = UpdateTemplateList(processDocuments, addDocument);
                    break;
                case DocumentType.工具使用:
                    toolDocuments = UpdateTemplateList(toolDocuments, addDocument);
                    break;
                default:
                    otherDocuments = UpdateTemplateList(otherDocuments, addDocument);
                    break;
            }
        }
        // 更新模板
        private List<DocumentAsset> UpdateTemplateList(List<DocumentAsset> originTemplateList, DocumentAsset addTemplate)
        {
            List<DocumentAsset> tempTemplateList = new List<DocumentAsset>();
            bool isAddOriginTemplate = true;
            int originCount = 0;
            foreach (var originTemplate in originTemplateList)
            {
                if (addTemplate.department == originTemplate.department && addTemplate.toolType == originTemplate.toolType)
                    isAddOriginTemplate = false;
                else
                {
                    isAddOriginTemplate = true;
                    originCount ++;
                }

                if (!isAddOriginTemplate)
                {
                    if (UnityEditor.EditorUtility.DisplayDialog("警告", "已存在当前类型的资源，是否覆盖!", "确定", "取消"))
                        tempTemplateList.Add(addTemplate);
                    else tempTemplateList.Add(originTemplate);
                }
                else tempTemplateList.Add(originTemplate);
            }
            if (originCount == originTemplateList.Count) tempTemplateList.Add(addTemplate);
            originTemplateList.Clear();
            return tempTemplateList;
        }
    }
}
#endif