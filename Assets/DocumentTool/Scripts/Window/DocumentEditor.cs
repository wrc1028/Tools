#if UNITY_EDITOR
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace TopJoy.Document
{
    public class DocumentEditor : OdinMenuEditorWindow
    {
        private static DocumentEditor window;
        [MenuItem("文档/文档编辑")]
        private static void ShowWindow()
        {
            window = (DocumentEditor)EditorWindow.GetWindow(typeof(DocumentEditor), false, "文档编辑");
            window.minSize = new Vector2(800, 540);
            window.maxSize = new Vector2(1200, 810);
            window.Show();
        }
        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree();
            tree.Selection.SupportsMultiSelect = false;

            tree.AddAssetAtPath("新建文档", "Assets/DocumentTool/Scripts/EditorItem/TreeAssets/CreateDocument.asset", typeof(CreateDocument));
            tree.AddAssetAtPath("编辑文档", "Assets/DocumentTool/Scripts/EditorItem/TreeAssets/EditDocument.asset", typeof(EditDocument));
            tree.AddAssetAtPath("文档设置", "Assets/DocumentTool/Scripts/EditorItem/TreeAssets/DocumentSetting.asset", typeof(ScriptableObject));
            tree.AddAssetAtPath("添加模板", "Assets/DocumentTool/Scripts/EditorItem/TreeAssets/TemplateList.asset", typeof(ScriptableObject));
            tree.Add("文档管理/校对", new DocumentChcek());
            tree.Add("文档管理/恢复", new DocumentRevert());
            tree.Add("测试", new TestRegex());
            return tree;
        }
    }
}
#endif