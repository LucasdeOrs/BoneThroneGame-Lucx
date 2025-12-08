using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // <- Importante!

public class PauseMenuUI : MonoBehaviour
{
    [Header("Referências")]
    public GameObject menuPanel;
    public Image background;
    public Button startButton;
    public Button exitToHomeButton;
    public Button exitGameButton;
    public GameObject darkBackground;

    [Header("Tutorial / Como jogar")]
    public GameObject tutorialPanel;
    public Button tutorialOkButton;
    [TextArea] public string tutorialMessage = "Clique na tela para atacar/ganhar XP. Use o XP para comprar upgrades e sobreviver às hordas.";
    public Text tutorialText;
    public RoundManager roundManager; // para iniciar o jogo só após "entendi"

    private bool isPaused = false;
    private bool isInHomeMenu = true;

    void Start()
    {
        ShowHomeMenu();

        startButton.onClick.AddListener(OnStartClicked);
        exitToHomeButton.onClick.AddListener(OnExitToHomeClicked);
        exitGameButton.onClick.AddListener(OnExitGameClicked);
        if (tutorialOkButton != null)
            tutorialOkButton.onClick.AddListener(OnTutorialOkClicked);

        if (tutorialText != null && !string.IsNullOrEmpty(tutorialMessage))
            tutorialText.text = tutorialMessage;

        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
    }

    void Update()
    {
        if (!isInHomeMenu && Keyboard.current != null && Keyboard.current.escapeKey != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Debug.Log("ESC detectado!");
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    private void ShowHomeMenu()
    {
        Time.timeScale = 0f;
        menuPanel.SetActive(true);
        background.gameObject.SetActive(true);
        darkBackground.SetActive(true);

        startButton.GetComponentInChildren<Text>().text = "Começar!";
        exitToHomeButton.GetComponentInChildren<Text>().text = "Sair para o menu";
        exitGameButton.GetComponentInChildren<Text>().text = "Sair do jogo";
        exitToHomeButton.gameObject.SetActive(false);
        exitGameButton.gameObject.SetActive(true);

        isPaused = true;
        isInHomeMenu = true;
    }

    private void PauseGame()
    {
        Time.timeScale = 0f;
        menuPanel.SetActive(true);
        background.gameObject.SetActive(false);
        darkBackground.SetActive(true);

        startButton.GetComponentInChildren<Text>().text = "Continuar";
        exitToHomeButton.GetComponentInChildren<Text>().text = "Sair para o menu";
        exitGameButton.GetComponentInChildren<Text>().text = "Sair do jogo";
        exitToHomeButton.gameObject.SetActive(true);
        exitGameButton.gameObject.SetActive(true);

        isPaused = true;
    }

    private void ResumeGame()
    {
        Time.timeScale = 1f;
        menuPanel.SetActive(false);
        isPaused = false;
        darkBackground.SetActive(false);
    }

    private void OnStartClicked()
    {
        if (isInHomeMenu)
        {
            if (tutorialPanel != null)
            {
                // mostra tutorial e mantém pausado até confirmar
                menuPanel.SetActive(false);
                tutorialPanel.SetActive(true);
                darkBackground.SetActive(true);
            }
            else
            {
                Time.timeScale = 1f;
                menuPanel.SetActive(false);
                darkBackground.SetActive(false);
                isPaused = false;
                isInHomeMenu = false;
                if (roundManager != null)
                    roundManager.StartGame();
            }
        }
        else
        {
            ResumeGame();
        }
    }

    private void OnExitToHomeClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnExitGameClicked()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    // expõe para botões externos (ex.: botão Carregar) para fechar o pause
    public void ClosePauseMenu()
    {
        ResumeGame();
    }

    private void OnTutorialOkClicked()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        Time.timeScale = 1f;
        darkBackground.SetActive(false);
        menuPanel.SetActive(false);
        isPaused = false;
        isInHomeMenu = false;
        if (roundManager != null)
            roundManager.StartGame();
    }
}
