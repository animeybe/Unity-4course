using UnityEngine;
using TMPro;

public class CoinCollector : MonoBehaviour
{
    private int coinsCollected = 0;
    private TMP_Text coinText;

    void Start()
    {
        // Находим UI текст
        FindCoinText();
        UpdateCoinText();
    }
    
    void FindCoinText()
    {
        GameObject textObject = GameObject.Find("CoinCounter");
        if (textObject != null)
        {
            coinText = textObject.GetComponent<TMP_Text>();
        }
        
        if (coinText == null)
        {
            Debug.LogWarning("CoinCounter TMP_Text not found!");
        }
    }
    
    public void CollectCoin(int value)
    {
        coinsCollected += value;
        Debug.Log($"Монет собрано: {coinsCollected}");
        UpdateCoinText();
    }
    
    private void UpdateCoinText()
    {
        if (coinText != null)
        {
            coinText.text = $"Грязных долларов: {coinsCollected}";
        }
    }
    
    public int GetCoinCount()
    {
        return coinsCollected;
    }
}