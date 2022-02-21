using UnityEngine;
using UnityEditor;
using System.Text;
using System.IO;

public class ConfigureEmployeePhone : EditorWindow
{
    private static ConfigureEmployeePhone window;
    private static string employeePhone = "18910000001";
    private string currentFilePath;
    private string configureFilePath;

    [MenuItem("Tools/配置员工手机号")]
    private static void ShowWindow()
    {
        window = (ConfigureEmployeePhone)EditorWindow.GetWindow(typeof(ConfigureEmployeePhone), true, "配置员工手机号");
        window.maxSize = new Vector2(160, 75);
        window.minSize = new Vector2(160, 75);
        window.Show();
    }
    private void OnGUI() 
    {
        GUILayout.Label("请输入你的手机号");
        employeePhone = EditorGUILayout.TextField(employeePhone);
        if (GUI.Button(new Rect(3, 46, 152, 23), "配置"))
        {
            currentFilePath = Application.dataPath;
            //configureFilePath = currentFilePath.Replace("Plugs/Temp", "TestFolder/EmployeePhoneNum.lua");
            configureFilePath = currentFilePath + "/TestFolder/EmployeePhoneNum.lua";
            AutoConfigure();
        }
    }

    private void AutoConfigure()
    {
        if (employeePhone == null)
        {
            Debug.LogError("输入的手机号为空");
            return;
        }
        if (System.Text.RegularExpressions.Regex.IsMatch(employeePhone, @"^[+-]?\d*[.]?\d*$") && employeePhone.Length == 11)
        {
            configureFilePath = configureFilePath.Replace("/", "\\");
            string[] allContent = File.ReadAllLines(configureFilePath);
            foreach (var item in allContent)
            {
                if (item.Contains(employeePhone))
                {
                    Debug.LogError("出现重复手机号");
                    return;
                }
            }
            File.WriteAllText(configureFilePath, "", Encoding.UTF8);
            // File.AppendAllText(configureFilePath, "GameStatic.loginFrontMobile = \"" + employeePhone + "\"\n", Encoding.UTF8);
            foreach (var item in allContent)
            {
                File.AppendAllText(configureFilePath, item + "\n", Encoding.UTF8);
                if (item.Contains("以下是基础配置"))
                {
                    
                }
            }
            Debug.Log("添加成功");
            window.Close();
        }
        else
            Debug.LogError("输入的手机号格式错误");
    }
}