using UnityEngine;
using Sirenix.OdinInspector;

[ExecuteInEditMode, DisallowMultipleComponent]
public class LightControllerComponent_v2 : MonoBehaviour
{
    [LabelText("日出偏移")]
    [Range(-180, 180)]
    public float sunriseOffset;
    [LabelText("当前时间")]
    [Range(0, 24)]
    public float clock;
    [LabelText("颜色渐变")]
    public Gradient lightGradient;
    [Range(0, 3)]
    [LabelText("灯光强度")]
    public float lightIntensity;
    private float verticalAngle;
    private Light currentLight;
    private Vector3 lightDir;

    private void Start()
    {
        InitProp();
    }
    private void InitProp()
    {
        Vector3 riseDir = new Vector3(transform.forward.x, 0, transform.forward.z);
        riseDir = riseDir.normalized;
        float LdotR = Vector3.Dot(Vector3.left, riseDir);
        sunriseOffset = transform.forward.z < 0 ? 
            Mathf.Acos(LdotR) * 180 / Mathf.PI : - Mathf.Acos(LdotR) * 180 / Mathf.PI;
        float FdotR = Vector3.Dot(transform.forward, riseDir);
        verticalAngle = transform.forward.y > 0 ? 
            (360 - Mathf.Acos(FdotR) * 180 / Mathf.PI) : Mathf.Acos(FdotR) * 180 / Mathf.PI;
        verticalAngle = verticalAngle > 270 ? verticalAngle - 360 : verticalAngle;
        clock = (verticalAngle + 90) / 15;

        lightGradient = new Gradient();
        currentLight = transform.GetComponent<Light>();
        lightDir = currentLight.transform.forward;
        lightIntensity = currentLight.intensity;
    }
    private void Update()
    {
        ApplyLightSetting();
    }

    private void ApplyLightSetting()
    {
        verticalAngle = clock * 15 - 90;
        lightDir.x = Mathf.Cos(sunriseOffset * Mathf.PI / 180) * Mathf.Cos(verticalAngle * Mathf.PI / 180);
        lightDir.y = Mathf.Sin(verticalAngle * Mathf.PI / 180);
        lightDir.z = Mathf.Sin(sunriseOffset * Mathf.PI / 180) * Mathf.Cos(verticalAngle * Mathf.PI / 180);

        currentLight.color = lightGradient.Evaluate((verticalAngle + 90) / 360);
        currentLight.transform.forward = -lightDir.normalized;
        currentLight.intensity = lightIntensity;
    }
}
