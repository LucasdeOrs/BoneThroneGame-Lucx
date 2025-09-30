using UnityEngine;
using UnityEngine.UI;

public class SoundManagement : MonoBehaviour
{
    [Header("Ícones")]
    public Sprite iconSoundOn;
    public Sprite iconSoundOff;

    [Header("Referência ao botão")]
    public Image buttonImage;

    private bool isMuted = false;

    void Start()
    {
        // Se já tiver salvo a preferência do player
        isMuted = PlayerPrefs.GetInt("Muted", 0) == 1;
        ApplyMute();
    }

    public void ToggleMute()
    {
        isMuted = !isMuted;
        PlayerPrefs.SetInt("Muted", isMuted ? 1 : 0);
        PlayerPrefs.Save();

        ApplyMute();
    }

    private void ApplyMute()
    {
        AudioListener.pause = isMuted;
        if (buttonImage != null)
            buttonImage.sprite = isMuted ? iconSoundOff : iconSoundOn;
    }
}
