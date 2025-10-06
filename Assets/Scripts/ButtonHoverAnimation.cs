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

        // 初始状态：pivot 左侧
        rt.pivot = new Vector2(0f, 0.5f);
        rt.sizeDelta = new Vector2(0f, height);
        rt.localPosition = originPos;

        // 扫光阶段：从左到右扩展宽度
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            rt.sizeDelta = new Vector2(Mathf.Lerp(0f, maxWidth, t), height);
            yield return null;
        }

        // 翻转 pivot 到右边
        rt.pivot = new Vector2(1f, 0.5f);
        rt.localPosition = new Vector2(rt.localPosition.x + maxWidth, rt.localPosition.y);

        // 收回阶段：宽度从 maxWidth -> 0
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            rt.sizeDelta = new Vector2(Mathf.Lerp(maxWidth, 0f, t), height);
            yield return null;
        }

        // 重置
        rt.sizeDelta = new Vector2(0f, height);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.localPosition = originPos;
    }



}
