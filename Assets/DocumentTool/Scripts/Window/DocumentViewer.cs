#if UNITY_EDITOR
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using System.Linq;
using Sirenix.Utilities.Editor;
using Sirenix.OdinInspector.Editor;

namespace TopJoy.Document
{
    public class DocumentViewer : OdinMenuEditorWindow
    {
        private DocumentSetting setting;
        private static DocumentViewer window;
        [MenuItem("文档/查看文档")]
        private static void ShowWindow()
        {
            window = (DocumentViewer)EditorWindow.GetWindow(typeof(DocumentViewer), false, "文档");
            window.minSize = new Vector2(800, 540);
            window.maxSize = new Vector2(1200, 810);
            window.Show();
        }
        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree();
            tree.Selection.SupportsMultiSelect = false;
            tree.Config.DrawSearchToolbar = true;
            setting = AssetDatabase.LoadAssetAtPath<DocumentSetting>(CommonPropAndMethod.toolSettingPath);
            // 收藏文档
            tree.Add("收藏", null);
            tree.AddAllAssetsAtPath("收藏", "Assets/DocumentTool/Documents/收藏/", typeof(DocumentAsset), true, true);
            // 基础文档
            tree.Add("基础文档", null);
            AddTreeByPath(ref tree, "Assets/DocumentTool/Documents/基础文档/", false);
            // 资源规范
            tree.Add("资源规范", null);
            AddTreeByPath(ref tree, "Assets/DocumentTool/Documents/资源规范/", false);
            // 生产流程
            tree.Add("生产流程", null);
            AddTreeByPath(ref tree, "Assets/DocumentTool/Documents/生产流程/", false);
            // 工具使用
            tree.Add("工具使用", null);
            AddTreeByPath(ref tree, "Assets/DocumentTool/Documents/工具使用/", false);
            // 其他
            tree.Add("其他", null);
            AddTreeByPath(ref tree, "Assets/DocumentTool/Documents/其他/", false);
            // 文档设置
            // tree.Add("文档设置", setting);
            return tree;
        }

        private void AddTreeByPath(ref OdinMenuTree tree, string rootPath, bool isShowCollect)
        {
            // TODO: 优化
            var documentPaths = AssetDatabase.GetAllAssetPaths().Where(x => x.StartsWith(rootPath)).OrderBy(x => x);
            foreach (var path in documentPaths)
            {
                string absolutePath = path.Substring("Assets/DocumentTool/Documents/".Length);
                if (!absolutePath.Contains(".asset"))
                {
                    var nodePaths = Directory.GetFiles(path);
                    DocumentAsset asset = null;
                    foreach (var nodePath in nodePaths)
                    {
                        if (Path.GetExtension(nodePath) != ".asset") continue;
                        asset = AssetDatabase.LoadAssetAtPath<DocumentAsset>(nodePath.Replace("\\", "/"));
                        if (asset.documentType == DocumentType.节点) break;
                        asset = null;
                    }
                    tree.Add(absolutePath, asset);
                }
                else
                {
                    DocumentAsset asset = AssetDatabase.LoadAssetAtPath<DocumentAsset>(path);
                    if (asset.documentType != DocumentType.节点)
                        tree.Add(absolutePath.Remove(absolutePath.Length - 6), asset);
                }
            }
        }

        protected override void OnBeginDrawEditors()
        {
            var selected = this.MenuTree.Selection.FirstOrDefault();
            var toolbarHeight = this.MenuTree.Config.SearchToolbarHeight;
            SirenixEditorGUI.BeginHorizontalToolbar(toolbarHeight);
            {
                if (selected != null)
                {
                    GUILayout.Label(selected.Name);
                }
                if (selected != null && selected.Value != null && selected.Value.GetType() == typeof(DocumentAsset))
                {
                    DocumentAsset currentSelected = selected.Value as DocumentAsset;
                    // 访问网页端
                    if (SirenixEditorGUI.ToolbarButton(EditorIcons.Globe))
                    {
                        if (!string.IsNullOrEmpty(currentSelected.documentUrl)) Application.OpenURL(currentSelected.documentUrl);
                        else UnityEditor.EditorUtility.DisplayDialog("警告", "当前文档未指定地址!", "确定");
                    }
                    // 工具
                    if (currentSelected.documentType == DocumentType.工具使用)
                    {
                        if (SirenixEditorGUI.ToolbarButton(EditorIcons.Redo))
                        {
                            if (!string.IsNullOrEmpty(currentSelected.toolName)) EditorApplication.ExecuteMenuItem(currentSelected.toolName);
                            else UnityEditor.EditorUtility.DisplayDialog("警告", "当前文档未指定工具名!", "确定");
                        }
                    }
                    // 收藏
                    GUIContent collectContent = new GUIContent(EditorGUIUtility.FindTexture("Favorite@2x"), "收藏当前文档");
                    currentSelected.isCollect = SirenixEditorGUI.ToolbarToggle(currentSelected.isCollect, collectContent);
                    string originAssetPath = setting.documentRootPath + "/" + currentSelected.documentSavePath + "/" + currentSelected.documentName + ".asset";
                    string collectAssetPath = setting.collectPath + "/" + currentSelected.documentName + ".asset";
                    if (currentSelected.isCollect && !File.Exists(collectAssetPath) && File.Exists(originAssetPath))
                    {
                        AssetDatabase.CopyAsset(originAssetPath, collectAssetPath);
                        DocumentAsset collectAsset = AssetDatabase.LoadAssetAtPath<DocumentAsset>(collectAssetPath);
                        collectAsset.isCollect = true;
                    }
                    else if (!currentSelected.isCollect && File.Exists(collectAssetPath))
                    {
                        AssetDatabase.DeleteAsset(collectAssetPath);
                        DocumentAsset originAsset = AssetDatabase.LoadAssetAtPath<DocumentAsset>(originAssetPath);
                        originAsset.isCollect = false;
                    }
                }
            }
            SirenixEditorGUI.EndHorizontalToolbar();
        }

        protected override void OnEndDrawEditors()
        {
            
        }
    }
}
#endif