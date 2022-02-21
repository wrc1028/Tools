using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ModelImportSetting : AssetPostprocessor
{
    void OnPostprocessModel(GameObject rImaportModel)
    {
        // this.ClearMeshUVAndColorChannel(rImaportModel);
    }

    private void ClearMeshUVAndColorChannel(GameObject rImportModel)
    {
        List<Vector2> rNewUV = null;
        List<Color32> rNewColor = null;
        var rFilters= rImportModel.GetComponentsInChildren<MeshFilter>();
        for (int filter_index = 0; filter_index < rFilters.Length; filter_index++)
        {
            rFilters[filter_index].sharedMesh.SetColors(rNewColor);
            rFilters[filter_index].sharedMesh.SetUVs(1, rNewUV);
            rFilters[filter_index].sharedMesh.SetUVs(2, rNewUV);
            rFilters[filter_index].sharedMesh.SetUVs(3, rNewUV);
        }
    }
}
