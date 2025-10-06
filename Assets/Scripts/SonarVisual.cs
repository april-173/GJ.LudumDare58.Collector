using UnityEngine;
using UnityEngine.UI;

public class SonarVisual : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image ringImage;

    [Header("Wave Setting")]
    [SerializeField] private float duration = 1f;
    [SerializeField] private float maxScale = 5f;
    [SerializeField] private float startAlpha = 1f;
    [SerializeField] private float endAlpha = 0f;

    private RectTransform ringRect;
    private Vector3 basePosition;
    private bool isRunning = false;
    private float timer = 0f;

    private void Awake()
    {
        if (ringImage == null) ringImage = GetComponent<Image>();
        ringRect = ringImage.GetComponent<RectTransform>();
        basePosition = ringRect.localPosition;
    }

    private void Update()
    {
        UpdateSonar();
    }

    private void UpdateSonar()
    {
        if (!isRunning) return;

        timer += Time.deltaTime;
        float t = timer / duration;

        float scale = Mathf.Lerp(0f, maxScale, t);
        ringRect.localScale = Vector3.one * scale;

        float alpha = Mathf.Lerp(startAlpha, endAlpha, t);
        var color = ringImage.color;
        color.a = alpha;
        ringImage.color = color;

        if (timer >= duration) isRunning = false;
    }

    public void StartSonar()
    {
        timer = 0f;
        isRunning = true;
    }

    public void StopSonar()
    {
        isRunning = false;
    }
}
