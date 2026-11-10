using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// THABAT > Apply Holo UI
// Restyles the panels under "LeenaCanvas" in place.
// Safe by design:
//   - never renames, deletes, activates or deactivates existing objects
//   - never touches Button onClick events or any gameplay script
//   - every object referenced by AdaptiveTrainingManager / BombResult / BombTimer /
//     MissionStartController / LiveAIHUD stays the same object
//   - saves a backup copy of the scene first, and the whole change is one Ctrl+Z
//   - running it again first removes what it added before (objects named "Holo_...")
public static class ApplyHoloUI
{
    const string Root = "Assets/THABAT_UI";
    const string SpriteDir = Root + "/Sprites";
    const string BackupDir = Root + "/Backup";

    static readonly Color Holo = Hex(0x6FE3F0);
    static readonly Color PanelBg = Hex(0x0E1A28, 160);
    static readonly Color Ink = Hex(0xE6F2F5);
    static readonly Color Mute = Hex(0x8AA3AE);
    static readonly Color Safe = Hex(0x5BE3A2);
    static readonly Color Warn = Hex(0xF4B54A);
    static readonly Color Fail = Hex(0xFF5A62);
    static readonly Color Dark = Hex(0x070B12);

    static Sprite sFill, sBorder, sBtn, sBtnBorder, sBracket, sOrb, sRing, sScan;
    static TMP_FontAsset fDisplay, fMono;

