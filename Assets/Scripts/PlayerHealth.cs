using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("HP do Jogador")]
    public int maxHP = 100;
    public int currentHP;

    [Header("UI (opcional)")]
    public Text hpText;
    public Slider hpBar;

    void Start()
    {
        currentHP = maxHP;
        UpdateHPUI();
    }

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        if (currentHP < 0) currentHP = 0;
        UpdateHPUI();

        if (currentHP <= 0)
        {
            Debug.Log("💀 Game Over!");
            // aqui você pode desabilitar cliques, mostrar tela de game over, etc.
        }
    }

    public void Heal(int amount)
    {
        currentHP += amount;
        if (currentHP > maxHP) currentHP = maxHP;
        UpdateHPUI();
    }

    private void UpdateHPUI()
    {
        if (hpText != null)
            hpText.text = "HP: " + currentHP + "/" + maxHP;

        if (hpBar != null)
        {
            hpBar.maxValue = maxHP;
            hpBar.value = currentHP;
        }
    }
}

