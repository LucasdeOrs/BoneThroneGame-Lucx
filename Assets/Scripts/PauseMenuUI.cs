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

    private bool isPaused = false;
    private bool isInHomeMenu = true;

    void Start()
    {
        ShowHomeMenu();

        startButton.onClick.AddListener(OnStartClicked);
        exitToHomeButton.onClick.AddListener(OnExitToHomeClicked);
        exitGameButton.onClick.AddListener(OnExitGameClicked);
    }

    void Update()
    {
        if (!isInHomeMenu && Keyboard.current.escapeKey.wasPressedThisFrame)
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
            Time.timeScale = 1f;
            menuPanel.SetActive(false);
            isPaused = false;
            isInHomeMenu = false;
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
}
