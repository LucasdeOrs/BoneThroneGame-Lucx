using UnityEngine;
using UnityEngine.UI;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager _instance;

    [Header("Referências")]
    public GameObject tooltipObject;
    public Text tooltipText;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    private void Start()
    {
        HideTooltip();
    }

    public void ShowTooltip(string message, Vector3 position)
    {
        tooltipObject.SetActive(true);
        tooltipText.text = message;
        tooltipObject.transform.position = position;
    }

    public void HideTooltip()
    {
        tooltipObject.SetActive(false);
    }
}
