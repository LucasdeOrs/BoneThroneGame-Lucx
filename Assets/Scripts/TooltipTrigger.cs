using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [TextArea] 
    public string tooltipMessage;
    public void OnPointerEnter(PointerEventData eventData)
    {
        RectTransform rect = GetComponent<RectTransform>();
        Vector3 rightPosition = rect.position + new Vector3(rect.rect.width * rect.lossyScale.x, 0, 0);

        TooltipManager._instance.ShowTooltip(
            tooltipMessage, 
            rightPosition
        );
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipManager._instance.HideTooltip();
    }
}

