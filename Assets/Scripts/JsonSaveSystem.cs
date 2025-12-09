using UnityEngine;
using UnityEngine.UI;
using System.IO;

[System.Serializable]
public class PlayerSaveData
{
    // PlayerXPClick
    public int xp;
    public int baseXPPerClick;
    public float passiveXPPerSecond;
    public float missChance;
    public float critChance;
    public float critMultiplier;
    public float tempClickMultiplier;
    public int currentRound;

    // PlayerHealth
    public int playerCurrentHP;
    public int playerMaxHP;

    // Equipamentos equipados (sprites)
    public string headName;
    public string chestName;
    public string legsName;
    public string swordName;
    public string shieldName;

    // NÍVEIS de upgrade (parte da esquerda)
    public int headIndex;
    public int chestIndex;
    public int legsIndex;
    public int swordIndex;
    public int shieldIndex;
}

public class JsonSaveSystem : MonoBehaviour
{
    [Header("Referências do jogo")]
    public PlayerXPClick xpClick;
    public PlayerEquipment playerEquipment;
    public EquipmentDatabase equipmentDatabase;
    public UIEquipmentManager uiEquipment;
    public PlayerHealth playerHealth;
    public RoundManager roundManager;

    [Header("UI")]
    public Button saveButton;
    public Text saveButtonLabel;
    public string saveReadyText = "Salvar";
    public string saveBlockedText = "Salvar (aguarde a preparação)";

    private string filePath;

    void Awake()
    {
        filePath = Path.Combine(Application.persistentDataPath, "save.json");
        Debug.Log("Save path: " + filePath);
    }

    void Update()
    {
        UpdateSaveButtonUI();
    }

    // ========== SALVAR ==========
    public void SaveGame()
    {
        if (xpClick == null)
        {
            Debug.LogWarning("JsonSaveSystem: xpClick não atribuído!");
            return;
        }

        if (roundManager != null && !roundManager.IsInPrepPhase())
        {
            Debug.LogWarning("JsonSaveSystem: só é permitido salvar na fase de preparação. Aguarde o intervalo entre hordas.");
            return;
        }

        PlayerSaveData data = new PlayerSaveData();

        // XP & stats
        data.xp = xpClick.xp;
        data.baseXPPerClick = xpClick.baseXPPerClick;
        data.passiveXPPerSecond = xpClick.passiveXPPerSecond;
        data.missChance = xpClick.missChance;
        data.critChance = xpClick.critChance;
        data.critMultiplier = xpClick.critMultiplier;
        data.tempClickMultiplier = xpClick.tempClickMultiplier;

        // HP
        if (playerHealth != null)
        {
            data.playerCurrentHP = playerHealth.currentHP;
            data.playerMaxHP = playerHealth.maxHP;
        }

        // Equip sprites
        if (playerEquipment != null)
        {
            data.headName = GetName(playerEquipment.head);
            data.chestName = GetName(playerEquipment.chest);
            data.legsName = GetName(playerEquipment.legs);
            data.swordName = GetName(playerEquipment.sword);
            data.shieldName = GetName(playerEquipment.shield);
        }

        // Round
        if (roundManager != null)
            data.currentRound = roundManager.GetCurrentRound();

        // Níveis de upgrade (parte esquerda)
        if (uiEquipment != null)
        {
            data.headIndex = uiEquipment.headIndex;
            data.chestIndex = uiEquipment.chestIndex;
            data.legsIndex = uiEquipment.legsIndex;
            data.swordIndex = uiEquipment.swordIndex;
            data.shieldIndex = uiEquipment.shieldIndex;
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, json);

        Debug.Log("Jogo salvo em: " + filePath);
    }

    private string GetName(EquipmentPart part)
    {
        return part != null ? part.partName : null;
    }


    // ========== CARREGAR ==========
    public void LoadGame()
    {
        if (!File.Exists(filePath))
        {
            Debug.LogWarning("Nenhum arquivo de save encontrado.");
            return;
        }

        Time.timeScale = 1f; // garante que o jogo retome após carregar (ex.: se veio do pause)
        ClearEnemiesAll();

        string json = File.ReadAllText(filePath);
        PlayerSaveData data = JsonUtility.FromJson<PlayerSaveData>(json);

        // XP + stats
        if (xpClick != null)
        {
            xpClick.xp = data.xp;
            xpClick.baseXPPerClick = data.baseXPPerClick;
            xpClick.passiveXPPerSecond = data.passiveXPPerSecond;
            xpClick.missChance = data.missChance;
            xpClick.critChance = data.critChance;
            xpClick.critMultiplier = data.critMultiplier;
            xpClick.tempClickMultiplier = data.tempClickMultiplier;

            xpClick.UpdateXPText();
        }

        // HP
        if (playerHealth != null)
        {
            playerHealth.maxHP = data.playerMaxHP > 0 ? data.playerMaxHP : playerHealth.maxHP;
            playerHealth.currentHP = Mathf.Clamp(data.playerCurrentHP, 0, playerHealth.maxHP);
            playerHealth.UpdateHPUI();
        }

        // Equip sprites pelo nome (opcional, mas ajuda a garantir visual certo)
        if (playerEquipment != null && equipmentDatabase != null)
        {
            playerEquipment.Equip("Head", equipmentDatabase.GetByName(data.headName));
            playerEquipment.Equip("Chest", equipmentDatabase.GetByName(data.chestName));
            playerEquipment.Equip("Legs", equipmentDatabase.GetByName(data.legsName));
            playerEquipment.Equip("Sword", equipmentDatabase.GetByName(data.swordName));
            playerEquipment.Equip("Shield", equipmentDatabase.GetByName(data.shieldName));
        }

        // Níveis de upgrade + aplicar efeitos e textos
        if (uiEquipment != null)
        {
            uiEquipment.headIndex = data.headIndex;
            uiEquipment.chestIndex = data.chestIndex;
            uiEquipment.legsIndex = data.legsIndex;
            uiEquipment.swordIndex = data.swordIndex;
            uiEquipment.shieldIndex = data.shieldIndex;

            uiEquipment.ApplyCurrentUpgrades();    // aplica stats no PlayerXP
            uiEquipment.ApplyVisualsFromIndices(); // equipa os sets corretos
            uiEquipment.UpdateButtonTexts();       // atualiza "Head (50 XP)" etc.
        }

        // Round
        if (roundManager != null)
        {
            int round = data.currentRound > 0 ? data.currentRound : 1;
            roundManager.SetRoundAndRestartFromLoad(round);
        }
        else
        {
            ClearEnemiesAll();
        }

        Debug.Log("Jogo carregado!");
    }


    // OPCIONAL: salvar/carregar por teclado pra testar rápido
    private void UpdateSaveButtonUI()
    {
        if (saveButton == null) return;

        bool canSave = roundManager == null || roundManager.IsInPrepPhase();
        saveButton.interactable = canSave;

        if (saveButtonLabel != null)
            saveButtonLabel.text = canSave ? saveReadyText : saveBlockedText;
    }

    private void ClearEnemiesAll()
    {
        foreach (var enemy in FindObjectsOfType<EnemyController>())
        {
            if (enemy != null)
                Destroy(enemy.gameObject);
        }
    }
}
