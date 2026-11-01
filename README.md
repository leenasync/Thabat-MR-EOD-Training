# Thabat (ثبات) — AI-Enhanced Mixed Reality EOD Training

> A Mixed Reality training simulation on Meta Quest for **Explosive Ordnance Disposal (EOD)**. Trainees defuse explosive devices by cutting the right wires under time pressure, while a **smart glove** and a **hybrid AI model** measure how steady their hands are in real time.

🏆 **3rd Place, Robotics Track, Defensathon 2026**

![Unity](https://img.shields.io/badge/Unity-6000.3-black?logo=unity)
![Meta Quest](https://img.shields.io/badge/Platform-Meta%20Quest%20MR-0467DF?logo=meta)
![OpenXR](https://img.shields.io/badge/XR-OpenXR%20%2B%20XRI%203-green)
![ESP32](https://img.shields.io/badge/Hardware-ESP32%20Smart%20Glove-E7352C?logo=espressif)
![Award](https://img.shields.io/badge/Defensathon%202026-3rd%20Place%20%C2%B7%20Robotics-FFB000?logo=trophy)

---

## Overview

*Thabat* means **"steadiness"** in Arabic. In bomb disposal, a shaking hand can cost a life.

The simulation brings the explosive device into the trainee's real room using Mixed Reality passthrough. The trainee must:

1. **Gear up:** place the helmet, backpack and radio. A checklist confirms each item.
2. **Start the mission:** the device appears in front of the trainee and the countdown begins.
3. **Defuse it:** use the wire cutter to cut the correct wire, across **three levels** of increasing difficulty.
4. **Stay steady:** a smart glove streams hand-motion data the whole time. A shaky hand at the wrong moment can fail the mission.
5. **Beat the clock.** A wrong wire or running out of time triggers an explosion.

The **Hybrid Hand AI** gives the trainee a live self-assessment of hand stability, and an instructor can follow everything on a **live dashboard**.

## Features

- 🧤 **ESP32 smart glove:** streams vibration, speed and movement over WebSocket
- 🤖 **Hybrid Hand AI:** calibrates the trainee's normal hand movement, detects shaking, issues warnings and danger alerts, and outputs a stability score
- 📈 **Adaptive training levels:** three bomb levels, where hand stability directly affects the outcome. Each level records its score, warnings and danger count.
- 🖥️ **Live AI HUD:** the trainee sees their own hand-stability status inside the headset
- 📊 **Live instructor dashboard:** Unity streams glove data, AI score, warnings and mission status in real time
- ✂️ **Physical wire cutting:** an animated cutter with real jaw movement, and correct and wrong wires
- 🎒 **Gear checklist:** XR sockets for the helmet, backpack and radio
- ⏱️ **Countdown timer** with success and fail outcomes
- 💥 **Explosion and feedback:** VFX, sound and UI feedback

## Tech Stack

| Layer | Technology |
|-------|------------|
| Engine | Unity 6 (6000.3.21f1), URP |
| XR | Meta XR SDK Core, OpenXR, XR Interaction Toolkit 3, XR Hands |
| Device | Meta Quest (Mixed Reality passthrough) |
| Hardware | ESP32 smart glove with motion and vibration sensors |
| Networking | WebSocket (glove → Unity → dashboard) |
| Language | C# |

## Architecture

```
┌───────────────┐  WebSocket   ┌──────────────────────────┐  WebSocket   ┌──────────────────┐
│ ESP32 Smart   │ ───────────▶ │  Unity MR (Quest)        │ ───────────▶ │ Instructor       │
│ Glove         │  vibration,  │  SmartGloveToHybridAI    │  score,      │ Dashboard        │
│ (sensors)     │  speed,      │  → HybridHandAI          │  warnings,   │ (live view)      │
└───────────────┘  movement    │  → AdaptiveTrainingMgr   │  status      └──────────────────┘
                               └──────────────────────────┘
```

### Main scripts

| Script | Responsibility |
|--------|----------------|
| `AdaptiveTrainingManager` | Mission flow, three bomb levels, panels, per-level results |
| `HybridHandAI` | Calibration, stability evaluation, warnings, danger count, final score |
| `SmartGloveToHybridAI` | WebSocket client for the ESP32 glove, with auto-reconnect |
| `UnityDashboardSender` | Streams live mission data to the instructor dashboard |
| `LiveHandUI` | In-headset live AI status |
| `BomWire` / `WireCut` / `CutterController` / `WireCutterZone` | Wire-cutting interaction and correct or wrong wire logic |
| `BombTimer` / `BombResult` | Countdown, success and fail outcomes |
| `SocketChecklist` / `EquipHandler` | Gear check-in before the mission |

## Getting Started

1. Open the project in **Unity 6000.3.21f1** with Android Build Support.
2. Open the scene `Assets/Scenes/defensTHONE3rd.unity`.
3. Set the network addresses in the Inspector:
   - `SmartGloveToHybridAI` → **ESP32 URL**, for example `ws://192.168.4.1:81`
   - `UnityDashboardSender` → **Dashboard Server URL**, for example `ws://<dashboard-PC-IP>:8002/unity`
4. Connect the Quest, the glove and the dashboard PC to the same network.
5. **File → Build Profiles → Android → Build And Run**.

> Without the glove, the simulation still runs. The AI simply receives no glove input.

## My Role

**Team Leader & Lead Unity / XR Developer**

Thabat was built as a team project. I led the team and owned the entire Unity experience end to end.

- **Team leadership:** led the team from the initial idea to the final demo at Defensathon 2026
- 💡 **Concept & experience design:** came up with the idea and designed the full training scenario and mission flow
- 🥽 **Full Unity development:** built the complete Mixed Reality experience on Meta Quest, including:
  - Gear check-in and mission start
  - Three adaptive bomb levels and wire-cutting interaction
  - Timer, scoring and success / fail outcomes
  - Explosion VFX and audio cues
  - All UI programming and interaction logic
- 🧤 **Live smart-glove integration:** built the real-time integration between the experience and the live data coming from the smart glove, and tied it into the concept and the gameplay:
  - Receiving the glove data live inside Unity (WebSocket, auto-reconnect)
  - The **Hybrid Hand AI**, which calibrates and evaluates hand stability and gives the trainee a **live self-assessment** during the mission
  - Making hand stability **drive the gameplay**: wire cutting, adaptive levels, scoring and mission outcome
  - Streaming the live assessment to the instructor dashboard
- 🏆 Led the team to **3rd Place in the Robotics Track at Defensathon 2026**

> Thabat was a team effort. The smart-glove hardware and the visual UI/UX design (layout, shapes and colors) were created by my teammates.

## Demo

<!-- ✏️ Replace the placeholders with your real links -->
- 🎥 Video demo: _coming soon_
- 🏆 Defensathon 2026 submission: _link_

## Credits

Third-party assets are the property of their respective authors and are used under their licenses:
Hovl Studio (Magic Effects), SnowyCocoon (Camouflage Texture Pack), Unity XR Interaction Toolkit and XR Hands samples, and 3D models from Sketchfab (military helmet, backpack, walkie-talkie: see each model's license for attribution).

## Author

**Leena** · [github.com/leenasync](https://github.com/leenasync)
