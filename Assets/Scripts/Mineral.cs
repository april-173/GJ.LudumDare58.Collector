using System.Collections;
using UnityEngine;

public class Mineral : MonoBehaviour
{
    [Header("Scan Settings")]
    public SpriteRenderer mineralRenderer;
    public float scanSpeed;
    public float breakThreshold;

    [Header("Break Settings")]
    public Transform mineralVisual;
    public float breakScale;
    public GameObject fragmentPrefab;
    public int fragmentCount;
    public float fragmentSpreadRadius;
    public Transform playerTransform;

    [Header("Color")]
    public Color startColor;
    public Color endColor;

    [Header("Rebirth")]
    public float rebirthTime;
    private float rebirthTimer;

    private float scanProgress;
    private bool isBroken;
    private bool isTrigger;
    private bool isDestroy;

    private void Awake()
    {
        mineralRenderer.enabled = false;
    }

    private void Update()
    {
        if (scanProgress > 0 && !isTrigger && !isDestroy) 
        {
            scanProgress -= scanSpeed * Time.deltaTime;
            scanProgress = Mathf.Max(0, scanProgress);
            mineralRenderer.color = Color.Lerp(startColor, endColor, scanProgress / breakThreshold);
        }

        if (isDestroy)
        {
            rebirthTimer += Time.deltaTime;
        }
        else
        {
            rebirthTimer = 0;
        }

        if (rebirthTimer >= rebirthTime)
        {
            scanProgress = 0;
            isBroken = false;
            isTrigger = false;
            isDestroy = false;
            mineralRenderer.color = new Color(startColor.r, startColor.g, startColor.b, 1);
            mineralVisual.localScale = Vector3.one;
        }

        float dis = Vector2.Distance(transform.position, playerTransform.position);
        float radius = 14f;

        if (mineralRenderer.enabled && dis > radius)
        {
            mineralRenderer.enabled = false;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (isBroken || !mineralRenderer.enabled) return;

        if (other.CompareTag("ScannerTrigger"))
        {
            isTrigger = true;

            scanProgress += scanSpeed * Time.deltaTime;
            mineralRenderer.color = Color.Lerp(startColor, endColor, scanProgress / breakThreshold);

            if (scanProgress >= breakThreshold) StartCoroutine(BreakMineral());
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("ScannerTrigger"))
        {
            isTrigger = false;
        }
    }

    private IEnumerator BreakMineral()
    {
        isBroken = true;
        AudioManager.Instance.PlayMineralSound();

        Vector3 startScale = mineralVisual.localScale;
        Vector3 targetScale = startScale * breakScale;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.2f;
            mineralVisual.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        for (int i = 0; i < fragmentCount; i++)
        {
            Vector3 offset = Random.insideUnitCircle * fragmentSpreadRadius;
            GameObject frag = Instantiate(fragmentPrefab, mineralVisual.position + offset, Quaternion.identity);
            StartCoroutine(MoveFragmentToPlayer(frag));
        }

        mineralRenderer.color = new Color(startColor.r, startColor.g, startColor.b, 0);
        isDestroy = true;
    }

    private IEnumerator MoveFragmentToPlayer(GameObject fragment)
    {
        float speed = 8f;
        float minDistance = 0.1f;

        while (fragment != null)
        {
            if (playerTransform == null) break;

            Vector3 targetPos = playerTransform.position;
            fragment.transform.position = Vector3.MoveTowards(fragment.transform.position, targetPos, speed * Time.deltaTime);

            speed += Time.deltaTime * 10f;

            if (Vector3.Distance(fragment.transform.position, targetPos) < minDistance)
                break;

            yield return null;
        }

        Destroy(fragment);
        if (StateManager.Instance.HydrogenRation < 1)
            StartCoroutine(ResourcesAccess());
    }

    private IEnumerator ResourcesAccess()
    {
        float random = Random.Range(5, 15);

        for (int i = 0; i < random; i++)
        {
            StateManager.Instance.ChangeHydrogen(1);
            yield return new WaitForSeconds(0.01f);
        }
    }

    public void RadarScan()
    {
        Vector2 dir = playerTransform.position - transform.position;
        float distance = dir.magnitude;

        int layerMask = LayerMask.GetMask("Background");

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir.normalized, distance, layerMask);

        if (!mineralRenderer.enabled) 
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        if (hit.collider == null)
        {
            mineralRenderer.enabled = true;
        }
        else
        {
            mineralRenderer.enabled = false;
        }
    }
}
