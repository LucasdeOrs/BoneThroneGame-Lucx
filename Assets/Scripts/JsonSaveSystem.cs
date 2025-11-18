using UnityEngine;
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

    private string filePath;

    void Awake()
    {
        filePath = Path.Combine(Application.persistentDataPath, "save.json");
        Debug.Log("Save path: " + filePath);
    }

    // ========== SALVAR ==========
    public void SaveGame()
    {
        if (xpClick == null)
        {
            Debug.LogWarning("JsonSaveSystem: xpClick não atribuído!");
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

        // Equip sprites
        if (playerEquipment != null)
        {
            data.headName = GetName(playerEquipment.head);
            data.chestName = GetName(playerEquipment.chest);
            data.legsName = GetName(playerEquipment.legs);
            data.swordName = GetName(playerEquipment.sword);
            data.shieldName = GetName(playerEquipment.shield);
        }

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

        Debug.Log("Jogo carregado!");
    }


    // OPCIONAL: salvar/carregar por teclado pra testar rápido

}
