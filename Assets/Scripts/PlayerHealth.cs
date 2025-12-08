using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("HP do Jogador")]
    public int maxHP = 100;
    public int currentHP;

    [Header("UI (opcional)")]
    public Text hpText;
    public Slider hpBar;

    [Header("Game Over (opcional)")]
    public GameObject gameOverPanel;
    public bool freezeOnDeath = true;
    public Button backToMenuButton;
    public Button loadButton;
    public string menuSceneName = "MainMenu";
    public JsonSaveSystem saveSystem;

    private bool hasDied = false;

    void Start()
    {
        currentHP = maxHP;
        UpdateHPUI();

        if (backToMenuButton != null)
            backToMenuButton.onClick.AddListener(OnClickBackToMenu);
        if (loadButton != null)
            loadButton.onClick.AddListener(OnClickLoad);
    }

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        if (currentHP < 0) currentHP = 0;
        UpdateHPUI();

        if (currentHP <= 0 && !hasDied)
            HandleDeath();
    }

    public void Heal(int amount)
    {
        currentHP += amount;
        if (currentHP > maxHP) currentHP = maxHP;
        UpdateHPUI();
    }

    public void UpdateHPUI()
    {
        if (hpText != null)
            hpText.text = "HP: " + currentHP + "/" + maxHP;

        if (hpBar != null)
        {
            hpBar.maxValue = maxHP;
            hpBar.value = currentHP;
        }
    }

    private void HandleDeath()
    {
        hasDied = true;
        Debug.Log("Game Over!");

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (freezeOnDeath)
            Time.timeScale = 0f;
    }

    // ==== Botões de Game Over ====
    private void OnClickBackToMenu()
    {
        Time.timeScale = 1f;
        if (string.IsNullOrEmpty(menuSceneName))
        {
            Debug.LogWarning("PlayerHealth: menuSceneName não configurado.");
            return;
        }

        if (Application.CanStreamedLevelBeLoaded(menuSceneName))
        {
            SceneManager.LoadScene(menuSceneName);
        }
        else
        {
            Debug.LogWarning($"PlayerHealth: cena '{menuSceneName}' não está na Build Settings. Adicione via File > Build Settings.");
        }
    }

    private void OnClickLoad()
    {
        Time.timeScale = 1f;
        hasDied = false;
        if (saveSystem != null)
        {
            saveSystem.LoadGame();
            currentHP = maxHP;
            UpdateHPUI();
            Debug.Log("Carregado após game over.");
        }
        else
        {
            Debug.LogWarning("PlayerHealth: saveSystem não atribuído para Load.");
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }
}
