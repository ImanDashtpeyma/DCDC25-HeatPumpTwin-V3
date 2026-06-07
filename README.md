# HeatPumpTwin — V3
### Mixed Reality Digital Twin for Guided HVAC Training with Physical Hardware Integration

> **Course:** Design for Complex and Dynamic Contexts (DCDC 2025)
> **Author:** Iman Dashtpeyma
> **Supervisors:** Jordi Solsona Belenguer, Charles Windlin

---

<p align="center">
  <img src="./Poster-Video/HeatPump-Poster.jpg" width="30%">
</p>

## Introduction

HeatPumpTwin V3 is a collaborative Mixed Reality (MR) digital twin prototype designed to support guided, role-based HVAC configuration training in a shared physical and digital space.

This version extends V2 with a key addition: a **physical hardware prototype** — a cardboard-scale HVAC model embedded with an Arduino UNO R4 WiFi — that responds in real time to approval decisions made inside the MR environment via MQTT. When the Engineer approves a configuration change, the physical model responds immediately: the LED turns green, the fans spin, and the buzzer confirms. When a change is rejected, the hardware reflects that state too.

V3 introduces a **training scenario**: the Engineer is an experienced professional who uses the shared MR space to teach and guide a new Technician through HVAC configuration. Both users are physically colocated — in the same room, wearing Meta Quest 3 headsets — sharing both the physical HVAC model and the digital twin overlaid on their real environment.

---

## What's New in V3

| Area | V2 | V3 |
|---|---|---|
| Scenario | Generic collaborative configuration | Engineer teaches Technician |
| Physical hardware | None | Arduino UNO R4 WiFi + fans + LED + buzzer |
| IoT integration | Simulated | MQTT over WiFi (test.mosquitto.org) |
| Hardware response | None | LED, fans, and buzzer reflect approval state |
| Physical model | None | Cardboard-scale HVAC model with embedded electronics |

---

## Project Goal

HeatPumpTwin V3 explores how a shared Mixed Reality digital twin — connected to real physical hardware — can support guided, colocated HVAC training between an experienced Engineer and a new Technician.

The project investigates how two users physically sharing the same space can:

- inspect a shared digital twin overlaid on a physical HVAC model
- propose and review configuration changes through role-based interaction
- receive immediate physical feedback when a configuration is approved or rejected
- experience the connection between digital decisions and real hardware response

---

## Scenario

An Engineer and a Technician are physically in the same room. Both wear Meta Quest 3 headsets. A cardboard-scale HVAC model sits between them — it contains a real Arduino, fans, LEDs, and a buzzer.

The Engineer uses the MR environment to guide the Technician through the configuration process. The Technician proposes changes. The Engineer reviews them. When the Engineer approves, the physical hardware responds — not just the UI. This makes the learning moment tangible, immediate, and memorable.

### Interaction Flow

1. The Engineer selects a spatial location using map pins.
2. A virtual heat pump appears in the shared MR space, anchored to the physical model.
3. The Technician adjusts operational parameters (power, pressure, phase).
4. The system enters a **Pending** state — LED turns red, fans off, buzzer signals.
5. The Engineer reviews the proposed configuration.
6. If **approved**: LED turns green, fans spin, buzzer double-beeps. State syncs across all users.
7. If **rejected**: LED turns blue, fans off, buzzer plays descending tones.

The physical hardware response makes the approval decision visible not just in MR, but in the real world.

---

## Design Process

### Design Goals

- Support real-time guided training between Engineer and Technician in a colocated MR environment
- Make the connection between digital decisions and physical hardware response visible and immediate
- Reflect a realistic industrial workflow: propose → validate → apply
- Use the physical model to make the digital twin tangible for the Technician

### Challenges

- Synchronizing MR state changes with physical hardware in real time
- Keeping MQTT communication reliable over a shared WiFi network
- Designing hardware response patterns that are distinct and immediately understandable
- Balancing simplicity of interaction with meaningful feedback across both the digital and physical layer

### Solution

