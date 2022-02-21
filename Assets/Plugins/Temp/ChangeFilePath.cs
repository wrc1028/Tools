using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class ChangeFilePath : MonoBehaviour
{
    static string srcPath = "D:/UnityProject/ToolsDevProject/Assets/TestFolder/Folder_2";
    static string desPath = "D:/UnityProject/ToolsDevProject/Assets/TestFolder/Folder_3";
    // [MenuItem("Tools/改变路径？")]
    static void Change()
    {
        // 获取当前路径下的全部子文件夹的路径和名称
        var srcDirs = Directory.GetDirectories(srcPath);
        var desDirs = Directory.GetDirectories(desPath);
        foreach (var item_i in srcDirs)
        {
            // 路径下文件夹的名称
            string srcName = Path.GetFileName(item_i);
            // 文件夹中后缀为.txt的文件
            var srcFiles = Directory.GetFiles(item_i, "*.txt");

            foreach (var item_j in desDirs)
            {
                string desName = Path.GetFileName(item_j);
                if (srcName == desName)
                {
                    Debug.Log(srcName);
                }
            }
        }
    }
}