    [MenuItem("THABAT/Apply Holo UI")]
    public static void Apply()
    {
        var canvasGO = GameObject.Find("LeenaCanvas");
        if (canvasGO == null)
        {
            EditorUtility.DisplayDialog("THABAT", "LeenaCanvas was not found in the open scene. Open defensTHONE3rd first.", "OK");
            return;
        }

        if (!EditorUtility.DisplayDialog("THABAT · Apply Holo UI",
            "This restyles the 7 panels under LeenaCanvas.\n\n" +
            "Scripts, button events and object names stay the same.\n" +
            "A backup of the scene is saved to Assets/THABAT_UI/Backup first.",
            "Apply", "Cancel")) return;

        var scene = canvasGO.scene;
        if (scene.isDirty && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        if (!Backup(scene.path)) return;

        LoadSprites();
        LoadFonts();

        Undo.SetCurrentGroupName("Apply Holo UI");
        int group = Undo.GetCurrentGroup();

        Transform canvas = canvasGO.transform;
        RemoveOldHolo(canvas);

        // Canvas: 1.6 m x 0.9 m in the world (1 unit = 1 mm)
        var crt = (RectTransform)canvas;
        Undo.RecordObject(crt, "Holo");
        crt.sizeDelta = new Vector2(1600f, 900f);
        crt.localScale = Vector3.one * 0.001f;

        StylePanelsAndHud(canvas);

        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(scene);
        Selection.activeGameObject = canvasGO;

        EditorUtility.DisplayDialog("THABAT",
            "Holo UI applied.\n\n" +
            "1. Press Play and run one full level.\n" +
            "2. If it looks right, press Ctrl+S.\n" +
            "3. To undo everything, press Ctrl+Z once.",
            "OK");
    }

    // ---------------------------------------------------------------- panels

    static void StylePanelsAndHud(Transform canvas)
    {
        // 01 StartPanel
        var start = Child(canvas, "StartPanel");
        if (start != null)
        {
            StylePanel(start, new Vector2(0, 40), new Vector2(900, 440), Holo, true);
            Orb(start, new Vector2(-310, 70));
            Label(start, "Holo_Tag", "EOD TRAINING SYSTEM", fMono, 22, Holo, new Vector2(90, 150), new Vector2(560, 30), TextAlignmentOptions.Left);
            Label(start, "Holo_Title", "THABAT", fDisplay, 84, Ink, new Vector2(90, 95), new Vector2(560, 90), TextAlignmentOptions.Left);
            var sub = Label(start, "Holo_Sub", "Mixed reality training for safe ordnance disposal. Hand movement, speed and stability are measured live.",
                fDisplay, 26, Mute, new Vector2(90, 5), new Vector2(560, 100), TextAlignmentOptions.TopLeft);
            sub.enableAutoSizing = true; sub.fontSizeMin = 18; sub.fontSizeMax = 26;
            Label(start, "Holo_Hint", "Pinch or press to begin", fMono, 22, Mute, new Vector2(-200, -125), new Vector2(380, 30), TextAlignmentOptions.Left);
            var b = Child(start, "StartButton");
            if (b != null) StyleButton(b, new Vector2(240, -125), new Vector2(340, 84), "START TRAINING", Holo, Dark, false);
        }

        // 02 LevelIntroPanel (not driven by any script right now; restyled for consistency)
        var intro = Child(canvas, "LevelIntroPanel");
        if (intro != null)
        {
            StylePanel(intro, new Vector2(0, 40), new Vector2(820, 360), Holo, true);
            Label(intro, "Holo_Tag", "LEVEL BRIEFING", fMono, 22, Holo, new Vector2(0, 140), new Vector2(740, 30), TextAlignmentOptions.Left);
            var t = Child(intro, "LevelIntroText");
            if (t != null)
            {
                var tmp = t.GetComponent<TMP_Text>();
                StyleText(tmp, fDisplay, 54, Ink, TextAlignmentOptions.Left, true, 30);
                Place((RectTransform)t, new Vector2(0, 40), new Vector2(740, 150));
                if (tmp != null && string.IsNullOrWhiteSpace(tmp.text)) { Undo.RecordObject(tmp, "Holo"); tmp.text = "Get ready"; }
            }
            var b = Child(intro, "BEGINlevel");
            if (b != null) StyleButton(b, new Vector2(230, -80), new Vector2(300, 84), "BEGIN LEVEL", Holo, Dark, false);
        }

        // 03 InstructionPanel (shown for a few seconds before each level; bomb is not active yet)
        var instr = Child(canvas, "InstructionPanel");
        if (instr != null)
        {
            StylePanel(instr, new Vector2(0, 40), new Vector2(900, 380), Holo, true);
            MiniOrb(instr, new Vector2(-385, 140));
            Label(instr, "Holo_Tag", "BRIEFING", fMono, 22, Holo, new Vector2(30, 140), new Vector2(740, 30), TextAlignmentOptions.Left);
            var t = Child(instr, "Instruction");
            if (t != null)
            {
                StyleText(t.GetComponent<TMP_Text>(), fDisplay, 40, Ink, TextAlignmentOptions.TopLeft, true, 24);
                Place((RectTransform)t, new Vector2(0, -25), new Vector2(800, 250));
            }
        }

        // 04 HUD: top band only, lower field stays clear for the bomb
        var hud = Child(canvas, "HUD");
        if (hud != null) StyleHud(hud);

        // 05 SuccessPanel
        var ok = Child(canvas, "SuccessPanel");
        if (ok != null)
        {
            StylePanel(ok, new Vector2(0, 0), new Vector2(1000, 800), Safe, true);
            var tag = Child(ok, "SuccessPanaTEXT");
            if (tag != null)
            {
                var tmp = tag.GetComponent<TMP_Text>();
                StyleText(tmp, fMono, 26, Safe, TextAlignmentOptions.Left, false, 0);
                Place((RectTransform)tag, new Vector2(0, 350), new Vector2(900, 36));
                if (tmp != null) { Undo.RecordObject(tmp, "Holo"); tmp.text = "MISSION COMPLETE"; }
            }
            var report = Child(ok, "FinalReportText");
            if (report != null)
            {
                // AdaptiveTrainingManager writes a long multi-level report here, so it auto-sizes
                StyleText(report.GetComponent<TMP_Text>(), fMono, 30, Ink, TextAlignmentOptions.TopLeft, true, 14);
                Place((RectTransform)report, new Vector2(0, 10), new Vector2(900, 620));
            }
            // continueButton and restartButton are never shown together, so they share one spot
            var next = Child(ok, "Continue Training");
            if (next != null) StyleButton(next, new Vector2(0, -340), new Vector2(340, 84), "NEXT LEVEL", Safe, Dark, false);
            var again = Child(ok, "Play Again Button (1)");
            if (again != null) StyleButton(again, new Vector2(0, -340), new Vector2(380, 84), "IMPROVE TRAINING", Holo, Holo, true);
        }

        // 06 FailPanel
        var fail = Child(canvas, "FailPanel");
        if (fail != null)
        {
            StylePanel(fail, new Vector2(0, 20), new Vector2(920, 500), Fail, true);
            var flash = NewImage(fail, "Holo_Flash", null, WithA(Fail, 0.12f), false);
            Stretch(flash.rectTransform, 0);
            flash.transform.SetSiblingIndex(0);
            var pulse = Undo.AddComponent<HoloPulse>(flash.gameObject);
            pulse.speed = 0.5f; pulse.scaleAmount = 0f; pulse.fadeAlpha = true; pulse.minAlpha = 0f;

            Label(fail, "Holo_Tag", "MISSION FAILED", fMono, 26, Fail, new Vector2(0, 200), new Vector2(820, 36), TextAlignmentOptions.Left);
            var t = Child(fail, "FailPanelTEXT");
            if (t != null)
            {
                var tmp = t.GetComponent<TMP_Text>();
                StyleText(tmp, fDisplay, 34, Ink, TextAlignmentOptions.TopLeft, true, 22);
                Place((RectTransform)t, new Vector2(0, 20), new Vector2(820, 280));
                if (tmp != null && tmp.text.Trim() == "Faild") { Undo.RecordObject(tmp, "Holo"); tmp.text = "Mission failed"; }
            }
            var b = Child(fail, "RetryButton");
            if (b != null) StyleButton(b, new Vector2(200, -170), new Vector2(380, 84), "IMPROVE TRAINING", Fail, Dark, false);
        }

        // 07 Ruselt (BombResult.resultText; kept inactive as before)
        var res = Child(canvas, "Ruselt");
        if (res != null)
        {
            StyleText(res.GetComponent<TMP_Text>(), fMono, 22, Ink, TextAlignmentOptions.TopLeft, true, 14);
            Place((RectTransform)res, new Vector2(560, -330), new Vector2(420, 200));
        }
    }

    static void StyleHud(Transform hud)
    {
        var hrt = (RectTransform)hud;
        Undo.RecordObject(hrt, "Holo");
        hrt.anchorMin = Vector2.zero; hrt.anchorMax = Vector2.one;
        hrt.pivot = new Vector2(0.5f, 0.5f);
        hrt.offsetMin = Vector2.zero; hrt.offsetMax = Vector2.zero;
        hrt.localScale = Vector3.one; hrt.localRotation = Quaternion.identity;

        // Row 1: level | timer | score
        var level = Child(hud, "LevelText");
        if (level != null)
        {
            Card(hud, "Holo_LevelCard", new Vector2(-560, 360), new Vector2(400, 110), Holo);
            StyleText(level.GetComponent<TMP_Text>(), fDisplay, 34, Ink, TextAlignmentOptions.Center, true, 22);
            Place((RectTransform)level, new Vector2(-560, 360), new Vector2(360, 80));
        }

        var timerT = Child(hud, "TimerText");
        if (timerT != null)
        {
            Card(hud, "Holo_TimerCard", new Vector2(0, 350), new Vector2(320, 150), Holo);
            Label(hud, "Holo_TimerLabel", "TIME LEFT", fMono, 22, Mute, new Vector2(0, 400), new Vector2(300, 30), TextAlignmentOptions.Center);
            StyleText(timerT.GetComponent<TMP_Text>(), fDisplay, 96, Holo, TextAlignmentOptions.Center, false, 0);
            Place((RectTransform)timerT, new Vector2(0, 335), new Vector2(300, 100));
        }

        StyleHudCard(Child(hud, "scorePanal"), new Vector2(560, 360), new Vector2(400, 110), Holo, 44, 26);

        // Row 2: status | live stability graph | warnings
        StyleHudCard(Child(hud, "StatusPanal"), new Vector2(-560, 220), new Vector2(400, 100), Holo, 34, 22);

        Card(hud, "Holo_GraphCard", new Vector2(0, 215), new Vector2(560, 110), Holo);
        Label(hud, "Holo_GraphLabel", "HAND STABILITY · LIVE", fMono, 20, Mute, new Vector2(-50, 250), new Vector2(400, 26), TextAlignmentOptions.Left);
        var state = Label(hud, "Holo_GraphState", "STABLE", fMono, 20, Holo, new Vector2(190, 250), new Vector2(140, 26), TextAlignmentOptions.Right);
        var graphGO = new GameObject("Holo_Graph", typeof(RectTransform));
        graphGO.layer = hud.gameObject.layer;
        graphGO.transform.SetParent(hud, false);
        Undo.RegisterCreatedObjectUndo(graphGO, "Holo");
        Place((RectTransform)graphGO.transform, new Vector2(0, 200), new Vector2(520, 56));

        StyleHudCard(Child(hud, "WarningsPanal"), new Vector2(560, 220), new Vector2(400, 100), Warn, 30, 18);

        // Read-only helper that feeds the graph and the timer color
        var helper = hud.GetComponent<HoloHUD>();
        if (helper == null) helper = Undo.AddComponent<HoloHUD>(hud.gameObject);
        Undo.RecordObject(helper, "Holo");
        var atm = UnityEngine.Object.FindFirstObjectByType<AdaptiveTrainingManager>(FindObjectsInactive.Include);
        helper.handAI = atm != null && atm.handAI != null ? atm.handAI : UnityEngine.Object.FindFirstObjectByType<HybridHandAI>(FindObjectsInactive.Include);
        helper.timer = atm != null && atm.timerUI != null ? atm.timerUI : UnityEngine.Object.FindFirstObjectByType<BombTimer>(FindObjectsInactive.Include);
        helper.timerText = timerT != null ? timerT.GetComponent<TMP_Text>() : null;
        helper.graphArea = (RectTransform)graphGO.transform;
        helper.stateLabel = state;
        EditorUtility.SetDirty(helper);
    }

    static void StyleHudCard(Transform card, Vector2 pos, Vector2 size, Color accent, float fontSize, float minSize)
    {
        if (card == null) return;
        Place((RectTransform)card, pos, size);
        var img = card.GetComponent<Image>();
        if (img != null)
        {
            Undo.RecordObject(img, "Holo");
            img.sprite = sFill; img.type = Image.Type.Sliced; img.color = PanelBg;
        }
        var border = NewImage(card, "Holo_Border", sBorder, WithA(accent, 0.45f), true);
        Stretch(border.rectTransform, 0);
        border.transform.SetAsFirstSibling();

        var t = card.GetComponentInChildren<TMP_Text>(true);
        if (t != null)
        {
            StyleText(t, fDisplay, fontSize, Ink, TextAlignmentOptions.Center, true, minSize);
            Stretch(t.rectTransform, 14);
        }
    }

    // ---------------------------------------------------------------- building blocks

    static void StylePanel(Transform panel, Vector2 pos, Vector2 size, Color accent, bool motion)
    {
        Place((RectTransform)panel, pos, size);

        var img = panel.GetComponent<Image>();
        if (img != null)
        {
            Undo.RecordObject(img, "Holo");
            img.sprite = sFill; img.type = Image.Type.Sliced; img.color = PanelBg;
        }

        var border = NewImage(panel, "Holo_Border", sBorder, WithA(accent, 0.45f), true);
        Stretch(border.rectTransform, 0);
        border.transform.SetAsFirstSibling();

        var scan = NewImage(panel, "Holo_Scan", sScan, WithA(accent, 0.10f), false);
        var srt = scan.rectTransform;
        srt.anchorMin = new Vector2(0, 0.5f); srt.anchorMax = new Vector2(1, 0.5f);
        srt.pivot = new Vector2(0.5f, 0.5f);
        srt.sizeDelta = new Vector2(0, size.y * 0.4f);
        srt.anchoredPosition = Vector2.zero;
        scan.transform.SetSiblingIndex(1);
        Undo.AddComponent<HoloScan>(scan.gameObject);

        Brackets(panel, accent);

        if (panel.GetComponent<RectMask2D>() == null) Undo.AddComponent<RectMask2D>(panel.gameObject);

        if (motion)
        {
            if (panel.GetComponent<CanvasGroup>() == null) Undo.AddComponent<CanvasGroup>(panel.gameObject);
            if (panel.GetComponent<HoloPanel>() == null) Undo.AddComponent<HoloPanel>(panel.gameObject);
        }
    }

    static void StyleButton(Transform btn, Vector2 pos, Vector2 size, string label, Color fill, Color textColor, bool ghost)
    {
        Place((RectTransform)btn, pos, size);

        var img = btn.GetComponent<Image>();
        if (img != null)
        {
            Undo.RecordObject(img, "Holo");
            img.sprite = ghost ? sBtnBorder : sBtn;
            img.type = Image.Type.Sliced;
            img.color = fill;
            img.raycastTarget = true;
        }

        var b = btn.GetComponent<Button>();
        if (b != null)
        {
            Undo.RecordObject(b, "Holo");
            b.transition = Selectable.Transition.ColorTint;
            var cb = b.colors;
            cb.normalColor = new Color(0.88f, 0.88f, 0.88f, 1f);
            cb.highlightedColor = Color.white;
            cb.selectedColor = cb.normalColor;
            cb.pressedColor = new Color(0.65f, 0.65f, 0.65f, 1f);
            cb.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            cb.colorMultiplier = 1f;
            b.colors = cb;
            if (img != null) b.targetGraphic = img;
        }

        var t = btn.GetComponentInChildren<TMP_Text>(true);
        if (t != null)
        {
            StyleText(t, fDisplay, 30, textColor, TextAlignmentOptions.Center, true, 18);
            Undo.RecordObject(t, "Holo");
            t.text = label;
            t.characterSpacing = 6f;
            t.textWrappingMode = TextWrappingModes.NoWrap;
            Stretch(t.rectTransform, 8);
        }
    }

    static void Card(Transform parent, string name, Vector2 pos, Vector2 size, Color accent)
    {
        var bg = NewImage(parent, name, sFill, PanelBg, true);
        Place(bg.rectTransform, pos, size);
        bg.transform.SetAsFirstSibling();
        var border = NewImage(bg.transform, "Holo_Border", sBorder, WithA(accent, 0.45f), true);
        Stretch(border.rectTransform, 0);
    }

    static void Brackets(Transform panel, Color accent)
    {
        const float s = 30f, inset = 14f;
        var corners = new (Vector2 a, float rot)[]
        {
            (new Vector2(0, 1), 0f), (new Vector2(1, 1), -90f),
            (new Vector2(1, 0), 180f), (new Vector2(0, 0), 90f)
        };
        foreach (var c in corners)
        {
            var img = NewImage(panel, "Holo_Bracket", sBracket, WithA(accent, 0.85f), false);
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = c.a;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(s, s);
            rt.anchoredPosition = new Vector2(c.a.x == 0 ? inset + s / 2 : -inset - s / 2,
                                              c.a.y == 1 ? -inset - s / 2 : inset + s / 2);
            rt.localEulerAngles = new Vector3(0, 0, c.rot);
        }
    }

    static void Orb(Transform parent, Vector2 pos)
    {
        var ring2 = NewImage(parent, "Holo_OrbRingOuter", sRing, WithA(Holo, 0.3f), false);
        Place(ring2.rectTransform, pos, new Vector2(220, 220));
        Undo.AddComponent<HoloSpin>(ring2.gameObject).degreesPerSecond = -18f;

        var ring = NewImage(parent, "Holo_OrbRing", sRing, WithA(Holo, 0.55f), false);
        Place(ring.rectTransform, pos, new Vector2(185, 185));
        Undo.AddComponent<HoloSpin>(ring.gameObject).degreesPerSecond = 30f;

        var orb = NewImage(parent, "Holo_Orb", sOrb, Color.white, false);
        Place(orb.rectTransform, pos, new Vector2(140, 140));
        Undo.AddComponent<HoloPulse>(orb.gameObject);
    }

    static void MiniOrb(Transform parent, Vector2 pos)
    {
        var orb = NewImage(parent, "Holo_Orb", sOrb, Color.white, false);
        Place(orb.rectTransform, pos, new Vector2(36, 36));
        Undo.AddComponent<HoloPulse>(orb.gameObject).speed = 0.5f;
    }

    static TextMeshProUGUI Label(Transform parent, string name, string text, TMP_FontAsset font, float size, Color color,
                                 Vector2 pos, Vector2 box, TextAlignmentOptions align)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.layer = parent.gameObject.layer;
        go.transform.SetParent(parent, false);
        Undo.RegisterCreatedObjectUndo(go, "Holo");
        var t = go.GetComponent<TextMeshProUGUI>();
        if (font != null) t.font = font;
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        t.raycastTarget = false;
        if (font != null && font == fMono) t.characterSpacing = 10f;
        Place(t.rectTransform, pos, box);
        return t;
    }

