using UnityEngine;

public class EnemyEquipment : MonoBehaviour
{
    [Header("Renderizador")]
    public SpriteRenderer bodyRenderer;

    [Header("Sprite/Animação")]
    public EquipmentPart bodyPart;
    public float baseFrameRate = 0.16f;

    private int currentFrame;
    private float frameTimer;

    void Start()
    {
        ApplyFrame(0);
    }

    void Update()
    {
        if (bodyPart == null || bodyPart.animationFrames == null || bodyPart.animationFrames.Length == 0)
            return;

        frameTimer += Time.deltaTime;
        if (frameTimer >= baseFrameRate)
        {
            frameTimer -= baseFrameRate;
            currentFrame++;
            if (currentFrame >= bodyPart.animationFrames.Length)
                currentFrame = 0;
            ApplyFrame(currentFrame);
        }
    }

    public void SetPart(EquipmentPart part)
    {
        bodyPart = part;
        currentFrame = 0;
        frameTimer = 0f;
        ApplyFrame(0);
    }

    public void SetPart(EquipmentPart part, float? newFrameRate = null)
    {
        bodyPart = part;
        if (newFrameRate.HasValue) baseFrameRate = newFrameRate.Value;
        currentFrame = 0;
        frameTimer = 0f;
        ApplyFrame(0);
    }

    private void ApplyFrame(int frame)
    {
        if (bodyRenderer == null || bodyPart == null) return;
        if (bodyPart.animationFrames == null || bodyPart.animationFrames.Length == 0) return;
        int idx = Mathf.Clamp(frame, 0, bodyPart.animationFrames.Length - 1);
        bodyRenderer.sprite = bodyPart.animationFrames[idx];
    }
}
