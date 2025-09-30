using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonSFX : MonoBehaviour
{
    [Header("Som do botão")]
    public AudioClip clickSound;

    [Range(0f, 1f)]
    public float volume = 1f;

    // 🔊 Referência global
    public static AudioSource globalSource;

    void Awake()
    {
        // pega o botão e adiciona evento
        Button button = GetComponent<Button>();
        button.onClick.AddListener(PlayClickSound);
    }

    private void PlayClickSound()
    {
        if (clickSound == null || globalSource == null) return;
        globalSource.PlayOneShot(clickSound, volume);
    }
}
