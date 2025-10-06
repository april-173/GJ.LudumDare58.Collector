using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ButtonHoverAnimation : MonoBehaviour, IPointerEnterHandler
{
    public Image highlightImage;
    public float maxWidth;
    public float speed;

    private Coroutine currentCoroutine;
    private Vector2 originPos;

    private void Start()
    {
        originPos = highlightImage.rectTransform.localPosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(ButtonFlash());
    }

    private IEnumerator ButtonFlash()
    {
        if (highlightImage == null) yield break;

        RectTransform rt = highlightImage.rectTransform;
        float height = rt.rect.height;

        // ��ʼ״̬��pivot ���
        rt.pivot = new Vector2(0f, 0.5f);
        rt.sizeDelta = new Vector2(0f, height);
        rt.localPosition = originPos;

        // ɨ��׶Σ���������չ���
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            rt.sizeDelta = new Vector2(Mathf.Lerp(0f, maxWidth, t), height);
            yield return null;
        }

        // ��ת pivot ���ұ�
        rt.pivot = new Vector2(1f, 0.5f);
        rt.localPosition = new Vector2(rt.localPosition.x + maxWidth, rt.localPosition.y);

        // �ջؽ׶Σ���ȴ� maxWidth -> 0
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            rt.sizeDelta = new Vector2(Mathf.Lerp(maxWidth, 0f, t), height);
            yield return null;
        }

        // ����
        rt.sizeDelta = new Vector2(0f, height);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.localPosition = originPos;
    }



}
