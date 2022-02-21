using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
using System.IO;
using OfficeOpenXml;

public class FindFiles : MonoBehaviour
{
    private static string searchPath = "Assets";
    private static List<string> matsPath;
    private static List<string> shaderAndKeyworlds;
    private static List<string> shadersName;
    private static List<string> keyworlds;
    private static List<int> countMats;
    private static List<int> countKeyworlds;
    
    [MenuItem("Tools/文件夹内的全部材质")]
    static void FindAllFiles()
    {
        matsPath = new List<string>();
        shadersName= new List<string>();
        keyworlds = new List<string>();
        countMats = new List<int>();
        countKeyworlds = new List<int>();
        shaderAndKeyworlds = new List<string>();
        GetAllMats(searchPath, ref matsPath);
        // 输出到Text文件中
        // SaveToText();
        // SaveToExcel();
    }

    private static void GetAllMats(string filePath, ref List<string> dirs)
    {
        // 先遍历这个当前文件夹的所有文件，找到符合要求的文件
        foreach (var path in Directory.GetFiles(filePath))
        {
            if (System.IO.Path.GetExtension(path) == ".mat")
            {
                dirs.Add(path.Substring(path.IndexOf("Assets")));
                // 处理
                RecordShaderKeyworld(path);
            }
        }
        // 在遍历当前文件夹内的所有文件夹，并且递归调用当前
        if (Directory.GetDirectories(filePath).Length > 0)
        {
            foreach (var path in Directory.GetDirectories(filePath))
            {
                GetAllMats(path, ref dirs);
            }
        }
    }

    private static void RecordShaderKeyworld(string path)
    {
        Material mat = (Material)AssetDatabase.LoadAssetAtPath(path, typeof(Material));
        string shaderName = mat.shader.name;
        string keyworld = "";
        if (mat.shaderKeywords.Length != 0)
            keyworld = mat.shaderKeywords[0];
        for (int i = 1; i < mat.shaderKeywords.Length; i++)
        {
            keyworld += ", " + mat.shaderKeywords[i];
        }
        for (int i = 0; i < shaderAndKeyworlds.Count; i++)
        {
            // 进行判断是否相同，如果相同就返回
            if ((shaderName + keyworld) == shaderAndKeyworlds[i])
            {
                countMats[i] ++;
                return;
            }    
        }
        shaderAndKeyworlds.Add(shaderName + keyworld);
        shadersName.Add(shaderName);
        keyworlds.Add(keyworld);
        countMats.Add(1);
        countKeyworlds.Add(mat.shaderKeywords.Length);
    }

    private static void SaveToText()
    {
        string savePath = "Assets/MatKeyworlds";
        FileStream txtFile = File.Open(savePath + ".txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
        StreamWriter sw = new StreamWriter(txtFile, Encoding.UTF8);
        for (int i = 0; i < shaderAndKeyworlds.Count; i++)
        {
            sw.WriteLine(shaderAndKeyworlds[i] + "|" + countMats[i]);
        }
        sw.Close();
        txtFile.Close();
    }
    private static void SaveToExcel()
    {
        string savePath = "Assets/MatKeyworlds.xlsx";
        string sheetName = "详情";
        FileInfo excelFile = new FileInfo(savePath);
        if (excelFile.Exists)
        {
            excelFile.Delete();
            excelFile = new FileInfo(savePath);
        }

        using (ExcelPackage package = new ExcelPackage(excelFile))
        {
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(sheetName);
            worksheet.Cells[1, 1].Value = "Shader名称";
            worksheet.Cells[1, 2].Value = "材质使用数量";
            worksheet.Cells[1, 3].Value = "关键字数量";
            worksheet.Cells[1, 4].Value = "关键字";
            for (int i = 0; i < shaderAndKeyworlds.Count; i++)
            {
                string[] shaderName = shadersName[i].Split('/');
                worksheet.Cells[i + 2, 1].Value = shaderName[shaderName.Length - 1];
                worksheet.Cells[i + 2, 2].Value = countMats[i];
                worksheet.Cells[i + 2, 3].Value = countKeyworlds[i];
                worksheet.Cells[i + 2, 4].Value = keyworlds[i];
            }
            package.Save();
        }
    }
}
