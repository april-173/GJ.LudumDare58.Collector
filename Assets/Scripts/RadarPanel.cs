using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RadarPanel : MonoBehaviour
{
    [Header("Azimuthal Point")]
    public RectTransform center;
    public RectTransform azimuthalPointPrefab;
    public float azimuthalPointRadius;
    public int azimuthalPointCount;
    public bool autoInstantiate = true;
    public bool showDebugGizmos = true;

    private List<RectTransform> spawnedAzimuthalPoints = new List<RectTransform>();

    [Header("Auto")]
    public Button autoRadar;
    public Button autoFastMove;
    public Color offColor;
    public Color onColor;
    public float SmoothColorTransition;

    private bool isRadarOn = false;
    private bool isFastMoveOn = false;

    [Header("Velocity Lines")]
    public PlayerController playerController;
    public RectTransform debugCanvas;
    public RectTransform playerIcon;
    public Color velocityLineColor;
    public Color inputLineColor;
    public float lineLengthScale;
    public float lineThickness;
    public float maxLineLength;

    private Image velocityLine;
    private Image inputLine;

    [Header("Speed Info Display ")]
    public TextMeshProUGUI hSpeedText;
    public TextMeshProUGUI vSpeedText;

    [Header("Breathing Oxygen")]
    public Image oxygenCircle;
    public float breathingSpeed;
    public float breathingScale;

    private Vector3 breathingBaseScale;
    private float breathingTimer;

    [Header("Consumption Energy")]
    public Image energyCircle;
    public float scaleSmoothSpeed;
    public float moveScale;
    public float fastMoveScale;
    public float pingScale;
    public float pingDuration;

    private Vector3 energyBaseScale;
    private Vector3 targetScale;
    private bool isMove;
    private bool isFastMove;
    private bool isPingEffectPlaying;

    [Header("Depth")]
    public TextMeshProUGUI depthText;
    public Transform playerTransform;
    public float baseDepth = 0f;
    public float depthScale = 1f;
    public float minDepth = 0f;
    public float maxDepth = 99999f;

    [Header("State")]
    public Image oxygenImage;
    public Image energyImage;
    public float maxHight;
    public Image backupEnergyImage;
    public Button generateOxygen;
    public Button generateEnergy;
    public int generarteValue;
    public float backupEnergyColorRecoveryTime;

    private Vector2 backupEnergyImageBaseSize;
    private float lastBackupEnergyValue;
    private float backupEnergyColorRecoveryTimer;

    [Header("Help")]
    public GameObject helpPanel;

    [Header("Button")]
    public PressProgressButton main;
    public PressProgressButton surface;
    public PressProgressButton help;
    public PressProgressButton resume;

    [Header("Target")]
    public Image[] targets = new Image[6];
    public bool[] targetsState = new bool[6];


    private void Awake()
    {
        backupEnergyImageBaseSize = backupEnergyImage.rectTransform.sizeDelta;
        generateOxygen.interactable = true;
        generateEnergy.interactable = true;
    }


    private void Start()
    {
        if (autoInstantiate && Application.isPlaying) UpdateOrCreateAzimuthalPoints();

        autoRadar.onClick.AddListener(() => ToggleAuto(autoRadar, ref isRadarOn));
        autoFastMove.onClick.AddListener(() => ToggleAuto(autoFastMove, ref isFastMoveOn));
        autoRadar.GetComponent<Image>().color = offColor;
        autoFastMove.GetComponent<Image>().color = offColor;
        generateOxygen.onClick.AddListener(() => StartGenerateOxygen());
        generateEnergy.onClick.AddListener(() => StartGenerateEnergy());

        CreateLine(ref velocityLine, "VelocityLine", velocityLineColor);
        CreateLine(ref inputLine, "InputLine", inputLineColor);

        if (oxygenCircle != null)
            breathingBaseScale = oxygenCircle.rectTransform.localScale;
        if (energyCircle != null) 
            energyBaseScale = energyCircle.rectTransform.localScale;

        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnMove += HandleMove;
            InputManager.Instance.OnFastMove += HandleFastMove;
            InputManager.Instance.OnPing += HandlePing;
        }

        helpPanel.SetActive(false);

        main.onHoldComplete += MainButton;
        surface.onHoldComplete += SurfaceButton;
        help.onHoldComplete += () => HelpButton(true);
        resume.onHoldComplete += () => HelpButton(false);
    }

    private void OnEnable()
    {
        if (!Application.isPlaying && autoInstantiate) UpdateOrCreateAzimuthalPoints();
    }

    private void Update()
    {
        if (playerController == null) return;

        Vector2 velocity = playerController.LinearVelocity;
        Vector2 inputDir = playerController.SmoothedInput * 5f;
        UpdateDebugLine(velocityLine, velocity, velocityLineColor);
        UpdateDebugLine(inputLine, inputDir, inputLineColor);

        UpdateSpeedInfo();
        UpdateOxygenBreathing();
        UpdateEnergyConsumption();
        DepthDisplay();
        UpdateState();

        BackupEnergyColor();
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnMove -= HandleMove;
            InputManager.Instance.OnFastMove -= HandleFastMove;
            InputManager.Instance.OnPing -= HandlePing;
        }
    }

    #region < 方位点 >
    private void UpdateOrCreateAzimuthalPoints()
    {
        if (azimuthalPointCount <= 0 || azimuthalPointPrefab == null || center == null)
        { ClearExistingAzimuthalPoints(); return; }

        if (spawnedAzimuthalPoints.Count != azimuthalPointCount) 
        {
            ClearExistingAzimuthalPoints();
            spawnedAzimuthalPoints = new List<RectTransform>(azimuthalPointCount);

            for (int i = 0; i < azimuthalPointCount; i++)
            {
                RectTransform inst = Instantiate(azimuthalPointPrefab, center);
                inst.name = $"RadarPoint_{i:00}";

                inst.anchorMin = inst.anchorMax = new Vector2(0.5f, 0.5f);
                inst.pivot = new Vector2(0.5f, 0.5f);

                spawnedAzimuthalPoints.Add(inst);
            }
        }

        float interval = 360f / azimuthalPointCount;
        for (int i = 0; i < azimuthalPointCount; i++)
        {
            float angle = i * interval;
            float rad = angle * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Sin(rad), Mathf.Cos(rad)) * azimuthalPointRadius;

            RectTransform rt = spawnedAzimuthalPoints[i];
            if (rt == null) continue;

            rt.localRotation = Quaternion.Euler(0f, 0f, angle);
            rt.anchoredPosition = offset;
            rt.localScale = Vector3.one;
        }
    }

    private void ClearExistingAzimuthalPoints()
    {
        if(spawnedAzimuthalPoints == null) return;
        for (int i = spawnedAzimuthalPoints.Count - 1; i >= 0; i--)
        {
            var rt = spawnedAzimuthalPoints[i];
            if (rt == null) continue;
            Destroy(rt.gameObject);
        }
        spawnedAzimuthalPoints.Clear();
    }
    #endregion

    #region < 自动化 >
    private Coroutine buttonColorCoroutine;

    private void ToggleAuto(Button button, ref bool state)
    {
        AudioManager.Instance.PlayButtonSound();

        state = !state;
        if (buttonColorCoroutine != null)
            StopCoroutine(buttonColorCoroutine);
        buttonColorCoroutine = StartCoroutine(TransitionButtonColor(button, state ? onColor : offColor));

        if (button == autoRadar)
            InputManager.Instance.autoPing = state;
        else if (button == autoFastMove) 
            InputManager.Instance.autoFastMove = state;
    }

    private IEnumerator TransitionButtonColor(Button button, Color targetColor)
    {
        Image img = button.GetComponent<Image>();
        if (img == null) yield break;

        Color startColor = img.color;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * SmoothColorTransition;
            img.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }
        img.color = targetColor;
    }

    #endregion

    #region < 速度线 >
    private void CreateLine(ref Image line, string name, Color color)
    {
        GameObject lineObj = new GameObject(name, typeof(RectTransform), typeof(Image));
        lineObj.transform.SetParent(debugCanvas != null ? debugCanvas : transform, false);

        RectTransform rt = lineObj.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0f, 0.5f);

        line = lineObj.GetComponent<Image>();
        line.color = color;
        rt.sizeDelta = new Vector2(0f, lineThickness);

        rt.anchoredPosition = Vector2.zero;
    }

    private void UpdateDebugLine(Image line, Vector2 direction, Color color)
    {
        if (line == null || playerIcon == null) return;

        float length = Mathf.Min(direction.magnitude * lineLengthScale, maxLineLength);
        RectTransform rt = line.rectTransform;

        Vector2 basePos = playerIcon.anchoredPosition;
        Vector2 dirNorm = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;
        rt.anchoredPosition = Vector2.zero;

        rt.sizeDelta = new Vector2(length, lineThickness);
        if (length > 0.001f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            rt.localRotation = Quaternion.Euler(0, 0, angle);
        }

        line.color = color;
    }
    #endregion

    #region < 速度信息 >
    private void UpdateSpeedInfo()
    {
        float h = playerController.LinearVelocity.x;
        float v = playerController.LinearVelocity.y;
        InfoDisplay(h, hSpeedText);
        InfoDisplay(v, vSpeedText);

        void InfoDisplay(float f, TextMeshProUGUI text)
        {
            if (Mathf.Abs(f) < 0.01) f = 0f;
            text.text = "";
            string s = f.ToString("F2");

            if (f == 0f) text.text = $"{s}";
            else if (f < 0f) text.text = $"-{s}";
            else if (f > 0f) text.text = $"+{s}";
        }
    }

    #endregion

    #region < 耗能 >
    private void UpdateOxygenBreathing()
    {
        if (oxygenCircle == null) return;

        breathingTimer += Time.deltaTime * breathingSpeed;
        float breathingOffset = Mathf.Sin(breathingTimer) * breathingScale;

        oxygenCircle.rectTransform.localScale = breathingBaseScale * (1f + breathingOffset);
    }

    private void UpdateEnergyConsumption()
    {
        if(energyCircle == null) return;

        targetScale = energyBaseScale;

        if (isFastMove)
            targetScale *= fastMoveScale;
        else if(isMove)
            targetScale *= moveScale;

        energyCircle.transform.localScale = Vector3.Lerp(
            energyCircle.transform.localScale,
            targetScale,
            Time.deltaTime * scaleSmoothSpeed
        );
        isMove = false;
    }

    private void HandleMove(Vector2Int move)
    {
        if (move != Vector2Int.zero)
            isMove = true;
    }

    private void HandleFastMove(bool isFast)
    {
        isFastMove = isFast;
    }

    private void HandlePing()
    {
        if (!isPingEffectPlaying)
            StartCoroutine(PingEffect());
    }

    private System.Collections.IEnumerator PingEffect()
    {
        isPingEffectPlaying = true;

        Vector3 startScale = energyCircle.transform.localScale;
        Vector3 peakScale = startScale * pingScale;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / (pingDuration * 0.5f);
            energyCircle.transform.localScale = Vector3.Lerp(startScale, peakScale, t);
            yield return null;
        }

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / (pingDuration * 0.5f);
            energyCircle.transform.localScale = Vector3.Lerp(peakScale, targetScale, t);
            yield return null;
        }

        isPingEffectPlaying = false;
    }

    #endregion

    #region < 深度 >

    private void DepthDisplay()
    {
        if (playerTransform == null || depthText == null) return;

        float rawDepth = Mathf.Abs((playerTransform.position.y - baseDepth) * depthScale);
        rawDepth = Mathf.Clamp(rawDepth, minDepth, maxDepth);
        depthText.text = $"{(int)rawDepth:D5}";
    }

    #endregion

    #region < 状态 >
    private void UpdateState()
    {
        oxygenImage.rectTransform.sizeDelta = new Vector2(oxygenImage.rectTransform.rect.width, maxHight * StateManager.Instance.OxygenRatio);
        energyImage.rectTransform.sizeDelta = new Vector2(energyImage.rectTransform.rect.width, maxHight * StateManager.Instance.EnergyRation);
        float ratio = StateManager.Instance.HydrogenRation;
        RectTransform rt = backupEnergyImage.rectTransform;
        rt.sizeDelta = new Vector2(ratio * backupEnergyImageBaseSize.x, ratio * backupEnergyImageBaseSize.y);
    }

    private void StartGenerateOxygen()
    {
        if (StateManager.Instance.HydrogenRation == 0)
        {
            return;
        }
        StartCoroutine(GenerateOxygen());
    }

    private IEnumerator GenerateOxygen()
    {
        AudioManager.Instance.PlayButtonSound();
        generateOxygen.interactable = false;
        Color c = oxygenImage.color;
        StartCoroutine(ChangeColor(oxygenImage, c, Color.white, 16));

        for (int i = 0; i < generarteValue; i++)
        {
            StateManager.Instance.GenerateOxygen(1);
            generateOxygen.GetComponent<Image>().color = Color.Lerp(offColor, onColor, (float)i / (float)generarteValue);
            yield return new WaitForSeconds(0.001f);
        }
        StartCoroutine(ChangeColor(oxygenImage, Color.white, c, 16));

        float t = 16;
        for (int i = 0; i < t; i++)
        {
            generateOxygen.GetComponent<Image>().color = Color.Lerp(onColor, offColor, (float)i / (float)t);
            yield return null;
        }

        generateOxygen.GetComponent<Image>().color = offColor;
        generateOxygen.interactable= true;
    }

    private void StartGenerateEnergy()
    {
        if(StateManager.Instance.HydrogenRation == 0)
        {
            return;
        }
        StartCoroutine (GenerateEnergy());
    }

    private IEnumerator GenerateEnergy()
    {
        AudioManager.Instance.PlayButtonSound();
        generateEnergy.interactable = false;
        Color c = energyImage.color;
        StartCoroutine(ChangeColor(energyImage, c, Color.white, 16));

        for (int i = 0; i < generarteValue; i++)
        {
            StateManager.Instance.GenerateEnergy(1);
            generateEnergy.GetComponent<Image>().color = Color.Lerp(offColor, onColor, (float)i / (float)generarteValue);
            yield return new WaitForSeconds(0.001f);
        }
        StartCoroutine(ChangeColor(energyImage, Color.white, c, 16));

        float t = 16;
        for (int i = 0; i < t; i++)
        {
            generateEnergy.GetComponent<Image>().color = Color.Lerp(onColor, offColor, (float)i / (float)t);
            yield return null;
        }

        generateEnergy.GetComponent<Image>().color = offColor;
        generateEnergy.interactable = true;
    }

    private IEnumerator ChangeColor(Image image, Color startColor, Color endColor,float time)
    {
        for (int i = 0; i <= time; i++)
        {
            float t = (float)i / (float)time;
            image.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }
        image.color = endColor;
        colorCoroutine = null;
    }

    private Coroutine colorCoroutine;
    private bool isWhite;
    private bool isOriginal;
    private void BackupEnergyColor()
    {
        float currentHydrogen = StateManager.Instance.CurrentHydrogen;
        if (currentHydrogen > lastBackupEnergyValue)
        {
            if (isOriginal && colorCoroutine != null) StopCoroutine(colorCoroutine);
            if (!isWhite) colorCoroutine = StartCoroutine(ChangeColor(backupEnergyImage, backupEnergyImage.color, Color.white, 18));
            lastBackupEnergyValue = currentHydrogen;
            backupEnergyColorRecoveryTimer = 0;
            isWhite = true;
            isOriginal = false;
        }
        else if(currentHydrogen < lastBackupEnergyValue)
        {
            lastBackupEnergyValue = currentHydrogen;
        }

        if (lastBackupEnergyValue == currentHydrogen && backupEnergyColorRecoveryTimer < backupEnergyColorRecoveryTime)
        {
            backupEnergyColorRecoveryTimer += Time.deltaTime;
        }

        if (backupEnergyColorRecoveryTimer >= backupEnergyColorRecoveryTime && backupEnergyImage.color != onColor)
        {
            if (colorCoroutine == null)
                colorCoroutine = StartCoroutine(ChangeColor(backupEnergyImage, Color.white, onColor, 24));
            isWhite = false;
            isOriginal = true;
        }

        if (backupEnergyColorRecoveryTimer >= backupEnergyColorRecoveryTime && colorCoroutine == null) 
        {
            isWhite = false;
            isOriginal = false;
            backupEnergyImage.color = onColor;
        }
    }


    #endregion

    #region < 按钮 >
    public void MainButton()
    {
        AudioManager.Instance.PlayButtonSound();
        GameManager.Instance.StartMain();
    }    

    public void SurfaceButton()
    {
        AudioManager.Instance.PlayButtonSound();
        GameManager.Instance.PlayReverseAnimation();
    }

    public void HelpButton(bool b)
    {
        AudioManager.Instance.PlayButtonSound();
        helpPanel.SetActive(b);
        InputManager.Instance.enableInput = !b;
    }
    #endregion

    #region < 目标 >

    public void StartTargetCollect(int id)
    {
        StartCoroutine(TargetCollect(id));
    }

    private IEnumerator TargetCollect(int id)
    {
        if (id > targets.Length) id = targets.Length;
        targetsState[id - 1] = true;

        targets[id - 1].color = Color.white;
        yield return new WaitForSeconds(0.2f);
        targets[id - 1].color = offColor;

        float t = 18;
        for (int i = 0; i < t; i++)
        {
            targets[id - 1].color = Color.Lerp(offColor, onColor, (float)i / t);
            yield return null;
        }
    }

    #endregion

    #region < 调试方法 >
    private void OnDrawGizmos()
    {
        if (!showDebugGizmos || center == null) return;

        Gizmos.color = Color.green;
        int count = Mathf.Max(1, azimuthalPointCount);
        float interval = 360f / count;

        for (int i = 0; i < count; i++)
        {
            float angle = i * interval;
            float rad = angle * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Sin(rad), Mathf.Cos(rad)) * azimuthalPointRadius;
            Vector3 worldPos = center.TransformPoint((Vector3)offset);
            Gizmos.DrawWireSphere(worldPos, 0.4f);
        }
    }

    #endregion
}
