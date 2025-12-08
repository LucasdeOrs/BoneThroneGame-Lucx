using UnityEngine;

public class EnemyHealthBar : MonoBehaviour
{
    public EnemyController enemy;
    public Transform followTarget;
    public Vector3 offset = new Vector3(0f, 1.5f, 0f);
    public Transform fill;

    private float baseFillScaleX = 1f;

    void Awake()
    {
        if (followTarget == null && enemy != null)
            followTarget = enemy.transform;

        if (fill != null)
            baseFillScaleX = fill.localScale.x;
    }

    void LateUpdate()
    {
        if (followTarget != null)
            transform.position = followTarget.position + offset;
    }

    public void SetValue(int current, int max)
    {
        if (fill == null || max <= 0) return;
        float ratio = Mathf.Clamp01((float)current / max);
        Vector3 s = fill.localScale;
        s.x = baseFillScaleX * ratio;
        fill.localScale = s;
    }
}
