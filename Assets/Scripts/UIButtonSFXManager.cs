using UnityEngine;

public class UIButtonSFXManager : MonoBehaviour
{
    void Awake()
    {
        // Cria (ou pega) um AudioSource global
        AudioSource src = GetComponent<AudioSource>();
        if (src == null) src = gameObject.AddComponent<AudioSource>();

        src.playOnAwake = false;
        src.loop = false;
        src.spatialBlend = 0f; // 2D

        UIButtonSFX.globalSource = src;
    }
}
