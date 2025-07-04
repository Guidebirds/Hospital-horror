using UnityEngine;
using UnityEngine.UI;

public class CrosshairUI : MonoBehaviour
{
    public RawImage crosshairImage;
    public float normalAlpha = 1f;
    public float highlightAlpha = 0.5f;
    public float normalScale = 1f;
    public float highlightScale = 1.3f;
    public float darkenMultiplier = 0.5f;

    private Color baseColor;

    void Reset()
    {
        crosshairImage = GetComponent<RawImage>();
    }

    void Awake()
    {
        if (crosshairImage == null)
            crosshairImage = GetComponent<RawImage>();
        if (crosshairImage != null)
            baseColor = crosshairImage.color;
    }

    public void SetHighlighted(bool highlighted)
    {
        if (crosshairImage == null)
            return;

        Color c = baseColor;
        c.a = highlighted ? highlightAlpha : normalAlpha;
        if (highlighted)
        {
            c.r *= darkenMultiplier;
            c.g *= darkenMultiplier;
            c.b *= darkenMultiplier;
        }
        crosshairImage.color = c;

        float scale = highlighted ? highlightScale : normalScale;
        crosshairImage.rectTransform.localScale = Vector3.one * scale;
    }
}