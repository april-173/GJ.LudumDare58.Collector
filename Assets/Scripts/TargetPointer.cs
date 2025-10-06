using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetPointer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;                // ���
    [SerializeField] private RectTransform indicatorParent;   // UI�����壨ͨ����Canvas��
    [SerializeField] private GameObject indicatorPrefab;      // ָʾ��Ԥ�Ƽ�

    [Header("Indicator Settings")]
    [SerializeField] private float radius = 150f;             // UI�뾶��ָʾ����������ĵľ��룩
    [SerializeField] private float smoothSpeed = 8f;          // ƽ���ƶ��ٶ�
    [SerializeField] private float maxAlpha = 1f;             // ���͸����
    [SerializeField] private float minAlpha = 0.3f;           // ��С͸����
    [SerializeField] private float maxDistance = 100f;        // ͸������Զ������루������ֵ͸���ȱ�����С��

    [Header("Targets")]
    [SerializeField] private Transform[] targets;             // Ŀ�����飨�ɶ�̬���£�

    private Dictionary<Transform, RectTransform> indicators = new();

    void Update()
    {
        if (player == null || indicatorPrefab == null || indicatorParent == null) return;

        // ���Ŀ���б�����ʧЧĿ��
        UpdateTargets();

        foreach (var target in targets)
        {
            if (target == null) continue;

            // �����û��ΪĿ�괴��ָʾ�����򴴽�
            if (!indicators.ContainsKey(target))
            {
                GameObject newIndicator = Instantiate(indicatorPrefab, indicatorParent);
                indicators[target] = newIndicator.GetComponent<RectTransform>();
            }

            RectTransform indicator = indicators[target];

            // ����Ŀ�������ҵķ���
            Vector3 dir = (target.position - player.position).normalized;

            // �̶��뾶λ��
            Vector3 desiredPos = dir * radius;

            // ƽ���ƶ�
            indicator.anchoredPosition = Vector2.Lerp(indicator.anchoredPosition, desiredPos, Time.deltaTime * smoothSpeed);

            // ��תʹָʾ������Ŀ��
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            indicator.rotation = Quaternion.Lerp(indicator.rotation, Quaternion.Euler(0, 0, angle - 90), Time.deltaTime * smoothSpeed);

            // ����͸���ȣ����ھ��룩
            float distance = Vector3.Distance(player.position, target.position);
            float alpha = Mathf.Lerp(maxAlpha, minAlpha, Mathf.Clamp01(distance / maxDistance));

            Image img = indicator.GetComponent<Image>();
            if (img != null)
            {
                Color c = img.color;
                c.a = alpha;
                img.color = c;
            }
        }
    }

    /// <summary>
    /// �����Ѿ����ٵ�Ŀ��򳬳���������
    /// </summary>
    private void UpdateTargets()
    {
        // �Ƴ������ڵ�Ŀ��������ٵĶ���
        List<Transform> toRemove = new List<Transform>();
        foreach (var kvp in indicators)
        {
            if (kvp.Key == null)
            {
                Destroy(kvp.Value.gameObject);
                toRemove.Add(kvp.Key);
            }
        }
        foreach (var key in toRemove)
        {
            indicators.Remove(key);
        }

        // Ŀ������Ϊ��ʱֱ�ӷ���
        if (targets == null || targets.Length == 0) return;

        // �Ƴ����������ڵ�ָʾ��
        List<Transform> currentList = new List<Transform>(targets);
        List<Transform> extraKeys = new List<Transform>();

        foreach (var key in indicators.Keys)
        {
            if (!currentList.Contains(key))
                extraKeys.Add(key);
        }

        foreach (var key in extraKeys)
        {
            Destroy(indicators[key].gameObject);
            indicators.Remove(key);
        }
    }
}


