using UnityEngine;
using UnityEngine.EventSystems;

public class EnemyClick : MonoBehaviour, IPointerClickHandler
{
    public EnemyController enemy;
    public PlayerXPClick playerXP;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (enemy == null || playerXP == null) return;

        int damage = Mathf.Max(1, playerXP.baseXPPerClick);
        enemy.TakeDamage(damage);
    }
}
