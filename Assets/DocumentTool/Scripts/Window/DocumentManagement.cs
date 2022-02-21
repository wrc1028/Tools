#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
namespace TopJoy.Document
{
    public class TestRegex
    {
        [LabelText("输入字符")]
        [LabelWidth(58)]
        public string inputStr;
        [Button("处理")]
        public void Excude()
        {
            string outStr = String.Empty;
            CommonPropAndMethod.TransformAssetPathToBackupPath(inputStr, out outStr);
            Debug.Log(outStr);
        }
    }
    public enum CheckTarget
    {
        指定资源, 指定路径,
    }
    public class DocumentManagement
    {
        // 一些常用的方法
        protected void GetAllFile(string rootPath, ref List<string> allAssetPaths, ref List<string> allTextPaths)
        {
           foreach (var filePath in Directory.GetFiles(rootPath))
           {
                if (string.Compare(Path.GetExtension(filePath), ".meta") == 0) continue;
                if (string.Compare(Path.GetExtension(filePath), ".asset") == 0) allAssetPaths.Add(filePath);
                if (string.Compare(Path.GetExtension(filePath), ".txt") == 0) allTextPaths.Add(filePath);
           }
           if (Directory.GetDirectories(rootPath).Length > 0)
           {
               foreach (var folderPath in Directory.GetDirectories(rootPath))
               {
                   GetAllFile(folderPath, ref allAssetPaths, ref allTextPaths);
               }
           }
        }
    }
    public enum RevertTarget
    {
        文本文件, 文档资源, 
    }
    public class DocumentChcek : DocumentManagement
    {
        private DocumentSetting setting;
        [LabelText("校验形式")]
        [LabelWidth(58)]
        public CheckTarget checkTarget = CheckTarget.指定路径;
        
        [ShowIf("@checkTarget == CheckTarget.指定资源")]
        [LabelText("资源路径")]
        [LabelWidth(58)]
        public DocumentAsset targetAssetPath;

        [ShowIf("@checkTarget == CheckTarget.指定路径")]
        [FolderPath(AbsolutePath = false)]
        [LabelText("校验路径")]
        [LabelWidth(58)]
        public string targetPath;

        [Button("校对")]
        public void CheckDocument()
        {
            setting = AssetDatabase.LoadAssetAtPath<DocumentSetting>(CommonPropAndMethod.toolSettingPath);
            if (checkTarget == CheckTarget.指定资源 && targetAssetPath != null)
            {
                CheckDocument(targetAssetPath);
            }
            else if (checkTarget == CheckTarget.指定路径 && !string.IsNullOrEmpty(targetPath))
            {
                // 遍历当前文件夹下的全部资源，将多余的资源备份移送到回收站
                List<string> allAssetPaths = new List<string>();
                List<string> allTextPaths = new List<string>();
                GetAllFile(targetPath, ref allAssetPaths, ref allTextPaths);
                foreach (var path in allAssetPaths)
                {
                    DocumentAsset asset = AssetDatabase.LoadAssetAtPath<DocumentAsset>(path) as DocumentAsset;
                    CheckDocument(asset);
                }
                foreach (var path in allTextPaths)
                {
                    string assetPath = Regex.Replace(path, @"\\\.", "\\");
                    assetPath = assetPath.Remove(assetPath.Length - 4) + ".asset";
                    if (assetPath.Contains("回收站")) continue;
                    if(!File.Exists(assetPath))
                    {
                        // 移动到回收站
                        // File.Delete(path);
                        string deleteTime = DateTime.Now.ToString("yyyy-MM-dd hh mm ss");
                        string redundantFileName = Path.GetFileNameWithoutExtension(path).Substring(1);
                        string recyclePath = setting.recycleBinPath + "/" + redundantFileName + deleteTime + ".txt";
                        File.Move(path, recyclePath);
                        Debug.Log(string.Format("已经将多余备份文件\"{0}\"移动到回收站!", redundantFileName));
                    }
                }
            }
            else UnityEditor.EditorUtility.DisplayDialog("警告", "校验的资源或文件夹路径为空!", "确定");
            AssetDatabase.Refresh();
        }
        // 检验文档资源：将文档资源内的文档名和保存路径修改成当前文件的名称和路径(绝对)，并修改文档备份
        // 如果文档资源没有备份，就新建一个
        private void CheckDocument(DocumentAsset asset)
        {
            string assetPath = AssetDatabase.GetAssetPath(asset);
            if (Regex.IsMatch(assetPath, @".*收藏.*"))
            {
                Debug.Log(string.Format("无法对收藏内的文件\"{0}\"进行操作", asset.name));
                return;
            }
            string assetName = Path.GetFileNameWithoutExtension(assetPath);
            assetPath = assetPath.Substring(setting.documentRootPath.Length + 1);
            assetPath = assetPath.Remove(assetPath.Length - (assetName.Length + 7));
            if (asset.documentName != assetName || asset.documentSavePath != assetPath)
            {
                asset.documentName = assetName;
                asset.documentSavePath = assetPath;
            }
            string backupAssetPath = setting.documentRootPath + "/" + assetPath + "/." + assetName + ".txt";
            if (File.Exists(backupAssetPath) && (asset.documentName != assetName || asset.documentSavePath != assetPath))
            {
                // TODO: 
                StreamReader sr = new StreamReader(backupAssetPath, Encoding.UTF8);
                string replaceNamePatterm = @"<DocumentName>(.+)</DocumentName>";
                string replaceName = string.Format("<DocumentName>{0}</DocumentName>", assetName);
                string replacePathPatterm = @"<DocumentPath>(.+)</DocumentPath>";
                string replacePath = string.Format("<DocumentPath>{0}</DocumentPath>", assetPath);
                string result = Regex.Replace(sr.ReadToEnd(), replaceNamePatterm, replaceName);
                result = Regex.Replace(result, replacePathPatterm, replacePath);
                sr.Close();
                FileStream assetBackup = new FileStream(backupAssetPath, FileMode.Open);
                assetBackup.Flush();
                StreamWriter sw = new StreamWriter(assetBackup, Encoding.UTF8);
                sw.Write(result);
                sw.Close();     
                assetBackup.Close();
            }
            else if (!File.Exists(backupAssetPath))
            {
                CommonPropAndMethod.CreateAssetBackup(asset, backupAssetPath);
                Debug.Log(string.Format("未找到当前文档的备份，已自动创建\"{0}\" 的备份", asset.documentName));
            }
            else Debug.Log(string.Format("文档: {0} 资源正常", asset.documentName));
        }
    }
    public class DocumentRevert : DocumentManagement
    {
        private DocumentSetting setting;
        // 用于恢复文档资源
        [HorizontalGroup("layer01", LabelWidth = 58)]
        [LabelText("还原格式")]
        public RevertTarget revertTarget = RevertTarget.文档资源;
        
