using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    [Header("Referências das partes")]
    public SpriteRenderer headRenderer;
    public SpriteRenderer chestRenderer;
    public SpriteRenderer legsRenderer;
    public SpriteRenderer swordRenderer;
    public SpriteRenderer shieldRenderer;

    [Header("Peças equipadas")]
    public EquipmentPart head;
    public EquipmentPart chest;
    public EquipmentPart legs;
    public EquipmentPart sword;
    public EquipmentPart shield;

    private int currentFrame = 0;
    private float frameTimer = 0f;
    private bool isAttacking = false;
    private bool fastClickMode = false;
    private bool isClicking = false;

    [Header("Configuração da Animação")]
    public float baseFrameRate = 0.16f;

    void Start()
    {
        ApplyFrame(0);
    }

    void Update()
    {
        if (!isAttacking) return;

        frameTimer += Time.deltaTime;

        if (frameTimer >= baseFrameRate)
        {
            frameTimer -= baseFrameRate;
            currentFrame++;
            int maxFrames = GetMaxFrames();

            if (fastClickMode)
            {
                int start = Mathf.Max(0, maxFrames - 3);

                if (currentFrame > maxFrames - 1)
                    currentFrame = start;

                ApplyFrame(currentFrame);

                if (!isClicking && currentFrame >= maxFrames - 1)
                    StopAttackAndReset();
            }
            else
            {
                if (currentFrame >= maxFrames)
                    StopAttackAndReset();
                else
                    ApplyFrame(currentFrame);
            }
        }
    }

    public void PlayAttack(bool isFastClick)
    {
        fastClickMode = isFastClick;
        isAttacking = true;
        isClicking = true;
        frameTimer = 0f;

        int maxFrames = GetMaxFrames();

        if (fastClickMode)
            currentFrame = Mathf.Max(0, maxFrames - 3);
        else
            currentFrame = 2;

        ApplyFrame(currentFrame);
    }

    public void StopClicking()
    {
        isClicking = false;
    }

    private void StopAttackAndReset()
    {
        isAttacking = false;
        fastClickMode = false;
        currentFrame = 0;
        ApplyFrame(currentFrame);
    }

    private void ApplyFrame(int frame)
    {
        if (head != null && frame < head.animationFrames.Length)
            headRenderer.sprite = head.animationFrames[frame];

        if (chest != null && frame < chest.animationFrames.Length)
            chestRenderer.sprite = chest.animationFrames[frame];

        if (legs != null && frame < legs.animationFrames.Length)
            legsRenderer.sprite = legs.animationFrames[frame];

        if (sword != null && frame < sword.animationFrames.Length)
            swordRenderer.sprite = sword.animationFrames[frame];

        if (shield != null && frame < shield.animationFrames.Length)
            shieldRenderer.sprite = shield.animationFrames[frame];
    }

    public void Equip(string part, EquipmentPart newPart)
    {
        switch (part)
        {
            case "Head":
                head = newPart;
                if (head != null && head.animationFrames.Length > 0)
                    headRenderer.sprite = head.animationFrames[0];
                break;

            case "Chest":
                chest = newPart;
                if (chest != null && chest.animationFrames.Length > 0)
                    chestRenderer.sprite = chest.animationFrames[0];
                break;

            case "Legs":
                legs = newPart;
                if (legs != null && legs.animationFrames.Length > 0)
                    legsRenderer.sprite = legs.animationFrames[0];
                break;

            case "Sword":
                sword = newPart;
                if (sword != null && sword.animationFrames.Length > 0)
                    swordRenderer.sprite = sword.animationFrames[0];
                break;

            case "Shield":
                shield = newPart;
                if (shield != null && shield.animationFrames.Length > 0)
                    shieldRenderer.sprite = shield.animationFrames[0];
                break;
        }
    }

    private int GetMaxFrames()
    {
        int max = 0;
        if (head != null) max = Mathf.Max(max, head.animationFrames.Length);
        if (chest != null) max = Mathf.Max(max, chest.animationFrames.Length);
        if (legs != null) max = Mathf.Max(max, legs.animationFrames.Length);
        if (sword != null) max = Mathf.Max(max, sword.animationFrames.Length);
        if (shield != null) max = Mathf.Max(max, shield.animationFrames.Length);
        return max;
    }
}
