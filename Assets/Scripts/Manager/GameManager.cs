using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI References")]
    public Image fullScreenImage;        // ȫ������
    public TextMeshProUGUI centerText;  // ��������/����
    public Button diveButton;            // Dive ��ť

    [Header("Animation Settings")]
    public float fadeDuration = 1f;
    public float numberAnimationDuration = 2f;
    public float pauseTime = 0.5f;

    private void Awake()
    {
        // ��������
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // ��ʼ�� UI ״̬
        ResetUI();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        StopAllCoroutines();
    }

    private void Start()
    {
        // ���������һ��������ֱ�Ӳ��ſ�������
        StartCoroutine(StartSceneRoutine());
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ÿ�μ��س��������� UI �����ſ�������
        ResetUI();
        StartCoroutine(StartSceneRoutine());
    }

    private void ResetUI()
    {
        if (InputManager.Instance != null)
            InputManager.Instance.enableInput = false;

        if (fullScreenImage != null)
        {
            fullScreenImage.gameObject.SetActive(true);
            fullScreenImage.color = new Color(fullScreenImage.color.r, fullScreenImage.color.g, fullScreenImage.color.b, 1f);
        }

        if (centerText != null)
        {
            centerText.gameObject.SetActive(true);
            centerText.text = "0";
        }

        if (diveButton != null)
            diveButton.gameObject.SetActive(false);
    }

    #region -- �������� 0 -> 1000 --
    private IEnumerator StartSceneRoutine()
    {
        centerText.text = 0.ToString("D5");
        yield return new WaitForSeconds(1f);

        if (centerText != null)
            yield return AnimateNumber(0, 1000, numberAnimationDuration);

        yield return new WaitForSeconds(pauseTime);

        if (fullScreenImage != null)
            yield return FadeImage(fullScreenImage, 1f, 0f, fadeDuration);

        if (centerText != null)
            centerText.gameObject.SetActive(false);

        if (fullScreenImage != null)
            fullScreenImage.gameObject.SetActive(false);

        if (InputManager.Instance != null)
            InputManager.Instance.enableInput = true;
    }
    #endregion

    #region -- ���򶯻� 1000 -> 0 ����ʾ Dive ��ť --
    public void PlayReverseAnimation()
    {
        StartCoroutine(ReverseRoutine());
    }

    private IEnumerator ReverseRoutine()
    {
        ResetUI();
        centerText.text = 1000.ToString("D5");

        if (fullScreenImage != null)
            yield return FadeImage(fullScreenImage, 0f, 1f, fadeDuration);

        if (centerText != null)
            yield return AnimateNumber(1000, 0, numberAnimationDuration);

        yield return new WaitForSeconds(pauseTime);

        if (diveButton != null)
        {
            diveButton.gameObject.SetActive(true);
            diveButton.onClick.RemoveAllListeners();
            diveButton.onClick.AddListener(() =>
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            });
        }
    }
    #endregion

    #region -- �������� --
    public void PlayDeathAnimation(string deathMessage)
    {
        StartCoroutine(DeathRoutine(deathMessage));
    }

    private IEnumerator DeathRoutine(string deathMessage)
    {
        if (InputManager.Instance != null)
            InputManager.Instance.enableInput = false;

        ResetUI();

        if (centerText != null)
            centerText.text = deathMessage;

        if (fullScreenImage != null)
            yield return FadeImage(fullScreenImage, 0f, 1f, fadeDuration);

        yield return new WaitForSeconds(pauseTime);

        if (diveButton != null)
        {
            diveButton.gameObject.SetActive(true);
            diveButton.onClick.RemoveAllListeners();
            diveButton.onClick.AddListener(() =>
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            });
        }
    }
    #endregion

    #region -- ���ֶ��� --
    private IEnumerator AnimateNumber(int from, int to, float duration)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            int value = Mathf.RoundToInt(Mathf.Lerp(from, to, t));
            if (centerText != null)
                centerText.text = value.ToString("D5");
            yield return null;
        }
        if (centerText != null)
            centerText.text = to.ToString("D5");
    }
    #endregion

    #region -- �������뵭������ --
    public IEnumerator FadeImage(Image img, float fromAlpha, float toAlpha, float duration)
    {
        if (img == null) yield break;

        float t = 0f;
        Color c = img.color;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float alpha = Mathf.Lerp(fromAlpha, toAlpha, t);
            img.color = new Color(c.r, c.g, c.b, alpha);
            yield return null;
        }

        img.color = new Color(c.r, c.g, c.b, toAlpha);
    }
    #endregion
}