- Role-based MR workflow (Technician proposes, Engineer validates)
- MQTT bridge from Unity to Arduino over WiFi
- Arduino hardware responds to `pending`, `approved`, and `rejected` messages
- Three distinct hardware states: LED color + buzzer tone + fan state

---

## Digital Twin Concept

The digital twin in V3 represents:

- a virtual HVAC heat pump anchored spatially to a physical cardboard model
- parameter states: power, pressure, phase
- status transitions: **Pending → Applied** or **Pending → Rejected**
- real-time synchronization across all connected MR users
- **physical state mirroring**: the cardboard model reflects digital state via MQTT

The twin functions as both a collaborative decision interface and a training tool. The physical hardware layer closes the loop between digital action and real-world consequence.

---

## Collaboration and Colocation

### Physical Colocation

Both the Engineer and the Technician are physically in the same room. They share:

- the same physical space
- the same MR overlay (Meta Quest 3)
- the same physical HVAC model
- the same digital twin state

The Engineer can point to the physical model while adjusting the digital twin. The Technician can see both the MR interface and the hardware respond to the same action.

### Multi-User Synchronization

- Photon PUN 2 networking
- RPC-based state updates
- Dynamic instantiation of `NetworkAvatar` via `Resources`
- Real-time synchronization of head and hand presence

### Role-Based Control

- The Technician proposes configuration changes
- The Engineer reviews and approves or rejects
- The approval workflow enforces structured learning, not just safety

---

## Physical Hardware Integration

### Hardware Components

| Component | Model | Purpose |
|---|---|---|
| Microcontroller | Arduino UNO R4 WiFi | WiFi + MQTT client |
| RGB LED | Common cathode | Visual state indicator |
| Relay module | TONGLING JQC-3FF-S-Z (5V) | Fan control (avoids exceeding 40mA pin limit) |
| DC Fans (×2) | LD3007MS 5V / 0.2A | Physical airflow feedback |
| Passive Buzzer | — | Audio feedback per state |

### Pin Mapping

| Pin | Component |
|---|---|
| 12 | LED Red |
| 11 | LED Green |
| 10 | LED Blue |
| 7 | Relay (fan control) |
| 9 | Passive Buzzer |

### MQTT Configuration

- **Broker:** `test.mosquitto.org`
- **Port:** `1883`
- **Topic:** `hvac/heatpumptwin/iman2026`

### Hardware State Behavior

| MQTT Message | LED | Fans | Buzzer |
|---|---|---|---|
| `pending` | 🔴 Red | Off | One long low tone |
| `approved` | 🟢 Green | On | Two short high beeps |
| `rejected` | 🔵 Blue | Off | Three descending tones |
| Startup | 🟢 Green | Off | Three ascending notes |

### Why a Relay Instead of Direct Pin Control?

The DC fans draw 200mA each. Arduino digital output pins are limited to 40mA. Driving the fans directly would damage the board. The relay module acts as a switch: the Arduino controls the relay with a safe signal current, and the relay switches the fan circuit independently.

---

## Features

- Real-time multi-user MR collaboration
- Shared digital twin visualization anchored to a physical model
- Role-based guided training workflow (Engineer → Technician)
- Parameter editing for HVAC configuration (power, pressure, phase)
- Approval and rejection logic with physical hardware response
- MQTT bridge from Unity to Arduino over WiFi
- LED, fan, and buzzer feedback reflecting approval state
- Photon-based synchronized state updates across MR clients
- Hand-tracked interaction in mixed reality
- Spatially anchored asset selection using map pins

---

## Technical Implementation

### Software Stack

| Technology | Version | Purpose |
|---|---|---|
| Unity | 2022.3.62f2 (URP) | Core development platform |
| Meta XR SDK | 74.0.2 | MR, hand tracking, passthrough |
| Photon PUN 2 | Free | Multi-user networking |
| OpenXR | — | XR runtime |
| ArduinoMqttClient | — | MQTT client on Arduino |
| WiFiS3 | — | WiFi on Arduino UNO R4 |

