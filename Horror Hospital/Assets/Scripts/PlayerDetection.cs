using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(playerMovement))]
public class playerDetection : MonoBehaviour
{
    public Volume postProcessVolume;
    public float vignetteIntensityDetected = 0.45f;
    public Color detectedColor = Color.red;
    public float smoothSpeed = 4f;

    private playerMovement movementScript;
    private Vignette vignette;
    private ColorAdjustments colorAdjust;

    void Start()
    {
        movementScript = GetComponent<playerMovement>();
        if (postProcessVolume != null)
        {
            postProcessVolume.profile.TryGet(out vignette);
            postProcessVolume.profile.TryGet(out colorAdjust);
        }
    }

    void Update()
    {
        bool detected = false;
        EnemyFollow[] enemies = FindObjectsOfType<EnemyFollow>();
        foreach (var enemy in enemies)
        {
            if (enemy.PlayerDetected)
            {
                detected = true;
                break;
            }
        }

        if (movementScript != null)
            movementScript.isDetected = detected;

        float targetVig = detected ? vignetteIntensityDetected : 0f;
        if (vignette != null)
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, targetVig, Time.deltaTime * smoothSpeed);
        if (colorAdjust != null)
        {
            Color targetColor = detected ? detectedColor : Color.white;
            colorAdjust.colorFilter.value = Color.Lerp(colorAdjust.colorFilter.value, targetColor, Time.deltaTime * smoothSpeed);
        }
    }
}