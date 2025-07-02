using UnityEngine;

[RequireComponent(typeof(Light))]
public class LightPulsate : MonoBehaviour
{
    public float minIntensity = 0.5f;
    public float maxIntensity = 2f;
    public float speed = 2f;

    private Light myLight;

    void Start()
    {
        myLight = GetComponent<Light>();
    }

    void Update()
    {
        float pulse = Mathf.Sin(Time.time * speed) * 0.5f + 0.5f; // Ranges from 0 to 1
        myLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, pulse);
    }
}
