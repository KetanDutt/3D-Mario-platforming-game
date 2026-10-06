using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Lives : MonoBehaviour
{
    private Text lives_ui;
    public static int LIVES = 5;
    private int lastLives = -1;

    void Start()
    {
        lives_ui = GetComponent<Text>();
        UpdateUI();
    }

    void Update()
    {
        if (LIVES != lastLives)
        {
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        lastLives = LIVES;
        if (lives_ui != null)
        {
            lives_ui.text = "X" + LIVES;
        }
    }

    public static void ResetLives()
    {
        LIVES = 5;
    }
}
