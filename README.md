# Thabat (ثبات) — AI-Enhanced Mixed Reality EOD Training

> A Mixed Reality training simulation on Meta Quest for **Explosive Ordnance Disposal (EOD)**. Trainees detect, uncover and defuse a landmine under time pressure, while a **smart glove** and a **hybrid AI model** measure how steady their hands are in real time. Built for **Defensathon 2026**.

![Unity](https://img.shields.io/badge/Unity-6000.1-black?logo=unity)
![Meta Quest](https://img.shields.io/badge/Platform-Meta%20Quest%20MR-0467DF?logo=meta)
![OpenXR](https://img.shields.io/badge/XR-OpenXR%20%2B%20XRI%203-green)
![ESP32](https://img.shields.io/badge/Hardware-ESP32%20Smart%20Glove-E7352C?logo=espressif)
![Hackathon](https://img.shields.io/badge/Defensathon-2026-8A2BE2)

---

## Overview

*Thabat* means **"steadiness"** in Arabic. In bomb disposal, a shaking hand can cost a life.

The simulation turns the trainee's real room into a mission area using Mixed Reality passthrough. The trainee must:

1. **Equip** the gear: backpack, helmet and tools
2. **Detect** the landmine with a handheld detector. The beeps get faster and the haptics get stronger as the trainee gets closer.
3. **Dig** the sand to uncover the mine, using a deformable terrain
4. **Defuse** it by following the correct sequence: open the cover, remove the pin, cut the right wire
5. **Beat the clock.** A wrong step or running out of time triggers an explosion.

Throughout the mission, a smart glove streams hand-motion data. The **Hybrid Hand AI** scores the trainee's stability, and an instructor can watch everything on a **live dashboard**.

## Features

- 🧤 **ESP32 smart glove:** streams vibration, speed and movement over WebSocket
- 🤖 **Hybrid Hand AI:** calibrates the trainee's normal hand movement, detects shaking, issues warnings and danger alerts, and outputs a stability score
- 📊 **Live instructor dashboard:** Unity sends the glove data, AI score, warnings and mission status in real time
- 📡 **Proximity-based detector:** dynamic beep rate and controller haptics
- ⛏️ **Real-time terrain digging:** heightmap deformation in VR
- ✂️ **Ordered defuse sequence:** wrong order means failure, with explosion VFX, sound and haptics
- ⏱️ **Timer, score and leaderboard:** fastest successful defuses
- ⌨️ **In-headset virtual keyboard** for entering the player name
- 🔊 **Narration and audio cues:** mission briefing, heartbeat, ticking and success or fail sounds

## Tech Stack

| Layer | Technology |
|-------|------------|
| Engine | Unity 6 (6000.1.14f1), URP |
| XR | Meta XR SDK Core, OpenXR 1.15, XR Interaction Toolkit 3.1, XR Hands 1.6 |
| Device | Meta Quest (Mixed Reality passthrough) |
| Hardware | ESP32 smart glove with motion and vibration sensors |
| Networking | WebSocket (glove → Unity → dashboard) |
| Language | C# |

## Architecture

```
┌───────────────┐  WebSocket   ┌────────────────────────┐  WebSocket   ┌──────────────────┐
│ ESP32 Smart   │ ───────────▶ │  Unity MR (Quest)      │ ───────────▶ │ Instructor       │
│ Glove         │  vibration,  │  SmartGloveToHybridAI  │  score,      │ Dashboard        │
│ (sensors)     │  speed,      │  → HybridHandAI        │  warnings,   │ (live view)      │
└───────────────┘  movement    │  → BombTrainingManager │  status      └──────────────────┘
                               └────────────────────────┘
```

### Main scripts

| Script | Responsibility |
|--------|----------------|
| `BombTrainingManager` | Mission flow, timer, score, success and fail panels, haptics |
| `HybridHandAI` | Calibration, stability evaluation, warnings, danger count, final score |
| `SmartGloveToHybridAI` | WebSocket client for the ESP32 glove, with auto-reconnect |
| `UnityDashboardSender` | Streams live mission data to the instructor dashboard |
| `BombDetectorHoldOnly` | Distance-based beeping and haptic feedback |
| `DigManager` / `TerrainDigVR` | Real-time terrain heightmap digging |
| `DefuseSequence` / `WireCutterZone` | Ordered defuse steps and wire cutting |
| `LeaderboardManager` | Ranks runs by completion time |

## Getting Started

1. Open the project in **Unity 6000.1.14f1** with Android Build Support.
2. Open the main scene in `Assets/` (`main.unity` / `MR.unity`).
3. Set the network addresses in the Inspector:
   - `SmartGloveToHybridAI` → **ESP32 URL**, for example `ws://192.168.4.1:81`
   - `UnityDashboardSender` → **Dashboard Server URL**, for example `ws://<dashboard-PC-IP>:8002/unity`
4. Connect the Quest, the glove and the dashboard PC to the same network.
5. **File → Build Profiles → Android → Build And Run**.

> Without the glove, the simulation still runs. The AI simply receives no glove input.

## My Role

**Team Leader & Lead Unity / XR Developer**

Thabat was built as a team project. I led the team and owned the entire Unity experience end to end.

- 👑 **Team leadership:** led the team from the initial idea to the final demo at Defensathon 2026
- 💡 **Concept & experience design:** came up with the idea and designed the full training scenario and mission flow
- 🥽 **Full Unity development:** built the complete Mixed Reality experience on Meta Quest, including:
  - Landmine detection with distance-based audio and haptics
  - Real-time terrain digging
  - Ordered defuse sequence and wire cutting
  - Timer, scoring, leaderboard and success / fail outcomes
  - Explosion VFX, narration and audio cues
- 🧤 **Live smart-glove integration:** built the real-time integration between the experience and the live data coming from the smart glove. This powers the **Hybrid Hand AI**, which gives the trainee a **live self-assessment** of hand stability during the mission and streams the results to the instructor dashboard.

## Demo

<!-- ✏️ Replace the placeholders with your real links -->
- 🎥 Video demo: _coming soon_
- 🏆 Defensathon 2026 submission: _link_

## Credits

Third-party assets are the property of their respective authors and are used under their licenses:
Hovl Studio (Magic Effects), JMO (Cartoon FX Remaster), Staggart (Procedural Terrain Painter), SkySeries Freebie, SnowyCocoon (Camouflage Texture Pack), Unity Terrain Sample Assets, and 3D models from Sketchfab (military helmet, backpack, walkie-talkie: see each model's license for attribution).

## Author

**Leena** · [github.com/leenasync](https://github.com/leenasync)