    static Image NewImage(Transform parent, string name, Sprite sprite, Color color, bool sliced)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.layer = parent.gameObject.layer;
        go.transform.SetParent(parent, false);
        Undo.RegisterCreatedObjectUndo(go, "Holo");
        var img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.type = sliced && sprite != null ? Image.Type.Sliced : Image.Type.Simple;
        img.raycastTarget = false; // decoration must never block hand rays
        return img;
    }

    static void StyleText(TMP_Text t, TMP_FontAsset font, float size, Color color, TextAlignmentOptions align, bool autoSize, float minSize)
    {
        if (t == null) return;
        Undo.RecordObject(t, "Holo");
        if (font != null) t.font = font;
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        t.enableAutoSizing = autoSize;
        if (autoSize) { t.fontSizeMin = minSize; t.fontSizeMax = size; }
        t.raycastTarget = false;
    }

    static void Place(RectTransform rt, Vector2 pos, Vector2 size)
    {
        Undo.RecordObject(rt, "Holo");
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;
        rt.sizeDelta = size;
        rt.anchoredPosition3D = new Vector3(pos.x, pos.y, 0f);
    }

    static void Stretch(RectTransform rt, float inset)
    {
        Undo.RecordObject(rt, "Holo");
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;
        rt.offsetMin = new Vector2(inset, inset);
        rt.offsetMax = new Vector2(-inset, -inset);
        var p = rt.anchoredPosition3D; p.z = 0f; rt.anchoredPosition3D = p;
    }

    // ---------------------------------------------------------------- setup

    static Transform Child(Transform parent, string name)
    {
        foreach (Transform c in parent)
            if (c.name.Trim() == name) return c;
        Debug.LogWarning("[THABAT] Not found: " + parent.name + "/" + name + " (skipped)");
        return null;
    }

    static void RemoveOldHolo(Transform t)
    {
        for (int i = t.childCount - 1; i >= 0; i--)
        {
            var c = t.GetChild(i);
            if (c.name.StartsWith("Holo_")) Undo.DestroyObjectImmediate(c.gameObject);
            else RemoveOldHolo(c);
        }
    }

    static bool Backup(string scenePath)
    {
        if (string.IsNullOrEmpty(scenePath)) return true;
        if (!AssetDatabase.IsValidFolder(BackupDir)) AssetDatabase.CreateFolder(Root, "Backup");
        string name = System.IO.Path.GetFileNameWithoutExtension(scenePath);
        string dest = BackupDir + "/" + name + "_beforeHolo_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".unity";
        if (!AssetDatabase.CopyAsset(scenePath, dest))
        {
            EditorUtility.DisplayDialog("THABAT", "Could not save the scene backup. Nothing was changed.", "OK");
            return false;
        }
        Debug.Log("[THABAT] Scene backup saved: " + dest);
        return true;
    }

    static void LoadSprites()
    {
        sFill = LoadSprite("Holo_PanelFill", 24);
        sBorder = LoadSprite("Holo_PanelBorder", 24);
        sBtn = LoadSprite("Holo_ButtonFill", 16);
        sBtnBorder = LoadSprite("Holo_ButtonBorder", 16);
        sBracket = LoadSprite("Holo_Bracket", 0);
        sOrb = LoadSprite("Holo_Orb", 0);
        sRing = LoadSprite("Holo_DashRing", 0);
        sScan = LoadSprite("Holo_ScanBand", 0);
    }

    static Sprite LoadSprite(string file, int border)
    {
        string path = SpriteDir + "/" + file + ".png";
        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null) { Debug.LogWarning("[THABAT] Missing sprite: " + path); return null; }

        bool changed = false;
        if (ti.textureType != TextureImporterType.Sprite) { ti.textureType = TextureImporterType.Sprite; changed = true; }
        if (ti.spriteImportMode != SpriteImportMode.Single) { ti.spriteImportMode = SpriteImportMode.Single; changed = true; }
        if (ti.mipmapEnabled) { ti.mipmapEnabled = false; changed = true; }
        if (!ti.alphaIsTransparency) { ti.alphaIsTransparency = true; changed = true; }
        var b = new Vector4(border, border, border, border);
        if (ti.spriteBorder != b) { ti.spriteBorder = b; changed = true; }
        if (changed) ti.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static void LoadFonts()
    {
        fDisplay = null; fMono = null;
        foreach (var guid in AssetDatabase.FindAssets("t:TMP_FontAsset"))
        {
            string p = AssetDatabase.GUIDToAssetPath(guid);
            string n = System.IO.Path.GetFileName(p).ToLowerInvariant();
            if (fDisplay == null && n.Contains("chakra")) fDisplay = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(p);
            if (fMono == null && n.Contains("jetbrains")) fMono = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(p);
        }
        if (fDisplay == null) Debug.Log("[THABAT] Chakra Petch font asset not found. Keeping the current font for titles.");
        if (fMono == null) Debug.Log("[THABAT] JetBrains Mono font asset not found. Keeping the current font for labels.");
    }

    static Color Hex(int rgb, int a = 255) =>
        new Color32((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb, (byte)a);

    static Color WithA(Color c, float a) { c.a = a; return c; }
}
