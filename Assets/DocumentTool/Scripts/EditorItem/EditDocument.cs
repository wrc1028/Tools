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
    [CreateAssetMenu(fileName = "EditDocument", menuName = "文档/编辑器资源/编辑文档")]
    public class EditDocument : DocumentAsset
    {
        [HorizontalGroup("编辑文档", LabelWidth = 60, Order = -1)]
        [LabelText("目标文档")]
        public DocumentAsset targetAsset;
        [HorizontalGroup("编辑文档", Width = 122, Order = -1)]
        [Button("确认编辑")]
        public void EditTargetDocument()
        {
            if (targetAsset == null)
            {
                UnityEditor.EditorUtility.DisplayDialog("警告", "请选择需要编辑的文档资源!", "确认");
                return;
            }
            documentType = targetAsset.documentType;
            department = targetAsset.department;
            toolType = targetAsset.toolType;
            toolName = targetAsset.toolName;
            documentName = targetAsset.documentName;
            documentUrl = targetAsset.documentUrl;
            documentSavePath = targetAsset.documentSavePath;
            paragraphs = new List<Paragraph>();
            TransferContent(ref paragraphs, targetAsset.paragraphs);
        }
        
        [HorizontalGroup("保存", Order = 5)]
        [Button("保存修改"), GUIColor(0.8f, 0.8f, 1.5f)]
        public void SaveDocument()
        {
            // 直接删除原内容，将原内容备份加上时间引动到回收站
            string oldPath = documentRootPath + "/" + targetAsset.documentSavePath + "/" + targetAsset.documentName + ".asset";
            string newPath = documentRootPath + "/" + documentSavePath + "/" + documentName + ".asset";
            string oldBackupPath = documentRootPath + "/" + targetAsset.documentSavePath + "/." + targetAsset.documentName + ".txt";
            string newBackupPath = documentRootPath + "/" + documentSavePath + "/." + documentName + ".txt";
            if (string.Compare(oldPath, newPath) != 0 && File.Exists(newPath))
            {
                UnityEditor.EditorUtility.DisplayDialog("警告", "当前文件已存在，请重新命名或更改保存路径!", "确认", "取消");
                return;
            }
            AssetDatabase.DeleteAsset(oldPath);
            DocumentAsset asset = new DocumentAsset();
            CreateAsset(newPath, ref asset);
            // TODO: 将备份移动到回收站;优化:将备份文件的相对位置改成绝对位置
            string deleteTime = DateTime.Now.ToString("yyyy-MM-dd hh mm ss");
            string recyclePath = recycleBinPath + "/." + targetAsset.documentName + deleteTime + ".txt";
            if (File.Exists(oldBackupPath))
            {
                if (string.Compare(oldBackupPath, newBackupPath) != 0) File.Move(oldBackupPath, recyclePath);
                else File.Delete(oldBackupPath);
            }
            CommonPropAndMethod.CreateAssetBackup(asset, newBackupPath);
            ResetAll();
        }
    }
}
#endif