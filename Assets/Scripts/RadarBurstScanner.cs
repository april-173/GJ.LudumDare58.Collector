using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RadarBurstScanner : MonoBehaviour
{
    [Header("Scan Settings")]
    public float scanDelay = 0.5f;
    public int rayCount = 60;
    public float rayDistance = 10f;
    public LayerMask detectableLayer;
    public LayerMask reserveDetectableLayer;
    public LayerMask mineralLayer;
    public LayerMask targetLayer;
    public GameObject hitMarkerPrefab;
    public Transform hitMarkerParent;

    [Header("Debug")]
    public bool showDebugRays = true;

    private bool isScanning = false;

    private void Start()
    {
        InputManager.Instance.OnPing += TriggerRadarScan;
    }

    public void TriggerRadarScan()
    {
        if (!isScanning)
            StartCoroutine(PerformScan());
    }

    private IEnumerator PerformScan()
    {
        isScanning = true;
        yield return new WaitForSeconds(scanDelay);

        float angleStep = 360f / rayCount;

        for (int i = 0; i < rayCount; i++)
        {
            float angle = i * angleStep;
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, rayDistance, detectableLayer);
            if (hit.collider != null)
            {
                SpawnHitMarker(hit.point);
            }

            if (showDebugRays)
                Debug.DrawRay(transform.position, dir * rayDistance, hit.collider ? Color.green : Color.red, 0.3f);
        }

        Collider2D[] boidColliders = Physics2D.OverlapCircleAll(transform.position, rayDistance, reserveDetectableLayer);
        foreach(Collider2D col in boidColliders)
        {
            SpawnHitMarker(col.transform.position);
            col.gameObject.GetComponent<Boid>()?.RadarScan();
        }

        Collider2D[] mineralColliders = Physics2D.OverlapCircleAll(transform.position, rayDistance, mineralLayer);
        foreach(Collider2D col in mineralColliders)
        {
            col.gameObject.GetComponent<Mineral>()?.RadarScan();
        }

        Collider2D[] targetColliders = Physics2D.OverlapCircleAll(transform.position, rayDistance, targetLayer);
        foreach (Collider2D col in targetColliders)
        {
            col.gameObject.GetComponent<Target>()?.RadarScan();
        }

        isScanning = false;
    }

    private void SpawnHitMarker(Vector2 position)
    {
        if (hitMarkerPrefab == null) return;
        Instantiate(hitMarkerPrefab, position, Quaternion.identity, hitMarkerParent);
    }
}

