using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [Header("Status")]
    public int maxHP = 10;
    public int currentHP;
    public int damagePerHit = 5;
    public float attackInterval = 2f;

    private RoundManager roundManager;
    private PlayerXPClick playerXP;
    private PlayerHealth playerHealth;
    private int roundNumber;

    public void Init(RoundManager manager, PlayerXPClick xp, PlayerHealth health, int round)
    {
        roundManager = manager;
        playerXP = xp;
        playerHealth = health;
        roundNumber = round;

        // escalar dificuldade por round
        maxHP = 10 + (round * 5);
        currentHP = maxHP;
        damagePerHit = 5 + (round * 2);

        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        while (currentHP > 0 && playerHealth != null && playerHealth.currentHP > 0)
        {
            yield return new WaitForSeconds(attackInterval);
            playerHealth.TakeDamage(damagePerHit);
        }
    }

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // recompensa de XP por inimigo
        if (playerXP != null)
        {
            int reward = 5 * roundNumber;
            playerXP.xp += reward;
            playerXP.UpdateXPText();
        }

        if (roundManager != null)
            roundManager.EnemyDefeated();

        Destroy(gameObject);
    }
}
