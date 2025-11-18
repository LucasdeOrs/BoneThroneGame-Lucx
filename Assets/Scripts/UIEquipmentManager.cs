using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class UpgradeLevel
{
    public int level;
    public int costXP;
    public float value;
}

public class UIEquipmentManager : MonoBehaviour
{
    [Header("Referência ao Player")]
    public PlayerEquipment player;
    public PlayerXPClick playerXP;

    [Header("Sets de Equipamentos")]
    public EquipmentPart[] headSets;
    public EquipmentPart[] chestSets;
    public EquipmentPart[] legsSets;
    public EquipmentPart[] swordSets;
    public EquipmentPart[] shieldSets;

    [Header("Tabela de Upgrades (pode deixar vazia e o script cria defaults)")]
    public UpgradeLevel[] headUpgrades;   // Capacete: XP passivo
    public UpgradeLevel[] chestUpgrades;  // Peitoral: multiplicador temporário (valor = adicional, ex 0.5 -> x1.5)
    public UpgradeLevel[] bootsUpgrades;  // Botas: crítico (chance)
    public UpgradeLevel[] swordUpgrades;  // Espada: XP/click (valor inteiro)
    public UpgradeLevel[] shieldUpgrades; // Escudo: usado como (0.5 - value) -> missChance

    [Header("Botões de Upgrade")]
    public Button headButton;
    public Button chestButton;
    public Button legsButton;
    public Button swordButton;
    public Button shieldButton;

    public int headIndex = 0;
    public int chestIndex = 0;
    public int legsIndex = 0;
    public int swordIndex = 0;
    public int shieldIndex = 0;

    private int[] upgradeCosts = { 20, 50, 100, 500, 1000 };

    private int GetUpgradeCost(int currentLevel)
    {
        if (currentLevel < upgradeCosts.Length)
            return upgradeCosts[currentLevel];
        return upgradeCosts[upgradeCosts.Length - 1];
    }

    private void Start()
    {
        EnsureDefaultUpgrades();   // cria os upgrades padrão caso não existam
        ApplyCurrentUpgrades();    // aplica os valores iniciais ao playerXP
        UpdateButtonTexts();
    }

    private void EnsureDefaultUpgrades()
    {
        if (headUpgrades == null || headUpgrades.Length < 5)
            headUpgrades = CreateLevels(new float[] { 0f, 1f, 5f, 10f, 20f });

        if (chestUpgrades == null || chestUpgrades.Length < 5)
            chestUpgrades = CreateLevels(new float[] { 0f, 0.5f, 1f, 1.5f, 2f });

        if (bootsUpgrades == null || bootsUpgrades.Length < 5)
            bootsUpgrades = CreateLevels(new float[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f });

        if (swordUpgrades == null || swordUpgrades.Length < 5)
            swordUpgrades = CreateLevels(new float[] { 1f, 2f, 5f, 10f, 20f });

        if (shieldUpgrades == null || shieldUpgrades.Length < 5)
            shieldUpgrades = CreateLevels(new float[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f });
    }

    private UpgradeLevel[] CreateLevels(float[] vals)
    {
        UpgradeLevel[] arr = new UpgradeLevel[vals.Length];
        for (int i = 0; i < vals.Length; i++)
        {
            arr[i] = new UpgradeLevel();
            arr[i].level = i + 1;
            arr[i].costXP = GetUpgradeCost(i); // custo correspondente
            arr[i].value = vals[i];
        }
        return arr;
    }

    public void ApplyCurrentUpgrades()
    {
        if (playerXP == null) return;

        if (headUpgrades != null && headUpgrades.Length > headIndex)
            playerXP.passiveXPPerSecond = headUpgrades[headIndex].value;

        if (chestUpgrades != null && chestUpgrades.Length > chestIndex)
            playerXP.tempClickMultiplier = 1f + chestUpgrades[chestIndex].value;

        if (bootsUpgrades != null && bootsUpgrades.Length > legsIndex)
            playerXP.critChance = bootsUpgrades[legsIndex].value;

        if (swordUpgrades != null && swordUpgrades.Length > swordIndex)
            playerXP.baseXPPerClick = Mathf.Max(1, Mathf.RoundToInt(swordUpgrades[swordIndex].value));

        if (shieldUpgrades != null && shieldUpgrades.Length > shieldIndex)
            playerXP.missChance = Mathf.Max(0f, 0.5f - shieldUpgrades[shieldIndex].value);

        // garante multiplicador crítico fixo
        playerXP.critMultiplier = 2f;
    }

