# Bug Audit & Resolution Report

This document records all detected bugs, flaws, bottlenecks, and missing features discovered during the codebase audit, along with the precise resolutions implemented.

---

## 1. Critical Gameplay Bugs

### Bug 1: Dropped Jump & Action Inputs in `FixedUpdate`
- **Location**: `Player.cs` (legacy lines 135, 235, 335, 505)
- **Problem**: `Input.GetKeyDown(KeyCode.Space)`, `Input.GetKeyDown(KeyCode.V)`, and mouse click checks were placed inside `FixedUpdate()`. Because Unity clears `GetKeyDown` on frame render (`Update()`), any frame where `FixedUpdate` did not align with `Update` completely ignored user input. Jumps and attacks frequently failed to register.
- **Resolution**: Separated input polling and physics execution. All inputs are now sampled inside `Update()` and buffered using `jumpBufferTimer` and `coyoteTimer`. Physics forces and velocities execute deterministically in `FixedUpdate()`.

---

### Bug 2: Goomba & Mega Goomba Coroutine Flooding
- **Location**: `GoombaChase.cs` (lines 53-58) & `MegaGoomba.cs` (lines 52-57)
- **Problem**: `StartCoroutine(Chase())` was called every single `FixedUpdate` step (50 times/second) whenever the player was within distance. Each coroutine waited 0.4s and 0.45s before setting velocities and sounds, spawning hundreds of concurrent overlapping coroutines. This caused extreme CPU throttling, audio clipping, and erratic enemy movement.
- **Resolution**: Replaced coroutines with a clean, high-performance state machine (`GoombaState`: `Idle`, `Surprise`, `Chase`, `Dead`) driven by internal timers in `Update()`. Reduced CPU usage to near 0% with zero garbage generation.

---

### Bug 3: Infinite Loop on Underground Pipe Return
- **Location**: `PipeEntryManager.cs` (line 191)
- **Problem**: 
  ```csharp
  while (Camera.main.transform.GetChild(2).GetComponent<AudioSource>().volume < 0.6)
  {
      Camera.main.transform.GetChild(2).GetComponent<AudioSource>().volume -= 0.05f;
      yield return new WaitForSeconds(0.01f);
  }
  ```
  The condition checked if volume was *less* than 0.6 while *subtracting* 0.05! This created an infinite while-loop that locked the coroutine permanently.
- **Resolution**: Fixed condition to check `while (volume > 0.05f)` with proper clamping and safety timeouts.

---

### Bug 4: Memory Leak in Koopa Shell Particle Instantiation
- **Location**: `KoopaShell.cs` (line 104)
- **Problem**:
  ```csharp
  GameObject clone = Instantiate(spark, collision.contacts[0].point, spark.transform.rotation).gameObject;
  spark.Play();
  ```
  Every time the shell bounced off a wall, a new GameObject was instantiated in the scene hierarchy and never destroyed. Furthermore, `spark.Play()` played the prefab/source asset rather than the clone.
- **Resolution**: Stored the instantiated clone, triggered playback on the clone, and scheduled automated destruction with `Destroy(sparkClone.gameObject, 1.2f)`.

---

### Bug 5: Modification of Prefab Asset in Memory
- **Location**: `Player.cs` (line 809)
- **Problem**:
  ```csharp
  GameObject clone = Instantiate(Coin, other.transform.position, Coin.transform.rotation);
  ...
  Coin.GetComponent<Rigidbody>().useGravity = false;
  ```
  Instead of mutating `clone`, the script directly mutated `Coin` (the project asset reference), corrupting the prefab in memory.
- **Resolution**: Fixed to reference `coinRb` and `clone` directly.

---

### Bug 6: Local Velocity Reset in `Vector3.SmoothDamp`
- **Location**: `CameraFollow.cs` (lines 80-100), `PlayerLookAtHubWorld.cs` (line 27), `lookatscript.cs` (line 28), `CaveLookAt.cs` (line 32)
- **Problem**: Declaring `Vector3 none = Vector3.zero;` locally inside `LateUpdate` or `Update` before `Vector3.SmoothDamp(..., ref none, ...)`. `SmoothDamp` relies on preserving the velocity variable between frames. Reinitializing it to zero every frame disabled velocity tracking, causing jerky and stuttering camera motion.
- **Resolution**: Promoted all `SmoothDamp` velocity parameters to persistent class member variables.

---

### Bug 7: Infinite Spawning of Cloud Platforms
- **Location**: `CloudPlatform.cs` & `CloudPlatformSpawner.cs`
- **Problem**: `CloudPlatformSpawner` repeatedly spawned cloud platforms every 1.5 seconds, while `CloudPlatform` moved upward infinitely without ever being destroyed. Over minutes of gameplay, hundreds of moving colliders accumulated in the scene.
- **Resolution**: Added an automated lifetime timer `Destroy(gameObject, lifeTime);` to clean up platforms once out of view.

---

### Bug 8: Negative Audio Volume Arithmetic Overflow
- **Location**: `WorldMapLevelEnter.cs` (lines 75, 105)
- **Problem**:
  ```csharp
  for (int i = 100; i >= 0; i--)
  {
      Camera.main.transform.parent.GetComponent<AudioSource>().volume -= 0.025f;
      yield return new WaitForSeconds(0.01f);
  }
  ```
  Decreasing volume by 0.025 over 101 iterations deducted 2.525 from a volume between 0 and 1, forcing volume into negative numbers.
- **Resolution**: Implemented smooth fading using `Mathf.Max(0f, volume - fadeRate)`.

---

### Bug 9: Endless Falling Void Lock
- **Location**: `Player.cs`
- **Problem**: Mario had no out-of-bounds boundary check. If Mario fell off a platform into the void, he fell infinitely downwards into negative Y space. If falling while in groundpound, `while (!grounded)` never exited, locking controls forever.
- **Resolution**: Added a fall boundary threshold (`transform.position.y < -25f`) that catches fallen players, deducts a life, plays death audio, and respawns Mario at the last active checkpoint. Groundpound was also protected with a maximum fall duration safety timeout.

---

### Bug 10: Missing Damage for Regular Mario
- **Location**: `Player.cs`, `GoombaChase.cs`, `MegaGoomba.cs`
- **Problem**: When Mario collided with a Goomba, the code only checked `if (Player.FireMario) Downgrade_FireSuit()`. If Mario was regular Mario, zero damage was applied and nothing happened!
- **Resolution**: Created a comprehensive `TakeDamage(int damage)` method on `Player.cs`. If Fire Mario, downgrades suit; if regular Mario, deducts a life, triggers a 2-second invulnerability flicker, and initiates death/game over if lives reach 0.
