using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Read-only HUD helper. It never writes to HybridHandAI or BombTimer.
// - Draws a live hand-stability graph from HybridHandAI.stability
// - Colors the graph and its label by HybridHandAI.aiResult (Stable / Warning / Danger)
// - Turns the timer text red in the last seconds
public class HoloHUD : MonoBehaviour
{
    [Header("Sources (read only)")]
    public HybridHandAI handAI;
    public BombTimer timer;

    [Header("UI")]
    public TMP_Text timerText;
    public RectTransform graphArea;
    public TMP_Text stateLabel;

    [Header("Graph")]
    public int bars = 40;
    public float sampleInterval = 0.08f;
    public float lowTimeSeconds = 10f;

    [Header("Colors")]
    public Color holo = new Color32(0x6F, 0xE3, 0xF0, 0xFF);
    public Color warn = new Color32(0xF4, 0xB5, 0x4A, 0xFF);
    public Color fail = new Color32(0xFF, 0x5A, 0x62, 0xFF);

    Image[] barImages;
    float[] values;
    float elapsed;
    float barWidth;

    void Awake()
    {
        BuildBars();
    }

    void BuildBars()
    {
        if (graphArea == null || bars < 2) return;

        values = new float[bars];
        barImages = new Image[bars];
        barWidth = graphArea.rect.width / bars;

        for (int i = 0; i < bars; i++)
        {
            var go = new GameObject("bar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.layer = graphArea.gameObject.layer;
            go.transform.SetParent(graphArea, false);

            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.anchoredPosition = new Vector2(i * barWidth, 0f);
            rt.sizeDelta = new Vector2(barWidth * 0.6f, 4f);

            var img = go.GetComponent<Image>();
            img.raycastTarget = false;
            img.color = holo;
            barImages[i] = img;
        }
    }

    void Update()
    {
        if (timer != null && timerText != null)
            timerText.color = timer.timeLeft <= lowTimeSeconds ? fail : holo;

        if (handAI == null || barImages == null) return;

        elapsed += Time.unscaledDeltaTime;
        if (elapsed < sampleInterval) return;
        elapsed = 0f;

        // 0 = as steady as the calibration baseline, 1 = no stability left
        float baseline = Mathf.Max(handAI.normalStability, 0.001f);
        float tremor = Mathf.Clamp01(1f - handAI.stability / baseline);

        for (int i = 0; i < bars - 1; i++) values[i] = values[i + 1];
        values[bars - 1] = tremor;

        string state = handAI.aiResult != null ? handAI.aiResult.Trim() : "";
        Color c = state == "Danger" ? fail : state == "Warning" ? warn : holo;

        float h = graphArea.rect.height;
        for (int i = 0; i < bars; i++)
        {
            barImages[i].rectTransform.sizeDelta = new Vector2(barWidth * 0.6f, Mathf.Lerp(4f, h, values[i]));
            Color bc = c;
            bc.a = Mathf.Lerp(0.25f, 1f, i / (float)(bars - 1));
            barImages[i].color = bc;
        }

        if (stateLabel != null)
        {
            stateLabel.text = state.Length > 0 ? state.ToUpperInvariant() : "STABLE";
            stateLabel.color = c;
        }
    }
}