    public void NextHead()
    {
        TryUpgrade(ref headIndex, headSets, headUpgrades, "Head");
        if (headIndex < headUpgrades.Length)
            playerXP.passiveXPPerSecond = headUpgrades[headIndex].value;
    }

    public void NextChest()
    {
        TryUpgrade(ref chestIndex, chestSets, chestUpgrades, "Chest");
        if (chestIndex < chestUpgrades.Length)
            playerXP.tempClickMultiplier = 1f + chestUpgrades[chestIndex].value;
    }

    public void NextLegs()
    {
        TryUpgrade(ref legsIndex, legsSets, bootsUpgrades, "Legs"); 
        if (legsIndex < bootsUpgrades.Length)
            playerXP.critChance = bootsUpgrades[legsIndex].value;
    }

    public void NextSword()
    {
        TryUpgrade(ref swordIndex, swordSets, swordUpgrades, "Sword");
        if (swordIndex < swordUpgrades.Length)
            playerXP.baseXPPerClick = Mathf.RoundToInt(swordUpgrades[swordIndex].value);
    }

    public void NextShield()
    {
        TryUpgrade(ref shieldIndex, shieldSets, shieldUpgrades, "Shield");
        if (shieldIndex < shieldUpgrades.Length)
            playerXP.missChance = Mathf.Max(0f, 0.5f - shieldUpgrades[shieldIndex].value);
    }

    public void UpdateButtonTexts()
    {
        UpdateButtonText(headButton, "Head", headIndex, headSets.Length);
        UpdateButtonText(chestButton, "Chest", chestIndex, chestSets.Length);
        UpdateButtonText(legsButton, "Legs", legsIndex, legsSets.Length);
        UpdateButtonText(swordButton, "Sword", swordIndex, swordSets.Length);
        UpdateButtonText(shieldButton, "Shield", shieldIndex, shieldSets.Length);
    }

    private void UpdateButtonText(Button button, string name, int currentLevel, int maxLevel)
    {
        if (button == null) return;
        Text text = button.GetComponentInChildren<Text>();
        if (text != null)
        {
            if (currentLevel + 1 >= maxLevel)
                text.text = $"{name} (MAX)";
            else
            {
                int nextCost = GetUpgradeCost(currentLevel + 1);
                text.text = $"{name} ({nextCost} XP)";
            }
        }
    }

    private void TryUpgrade(ref int index, EquipmentPart[] parts, UpgradeLevel[] upgrades, string slot)
    {
        if (index + 1 >= parts.Length || index + 1 >= upgrades.Length) return;

        int cost = GetUpgradeCost(index + 1);
        if (playerXP.xp >= cost)
        {
            playerXP.xp -= cost;
            index++;
            player.Equip(slot, parts[index]);
            playerXP.UpdateXPText();
            ApplyCurrentUpgrades();
            UpdateButtonTexts();
        }
    }
    public void ApplyVisualsFromIndices()
{
    if (player == null) return;

    if (headSets != null && headSets.Length > headIndex)
        player.Equip("Head", headSets[headIndex]);

    if (chestSets != null && chestSets.Length > chestIndex)
        player.Equip("Chest", chestSets[chestIndex]);

    if (legsSets != null && legsSets.Length > legsIndex)
        player.Equip("Legs", legsSets[legsIndex]);

    if (swordSets != null && swordSets.Length > swordIndex)
        player.Equip("Sword", swordSets[swordIndex]);

    if (shieldSets != null && shieldSets.Length > shieldIndex)
        player.Equip("Shield", shieldSets[shieldIndex]);
}
}
