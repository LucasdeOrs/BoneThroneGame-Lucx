using UnityEngine;
using UnityEngine.UI;

public class FloatingText : MonoBehaviour
{
    public Text textComponent;
    public float moveSpeed = 50f;
    public float lifetime = 1f;

    private float timer = 0f;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void SetText(string text)
    {
        textComponent.text = text;
    }

    void Update()
    {
        timer += Time.deltaTime;

        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);

        canvasGroup.alpha = 1 - (timer / lifetime);

        if (timer >= lifetime)
            Destroy(gameObject);
    }
}
