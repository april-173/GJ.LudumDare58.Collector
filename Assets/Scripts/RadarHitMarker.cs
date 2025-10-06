using UnityEngine;

public class RadarHitMarker : MonoBehaviour
{
    public float lifetime = 1.0f;
    public float fadeDuration = 0.5f;

    private SpriteRenderer spriteRenderer;
    private float timer;
    private float initialAlpha;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        initialAlpha = spriteRenderer.color.a;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        float alpha = Mathf.Clamp01(initialAlpha - (timer / fadeDuration));
        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = alpha;
            spriteRenderer.color = c;
        }
    }
}

