# System Architecture & Technical Design

This document details the software architecture, runtime flow, component relationships, and engine mechanics of the **3D Mario Platforming Game**.

---

## 1. High-Level Architecture Overview

```
                      +-------------------+
                      |   GameManager     |
                      |  (Global State)   |
                      +---------+---------+
                                |
       +------------------------+------------------------+
       |                        |                        |
+------v------+          +------v------+          +------v------+
| Player      |          | CameraFollow|          | UI / HUD    |
| Controller  |          | & Occlusion |          | & Menus     |
+------+------+          +-------------+          +-------------+
       |
       +------------+--------------+--------------+
       |            |              |              |
+------v-----+ +----v----+   +-----v-----+  +-----v-----+
| Enemies    | | Blocks  |   | Power-ups |  | Warp Pipes|
| (Goomba,   | | (? and  |   | (Fire,    |  | & Goal    |
|  Koopa)    | |  Brick) |   |  Mega)    |  |  Flagpole |
+------------+ +---------+   +-----------+  +-----------+
```

The game is built around decoupled, modular subsystems communicating through clean events, delegates, and cached component references.

---

## 2. Core Subsystems

### A. Player Controller (`Player.cs`)
- **Input Sampling**: All frame-dependent user inputs (`GetKeyDown`, `GetKey`, mouse buttons) are polled during `Update()`.
- **Coyote Time & Jump Buffering**:
  - `coyoteTimer` (0.15s window): Allows players to jump slightly after stepping off an edge.
  - `jumpBufferTimer` (0.15s window): Captures jump inputs pressed right before touching the ground and triggers immediately upon landing.
- **Physics Execution**: Forces, raycasts, drag, and velocity updates are executed during `FixedUpdate()`, maintaining deterministic physics regardless of display framerate.
- **Triple Jump Chaining**: Tracks sequential jumps within a 1.2-second window to unlock Jump 1, Jump 2, and the high acrobatic Triple Jump.
- **State & Power-ups**: Manages normal Mario, Fire Mario (fireball shooting), and Mega Mario (scale scaling, invulnerability, obstacle destruction).
- **Damage & Invulnerability**: Integrated health and life counter. Taking damage downgrades power-ups or costs a life with a 2-second visual flicker invulnerability window.

### B. Enemy AI System (`GoombaChase.cs`, `MegaGoomba.cs`, `Koopa.cs`)
- **State Machine Architecture**:
  - `Idle`: Ambient patrol and distance monitoring.
  - `Surprise`: Reaction alert when Mario enters aggro radius with sound and visual cue.
  - `Chase`: Smooth directional orientation and forward pursuit with ground dust VFX.
  - `Dead`: Triggered on top stomp or shell collision; flattens mesh, spawns rewards, and plays destruction VFX.
- **Zero-Allocation Execution**: Replaced legacy per-frame coroutine spawning with deterministic timer state machines in `Update()`, removing CPU overhead and garbage collection pauses.

### C. Dynamic Camera System (`CameraFollow.cs`, `CaveLookAt.cs`)
- **Smooth Target Tracking**: Uses `Vector3.SmoothDamp` with persistent member velocity vectors to eliminate jitter.
- **Mouse Orbital Control**: Smooth mouse yaw and pitch control with angle clamping.
- **Contextual Zone Offsets**: Automatically adapts distance, elevation, and framing depending on environment triggers (cave entrances, cloud platforms, boss arenas, flagpole finale).

### D. Central Game Manager (`GameManager.cs`)
- **Singleton Persistence**: Persists across scene transitions with `DontDestroyOnLoad(gameObject)`.
- **State Tracking**: Centralizes score, coin count, lives, checkpoints, and audio settings.
- **Pause & Time Management**: Handles safe pauses with `Time.timeScale = 0` and unpauses, providing an interactive Pause Menu with Controls Reference and Volume Slider.

### E. Visual Juice & Tweens (`JuiceManager.cs`, `CameraShake.cs`)
- **Squash and Stretch**: Applied to character transforms during jump takeoff, hard landings, and enemy stomps.
- **Spring Block Bounce**: Overshooting spring sine curves on Question Mark blocks.
- **Impact Hit-Stop**: Micro-pauses (30-50ms) on heavy impacts to enhance physical crunch.
- **Decaying Camera Shake**: Procedural rotational and translational shake with exponential decay.

---

## 3. Scene Organization & Flow

1. **`World.unity`** (Hub / World Map):
   - Overworld hub where Mario navigates between courses.
   - Stepping on level pads and pressing Space launches transition animations into levels.
2. **`Level1.unity`** (Super Bell Hill):
   - Outdoor sunny stage with grassy hills, floating platforms, question blocks, brick blocks, Goombas, Koopas, and Goal Flagpole.
   - Background music: *Super Bell Hill Extended HD*.
3. **`Level2-Cave.unity`** (Underground Cavern):
   - Cave level with lanterns, rock formations, moving cloud elevators, and Mega Goomba boss.
   - Background music: *Underground Theme*.
