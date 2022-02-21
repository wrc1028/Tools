using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ControlLightDirection : EditorWindow
{
    private static ControlLightDirection window;
    Texture[] icons = new Texture[2];
    private string iconsPath = "Assets/Resources/Texture2D/";

    // [MenuItem("Tools/控制光的方向")]
    private static void ShowWindow()
    {
        window = (ControlLightDirection)EditorWindow.GetWindow(typeof(ControlLightDirection), true, "改变光的方向");
        window.minSize = new Vector2(320, 480);
        window.maxSize = new Vector2(320, 480);
        window.Show();
    }
    private Vector2 centerPos = new Vector2(160.0f, 160.0f);
    private float radius = 125.0f;
    private int controlID;
    private Rect controlHRect = new Rect(145, 20, 30, 30);
    private Vector2 controlHSize = new Vector2(30, 30);
    private Vector2 controlHDir = new Vector2(0, 0);
    private Vector2 offset;
    private bool isDrag = false;
    private Transform lightTransform;

    private void OnGUI()
    {
        Material mat = new Material(Shader.Find("Unlit/AlphaBlend"));
        lightTransform = GameObject.Find("Directional Light").transform;
        controlHDir = new Vector2(lightTransform.forward.z, lightTransform.forward.y);
        controlHDir = controlHDir.normalized;
        controlHRect.position = centerPos + radius * controlHDir - controlHSize / 2;
        icons[0] = AssetDatabase.LoadAssetAtPath(iconsPath + "circle_small.png", typeof(Texture2D)) as Texture;
        icons[1] = AssetDatabase.LoadAssetAtPath(iconsPath + "TouchPoint.png", typeof(Texture2D)) as Texture;
        EditorGUI.DrawTextureTransparent(new Rect(20, 20, 280, 280), icons[0]);
        Event e = Event.current;
        controlID = GUIUtility.GetControlID(FocusType.Passive);
        switch (e.GetTypeForControl(controlID))
        {
            case EventType.MouseDown:
                if (controlHRect.Contains(e.mousePosition))
                    isDrag = true;
                break;
            case EventType.MouseUp:
                isDrag = false;
                break;
            case EventType.MouseDrag:
                if (isDrag)
                {
                    offset = controlHSize / 2;
                    controlHDir = (e.mousePosition - centerPos).normalized;
                    controlHRect.position = centerPos + radius * controlHDir - offset;
                    lightTransform.forward = new Vector3(0, controlHDir.y, controlHDir.x);
                    e.Use();
                }
                break;
            default:
                break;
        }
        EditorGUI.DrawPreviewTexture(controlHRect, icons[1], mat);
    }
}
