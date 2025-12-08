using UnityEngine;
using UnityEngine.EventSystems;

public class EnemyClick : MonoBehaviour, IPointerClickHandler
{
    public EnemyController enemy;
    public PlayerXPClick playerXP;

    public void OnPointerClick(PointerEventData eventData)
    {
        // Encaminha o clique para o PlayerXPClick (para subir XP/atacar via overlap)
        if (playerXP != null)
            playerXP.OnPointerClick(eventData);
    }
}
