using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public int currentLives = 5;
    public int currentCoins = 0;
    public int currentScore = 0;
    public Vector3 currentCheckpoint = Vector3.zero;
    public bool isPaused = false;

    [Header("Audio Settings")]
    public float masterVolume = 1f;
    public float musicVolume = 0.8f;
    public float sfxVolume = 1f;

    private bool showPauseMenu = false;
    private bool showControlsGuide = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            currentLives = Lives.LIVES;
            currentCoins = CoinCollect.COIN_COUNT;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // Keep static variables in sync
        currentLives = Lives.LIVES;
        currentCoins = CoinCollect.COIN_COUNT;

        // Toggle Pause
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        showPauseMenu = isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        AudioListener.pause = isPaused;
    }

    public void ResumeGame()
    {
        isPaused = false;
        showPauseMenu = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    public void RestartCurrentLevel()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        isPaused = false;
        showPauseMenu = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToWorldMap()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        isPaused = false;
        showPauseMenu = false;
        SceneManager.LoadScene("World");
    }

    public void AddScore(int points)
    {
        currentScore += points;
    }

    // Clean IMGUI fallback pause overlay if no custom Canvas pause menu is active in scene
    void OnGUI()
    {
        if (!showPauseMenu) return;

        GUI.skin.box.fontSize = 20;
        GUI.skin.button.fontSize = 16;
        GUI.skin.label.fontSize = 16;

        float width = 340;
        float height = 400;
        float x = (Screen.width - width) / 2f;
        float y = (Screen.height - height) / 2f;

        GUI.Box(new Rect(x, y, width, height), "=== GAME PAUSED ===");

        if (!showControlsGuide)
        {
            float btnY = y + 50;
            if (GUI.Button(new Rect(x + 30, btnY, width - 60, 42), "Resume (ESC)"))
            {
                ResumeGame();
            }
            btnY += 52;
            if (GUI.Button(new Rect(x + 30, btnY, width - 60, 42), "Restart Level"))
            {
                RestartCurrentLevel();
            }
            btnY += 52;
            if (GUI.Button(new Rect(x + 30, btnY, width - 60, 42), "Return to World Map"))
            {
                ReturnToWorldMap();
            }
            btnY += 52;
            if (GUI.Button(new Rect(x + 30, btnY, width - 60, 42), "Controls Guide"))
            {
                showControlsGuide = true;
            }

            btnY += 60;
            GUI.Label(new Rect(x + 30, btnY, width - 60, 24), "Master Volume: " + Mathf.RoundToInt(masterVolume * 100) + "%");
            btnY += 24;
            masterVolume = GUI.HorizontalSlider(new Rect(x + 30, btnY, width - 60, 24), masterVolume, 0f, 1f);
            AudioListener.volume = masterVolume;
        }
        else
        {
            float textY = y + 50;
            GUI.Label(new Rect(x + 20, textY, width - 40, 24), "WASD / Arrows: Move");
            textY += 26;
            GUI.Label(new Rect(x + 20, textY, width - 40, 24), "SPACE: Jump / Wall Jump");
            textY += 26;
            GUI.Label(new Rect(x + 20, textY, width - 40, 24), "V: Ground Pound / Enter Pipe");
            textY += 26;
            GUI.Label(new Rect(x + 20, textY, width - 40, 24), "Left Shift: Crouch / Slide");
            textY += 26;
            GUI.Label(new Rect(x + 20, textY, width - 40, 24), "Left Click: Shoot Fireball");
            textY += 26;
            GUI.Label(new Rect(x + 20, textY, width - 40, 24), "Right Click: Hold / Throw Shell");
            textY += 26;
            GUI.Label(new Rect(x + 20, textY, width - 40, 24), "Mouse: Camera Orbit");

            if (GUI.Button(new Rect(x + 30, y + height - 55, width - 60, 38), "Back"))
            {
                showControlsGuide = false;
            }
        }
    }
}
