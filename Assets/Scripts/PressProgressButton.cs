using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PressProgressButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("UI Reference")]
    [SerializeField] private Image progressImage;

    [Header("Progress Settings")]
    [SerializeField] private float holdTime = 1.5f;
    [SerializeField] private float retreatSpeed = 5f;
    [SerializeField] private float flashSpeed = 3f;

    public event Action onHoldComplete;

    private RectTransform rt;
    private Coroutine progressRoutine;
    private bool isHolding = false;
    private float progress = 0f;
    private Vector2 origin;

    private void Awake()
    {
        if (progressImage != null)
        {
            rt = progressImage.rectTransform;
            rt.pivot = new Vector2(0f, 0.5f);
            rt.sizeDelta = new Vector2(0f, rt.sizeDelta.y);
        }

        origin = rt.localPosition;
    }

    private void OnDisable()
    {
        if (progressRoutine != null)
            StopCoroutine(progressRoutine);

        if (rt != null)
        {
            rt.sizeDelta = new Vector2(0f, rt.sizeDelta.y);
            rt.pivot = new Vector2(0f, 0.5f);
        }

        isHolding = false;
        progress = 0f;
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        if (progressRoutine != null) StopCoroutine(progressRoutine);
        isHolding = true;
        progressRoutine = StartCoroutine(ProgressRoutine());
    }

    public void OnPointerUp(PointerEventData eventData) => isHolding = false;
    public void OnPointerExit(PointerEventData eventData) => isHolding = false;

    private IEnumerator ProgressRoutine()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);

        rt.pivot = new Vector2(0f, 0.5f);
        rt.localPosition = origin;

        float width = ((RectTransform)transform).rect.width;
        float height = rt.rect.height;

        while (isHolding && progress < 1f)
        {
            progress += Time.deltaTime / holdTime;
            rt.sizeDelta = new Vector2(Mathf.Lerp(0f, width, progress), height);
            yield return null;
        }


        if (progress >= 1f)
        {
            onHoldComplete?.Invoke();
            
            if(gameObject.name != "Resume")
            {
                yield return StartCoroutine(FlashAndDisappear(width, height));
            }
        }
        else
        {
            yield return StartCoroutine(Retreat(width, height));
        }


        progress = 0f;
        rt.sizeDelta = new Vector2(0f, height);
        rt.pivot = new Vector2(0f, 0.5f);
        progressRoutine = null;
    }

    private IEnumerator Retreat(float width, float height)
    {
        float currentWidth = rt.sizeDelta.x;
        while (currentWidth > 0f)
        {
            currentWidth -= Time.deltaTime * width * retreatSpeed;
            rt.sizeDelta = new Vector2(Mathf.Max(0f, currentWidth), height);
            yield return null;
        }
    }

    private IEnumerator FlashAndDisappear(float width, float height)
    {
        float t = 0f;

        rt.pivot = new Vector2(1f, 0.5f);
        rt.localPosition = new Vector2(rt.localPosition.x + width, rt.localPosition.y);

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * flashSpeed;
            rt.sizeDelta = new Vector2(Mathf.Lerp(width, 0f, t), height);
            yield return null;
        }
    }
}

