using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetPointer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;                // 玩家
    [SerializeField] private RectTransform indicatorParent;   // UI父物体（通常是Canvas）
    [SerializeField] private GameObject indicatorPrefab;      // 指示器预制件

    [Header("Indicator Settings")]
    [SerializeField] private float radius = 150f;             // UI半径（指示器与玩家中心的距离）
    [SerializeField] private float smoothSpeed = 8f;          // 平滑移动速度
    [SerializeField] private float maxAlpha = 1f;             // 最大透明度
    [SerializeField] private float minAlpha = 0.3f;           // 最小透明度
    [SerializeField] private float maxDistance = 100f;        // 透明度最远计算距离（超过此值透明度保持最小）

    [Header("Targets")]
    [SerializeField] private Transform[] targets;             // 目标数组（可动态更新）

    private Dictionary<Transform, RectTransform> indicators = new();

    void Update()
    {
        if (player == null || indicatorPrefab == null || indicatorParent == null) return;

        // 检查目标列表并清理失效目标
        UpdateTargets();

        foreach (var target in targets)
        {
            if (target == null) continue;

            // 如果还没有为目标创建指示器，则创建
            if (!indicators.ContainsKey(target))
            {
                GameObject newIndicator = Instantiate(indicatorPrefab, indicatorParent);
                indicators[target] = newIndicator.GetComponent<RectTransform>();
            }

            RectTransform indicator = indicators[target];

            // 计算目标相对玩家的方向
            Vector3 dir = (target.position - player.position).normalized;

            // 固定半径位置
            Vector3 desiredPos = dir * radius;

            // 平滑移动
            indicator.anchoredPosition = Vector2.Lerp(indicator.anchoredPosition, desiredPos, Time.deltaTime * smoothSpeed);

            // 旋转使指示器朝向目标
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            indicator.rotation = Quaternion.Lerp(indicator.rotation, Quaternion.Euler(0, 0, angle - 90), Time.deltaTime * smoothSpeed);

            // 更新透明度（基于距离）
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
    /// 清理已经销毁的目标或超出数组的情况
    /// </summary>
    private void UpdateTargets()
    {
        // 移除不存在的目标或已销毁的对象
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

        // 目标数组为空时直接返回
        if (targets == null || targets.Length == 0) return;

        // 移除不在数组内的指示器
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


