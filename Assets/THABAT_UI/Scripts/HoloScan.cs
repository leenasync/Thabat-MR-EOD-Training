using UnityEngine;

// Moves a light band from the top of the panel to the bottom, then repeats.
// Put on a child Image (Holo_ScanBand) inside a panel that has a RectMask2D.
[RequireComponent(typeof(RectTransform))]
public class HoloScan : MonoBehaviour
{
    public float duration = 4.5f;

    RectTransform rt, parent;

    void Awake()
    {
        rt = (RectTransform)transform;
        parent = (RectTransform)transform.parent;
    }

    void Update()
    {
        float h = parent.rect.height;
        float band = rt.rect.height;
        float k = (Time.unscaledTime % duration) / duration;
        float y = Mathf.Lerp(h * 0.5f + band, -h * 0.5f - band, k);
        rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, y);
    }
}
