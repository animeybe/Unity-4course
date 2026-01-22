using UnityEngine;
using System.Collections;
using TMPro;
using System;

public class Health : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private TMP_Text healthText;

    private float currentHealth;
    private bool isDead = false;
    private bool isPoisoned = false;

    public static event System.Action OnPlayerDeath;

    void Start()
    {
        currentHealth = maxHealth;
        if (healthText == null)
            healthText = FindChildTMPText("HealthText");
        UpdateUI();
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

    public void TakeDamage(float damage)
    {
        if (isDead) return;
        
        currentHealth = Mathf.Max(0, currentHealth - damage);
        UpdateUI();
        
        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log("Player died!");
        OnPlayerDeath?.Invoke();
    }

    public void Respawn()
    {
        currentHealth = maxHealth;
        isDead = false;
        isPoisoned = false;
        UpdateUI();
    }

    public void ApplyPoison(float damagePerSecond, float duration)
    {
        if (isDead || isPoisoned) return;
        isPoisoned = true;
        StartCoroutine(PoisonEffect(damagePerSecond, duration));
    }

    private IEnumerator PoisonEffect(float dps, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration && !isDead)
        {
            TakeDamage(dps * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
        isPoisoned = false;
    }

    private void UpdateUI()
    {
        if (healthText != null)
        {
            healthText.text = $"HP: {Mathf.RoundToInt(currentHealth)}/{maxHealth}";
            healthText.color = currentHealth > maxHealth * 0.3f ? Color.white : Color.red;
        }
    }
}