### Architecture Overview

```
[Meta Quest 3 — Engineer]          [Meta Quest 3 — Technician]
        |                                       |
        └──────────── Photon PUN 2 ─────────────┘
                           |
                    Unity MQTT Client
                           |
               test.mosquitto.org:1883
                           |
                    Arduino UNO R4 WiFi
                    (topic: hvac/heatpumptwin/iman2026)
                           |
              ┌────────────┼────────────┐
            RGB LED      Relay        Buzzer
                           |
                        Fans ×2
```

### Key Scripts

- `NetworkManager` — Photon connection and room join
- `NetworkAvatar` — instantiated via `PhotonNetwork.Instantiate`
- `TwinNetworkHub` — synchronized digital twin state
- MQTT publisher — sends `pending` / `approved` / `rejected` to broker on state change

RPC methods control: pin selection, parameter proposal, approval, rejection, and MQTT publish.

---

## Installation and Setup

### Prerequisites

- Unity Hub
- Unity **2022.3.62f2**
- Meta Quest 3 in Developer Mode with USB Debugging enabled
- Git
- Photon Realtime App ID
- Arduino IDE (for Arduino setup)
- Arduino UNO R4 WiFi with required components

### Clone Repository

```bash
git clone https://github.com/ImanDashtpeyma/DCDC25-HeatPumpTwin-V3
cd DCDC25-HeatPumpTwin-V3
```

---

### Unity Setup

#### 1. Open in Unity Hub

Open the cloned folder using Unity Hub. Make sure the project uses **Unity 2022.3.62f2**.

#### 2. XR Configuration

Before running, confirm these settings are enabled:

- **OpenXR** under XR Plug-in Management
- **Meta Quest support**
- **Hand Tracking** in Meta XR settings
- **Android** as the build platform
- Passthrough / MR permissions on the headset if needed
- Main scene in **Scenes in Build**: `MainScene.unity`

#### 3. Photon PUN 2 Setup

- Obtain a **Photon App ID** from the Photon Dashboard
- Import **PUN 2 Free** from the Unity Asset Store if needed
- Configure via: `Window → Photon Unity Networking → PUN Wizard → AppId Realtime`
- Or manually edit: `Assets/Photon/PhotonUnityNetworking/Resources/PhotonServerSettings.asset`

Make sure `Assets/Resources/NetworkAvatar.prefab` exists. Do **not** place it manually in the scene — it is instantiated dynamically at runtime.

#### 4. Run Single-User Test

- Open `MainScene.unity`
- Press **Play** in Unity
- Verify Photon connection in Console
- Client joins default room: `PassthroughRoom`

#### 5. Build for Meta Quest 3

- **File → Build Settings → Android**
- Add correct scenes to **Scenes in Build**
- Connect Meta Quest 3 via USB
- Click **Build and Run**

#### 6. Multi-User Testing

**Option A:** Run one client in Unity Editor + one as a standalone build on Quest.

**Option B:** Install APK on one Quest, run second client in Unity Editor.

Both clients must use the same Photon App ID and room name, and the same project version.

---

### Arduino Setup

#### 1. Install Arduino IDE Libraries

In the Arduino IDE, install:

- `ArduinoMqttClient` (by Arduino)
- `WiFiS3` (included with Arduino UNO R4 board package)

Install the UNO R4 board package via **Tools → Board → Boards Manager** (search: `Arduino UNO R4`).

#### 2. Configure WiFi and MQTT

Open `HeatPumpTwin.ino` and update:

```cpp
const char* WIFI_SSID = "your_wifi_name";
const char* WIFI_PASS = "your_wifi_password";
```

The broker and topic are pre-configured:

```cpp
const char* BROKER = "test.mosquitto.org";
const int   PORT   = 1883;
const char* TOPIC  = "hvac/heatpumptwin/iman2026";
```

#### 3. Wire the Hardware

