#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
public class SetTag : OdinEditorWindow
{
    private static SetTag window;
    [MenuItem("Tools/SetTag")]
    private static void ShowWindow()
    {
        window = (SetTag)EditorWindow.GetWindow(typeof(SetTag), false, "SetTag");
        window.Show();
    }

    [LabelText("Tag")]
    [LabelWidth(25)]
    public string tagName = "LowEffect";

    [HorizontalGroup("SetTag")]
    [Button(buttonSize : 25)]
    [LabelText("处理")]
    public void SetThisTag()
    {
        Object[] selectedObjects = Selection.objects;
        foreach (var selectedObject in selectedObjects)
        {
            (selectedObject as GameObject).tag = tagName;
        }
    }
    [HorizontalGroup("SetTag")]
    [Button(buttonSize : 25)]
    [LabelText("还原")]
    public void RevertTag()
    {
        Object[] selectedObjects = Selection.objects;
        foreach (var selectedObject in selectedObjects)
        {
            (selectedObject as GameObject).tag = "Untagged";
        }
    }

    [HorizontalGroup("Hide")]
    [Button(buttonSize : 25)]
    [LabelText("显隐物体")]
    public void HideObject()
    {
        Object[] selectedObjects = Selection.objects;
        foreach (var selectedObject in selectedObjects)
        {
            if ((selectedObject as GameObject).activeSelf)
            {
                (selectedObject as GameObject).SetActive(false);
            }
            else
            {
                (selectedObject as GameObject).SetActive(true);
            }
        }
    }
    [HorizontalGroup("Hide")]
    [Button(buttonSize : 25)]
    [LabelText("显隐带标记")]
    public void HideObjectWithTag()
    {
        Object[] selectedObjects = Selection.objects;
        foreach (var selectedObject in selectedObjects)
        {
            if ((selectedObject as GameObject).tag != tagName) continue;
            if ((selectedObject as GameObject).activeSelf)
            {
                (selectedObject as GameObject).SetActive(false);
            }
            else
            {
                (selectedObject as GameObject).SetActive(true);
            }
        }
    }
    [HorizontalGroup("Hide")]
    [Button(buttonSize : 25)]
    [LabelText("显隐不带标记")]
    public void HideObjectWithoutTag()
    {
        Object[] selectedObjects = Selection.objects;
        foreach (var selectedObject in selectedObjects)
        {
            if ((selectedObject as GameObject).tag == tagName) continue;
            if ((selectedObject as GameObject).activeSelf)
            {
                (selectedObject as GameObject).SetActive(false);
            }
            else
            {
                (selectedObject as GameObject).SetActive(true);
            }
        }
    }

    [HorizontalGroup("PSButton")]
    [Button(buttonSize : 25)]
    [LabelText("重置全部")]
    public void RestartPS()
    {
        Object[] selectedObjects = Selection.objects;
        foreach (var selectedObject in selectedObjects)
        {
            ParticleSystem currentPS;
            (selectedObject as GameObject).TryGetComponent<ParticleSystem>(out currentPS);
            if (currentPS == null) continue;
            currentPS.Simulate(0.0f, false, true);
            currentPS.Play();
        }
    }
    [HorizontalGroup("PSButton")]
    [Button(buttonSize : 25)]
    [LabelText("重置带标记")]
    public void PausePS()
    {
        Object[] selectedObjects = Selection.objects;
        foreach (var selectedObject in selectedObjects)
        {
            if ((selectedObject as GameObject).tag != tagName) continue;
            ParticleSystem currentPS;
            (selectedObject as GameObject).TryGetComponent<ParticleSystem>(out currentPS);
            if (currentPS == null) continue;
            currentPS.Simulate(0.0f, false, true);
            currentPS.Play();
        }
    }
    [HorizontalGroup("PSButton")]
    [Button(buttonSize : 25)]
    [LabelText("重置不带标记")]
    public void PlayPS()
    {
        Object[] selectedObjects = Selection.objects;
        foreach (var selectedObject in selectedObjects)
        {
            if ((selectedObject as GameObject).tag == tagName) continue;
            ParticleSystem currentPS;
            (selectedObject as GameObject).TryGetComponent<ParticleSystem>(out currentPS);
            if (currentPS == null) continue;
            currentPS.Simulate(0.0f, false, true);
            currentPS.Play();
        }
    }
}

#endif