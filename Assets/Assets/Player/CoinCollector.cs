using UnityEngine;
using TMPro;
using System;

public class CoinCollector : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text coinText;

    [Header("Audio")]
    [SerializeField] private AudioClip coinSound;
    [SerializeField] private float soundCooldown = 0.2f;

    private int coinsCollected = 0;
    private AudioSource audioSource;
    private float lastSoundTime;

    void Start()
    {
        if (coinText == null)
            coinText = FindChildTMPText("CoinText");
        audioSource = GetComponent<AudioSource>();
        UpdateCoinText();
        Health.OnPlayerDeath += ResetCoins;
    }

    void OnDestroy()
    {
        Health.OnPlayerDeath -= ResetCoins;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Coin"))
        {
            coinsCollected++;
            UpdateCoinText();
            
            if (Time.time - lastSoundTime > soundCooldown)
            {
                PlayCoinSound();
                lastSoundTime = Time.time;
            }
            
            Destroy(other.gameObject);
        }
    }

    private void PlayCoinSound()
    {
        if (audioSource != null && coinSound != null)
            audioSource.PlayOneShot(coinSound, 0.7f);
    }

    private void ResetCoins()
    {
        coinsCollected = 0;
        UpdateCoinText();
    }

    private void UpdateCoinText()
    {
        if (coinText != null)
            coinText.text = $"Грязных долларов: {coinsCollected}";
    }

    private TMP_Text FindChildTMPText(string name)
    {
        TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i].name.Contains(name))
                return texts[i];
        }
        return null;
    }
}
