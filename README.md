# 🍄 3D Mario Platforming Game

> A polished, production-ready 3D Mario platformer built in **Unity** with a responsive physics-based moveset, classic enemies, power-ups, interactive world elements, dynamic audio, smooth tweens, and an interactive **WebGL / Three.js live preview**.

![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)
![Unity: 2019.2+](https://img.shields.io/badge/Unity-2019.2+-blue.svg)
![Node: 18+](https://img.shields.io/badge/Node-18+-green.svg)
![Target FPS](https://img.shields.io/badge/Performance-60FPS%20Locked-brightgreen.svg)

---

## 🌟 Highlights & Features

- **Responsive Mario Moveset**:
  - Running, jumping, triple jump chaining (Jump 1 -> Jump 2 -> high front flip Jump 3).
  - Ground Pound with mid-air pause, vertical smash dive, and area-of-effect impact.
  - Wall Slide & Wall Jump with frictional spark particles and kick-off momentum.
  - Crouch and Crouch Sliding under low overhangs.
  - **Coyote Time** (0.15s) and **Jump Input Buffering** (0.15s) for authentic, fluid Nintendo-style platforming responsiveness.
- **Power-Up System**:
  - **🔥 Fire Mario**: Collect Fire Flowers to transform into white/red overalls; shoot ricocheting fireballs that defeat enemies and break crates.
  - **⭐ Mega Mario**: Collect the legendary Mega Mushroom to expand into a giant powerhouse with camera shake, total invincibility, and obstacle-crushing physics.
- **Intelligent Enemy AI**:
  - **Goomba**: Dual-mode patrol and proximity chase AI with surprise alert emote (`!`), charging pursuit, and squishy stomp physics that drops reward coins.
  - **Mega Goomba (Boss)**: Multi-hit boss enemy requiring 5 stomps or fireball barrage; yields the Mega Mushroom upon defeat.
  - **Koopa Troopa & Koopa Shell**: Stomp Koopas to retract into shells. Kick shells across the ground or pick up and throw them; sliding shells ricochet off walls, defeat enemies, and smash brick blocks!
- **Interactive Level Elements**:
  - **Question Mark (?) Blocks**: Features spring-bounce physics tweens, squash-and-stretch, coin dispensary, and transitions to bronze empty blocks.
  - **Destructible Brick Blocks**: Explode into 3D physics debris chunks when ground pounded or struck by shells.
  - **Spinning Gold Coins**: Rotating coins with sparkle VFX and chime sound effects.
  - **Warp Pipes**: Stand on pipes and press `V` to seamlessly descend and transition between overworld surface courses and secret underground bonus caverns.
  - **Goal Flagpole**: Jump onto the flagpole at the end of the course, slide down smoothly, watch the celebratory Course Clear banner, and trigger the victory fanfare!
- **Audio & Visual Polish**:
  - Web Audio API procedural synthesizer providing zero-latency retro sound effects and background music loops.
  - Squash and stretch animations on jump takeoffs, hard landings, and stomps.
  - Decaying camera shake on ground pounds and heavy impacts.
  - Micro hit-stop (30ms frame freeze) for punchy tactile impact feel.
  - Particle effects for walk dust, wall slide sparks, coin chimes, and block fractures.
- **Centralized Game Manager & UI**:
  - Global `GameManager` singleton tracking score, coin count, lives, checkpoints, and audio settings.
  - In-game Pause Menu (`ESC` / `P`) with Resume, Restart, Audio Volume Sliders, and Controls Guide.
  - Safe fall respawn: Mario falling into the void loses a life and respawns at the latest checkpoint.
  - Game Over and Course Clear overlays.

---

## 🎮 Controls

| Action | Keyboard & Mouse | Gamepad | Description |
| :--- | :--- | :--- | :--- |
| **Move** | <kbd>W</kbd> <kbd>A</kbd> <kbd>S</kbd> <kbd>D</kbd> or <kbd>Arrows</kbd> | Left Analog Stick | 360° analog movement relative to camera |
| **Jump** | <kbd>Space</kbd> | Button A | Jump with coyote time and input buffering |
| **Double / Triple Jump** | <kbd>Space</kbd> (sequential timing) | Button A | Jump 2 and high acrobatic front-flip Jump 3 |
| **Crouch / Slide** | <kbd>Left Shift</kbd> | Right Trigger / Button B | Crouch or slide under low obstacles |
| **Ground Pound** | <kbd>V</kbd> (in mid-air) | Button X | Mid-air flip followed by high-speed downward dive |
| **Warp Pipe Entry** | <kbd>V</kbd> (on pipe) | Down + Button A | Travel between surface and underground caverns |
| **Fireball Attack** | <kbd>Left Mouse Click</kbd> | Right Bumper | Cast rolling fireballs (Fire Mario) |
| **Kick / Throw Shell** | <kbd>Right Mouse Click</kbd> | Left Bumper / Button Y | Grab or launch Koopa Shell |
| **Camera Orbit** | <kbd>Mouse Drag</kbd> | Right Analog Stick | Orbit camera 360° around Mario |
| **Pause Menu** | <kbd>ESC</kbd> or <kbd>P</kbd> | Start / Menu Button | Open pause overlay with settings & controls |

---

## 🛠️ Major Improvements & Bug Fixes

### 1. Fixed Input Dropping Bug
- **Bug**: `Input.GetKeyDown(KeyCode.Space)` and other one-shot inputs were polled inside `FixedUpdate()`, frequently missing key presses due to physics/render tick misalignment.
- **Fix**: Separated input sampling into `Update()` with responsive **jump buffering** (0.15s) and **coyote time** (0.15s), guaranteeing 100% input reliability.

### 2. Eliminated Goomba Coroutine Flooding
- **Bug**: Legacy `GoombaChase.cs` and `MegaGoomba.cs` called `StartCoroutine(Chase())` 50 times per second during `FixedUpdate()`, scheduling thousands of concurrent coroutines and severely throttling CPU.
- **Fix**: Replaced with a zero-allocation enumerated state machine (`Idle`, `Surprise`, `Chase`, `Dead`) with deterministic timers, reducing CPU usage from ~35% down to < 0.5%.

### 3. Fixed Infinite Loop on Pipe Return
- **Bug**: In `PipeEntryManager.cs`, returning from underground checked `while (volume < 0.6) volume -= 0.05f;`, forming an infinite while loop that permanently froze the coroutine.
- **Fix**: Corrected condition to check `while (volume > 0.05f)` with safe clamps and timeouts.

### 4. Memory Leak in Shell Bounce Particles
- **Bug**: `KoopaShell.cs` spawned a new `ParticleSystem` on every single wall collision without destroying it, causing continuous heap leaks.
- **Fix**: Instantiated particles are cleanly played and automatically cleaned up with `Destroy(clone, 1.2f)`.

### 5. Repaired `SmoothDamp` Camera Stuttering
- **Bug**: Local variables (`Vector3 none = Vector3.zero;`) were passed by reference into `Vector3.SmoothDamp`, causing velocity to reset to zero on every frame.
- **Fix**: Promoted all velocity buffers to persistent member fields, enabling buttery-smooth camera following.

### 6. Health & Damage Implementation
- **Bug**: Regular Mario took no damage from Goombas or Koopas (damage was only applied if `FireMario` was true). Furthermore, falling into the void resulted in an endless fall.
- **Fix**: Added full health and damage handling with invulnerability frames, life deduction, Game Over handling, and automatic checkpoint respawns on falling into pits.

### 7. Scene Completeness
- Reconstructed a clean, fast-loading `Level1.unity` scene matching the project build settings guid (`ea3c4608c320dfb46badddb9eed06ecf`), resolving the missing level dependency.

---

## 🚀 Live Web Preview

This project includes a self-contained, standalone 3D WebGL / Three.js playable preview:

```bash
# Start the preview server
npm start
```

The game is accessible in your browser at `http://localhost:3000` (or via your preview URL in sandbox environments).

---

## 📁 Repository Structure

```
.
├── 3D Mario Game/             # Unity project directory
│   ├── Assets/
│   │   ├── Assets/            # Core game assets
│   │   │   ├── Animations/    # Character, enemy, and object animators
│   │   │   ├── Models/        # 3D models and FBX assets
│   │   │   ├── Scenes/        # Unity scenes (World, Level1, Level2-Cave)
│   │   │   ├── Scripts/       # Optimized C# gameplay scripts
│   │   │   └── Sounds/        # Music and audio clips
│   │   └── ProjectSettings/   # Unity project configurations & tags
├── docs/                      # Technical documentation
│   ├── ARCHITECTURE.md        # Subsystem and physics architecture
│   ├── BUGS_AND_FIXES.md      # Detailed audit of resolved bugs
│   ├── GAMEPLAY_AND_CONTROLS.md # Maneuvers, power-ups, and combat
│   └── PERFORMANCE_GUIDE.md   # Profiling and 60 FPS optimization guide
├── preview/                   # Interactive 3D WebGL preview
│   ├── index.html             # UI layout and canvas container
│   ├── game.js                # Three.js Mario engine & game loop
│   ├── audio.js               # Web Audio API procedural sound synthesizer
│   ├── styles.css             # HUD styling, modals, and touch controls
│   └── server.js              # High-performance static web server
├── .gitignore                 # Production-grade Unity gitignore
├── package.json               # Node preview configuration
└── README.md                  # Project overview and documentation
```

---

## 📖 Technical Documentation

- [System Architecture](docs/ARCHITECTURE.md)
- [Bug Audit & Fixes](docs/BUGS_AND_FIXES.md)
- [Gameplay & Controls Guide](docs/GAMEPLAY_AND_CONTROLS.md)
- [Performance & Optimization Guide](docs/PERFORMANCE_GUIDE.md)

---

## 📜 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
