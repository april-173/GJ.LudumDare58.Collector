using Unity.VisualScripting;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    public static StateManager Instance { get; private set; }

    [Header("State")]
    public float maxOxygen;
    public float oxygenUptakeRate;
    public float maxEnergy;
    public float energyConsumptionRateDuringIdle;
    public float energyConsumptionRateDuringMove;
    public float energyConsumptionRateDuringFastMove;
    public int energyConsumptionOfPing;
    public float maxHydrogen;

    private float currentOxygen;
    private float currentEnergy;
    private float currentHydrogen;

    private bool isIdle;
    private bool isMove;
    private bool isFastMove;

    private bool subscribed = false;

    public float OxygenRatio => currentOxygen / maxOxygen;
    public float EnergyRation => currentEnergy / maxEnergy;
    public float HydrogenRation => currentHydrogen / maxHydrogen;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        currentOxygen = maxOxygen;
        currentEnergy = maxEnergy;
        currentHydrogen = 360;
    }

    private void OnEnable()
    {
        TrySubscribeInput();
    }

    private void Start()
    {
        TrySubscribeInput();
    }

    private void OnDisable()
    {
        TryUnsubscribeInput();
    }

    private void Update()
    {
        UpdateOxygen();
        UpdateEnergy();

        if (currentEnergy <= 0 || currentOxygen <= 0)
            GameManager.Instance.PlayDeathAnimation("DEATH");
    }

    #region < 事件订阅 >
    private void TrySubscribeInput()
    {
        if (subscribed) return;
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnMove += HandleMoveEvent;
            InputManager.Instance.OnFastMove += HandleFastMoveEvent;
            InputManager.Instance.OnPing += HandlePingEvent;
            subscribed = true;
        }
    }

    private void TryUnsubscribeInput()
    {
        if (!subscribed) return;
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnMove -= HandleMoveEvent;
            InputManager.Instance.OnFastMove -= HandleFastMoveEvent;
            InputManager.Instance.OnPing -= HandlePingEvent;
        }
        subscribed = false;
    }
    #endregion

    #region < 事件处理 >
    private void HandleMoveEvent(Vector2Int v)
    {
        if (isFastMove)
        {
            isIdle = false;
            isMove = false;
            return;
        }

        if(v == Vector2Int.zero)
        {
            isIdle = true;
            isMove = false;
            isFastMove = false;
        }
        else
        {
            isIdle = false;
            isMove = true;
        }
    }

    private void HandleFastMoveEvent(bool b)
    {
        isFastMove = b;
    }

    private void HandlePingEvent()
    {
        ChangeEnergy(-energyConsumptionOfPing);
    }

    #endregion

    #region < 状态更新 >

    private void UpdateOxygen()
    {
        currentOxygen = currentOxygen - (Time.deltaTime * oxygenUptakeRate);
    }

    private void UpdateEnergy()
    {
        if (isFastMove) currentEnergy = currentEnergy - (Time.deltaTime * energyConsumptionRateDuringFastMove);
        else if (isMove) currentEnergy = currentEnergy - (Time.deltaTime * energyConsumptionRateDuringMove);
        else if (isIdle) currentEnergy = currentEnergy - (Time.deltaTime * energyConsumptionRateDuringIdle);
    }

    public void ChangeOxygen(float value)
    {
        if (currentOxygen >= maxOxygen) { currentOxygen = maxOxygen;return; }
        currentOxygen += value;
    }

    public void ChangeEnergy(float value)
    {
        if(currentEnergy >= maxEnergy) { currentEnergy = maxEnergy;return; }
        currentEnergy += value;
    }

    public void ChangeHydrogen(float value)
    {
        if(currentHydrogen >= maxHydrogen) { currentHydrogen = maxHydrogen;return; }
        currentHydrogen += value;
    }

    public void GenerateOxygen(float value)
    {
        if (currentHydrogen - value > 0 && currentHydrogen > 0 && currentOxygen < maxOxygen && currentOxygen + value < maxOxygen)
        {
            ChangeHydrogen(-value);
            ChangeOxygen(value);
        }
    }

    public void GenerateEnergy(float value)
    {
        if (currentHydrogen - value > 0 && currentHydrogen > 0 && currentEnergy < maxEnergy && currentEnergy + value < maxEnergy) 
        {
            ChangeHydrogen(-value);
            ChangeEnergy(value);
        }
    }

    #endregion
}
