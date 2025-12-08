using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class RoundManager : MonoBehaviour
{
    [Header("Referências")]
    public PlayerXPClick playerXP;
    public PlayerHealth playerHealth;
    public Transform playerTarget;      // alvo para os inimigos se moverem

    [Header("Inimigo")]
    public GameObject enemyPrefab;
    public Transform enemyParent;      // onde instanciar (Canvas ou empty)
    public Transform enemySpawnPoint;  // posição inicial (UI ou mundo)

    [Header("Configuração de rounds")]
    public float prepTime = 10f;          // tempo pra se equipar
    public int baseEnemiesPerRound = 3;   // começa aqui
    public float spawnInterval = 1.0f;
    public bool autoStart = false;
    public int winRound = 0; // se >0, finaliza ao completar esse round

    [Header("Balanceamento")]
    [Tooltip("Porcentagem do HP máximo recuperada a cada fase de preparação (0 a 1)")]
    public float healPercentOnPrep = 0.25f;
    [Tooltip("Valor fixo de HP recuperado a cada fase de preparação")]
    public int healFlatOnPrep = 0;

    [Header("UI")]
    public Text roundText;
    public GameObject winPanel;
    public string menuSceneName = "MainMenu";

    private int currentRound = 1;
    private int enemiesAlive = 0;
    private bool inBattle = false;
    private EnemyController currentAttacker;
    private bool hasStarted = false;
    private bool gameFinished = false;

    void Start()
    {
        if (autoStart)
            StartGame();
    }

    public void StartGame()
    {
        if (hasStarted) return;
        hasStarted = true;
        StartPrepPhase();
    }

    private void StartPrepPhase()
    {
        inBattle = false;
        HealPlayerForPrep();
        StartCoroutine(PrepRoutine());
    }

    IEnumerator PrepRoutine()
    {
        if (gameFinished) yield break;

        float timer = prepTime;

        while (timer > 0f)
        {
            if (roundText != null)
                roundText.text = $"Round {currentRound} - Equipe-se! Horda em {timer:F0}s";

            timer -= Time.deltaTime;
            yield return null;
        }

        StartBattlePhase();
    }

    private void StartBattlePhase()
    {
        if (gameFinished) return;
        inBattle = true;
        int enemiesThisRound = baseEnemiesPerRound + (currentRound - 1) * 2;
        enemiesAlive = enemiesThisRound;

        if (roundText != null)
            roundText.text = $"⚔️ Round {currentRound} - Horda: {enemiesThisRound} inimigos";

        StartCoroutine(SpawnEnemies(enemiesThisRound));
    }

    IEnumerator SpawnEnemies(int count)
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("RoundManager: enemyPrefab não atribuído.");
            yield break;
        }
        if (enemySpawnPoint == null)
        {
            Debug.LogError("RoundManager: enemySpawnPoint não atribuído.");
            yield break;
        }

        for (int i = 0; i < count; i++)
        {
            GameObject go = Instantiate(enemyPrefab, enemySpawnPoint.position, Quaternion.identity);

            if (enemyParent != null)
                go.transform.SetParent(enemyParent, worldPositionStays: false);

            // Garantir posição inicial correta (UI ou mundo)
            var enemyRect = go.GetComponent<RectTransform>();
            var spawnRect = enemySpawnPoint.GetComponent<RectTransform>();
            if (enemyRect != null && spawnRect != null &&
                enemyRect.GetComponentInParent<Canvas>() == spawnRect.GetComponentInParent<Canvas>())
            {
                enemyRect.anchoredPosition = spawnRect.anchoredPosition;
            }
            else
            {
                go.transform.position = enemySpawnPoint.position;
            }

            var enemy = go.GetComponent<EnemyController>();
            var click = go.GetComponent<EnemyClick>();

            if (enemy != null)
            {
                // usa playerTarget, se não setado tenta usar o transform do PlayerHealth
                Transform target = playerTarget != null ? playerTarget : playerHealth?.transform;
                enemy.Init(this, playerXP, playerHealth, currentRound, target);
            }

            if (click != null)
            {
                click.playerXP = playerXP;
                click.enemy = enemy;
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    public void EnemyDefeated()
    {
        if (gameFinished) return;
        enemiesAlive--;
        if (enemiesAlive <= 0)
        {
            currentRound++;
            if (winRound > 0 && currentRound > winRound)
            {
                HandleWin();
                return;
            }
            StartPrepPhase();
        }
    }

    public int GetCurrentRound()
    {
        return currentRound;
    }

    public void SetRoundAndRestart(int round)
    {
        StopAllCoroutines();
        enemiesAlive = 0;
        currentRound = Mathf.Max(1, round);
        currentAttacker = null;
        gameFinished = false;
        ClearExistingEnemies();
        StartPrepPhase();
    }

    public bool IsInPrepPhase()
    {
        return !inBattle;
    }

    private void HealPlayerForPrep()
    {
        if (playerHealth == null) return;
        int healPercent = Mathf.RoundToInt(playerHealth.maxHP * Mathf.Clamp01(healPercentOnPrep));
        int totalHeal = healPercent + Mathf.Max(0, healFlatOnPrep);
        if (totalHeal > 0)
            playerHealth.Heal(totalHeal);
    }

    public bool TrySetAttacker(EnemyController enemy)
    {
        if (currentAttacker == null || currentAttacker == enemy)
        {
            currentAttacker = enemy;
            return true;
        }

        // se o atacante atual foi destruído (Unity null), libera
        if (currentAttacker == null)
        {
            currentAttacker = enemy;
            return true;
        }

        return false;
    }

    public void ReleaseAttacker(EnemyController enemy)
    {
        if (currentAttacker == enemy)
            currentAttacker = null;
    }

    public void ClearExistingEnemies()
    {
        currentAttacker = null;
        if (enemyParent != null)
        {
            foreach (Transform child in enemyParent)
            {
                Destroy(child.gameObject);
            }
        }
        else
        {
            foreach (var enemy in FindObjectsOfType<EnemyController>())
            {
                Destroy(enemy.gameObject);
            }
        }
    }

    private void HandleWin()
    {
        gameFinished = true;
        Time.timeScale = 0f;
        if (winPanel != null)
            winPanel.SetActive(true);
        if (roundText != null)
            roundText.text = $"Vitória! Round {currentRound - 1}";
    }

    // Botão no painel de vitória
    public void OnWinBackToMenu()
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(menuSceneName) && Application.CanStreamedLevelBeLoaded(menuSceneName))
            SceneManager.LoadScene(menuSceneName);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
