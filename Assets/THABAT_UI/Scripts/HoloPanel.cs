using UnityEngine;

// Put on each panel (StartPanel, SuccessPanel, ...). Needs a CanvasGroup (added automatically).
// When the panel is turned on: fades in, scales up slightly, and slides up.
// While open: floats gently up and down.
[RequireComponent(typeof(CanvasGroup))]
public class HoloPanel : MonoBehaviour
{
    [Header("Enter")]
    public float enterDuration = 0.6f;
    public float startScale = 0.94f;
    public float slideDistance = 20f;     // canvas units (pixels)

    [Header("Idle float")]
    public bool floatIdle = true;
    public float floatAmount = 6f;       // canvas units (pixels)
    public float floatSpeed = 1f;

    CanvasGroup group;
    Vector3 basePos, baseScale;
    float t;
    bool captured;

    void Awake()
    {
        group = GetComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        if (!captured)
        {
            basePos = transform.localPosition;
            baseScale = transform.localScale;
            captured = true;
        }
        t = 0f;
        group.alpha = 0f;
        transform.localScale = baseScale * startScale;
        transform.localPosition = basePos - Vector3.up * slideDistance;
    }

    void OnDisable()
    {
        transform.localPosition = basePos;
        transform.localScale = baseScale;
    }

    void Update()
    {
        t += Time.unscaledDeltaTime;
        float k = Mathf.Clamp01(t / enterDuration);
        float e = 1f - Mathf.Pow(1f - k, 3f); // ease-out cubic

        group.alpha = e;
        transform.localScale = Vector3.LerpUnclamped(baseScale * startScale, baseScale, e);

        float bob = floatIdle ? Mathf.Sin(t * floatSpeed * Mathf.PI * 2f / 6f) * floatAmount : 0f;
        transform.localPosition = Vector3.LerpUnclamped(basePos - Vector3.up * slideDistance, basePos, e) + Vector3.up * bob * e;
    }
}