        [HorizontalGroup("layer01", LabelWidth = 58)]
        [LabelText("校验形式")]
        public CheckTarget checkTarget = CheckTarget.指定资源;

        [ShowIf("@checkTarget == CheckTarget.指定资源 && revertTarget == RevertTarget.文本文件")]
        [Sirenix.OdinInspector.FilePath(AbsolutePath = false, Extensions = ".txt")]
        [LabelText("文本路径")]
        [LabelWidth(58)]
        public string revertTextPath;

        [ShowIf("@checkTarget == CheckTarget.指定资源 && revertTarget == RevertTarget.文档资源")]
        [LabelText("文档资源")]
        [LabelWidth(58)]
        public DocumentAsset revertAsset;

        [ShowIf("@checkTarget == CheckTarget.指定路径")]
        [FolderPath(AbsolutePath = false)]
        [LabelText("恢复路径")]
        [LabelWidth(58)]
        public string revertPath;

        [Button("复原")]
        public void RevertDocument()
        {
            setting = AssetDatabase.LoadAssetAtPath<DocumentSetting>(CommonPropAndMethod.toolSettingPath);
            if (checkTarget == CheckTarget.指定资源)
            {
                if (revertTarget == RevertTarget.文本文件 && !string.IsNullOrEmpty(revertTextPath))
                {
                    RevertBackupToDocument(revertTextPath);
                }
                else if (revertTarget == RevertTarget.文档资源 && revertAsset != null)
                {
                    RevertDocumentFromBackup(revertAsset);
                }
                else UnityEditor.EditorUtility.DisplayDialog("警告", "需要还原的资源或文件路径为空!", "确定");
            }
            else if (checkTarget == CheckTarget.指定路径 && !string.IsNullOrEmpty(revertPath))
            {
                List<string> allAssetPaths = new List<string>();
                List<string> allTextPaths = new List<string>();
                GetAllFile(revertPath, ref allAssetPaths, ref allTextPaths);
                if (revertTarget == RevertTarget.文本文件)
                {

                }
                else
                {

                }
            }
            else UnityEditor.EditorUtility.DisplayDialog("警告", "需要还原的文件夹路径为空!", "确定");
        }

