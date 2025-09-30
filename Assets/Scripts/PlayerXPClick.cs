using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayerXPClick : MonoBehaviour, IPointerClickHandler
{
    [Header("XP e upgrades")]
    public int xp = 0;
    public Text xpText;
    public int baseXPPerClick = 1;
    public float passiveXPPerSecond = 0f;

    [Header("Multiplicador temporário de clique")]
    public float tempClickMultiplier = 1f;

    [Header("Referência ao PlayerEquipment")]
    public PlayerEquipment playerEquipment;

    [Header("Limite de cliques")]
    public float maxClicksPerSecond = 0.4f;
    private float lastClickTime = -999f;

    [Header("Chance de miss")]
    [Range(0f, 1f)]
    public float missChance = 0.1f;

    [Header("Chance de crítico")]
    [Range(0f, 1f)]
    public float critChance = 0.1f; // default 10% (você pode ajustar)
    [Header("Multiplicador de crítico")]
    public float critMultiplier = 2f; // mantém x2

    [Header("Efeito de texto flutuante")]
    public GameObject floatingTextPrefab;
    public Transform floatingTextSpawnPoint;

    private float passiveBuffer = 0f;

    void Start()
    {
        UpdateXPText();
        critMultiplier = 2f; // garante que seja 2
        if (tempClickMultiplier <= 0f) tempClickMultiplier = 1f;
    }

    void Update()
    {
        passiveBuffer += passiveXPPerSecond * Time.deltaTime;
        if (passiveBuffer >= 1f)
        {
            int toAdd = Mathf.FloorToInt(passiveBuffer);
            xp += toAdd;
            passiveBuffer -= toAdd;
            UpdateXPText();
        }

        if (Time.time - lastClickTime > 0.5f)
            playerEquipment?.StopClicking();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        float minInterval = 1f / maxClicksPerSecond;
        float now = Time.time;
        float delta = now - lastClickTime;

        if (delta < minInterval) return;

        lastClickTime = now;

        bool isMiss = Random.value < missChance;
        int xpToAddInt = 0;
        bool isCrit = false;

        if (!isMiss)
        {
            // calcula com float para evitar perdas por arredondamento precoce
            float baseFloat = Mathf.Max(0f, (float)baseXPPerClick);
            float total = baseFloat * tempClickMultiplier;

            isCrit = Random.value < critChance;
            if (isCrit)
                total *= critMultiplier; // aplica x2 no total já com multiplicador de peitoral

            xpToAddInt = Mathf.Max(0, Mathf.RoundToInt(total));
            xp += xpToAddInt;
            UpdateXPText();
        }

        if (floatingTextPrefab != null && floatingTextSpawnPoint != null)
        {
            GameObject go = Instantiate(floatingTextPrefab, xpText.canvas.transform);
            Vector3 screenPos = Camera.main.WorldToScreenPoint(floatingTextSpawnPoint.position);
            go.transform.position = screenPos;

            FloatingText ft = go.GetComponent<FloatingText>();
            ft.SetText(isMiss ? "MISS" : (isCrit ? "CRIT! +" + xpToAddInt + " XP" : "+" + xpToAddInt + " XP"));
        }

        float fastClickThreshold = 0.50f;
        bool isFastClick = delta <= fastClickThreshold;

        if (playerEquipment != null)
            playerEquipment.PlayAttack(isFastClick);
    }

    public void UpdateXPText()
    {
        if (xpText != null)
            xpText.text = "XP: " + xp;
    }
}
