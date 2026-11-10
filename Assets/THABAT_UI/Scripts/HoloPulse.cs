using UnityEngine;
using UnityEngine.UI;

// Breathing effect: the object scales up and down, and its Graphic (Image/TMP) can brighten.
// Use on the AI orb, the Start button ring, the crosshair.
public class HoloPulse : MonoBehaviour
{
    public float speed = 0.33f;        // pulses per second
    public float scaleAmount = 0.06f;
    public bool fadeAlpha = false;
    public float minAlpha = 0.35f;

    Vector3 baseScale;
    Graphic graphic;
    Color baseColor;

    void Awake()
    {
        baseScale = transform.localScale;
        graphic = GetComponent<Graphic>();
        if (graphic) baseColor = graphic.color;
    }

    void Update()
    {
        float s = (Mathf.Sin(Time.unscaledTime * speed * Mathf.PI * 2f) + 1f) * 0.5f;
        transform.localScale = baseScale * (1f + s * scaleAmount);
        if (fadeAlpha && graphic)
        {
            Color c = baseColor;
            c.a = Mathf.Lerp(minAlpha, baseColor.a, s);
            graphic.color = c;
        }
    }
}
