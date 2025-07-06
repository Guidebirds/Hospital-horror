using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(PlayerMovement))]
public class playerDetection : MonoBehaviour
{
    [Header("Post-processing")]
    [SerializeField] private Volume postProcessVolume;
    [SerializeField] private float vignetteIntensityDetected = 0.45f;
    [SerializeField] private Color detectedColor = Color.red;
    [SerializeField] private float smoothSpeed = 4f;

    private PlayerMovement movementScript;
    private Vignette vignette;
    private ColorAdjustments colorAdjust;
    private EnemyFollow[] enemies;

    void Awake()                                   // use Awake for component caching
    {
        movementScript = GetComponent<PlayerMovement>();

        if (postProcessVolume &&
            postProcessVolume.profile.TryGet(out vignette) &&
            postProcessVolume.profile.TryGet(out colorAdjust))
        {
            // nothing else to do
        }
    }
    void Start()
    {
        RefreshEnemies();
    }

    public void RefreshEnemies()
    {
        enemies = FindObjectsByType<EnemyFollow>(FindObjectsSortMode.None);
    }


    void Update()
    {
        bool detected = false;

        if (enemies == null || enemies.Length == 0)
            RefreshEnemies();

        foreach (EnemyFollow enemy in enemies)
        {
            if (enemy.PlayerDetected) { detected = true; break; }
        }

        if (movementScript) movementScript.isDetected = detected;

        // Smooth post-fx
        if (vignette)
        {
            float targetVig = detected ? vignetteIntensityDetected : 0f;
            vignette.intensity.value =
                Mathf.Lerp(vignette.intensity.value, targetVig, Time.deltaTime * smoothSpeed);
        }

        if (colorAdjust)
        {
            Color targetColor = detected ? detectedColor : Color.white;
            colorAdjust.colorFilter.value =
                Color.Lerp(colorAdjust.colorFilter.value, targetColor, Time.deltaTime * smoothSpeed);
        }
    }
}