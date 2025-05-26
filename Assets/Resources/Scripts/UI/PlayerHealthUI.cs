using UnityEngine;
using TMPro; 

public class PlayerHealthUI : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public TMP_Text healthText;

    void OnEnable()
    {
        PlayerHealth.OnPlayerHealthChanged += UpdateHealthUI;
    }

    void OnDisable()
    {
        PlayerHealth.OnPlayerHealthChanged -= UpdateHealthUI;
    }

    void Start()
    {
        if (playerHealth != null)
        {
            UpdateHealthUI(playerHealth.GetCurrentHealth(), playerHealth.GetMaxHealth());
        }
    }

    void UpdateHealthUI(int currentHealth, int maxHealth)
    {
        if (healthText != null)
        {
            healthText.text = $"Vida: {currentHealth} / {maxHealth}";
        }
    }
}


