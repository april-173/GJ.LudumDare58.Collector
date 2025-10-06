using System.Collections;
using UnityEngine;

public class Target : MonoBehaviour
{
    [Header("UI & Visuals")]
    public RadarPanel radarPanel;
    public SpriteRenderer[] childSprites;
    public Color startColor = Color.white;
    public Color endColor = Color.yellow;

    [Header("Collection Settings")]
    public float collectTime = 2f;
    public Transform playerTransform;
    public float flySpeed = 8f;

    [Header("Identification")]
    [Range(1, 6)] public int itemID = 1;

    private float progress = 0f;
    private bool isCollecting = false;
    private bool isCollected = false;
    private bool isTrigger = false;

    private void Awake()
    {
        foreach (var sprite in childSprites)
        {
            sprite.color = startColor;
            sprite.enabled = false;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (isCollecting || !childSprites[0].enabled) return;

        if (other.CompareTag("ScannerTrigger"))
        {
            isTrigger = true;

            progress += Time.deltaTime;
            foreach (var sprite in childSprites)
                sprite.color = Color.Lerp(startColor, endColor, progress / collectTime);

            if (progress >= collectTime) StartCollection();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("ScannerTrigger"))
        {
            isTrigger = false;
        }
    }

    private void Update()
    {
        if (!isTrigger && progress > 0)
        {
            progress -= Time.deltaTime;
            progress = Mathf.Max(0, progress);
            foreach (var sprite in childSprites)
                sprite.color = Color.Lerp(startColor, endColor, progress / collectTime);
        }

        float dis = Vector2.Distance(transform.position, playerTransform.position);
        float radius = 14f;

        if (childSprites[0].enabled && dis > radius) 
        {
            foreach (var sprite in childSprites) 
                sprite.enabled = false;
        }
    }

    public void StartCollection()
    {
        if (isCollected || isCollecting) return;
        isCollecting = true;
        StartCoroutine(CollectionRoutine());
    }

    public void StopCollection()
    {
        if (isCollected) return;
        isCollecting = false;
        progress = 0f;
        ResetChildColors();
    }

    private IEnumerator CollectionRoutine()
    {
        while (progress < 1f)
        {
            if (!isCollecting)
            {
                progress = 0f;
                ResetChildColors();
                yield break;
            }

            progress += Time.deltaTime / collectTime;

            foreach (var sprite in childSprites)
            {
                sprite.color = Color.Lerp(startColor, endColor, progress);
            }

            yield return null;
        }

        isCollected = true;
        isCollecting = false;

        StartCoroutine(FlyToPlayer());
    }

    private void ResetChildColors()
    {
        foreach (var sprite in childSprites)
        {
            sprite.color = startColor;
        }
    }

    private void OnCollected()
    {
        radarPanel.TargetCollect(itemID);
    }

    private IEnumerator FlyToPlayer()
    {
        float speed = flySpeed;
        float minDistance = 0.1f;

        while (true)
        {
            if (playerTransform == null) break;

            Vector3 targetPos = playerTransform.position;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

            speed += Time.deltaTime * 10f;

            if (Vector3.Distance(transform.position, targetPos) < minDistance)
                break;

            yield return null;
        }

        OnCollected();
        Destroy(gameObject);
    }

    public void RadarScan()
    {
        Vector2 dir = playerTransform.position - transform.position;
        float distance = dir.magnitude;

        int layerMask = LayerMask.GetMask("Background");

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir.normalized, distance, layerMask);

        if (hit.collider == null)
        {
            foreach (var sprite in childSprites)
                sprite.enabled = true;
        }
        else
        {
            foreach (var sprite in childSprites)
                sprite.enabled = false;
        }
    }
}

