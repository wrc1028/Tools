using UnityEngine;
using UnityEditor;

public class ModelImport : AssetPostprocessor
{
    public void OnPostprocessModel(GameObject reImportModel)
    {
        this.ShowMeshInfo(reImportModel);
    }
    private void ShowMeshInfo(GameObject model)
    {
        var filters = model.GetComponentsInChildren<MeshFilter>();
        bool isPass = true;
        foreach (var filter in filters)
        {
            if (filter.sharedMesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.Color))
            {
                Debug.Log(filter.name + "有顶点颜色");
                isPass = false;
            }
            else
                Debug.Log(filter.name + "无顶点颜色");
            if (filter.sharedMesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.TexCoord2))
                Debug.Log(filter.name + "有UV3");
            else
                Debug.Log(filter.name + "无UV3");
        }
        if (!isPass)
        {
            UnityEditor.EditorUtility.DisplayDialog("错误", model.name + "存在多余顶点色", "确定", "取消");
        }
    }
}
