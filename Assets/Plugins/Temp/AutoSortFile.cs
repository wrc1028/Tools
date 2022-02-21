using System.IO;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
// 先获取所选的文件夹路径
// 获得里面的资源
// 对资源进行保存
public class AutoSortFile : Editor
{
    public enum FileType
    {
        Texture, Model, Material, Prefab, 
    }

    static string texPattem = "\\w+.(PNG|png|JPG|jpg|TGA|tga).?";
    static string modelPattem = "\\w+.(OBJ|obj|FBX|fbx).?";
    static string matPattem = "\\w+.mat.?";
    static string prefabPattem = "\\w+.prefab.?";

    [MenuItem("Tools/自动归类文件")]
    public static void Excute()
    {
        var selectFolders = Selection.assetGUIDs;
        foreach (var folder in selectFolders)
        {
            GetAllFilesFromPath(AssetDatabase.GUIDToAssetPath(folder));
        }
        
        AssetDatabase.Refresh();
    }

    private static void GetAllFilesFromPath(string _path)
    {
        string[] filesPath = Directory.GetFiles(_path);
        // 判断文件类型，并且带上.mate文件
        foreach (var path in filesPath)
        {
            if (Regex.IsMatch(Path.GetFileName(path), texPattem))
            {
                MoveToFolder(Path.GetFileName(path), _path, FileType.Texture, false);
            }
            else if (Regex.IsMatch(Path.GetFileName(path), modelPattem))
            {
                MoveToFolder(Path.GetFileName(path), _path, FileType.Model, false);
            }
            else if (Regex.IsMatch(Path.GetFileName(path), matPattem))
            {
                MoveToFolder(Path.GetFileName(path), _path, FileType.Material, true);
            }
            else if (Regex.IsMatch(Path.GetFileName(path), prefabPattem))
            {
                MoveToFolder(Path.GetFileName(path), _path, FileType.Prefab, false);
            }
        }
    }

    private static void MoveToFolder(string _fileName, string _folderPath, FileType _fileType, bool addPrev)
    {
        string folderPath = string.Format("{0}/{1}", _folderPath, _fileType.ToString());
        string srcPath = string.Format("{0}/{1}", _folderPath, _fileName.ToString());
        string desPath = string.Format("{0}/{1}", folderPath, _fileName.ToString());
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);
        File.Copy(srcPath, desPath);
        if (addPrev)
            File.Delete(srcPath);
    }
}
