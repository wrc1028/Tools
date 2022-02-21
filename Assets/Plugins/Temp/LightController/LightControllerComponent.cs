using UnityEngine;

[ExecuteInEditMode, DisallowMultipleComponent]
public class LightControllerComponent : MonoBehaviour
{
    public Vector3 lightForwardDir;
    public float lightIntensity;
    public Color lightColor;
    public float sunriseTime;
    public float sundownTime;
    private Light currentLight;


    public void Start()
    {
        if (transform.GetComponent<Light>() == null)
            transform.gameObject.AddComponent<Light>();
        currentLight = transform.GetComponent<Light>();
        if (currentLight.type != LightType.Directional)
        {
            Debug.LogWarning("已将当前类型灯光转变为平行光!!!");
            currentLight.type = LightType.Directional;
        }
        lightIntensity = currentLight.intensity;
        lightColor = currentLight.color;
        sunriseTime = 6;
        sundownTime = 18;
    }

    public void Update()
    {
        transform.forward = lightForwardDir.normalized;
        transform.GetComponent<Light>().color = lightColor;
        transform.GetComponent<Light>().intensity = lightIntensity;
    }
}