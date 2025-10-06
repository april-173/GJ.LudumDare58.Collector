using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance {  get; private set; }

    [Header("Debug")]
    public bool eventDebug;

    [Header("Setting")]
    public bool enableInput = true;
    public bool usePingLoop = false;

    [Header("Key Bindings")]
    public Key moveUp = Key.W;
    public Key moveDown = Key.S;
    public Key moveLeft = Key.A;
    public Key moveRight = Key.D;
    public Key fastMove = Key.LeftShift;
    public Key ping = Key.Space;

    public event Action<Vector2Int> OnMove;
    public event Action<bool> OnFastMove;
    public event Action OnPing;
    public event Action<bool> OnScan;

    private Keyboard keyboard;

    private Vector2Int moveVector;

    [Header("Timing")]
    public float pingRepeatRate;
    private float lastPingTime = -999f;

    [Header("Auto")]
    public bool autoPing;
    public bool autoFastMove;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        keyboard = Keyboard.current;
    }

    private void Update()
    {
        if (!enableInput) return;

        HandleMove();
        HandleFastMove();
        HandlePing();
        HandleScan();
    }

    #region < 事件方法 >
    private void HandleMove()
    {
        int up = keyboard[moveUp].isPressed ? 1 : 0;
        int down = keyboard[moveDown].isPressed ? 1 : 0;
        int left = keyboard[moveLeft].isPressed ? 1 : 0;
        int right = keyboard[moveRight].isPressed ? 1 : 0;

        int x = right - left;
        int y = up - down;

        moveVector = new Vector2Int(x, y);
        OnMove?.Invoke(moveVector);
        EventDebug("OnMove");
    }

    private void HandleFastMove()
    {
        if (autoFastMove) 
        { 
            OnFastMove?.Invoke(true); 
            return; 
        }

        if (keyboard[fastMove].isPressed)
        {
            OnFastMove?.Invoke(true);
            EventDebug("OnFastMove");
            return;
        }

        OnFastMove?.Invoke(false);
    }    

    private void HandlePing()
    {
        usePingLoop = autoPing;

        if (usePingLoop && Time.time - lastPingTime >= pingRepeatRate)
        {
            lastPingTime = Time.time;
            OnPing?.Invoke();
            EventDebug("OnPing");
            return;
        }

        if (keyboard[ping].wasPressedThisFrame && Time.time - lastPingTime >= pingRepeatRate) 
        {
            lastPingTime = Time.time;
            OnPing?.Invoke();
            EventDebug("OnPing");
            return;
        }
    }

    private void HandleScan()
    {
        if (Mouse.current != null && Mouse.current.rightButton.isPressed)
        {
            OnScan?.Invoke(true);
            EventDebug("OnScan");
        }

        if (Mouse.current != null && Mouse.current.rightButton.wasReleasedThisFrame)
        {
            OnScan?.Invoke(false);
        }
    }
    #endregion

    #region < 调试方法 >
    private void EventDebug(string triggerEvent)
    {
#if UNITY_EDITOR
        if (!eventDebug) return;

        switch(triggerEvent)
        {
            case "OnMove":
                if(moveVector != Vector2Int.zero)
                    Debug.Log($"<color=#92BFD1><b>[{GetType().Name}]</b></color> <color=#4EC9B0>OnMove 事件触发</color>" + $" {moveVector}");
                break;
            case "OnFastMove":
                Debug.Log($"<color=#92BFD1><b>[{GetType().Name}]</b></color> <color=#4EC9B0>OnFastMove 事件触发</color>");
                break;
            case "OnPing":
                Debug.Log($"<color=#92BFD1><b>[{GetType().Name}]</b></color> <color=#4EC9B0>OnPing 事件触发</color>");
                break;
        }

#endif
    }

    #endregion
}
