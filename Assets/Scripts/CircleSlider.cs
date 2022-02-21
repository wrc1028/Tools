using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleSlider : MonoBehaviour
{
    public Transform lightTransform;
    public RectTransform BG;
    public RectTransform btnRect;
    [System.Serializable]
    public enum EastRef
    {
        正X方向, 负X方向, 正Z方向, 负Z方向
    }
    public EastRef eastRef;
    private EastRef currentDir;
    [Range(-45, 45)]
    public float clockwiseDrift = 0;
    private float piValue;
    private Vector3 lightPos;

    float cosAlpha = 0;
    float sinAlpha = 0;


    private void Start()
    {
        lightPos = new Vector3(1, 0, 0);
        eastRef = EastRef.正X方向;
        currentDir = eastRef;
    }
    
    private void OnValidate()
    {
        // 参考方向改变
        if (currentDir != eastRef)
        {
            clockwiseDrift = 0;
            currentDir = eastRef;
        }
        else
        {
            piValue = (clockwiseDrift / 180.0f) * Mathf.PI;
            cosAlpha = Mathf.Cos(piValue);
            sinAlpha = Mathf.Sin(piValue);
            switch (currentDir)
            {
                case EastRef.正X方向:
                    lightPos = new Vector3(cosAlpha, 0, -sinAlpha);
                    break;
                case EastRef.负X方向:
                    lightPos = new Vector3(-cosAlpha, 0, sinAlpha);
                    break;
                case EastRef.正Z方向:
                    lightPos = new Vector3(sinAlpha, 0, cosAlpha);
                    break;
                default:
                    lightPos = new Vector3(-sinAlpha, 0, -cosAlpha);
                    break;
            }

            lightTransform.forward = (Vector3.zero - lightPos).normalized;
        }
    }

    public void OnHandleDrag()
    {
        Vector3 riseDir = new Vector3(-1, 0, 0);
        Vector3 mousePos = Input.mousePosition;
        mousePos.x -= Screen.width / 2;
        mousePos.y -= Screen.height / 2;
        Vector3 dir = mousePos.normalized;
        btnRect.position = BG.position + dir * 227;
        float cosAlpha = Vector3.Dot(riseDir, dir);
        float angle = Mathf.Acos(cosAlpha) * 180 / Mathf.PI;
        Vector3 crossValue = Vector3.Cross(riseDir, dir);
        angle = crossValue.z > 0 ? 360 - angle : angle;
        float sinAngle = Mathf.Sin(angle * Mathf.PI / 180);
        float cosAngle = Mathf.Cos(angle * Mathf.PI / 180);
        Vector3 newPos = new Vector3(lightPos.x * cosAngle, sinAngle, lightPos.z * cosAngle);
        lightTransform.forward = (Vector3.zero - newPos).normalized;
    }
}
