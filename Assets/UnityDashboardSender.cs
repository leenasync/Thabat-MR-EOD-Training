using System;
using System.Collections.Generic;
using UnityEngine;
using NativeWebSocket;

[Serializable]
public class DashboardData
{
    public float vibration;
    public float speed;
    public string movement;
    public string unityStatus;
    public int score;
    public int warnings;
    public int dangerCount;
    public int level;
}

public class UnityDashboardSender : MonoBehaviour
{
    WebSocket websocket;

    public string dashboardServerUrl = "ws://192.168.4.3:8003/unity";
    public SmartGloveToHybridAI gloveReceiver;
    public HybridHandAI handAI;
    public AdaptiveTrainingManager trainingManager; // optional; found automatically if left empty

    public float sendRate = 0.1f;

    [Header("Auto reconnect / laptop discovery")]
    [Tooltip("If the URL above fails, also try the same port/path on 192.168.4.<scanFrom..scanTo> (the ESP32 hotspot hands out changing addresses).")]
    public bool scanEspSubnet = true;
    public string scanSubnetPrefix = "192.168.4.";
    public int scanFrom = 2;
    public int scanTo = 10;
    public float connectTimeout = 1.5f;
    public float retryDelay = 0.5f;
    public bool verboseLogging = false;

    float sendTimer = 0f;
    bool connected = false;
    bool connecting = false;
    bool quitting = false;
    float attemptStartTime;
    float nextAttemptTime;
    bool cancelRequested;

    readonly List<string> candidateUrls = new List<string>();
    int nextIndex = 0;
    int lastGoodIndex = 0;
    int attemptIndex = 0;
    bool loggedMissingRefs = false;

    void Start()
    {
        if (trainingManager == null)
            trainingManager = FindFirstObjectByType<AdaptiveTrainingManager>();

        BuildCandidateList();
        Debug.Log("Dashboard sender will try: " + string.Join(", ", candidateUrls));
    }

    void BuildCandidateList()
    {
        candidateUrls.Clear();
        candidateUrls.Add(dashboardServerUrl);

        if (!scanEspSubnet) return;

        Uri uri;
        if (!Uri.TryCreate(dashboardServerUrl, UriKind.Absolute, out uri)) return;

        for (int i = scanFrom; i <= scanTo; i++)
        {
            string url = uri.Scheme + "://" + scanSubnetPrefix + i + ":" + uri.Port + uri.AbsolutePath;
            if (!candidateUrls.Contains(url))
                candidateUrls.Add(url);
        }
    }

    void TryConnectNext()
    {
        if (candidateUrls.Count == 0) return;

        attemptIndex = nextIndex;
        nextIndex = (nextIndex + 1) % candidateUrls.Count;
        string url = candidateUrls[attemptIndex];

        connecting = true;
        cancelRequested = false;
        attemptStartTime = Time.unscaledTime;

        if (verboseLogging) Debug.Log("Connecting to Dashboard Server: " + url);

        WebSocket ws = new WebSocket(url);
        websocket = ws;

        ws.OnOpen += () =>
        {
            if (ws != websocket) return;
            connected = true;
            connecting = false;
            lastGoodIndex = attemptIndex;
            Debug.Log("Connected to Dashboard Server: " + url);
        };

        ws.OnError += (e) =>
        {
            if (ws != websocket) return;
            if (verboseLogging || connected) Debug.LogWarning("Dashboard WebSocket Error (" + url + "): " + e);
        };

        ws.OnClose += (e) =>
        {
            if (ws != websocket) return;
            if (connected)
            {
                Debug.LogWarning("Dashboard Server Disconnected, reconnecting...");
                nextIndex = lastGoodIndex; // retry the address that worked first
            }
            connected = false;
            connecting = false;
            nextAttemptTime = Time.unscaledTime + retryDelay;
        };

        _ = ws.Connect();
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket?.DispatchMessageQueue();
#endif
        if (quitting) return;

        if (!connected && !connecting && Time.unscaledTime >= nextAttemptTime)
        {
            TryConnectNext();
        }
        else if (connecting && !connected)
        {
            float elapsed = Time.unscaledTime - attemptStartTime;
            if (!cancelRequested && elapsed > connectTimeout)
            {
                cancelRequested = true;
                websocket?.CancelConnection(); // makes Connect() fail -> OnClose -> next candidate
            }
            else if (elapsed > connectTimeout + 3f)
            {
                connecting = false; // safety net if the socket never reports back
                nextAttemptTime = Time.unscaledTime + retryDelay;
            }
        }

        sendTimer += Time.deltaTime;

        if (sendTimer >= sendRate)
        {
            sendTimer = 0f;
            SendDashboardData();
        }
    }

    async void SendDashboardData()
    {
        if (!connected || websocket == null || websocket.State != WebSocketState.Open)
            return;

        if (gloveReceiver == null || handAI == null)
        {
            if (!loggedMissingRefs)
            {
                loggedMissingRefs = true;
                Debug.LogError("UnityDashboardSender: " + (gloveReceiver == null ? "Glove Receiver" : "HybridHandAI") + " is missing");
            }
            return;
        }

        DashboardData data = new DashboardData
        {
            vibration = gloveReceiver.vibration,
            speed = gloveReceiver.speed,
            movement = gloveReceiver.movement,
            unityStatus = handAI.aiResult,
            score = handAI.score,
            warnings = handAI.warningsCount,
            dangerCount = handAI.dangerCount,
            level = trainingManager != null ? trainingManager.currentLevel : 0
        };

        string json = JsonUtility.ToJson(data);

        if (verboseLogging) Debug.Log("Sending dashboard data: " + json);

        try
        {
            await websocket.SendText(json);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Dashboard send failed: " + e.Message);
        }
    }

    async void OnApplicationQuit()
    {
        quitting = true;
        if (websocket != null)
        {
            try
            {
                if (websocket.State == WebSocketState.Open) await websocket.Close();
                else websocket.CancelConnection();
            }
            catch (Exception) { }
        }
    }
}