| Arduino Pin | Component |
|---|---|
| 12 | LED Red (with resistor) |
| 11 | LED Green (with resistor) |
| 10 | LED Blue (with resistor) |
| 7 | Relay module signal (S pin) |
| 9 | Passive Buzzer |
| GND | LED cathode, Relay GND, Buzzer GND |
| 5V | Relay VCC, Fan power (via relay) |

#### 4. Upload Sketch

- Select **Board:** Arduino UNO R4 WiFi
- Select the correct **Port**
- Click **Upload**
- Open Serial Monitor at **9600 baud** to verify WiFi and MQTT connection

On successful startup, the buzzer plays three ascending notes.

---

## Usage

### Full Workflow

1. Power on the Arduino and confirm it connects (Serial Monitor shows `✅ WiFi!` and `✅ MQTT!`).
2. Start the Unity application and connect both users to the same Photon room.
3. The Engineer selects a location using a map pin — the virtual HVAC twin appears.
4. The Technician adjusts parameters (pressure, power, phase).
5. The system marks changes as **Pending** → Arduino receives `pending` → LED red, fans off, buzzer sounds.
6. The Engineer reviews the proposal and guides the Technician on the reasoning.
7. If **approved** → Arduino receives `approved` → LED green, fans spin, buzzer double-beeps.
8. If **rejected** → Arduino receives `rejected` → LED blue, fans off, buzzer descends.
9. State syncs across all connected MR users.

---

## Demo and Media

- **Video:** [Watch the demo video](https://drive.google.com/file/d/14ErgwlhwLzNp9T4oNFWZBnRtnHiqfCYw/)
- **V2 Repository:** [HeatPumpTwin V2](https://github.com/ImanDashtpeyma/DCDC25-HeatPumpTwin-V2)
- **Poster:** [View poster](https://github.com/ImanDashtpeyma/DCDC25-HeatPumpTwin-V2/blob/main/Poster-Video/HeatPump-Poster.jpg)

---

## References

### Assets and Libraries

| Resource | Purpose | License / Link |
|---|---|---|
| Unity 2022.3 URP | Core development platform | Unity License |
| Meta XR SDK 74.0.2 | MR, hand tracking, passthrough | Meta License |
| Photon PUN 2 (Free) | Multi-user networking | [Asset Store](https://assetstore.unity.com/packages/tools/network/pun-2-free-119922) |
| ArduinoMqttClient | MQTT client on Arduino | Arduino License |
| WiFiS3 | WiFi for Arduino UNO R4 | Arduino License |
| VR Interaction Framework | Base interaction utilities | Asset Store License |
| Rooftop AC HVAC Unit | 3D model for MR environment | [Asset Store](https://assetstore.unity.com/packages/3d/props/industrial/rooftop-ac-hvac-unit-315763) |
| Meta Avatar SDK | Avatar representation for users | [Asset Store](https://assetstore.unity.com/packages/tools/integration/meta-avatars-sdk-271958) |

---

## Known Limitations

- The MQTT broker (`test.mosquitto.org`) is public and unencrypted — not suitable for production use
- Cloud HVAC data is simulated; the prototype does not connect to a live HVAC backend
- The physical model is a proof-of-concept at cardboard scale
- Testing was conducted with a limited number of clients
- The system supports one HVAC asset type

## Future Work

- **Data Richness** — integrate real telemetry and maintenance records
- **Asset Support** — expand to multiple HVAC asset types
- **UI Optimization** — improve panel layout for faster parameter editing in MR
- **Secure MQTT** — replace public broker with authenticated, encrypted endpoint
- **Scale** — transition from cardboard model to industrial-grade physical prototype

---

## Contributor and Maintainer

**Author and Maintainer:** Iman Dashtpeyma
**Course:** Design for Complex and Dynamic Contexts (DCDC 2025)
**Institution:** Stockholm University, DSV

**Supervisors**
- Jordi Solsona Belenguer
- Charles Windlin

---

## License

MIT License © 2025 Iman Dashtpeyma

Supervised by Jordi Solsona Belenguer and Charles Windlin
Stockholm University, DSV
