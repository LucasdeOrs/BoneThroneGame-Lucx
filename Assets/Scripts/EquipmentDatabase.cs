using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/Database")]
public class EquipmentDatabase : ScriptableObject
{
    public EquipmentPart[] parts;

    public EquipmentPart GetByName(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;

        foreach (var part in parts)
        {
            if (part != null && part.partName == name)
                return part;
        }

        return null;
    }
}