        private void RevertBackupToDocument(string backupPath)
        {
            //TODO: 优化：如果其中某个结构发生错误，能尽量恢复没有出错的部分
            StreamReader sr = new StreamReader(backupPath, Encoding.UTF8);
            string line = string.Empty;
            List<string> allLine = new List<string>();
            while ((line = sr.ReadLine()) != null)
            {
                allLine.Add(line);
            }
            try
            {
                CreateDocument(allLine);
            }
            catch
            {
                Debug.LogError("文档备份结构可能发生了变化，导致文档无法恢复!");
            }
        }
        private void CreateDocument(List<string> allLine)
        {
            DocumentAsset asset = new DocumentAsset();
            BackupToDucumentAsset(ref asset, allLine);
            string savePath = setting.documentRootPath + "/" + asset.documentSavePath + "/" + asset.documentName + ".asset";
            // TODO: 增加提示以及创建备份
            if (!File.Exists(savePath)) AssetDatabase.CreateAsset(asset, savePath);
        }
        private void BackupToDucumentAsset(ref DocumentAsset asset, List<string> allLine)
        {
            asset.documentType = (DocumentType)int.Parse(Regex.Match(allLine[0], @"<DocumentType>([0-9]+)</DocumentType>").Groups[1].Value.ToString());
            asset.department = (Department)int.Parse(Regex.Match(allLine[1], @"<Department>([0-9]+)</Department>").Groups[1].Value.ToString());
            asset.toolType = (ToolType)int.Parse(Regex.Match(allLine[2], @"<ToolType>([0-9]+)</ToolType>").Groups[1].Value.ToString());
            asset.toolName = Regex.Match(allLine[3], @"<ToolName>(.*)</ToolName>").Groups[1].Value.ToString();
            asset.documentName = Regex.Match(allLine[4], @"<DocumentName>(.*)</DocumentName>").Groups[1].Value.ToString();
            asset.documentUrl = Regex.Match(allLine[5], @"<DocumentUrl>(.*)</DocumentUrl>").Groups[1].Value.ToString();
            asset.documentSavePath = Regex.Match(allLine[6], @"<DocumentSavePath>(.*)</DocumentSavePath>").Groups[1].Value.ToString();
            asset.paragraphs = new List<Paragraph>();
            int paragraphNum = -1;
            for (int i = 8; i < allLine.Count; i++)
            {
                if (string.IsNullOrEmpty(allLine[i])) continue;
                if (Regex.IsMatch(allLine[i], @"<Title>(.*)</Title>"))
                {
                    Paragraph tempParagraph = new Paragraph();
                    tempParagraph.sentences = new List<Sentence>();
                    tempParagraph.title = Regex.Match(allLine[i], @"<Title>(.*)</Title>").Groups[1].Value.ToString();
                    asset.paragraphs.Add(tempParagraph);
                    paragraphNum ++;
                }
                else
                {
                    Sentence sentence = new Sentence();
                    sentence.contentFormat = (ContentFormat)int.Parse(Regex.Match(allLine[i], @"<ContentFormat>([0-9]+)</ContentFormat>").Groups[1].Value.ToString());
                    sentence.titleFormat = (TitleFormat)int.Parse(Regex.Match(allLine[i], @"<TitleFormat>([0-9]+)</TitleFormat>").Groups[1].Value.ToString());
                    sentence.content = Regex.Match(allLine[i], @"<Content>(.*)</Content>").Groups[1].Value.ToString();
                    sentence.copyContent = Regex.Match(allLine[i], @"<Copy>(.*)</Copy>").Groups[1].Value.ToString();
                    sentence.texPath = Regex.Match(allLine[i], @"<TexPath>(.*)</TexPath>").Groups[1].Value.ToString();
                    sentence.fileUrl = Regex.Match(allLine[i], @"<FilePath>(.*)</FilePath>").Groups[1].Value.ToString();
                    asset.paragraphs[paragraphNum].sentences.Add(sentence);
                }
            }
        }
        private void RevertDocumentFromBackup(DocumentAsset asset)
        {
            // TODO: 优化
            string backupPath = AssetDatabase.GetAssetPath(asset);
            string backupName = Path.GetFileName(backupPath);
            backupPath = Regex.Replace(backupPath, backupName, "." + backupName);
            backupPath = Regex.Replace(backupPath, ".asset", ".txt");
            if(!File.Exists(backupPath))
            {
                UnityEditor.EditorUtility.DisplayDialog("警告", "备份文件丢失，无法复原!", "确定");
                return;
            }
            bool isRevert = UnityEditor.EditorUtility.DisplayDialog("提示", "是否通过备份文件对文档进行还原!", "确定", "取消");
            if (!isRevert) return;
            StreamReader sr = new StreamReader(backupPath, Encoding.UTF8);
            string line = string.Empty;
            List<string> allLine = new List<string>();
            while ((line = sr.ReadLine()) != null)
            {
                allLine.Add(line);
            }
            try
            {
                BackupToDucumentAsset(ref asset, allLine);
            }
            catch
            {
                Debug.LogError("文档备份结构可能发生了变化，导致文档无法恢复!");
            }
        }
    }
}
#endif