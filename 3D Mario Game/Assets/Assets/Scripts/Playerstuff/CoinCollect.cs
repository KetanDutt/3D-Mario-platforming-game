using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoinCollect : MonoBehaviour
{
    public Text CoinUI;
    public static int COIN_COUNT = 0;
    private int lastCoins = -1;

    void Start()
    {
        if (CoinUI == null) CoinUI = GetComponent<Text>();
        UpdateUI();
    }

    void Update()
    {
        if (COIN_COUNT != lastCoins)
        {
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        lastCoins = COIN_COUNT;
        if (CoinUI != null)
        {
            CoinUI.text = "X" + COIN_COUNT;
        }
    }

    public static void ResetCoins()
    {
        COIN_COUNT = 0;
    }
}
