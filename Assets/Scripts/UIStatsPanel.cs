using UnityEngine;
using UnityEngine.UI;

public class UIStatsPanel : MonoBehaviour
{
    [Header("Referências")]
    public PlayerXPClick playerXP;
    public Text statsText;

    void Update()
    {
        if (playerXP == null || statsText == null) return;

    statsText.text =
        "Força do clique: " + playerXP.baseXPPerClick + "\n" +
        "Golpe crítico: " + Mathf.RoundToInt(playerXP.critChance * 100f) + "%\n" +
        "Golpe perdido: " + Mathf.RoundToInt(playerXP.missChance * 100f) + "%\n" +
        "XP por segundo: " + Mathf.RoundToInt(playerXP.passiveXPPerSecond) + "\n" +
        "Bônus de clique: " + Mathf.RoundToInt((playerXP.tempClickMultiplier - 1f) * 100f) + "%";
    }
}
