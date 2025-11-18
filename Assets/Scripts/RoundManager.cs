using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RoundManager : MonoBehaviour
{
    [Header("Referências")]
    public PlayerXPClick playerXP;
    public PlayerHealth playerHealth;

    [Header("Inimigo")]
    public GameObject enemyPrefab;
    public Transform enemyParent;      // onde instanciar (Canvas ou empty)
    public Transform enemySpawnPoint;  // posição inicial (UI ou mundo)

    [Header("Configuração de rounds")]
    public float prepTime = 10f;          // tempo pra se equipar
    public int baseEnemiesPerRound = 3;   // começa aqui
    public float spawnInterval = 1.0f;

    [Header("UI")]
    public Text roundText;

    private int currentRound = 1;
    private int enemiesAlive = 0;
    private bool inBattle = false;

    void Start()
    {
        StartPrepPhase();
    }

    private void StartPrepPhase()
    {
        inBattle = false;
        StartCoroutine(PrepRoutine());
    }

    IEnumerator PrepRoutine()
    {
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
        inBattle = true;
        int enemiesThisRound = baseEnemiesPerRound + (currentRound - 1) * 2;
        enemiesAlive = enemiesThisRound;

        if (roundText != null)
            roundText.text = $"⚔️ Round {currentRound} - Horda: {enemiesThisRound} inimigos";

        StartCoroutine(SpawnEnemies(enemiesThisRound));
    }

    IEnumerator SpawnEnemies(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject go = Instantiate(enemyPrefab, enemySpawnPoint.position, Quaternion.identity);

            if (enemyParent != null)
                go.transform.SetParent(enemyParent, worldPositionStays: false);

            var enemy = go.GetComponent<EnemyController>();
            var click = go.GetComponent<EnemyClick>();

            if (enemy != null)
                enemy.Init(this, playerXP, playerHealth, currentRound);

            if (click != null)
                click.playerXP = playerXP;

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    public void EnemyDefeated()
    {
        enemiesAlive--;
        if (enemiesAlive <= 0)
        {
            currentRound++;
            StartPrepPhase();
        }
    }
}
