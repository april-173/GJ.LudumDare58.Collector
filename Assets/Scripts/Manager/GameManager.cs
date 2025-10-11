using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI References")]
    public RadarPanel radarPanel;
    public Image fullScreenImage;       // ȫ������
    public Image fullScreenImageRes;    // ����ȫ������
    public TextMeshProUGUI centerText;  // ��������/����
    public Button diveButton;           // Dive ��ť
    public Button mainButton;           // Dive ��ť
    public GameObject[] targetImages;

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

        PressProgressButton dive = diveButton.GetComponent<PressProgressButton>();
        if (dive != null)
            dive.onHoldComplete += StartDive;
        PressProgressButton main = mainButton.GetComponent<PressProgressButton>();
        if (main != null)
            main.onHoldComplete += StartMain;
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

        if(fullScreenImageRes != null)
        {
            fullScreenImageRes.gameObject.SetActive(false);
            fullScreenImageRes.color = new Color(fullScreenImageRes.color.r, fullScreenImageRes.color.g, fullScreenImageRes.color.b, 0f);
        }

        if (centerText != null)
        {
            centerText.gameObject.SetActive(true);
            centerText.text = "0";
        }

        if (diveButton != null)
            diveButton.gameObject.SetActive(false);

        if(mainButton != null)
            mainButton.gameObject.SetActive(false);

        if(targetImages != null)
            foreach (var image in targetImages)
                image.gameObject.SetActive(false); 
    }

    #region < �������� >
    private IEnumerator StartSceneRoutine()
    {
        centerText.text = 0.ToString("D5");

        if (fullScreenImageRes != null)
        {
            fullScreenImageRes.gameObject.SetActive(true);
            yield return FadeImage(fullScreenImageRes, 1f, 0f, fadeDuration);
        }

        yield return new WaitForSeconds(1f);

        if (fullScreenImageRes != null)
            fullScreenImageRes.gameObject.SetActive(false);

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

    #region < ���򶯻� >
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
            diveButton.gameObject.SetActive(true);

        if (mainButton != null)
            mainButton.gameObject.SetActive(true);

        if(targetImages != null)
            foreach (var image in targetImages)
                image.gameObject.SetActive(true);

        yield return new WaitForSeconds(1f);

        StartCoroutine(SettleAccounts());
    }
    #endregion

    #region < �������� >
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
            diveButton.gameObject.SetActive(true);

        if (mainButton != null)
            mainButton.gameObject.SetActive(true);
    }
    #endregion

    #region < ���ֶ��� >
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

    #region < �������뵭������ >
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

    #region < �������˳� >
    public void StartDive()
    {
        AudioManager.Instance.PlayButtonSound();
        StartCoroutine(Dive());
    }

    private IEnumerator Dive()
    {
        if (fullScreenImageRes != null)
        {
            fullScreenImageRes.gameObject.SetActive(true);
            yield return FadeImage(fullScreenImageRes, 0f, 1f, fadeDuration);
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void StartMain()
    {
        AudioManager.Instance.PlayButtonSound();
        StartCoroutine(Main());
    }

    private IEnumerator Main()
    {
        if (fullScreenImageRes != null)
        {
            fullScreenImageRes.gameObject.SetActive(true);
            yield return FadeImage(fullScreenImageRes, 0f, 1f, fadeDuration);
        }

        SceneManager.LoadScene(0);
    }

    #endregion

    #region < ���� >
    private IEnumerator SettleAccounts()
    {
        for (int i = 0; i < radarPanel.targetsState.Length; i++)
        {
            if (!radarPanel.targetsState[i]) continue;
            for (int j = 0; j < targetImages[i].transform.childCount; j++)
            {
                targetImages[i].transform.GetChild(j).GetComponent<Image>().color = radarPanel.onColor;
                AudioManager.Instance.PlayButtonSound();
            }
            yield return new WaitForSeconds(1f);
        }
    }

    #endregion
}



