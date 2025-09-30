using UnityEngine;

[CreateAssetMenu(fileName = "NewEquipmentPart", menuName = "Equipment/Part")]
public class EquipmentPart : ScriptableObject
{
    public string partName;
    public Sprite[] animationFrames;
}
