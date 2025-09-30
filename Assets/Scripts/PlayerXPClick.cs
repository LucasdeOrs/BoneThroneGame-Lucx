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
    [Range(0f, 1f)] public float missChance = 0.1f;

    [Header("Chance de crítico")]
    [Range(0f, 1f)] public float critChance = 0.1f; // default 10%
    [Header("Multiplicador de crítico")]
    public float critMultiplier = 2f; // mantém x2

    [Header("Efeito de texto flutuante")]
    public GameObject floatingTextPrefab;
    public Transform floatingTextSpawnPoint;

    // ==== SONS ====
    [Header("Sons (clips)")]
    public AudioClip sfxHit;     // som ao acertar
    public AudioClip sfxMiss;    // som ao errar
    public AudioClip sfxCrit;    // opcional: som diferente no crítico

    [Header("Volumes globais")]
    [Range(0f, 1f)] public float sfxVolume = 1f; // volume base para todos

    [Header("Volumes por evento")]
    [Range(0f, 2f)] public float sfxHitVolume = 1.2f;   // ↑ deixe maior para destacar o hit
    [Range(0f, 2f)] public float sfxMissVolume = 0.6f;  // ↓ deixe menor para suavizar o miss
    [Range(0f, 2f)] public float sfxCritVolume = 1.4f;  // opcional

    [Header("Expressividade")]
    [Tooltip("Variação leve de pitch para dar sensação de responsividade")]
    [Range(0f, 0.25f)] public float pitchJitter = 0.05f;

    private AudioSource sfxSource;
    private float passiveBuffer = 0f;

    void Start()
    {
        UpdateXPText();
        critMultiplier = 2f;
        if (tempClickMultiplier <= 0f) tempClickMultiplier = 1f;

        // AudioSource 2D dedicado a SFX
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.spatialBlend = 0f; // 2D
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
            // ACERTO
            float baseFloat = Mathf.Max(0f, (float)baseXPPerClick);
            float total = baseFloat * tempClickMultiplier;

            isCrit = Random.value < critChance;
            if (isCrit) total *= critMultiplier;

            xpToAddInt = Mathf.Max(0, Mathf.RoundToInt(total));
            xp += xpToAddInt;
            UpdateXPText();

            // 🔊 Som de acerto (crit usa volume próprio)
            if (isCrit && sfxCrit != null)
                PlaySFX(sfxCrit, sfxCritVolume);
            else
                PlaySFX(sfxHit, sfxHitVolume);
        }
        else
        {
            // 🔊 Som de erro/miss
            PlaySFX(sfxMiss, sfxMissVolume);
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

    // ===== util de áudio =====
    private void PlaySFX(AudioClip clip, float perEventVolume = 1f)
    {
        if (clip == null || sfxSource == null) return;

        // leve variação de pitch para clicks rápidos soarem naturais
        sfxSource.pitch = 1f + Random.Range(-pitchJitter, pitchJitter);

        // volume final = volume global * volume do evento (clamp por segurança)
        float vol = Mathf.Clamp01(sfxVolume * perEventVolume);
        sfxSource.PlayOneShot(clip, vol);
    }
}
